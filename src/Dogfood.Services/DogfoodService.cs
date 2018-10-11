extern alias DS14;
extern alias DS15;
using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using DogfoodService14 = DS14::Dogfood.Services.DogfoodService;
using DogfoodService15 = DS15::Dogfood.Services.DogfoodService;

namespace Dogfood.Services
{
    [Export(typeof(IAsyncInitializable))]
    [Export(typeof(IDogfoodService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DogfoodService : IAsyncInitializable, IDogfoodService
    {
        readonly IProjectUtilities projectUtilities;

        Lazy<IDogfoodService> dogfoodService;

        [ImportingConstructor]
        public DogfoodService(IProjectUtilities projectUtilities)
        {
            this.projectUtilities = projectUtilities;
        }

        public async Task InitializeAsync(IAsyncServiceProvider asyncServiceProvider)
        {
            var dte = (DTE)await asyncServiceProvider.GetServiceAsync(typeof(DTE));
            dogfoodService = new Lazy<IDogfoodService>(() => CreateDogfoodService(dte, asyncServiceProvider, projectUtilities));
        }

        public Task<bool> Reinstall(string vsixFile, IProgress<string> progress) =>
            dogfoodService.Value.Reinstall(vsixFile, progress);

        static IDogfoodService CreateDogfoodService(DTE dte, IAsyncServiceProvider asyncServiceProvider,
            IProjectUtilities projectUtilities)
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
