using System;
using System.Threading.Tasks;
using Dogfood.Exports;
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
            var installableExtension = em.CreateInstallableExtension(vsixFile);

            if (em.TryGetInstalledExtension(installableExtension.Header.Identifier, out IInstalledExtension previousExt))
            {
                progress.Report("Uninstalling " + previousExt.Header.Name);

                try
                {
                    await Task.Run(() => em.Uninstall(previousExt));
                }
                catch (RequiresAdminRightsException e)
                {
                    progress.Report(e.Message);
                }
            }

            var header = installableExtension.Header;
            if (header.AllUsers)
            {
                SetValue(header, nameof(header.AllUsers), false);
                progress.Report($"Changed extension to AllUsers={header.AllUsers}");
            }

            SetValue(header, nameof(header.IsExperimental), true);

            progress.Report("Installing " + installableExtension.Header.Name + " from " + vsixFile);
            await Task.Run(() => em.Install(installableExtension, false));

            var installedExt = em.GetInstalledExtension(installableExtension.Header.Identifier);
            ReportContents(progress, installedExt);

            var reason = em.Enable(installedExt);
            if (reason != RestartReason.None)
            {
                progress.Report("Please restart Visual Studio");
            }
        }

        static void ReportContents(IProgress<string> progress, IInstalledExtension installedExt)
        {
            progress.Report(installedExt.InstallPath);
            foreach (var content in installedExt.Content)
            {
                progress.Report($"  {content.RelativePath} ({content.ContentTypeName})");
            }
        }

        static void SetValue(object target, string name, object value)
        {
            target.GetType().GetProperty(name).SetValue(target, value);
        }
    }
}
