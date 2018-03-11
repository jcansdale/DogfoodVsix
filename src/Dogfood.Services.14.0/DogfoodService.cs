﻿using System;
using System.IO;
using System.Threading.Tasks;
using Dogfood.Exports;
using EnvDTE;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;

namespace Dogfood.Services
{
    public class DogfoodService : IDogfoodService
    {
        IAsyncServiceProvider asyncServiceProvider;
        IProjectUtilities projectUtilities;
        
        public DogfoodService(IAsyncServiceProvider asyncServiceProvider, IProjectUtilities projectUtilities)
        {
            this.asyncServiceProvider = asyncServiceProvider;
            this.projectUtilities = projectUtilities;
        }

        public async Task Reinstall(string vsixFile, IProgress<string> progress)
        {
            var em = (IVsExtensionManager)await asyncServiceProvider.GetServiceAsync(typeof(SVsExtensionManager));
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
