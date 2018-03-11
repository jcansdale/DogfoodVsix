extern alias DS14;
extern alias DS15;
using System;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using DogfoodService14 = DS14::Dogfood.Services.DogfoodService;
using DogfoodService15 = DS15::Dogfood.Services.DogfoodService;
using System.Threading.Tasks;

namespace Dogfood.Services
{
    [Export(typeof(IAsyncInitializable))]
    [Export(typeof(IDogfoodService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DogfoodService : IAsyncInitializable, IDogfoodService
    {
        readonly IProjectUtilities projectUtilities;
        readonly Lazy<IDogfoodService> dogfoodService;

        IAsyncServiceProvider asyncServiceProvider;
        DTE dte;

        [ImportingConstructor]
        public DogfoodService(IProjectUtilities projectUtilities)
        {
            this.projectUtilities = projectUtilities;
            dogfoodService = new Lazy<IDogfoodService>(CreateDogfoodService);
        }

        public async Task InitializeAsync(AsyncPackage package)
        {
            asyncServiceProvider = package;
            dte = (DTE)await package.GetServiceAsync(typeof(DTE));
        }

        public string FindVsixFile(Solution solution) =>
            dogfoodService.Value.FindVsixFile(solution);

        public Task Reinstall(string vsixFile, IProgress<string> progress) =>
            dogfoodService.Value.Reinstall(vsixFile, progress);

        IDogfoodService CreateDogfoodService()
        {
            switch (dte.Version)
            {
                case "14.0":
                    return CreateDogfoodService(() => new DogfoodService14(asyncServiceProvider, projectUtilities));
                case "15.0":
                    return CreateDogfoodService(() => new DogfoodService15(asyncServiceProvider, projectUtilities));
                default:
                    return null;
            }
        }

        static IDogfoodService CreateDogfoodService(Func<IDogfoodService> factory) => factory();
    }
}
