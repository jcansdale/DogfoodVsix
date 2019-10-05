using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using EnvDTE;

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

        IDogfoodService CreateDogfoodService(DTE dte, IServiceProvider serviceProvider, IProjectUtilities projectUtilities)
        {
            switch (dte.Version)
            {
                case "14.0":
                case "15.0":
                    return CreateDogfoodService(dte.Version, serviceProvider, projectUtilities);
                case "16.0":
                    try
                    {
                        Assembly.Load("Microsoft.VisualStudio.ExtensionManager, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                        return CreateDogfoodService(dte.Version, serviceProvider, projectUtilities);
                    }
                    catch (FileNotFoundException)
                    {
                        // If we can't load ExtensionManager 16.0, fall back to using the 15.0 version
                        return CreateDogfoodService("15.0", serviceProvider, projectUtilities);
                    }

                default:
                    return null;
            }
        }

        IDogfoodService CreateDogfoodService(string version, IServiceProvider serviceProvider, IProjectUtilities projectUtilities)
        {
            var executingType = GetType();
            var fullName = executingType.FullName;
            var assemblyName = executingType.Assembly.GetName();
            assemblyName.Name = $"{assemblyName.Name}.{version}";
            var assemblyQualifiedName = $"{fullName}, {assemblyName}";
            if (Type.GetType(assemblyQualifiedName, false) is Type targetType)
            {
                return (IDogfoodService)Activator.CreateInstance(targetType, serviceProvider, projectUtilities);
            }

            return null;
        }
    }
}
