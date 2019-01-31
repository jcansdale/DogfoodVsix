extern alias DS14;
extern alias DS15;
extern alias DS16;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using EnvDTE;
using DogfoodService14 = DS14::Dogfood.Services.DogfoodService;
using DogfoodService15 = DS15::Dogfood.Services.DogfoodService;
using DogfoodService16 = DS16::Dogfood.Services.DogfoodService;

namespace Dogfood.Services
{
    [Export(typeof(IMainThreadInitializable))]
    [Export(typeof(IDogfoodService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DogfoodService : IMainThreadInitializable, IDogfoodService
    {
        readonly IProjectUtilities projectUtilities;

        Lazy<IDogfoodService> dogfoodService;

        [ImportingConstructor]
        public DogfoodService(IProjectUtilities projectUtilities)
        {
            this.projectUtilities = projectUtilities;
        }

        public void InitializeOnMainThread(IServiceProvider serviceProvider)
        {
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            dogfoodService = new Lazy<IDogfoodService>(() => CreateDogfoodService(dte, serviceProvider, projectUtilities));
        }

        public Task<bool> Reinstall(string vsixFile, IProgress<string> progress) =>
            dogfoodService.Value.Reinstall(vsixFile, progress);

        static IDogfoodService CreateDogfoodService(DTE dte, IServiceProvider serviceProvider,
            IProjectUtilities projectUtilities)
        {
            switch (dte.Version)
            {
                case "14.0":
                    return CreateDogfoodService(() => new DogfoodService14(serviceProvider, projectUtilities));
                case "15.0":
                    return CreateDogfoodService(() => new DogfoodService15(serviceProvider, projectUtilities));
                case "16.0":
                    try
                    {
                        Assembly.Load("Microsoft.VisualStudio.ExtensionManager, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                        return CreateDogfoodService(() => new DogfoodService16(serviceProvider, projectUtilities));
                    }
                    catch (FileNotFoundException)
                    {
                        // If we can't load ExtensionManager 16.0, fall back to using the 15.0 version
                        return CreateDogfoodService(() => new DogfoodService15(serviceProvider, projectUtilities));
                    }

                default:
                    return null;
            }
        }

        static IDogfoodService CreateDogfoodService(Func<IDogfoodService> factory) => factory();
    }
}
