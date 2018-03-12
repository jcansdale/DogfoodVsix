using System;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.ComponentModelHost;
using Dogfood.Exports;

namespace InstallExperiment
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class DogfoodPackage : AsyncPackage
    {
        public const string PackageGuidString = "fc2bbba2-e7bf-42ab-b12e-0388cb5ff987";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var componentModel = (IComponentModel)await GetServiceAsync(typeof(SComponentModel));
            foreach(var initializable in componentModel.GetExtensions<IAsyncInitializable>())
            {
                await initializable.InitializeAsync(this);
            }
        }
    }
}
