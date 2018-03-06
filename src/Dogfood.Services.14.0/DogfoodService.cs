using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Dogfood.Exports;
using EnvDTE;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ExtensionManager;

namespace Dogfood.Services
{
    public class DogfoodService : IDogfoodService
    {
        IAsyncServiceProvider asyncServiceProvider;

        public DogfoodService(IAsyncServiceProvider asyncServiceProvider)
        {
            this.asyncServiceProvider = asyncServiceProvider;
        }

        public async Task Execute(IProgress<string> progress)
        {
            var em = (IVsExtensionManager)await asyncServiceProvider.GetServiceAsync(typeof(SVsExtensionManager));
            var dte = (DTE)await asyncServiceProvider.GetServiceAsync(typeof(DTE));
            await Execute(em, dte, progress);
        }

        public async Task Execute(IVsExtensionManager em, DTE dte, IProgress<string> progress)
        {
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
                    await Reinstall(em, openFileDialog.FileName, progress);
                }
            }
            catch (Exception e)
            {
                progress.Report(e.ToString());
            }
        }

        public async Task Reinstall(IVsExtensionManager em, string vsixFile, IProgress<string> progress)
        {
            var ext = em.CreateInstallableExtension(vsixFile);

            if (em.TryGetInstalledExtension(ext.Header.Identifier, out IInstalledExtension installedExt))
            {
                progress.Report("Uninstalling " + installedExt.Header.Name);

                try
                {
                    await Task.Run(() => em.Uninstall(installedExt));
                }
                catch(RequiresAdminRightsException e)
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
            if(reason != RestartReason.None)
            {
                progress.Report("Please restart Visual Studio");
            }
        }

        public string FindVsixFile(Solution solution)
        {
            foreach (Project project in solution)
            {
                var file = FindBuiltFile(project);
                if (file != null)
                {
                    file = Path.ChangeExtension(file, "vsix");
                    if (File.Exists(file))
                    {
                        return file;
                    }
                }
            }

            return null;
        }

        public string FindBuiltFile(Project project)
        {
            try
            {
                var dir = Path.GetDirectoryName(project.FileName);
                var outputPath = (string)project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value;
                var fileName = (project.ConfigurationManager.ActiveConfiguration.OutputGroups.Item("Built").FileNames as object[])[0] as string;
                var file = Path.Combine(dir, outputPath, fileName);
                return Path.GetFullPath(file);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }

        static void SetValue(object target, string name, object value)
        {
            target.GetType().GetProperty(name).SetValue(target, value);
        }
    }
}
