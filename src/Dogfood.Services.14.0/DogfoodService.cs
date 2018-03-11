using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Dogfood.Exports;
using EnvDTE;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ExtensionManager;
using System.Collections.Generic;
using VSLangProj;
using EnvDTE80;

namespace Dogfood.Services
{
    public class DogfoodService : IDogfoodService
    {
        IServiceProvider serviceProvider;
        IProjectUtilities projectUtilities;

        public DogfoodService(IServiceProvider serviceProvider, IProjectUtilities projectUtilities)
        {
            this.serviceProvider = serviceProvider;
            this.projectUtilities = projectUtilities;
        }

        public async Task Execute(IProgress<string> progress)
        {
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));

            progress = progress ?? new Progress<string>(Console.WriteLine);

            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Visual Studio Extension (*.vsix)|*.vsix"
                };

                var vsixFile = FindVsixFile(dte.Solution);
                if (vsixFile != null)
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(vsixFile);
                    openFileDialog.FileName = Path.GetFileName(vsixFile);
                }

                if (openFileDialog.ShowDialog() == true)
                {
                    await Reinstall(openFileDialog.FileName, progress);
                }
            }
            catch (Exception e)
            {
                progress.Report(e.ToString());
            }
        }

        public async Task Reinstall(string vsixFile, IProgress<string> progress)
        {
            var em = (IVsExtensionManager)serviceProvider.GetService(typeof(SVsExtensionManager));
            var ext = em.CreateInstallableExtension(vsixFile);

            if (em.TryGetInstalledExtension(ext.Header.Identifier, out IInstalledExtension installedExt))
            {
                progress.Report("Uninstalling " + installedExt.Header.Name);

                try
                {
                    await Task.Run(() => em.Uninstall(installedExt));
                }
                catch (RequiresAdminRightsException e)
                {
                    progress.Report(e.Message);
                }
            }

            var header = ext.Header;
            SetValue(header, nameof(header.AllUsers), false);
            SetValue(header, nameof(header.IsExperimental), true);
            progress.Report("Installing " + ext.Header.Name + " from " + vsixFile);
            await Task.Run(() => em.Install(ext, false));
            progress.Report("Installed " + ext.Header.Name + " from " + vsixFile);

            installedExt = em.GetInstalledExtension(ext.Header.Identifier);
            var reason = em.Enable(installedExt);
            if (reason != RestartReason.None)
            {
                progress.Report("Please restart Visual Studio");
            }
        }

        public string FindVsixFile(Solution solution)
        {
            var projects = projectUtilities.FindProjects(solution);
            foreach (Project project in projects)
            {
                var file = projectUtilities.FindBuiltFile(project);
                if (file == null)
                {
                    continue;
                }

                file = Path.ChangeExtension(file, "vsix");
                if (!File.Exists(file))
                {
                    continue;
                }

                return file;
            }

            return null;
        }

        static void SetValue(object target, string name, object value)
        {
            target.GetType().GetProperty(name).SetValue(target, value);
        }
    }
}
