extern alias DS14;
extern alias DS15;
using System;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using DogfoodService14 = DS14::Dogfood.Services.DogfoodService;
using DogfoodService15 = DS15::Dogfood.Services.DogfoodService;

namespace Dogfood.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DogfoodService
    {
        readonly IServiceProvider serviceProvider;
        readonly IProjectUtilities projectUtilities;
        readonly Lazy<IDogfoodService> dogfoodService;

        [ImportingConstructor]
        public DogfoodService(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            IProjectUtilities projectUtilities)
        {
            this.serviceProvider = serviceProvider;
            this.projectUtilities = projectUtilities;
            dogfoodService = new Lazy<IDogfoodService>(CreateDogfoodService);
        }

        [Export(typeof(IDogfoodService))]
        public IDogfoodService Instance => dogfoodService.Value;

        IDogfoodService CreateDogfoodService()
        {
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            switch (dte.Version)
            {
                case "14.0":
                    return CreateDogfoodService(() => new DogfoodService14(serviceProvider, projectUtilities));
                case "15.0":
                    return CreateDogfoodService(() => new DogfoodService15(serviceProvider, projectUtilities));
                default:
                    return null;
            }
        }

        static IDogfoodService CreateDogfoodService(Func<IDogfoodService> factory) => factory();
    }
}
