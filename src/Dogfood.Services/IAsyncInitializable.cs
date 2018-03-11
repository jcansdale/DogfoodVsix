using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Dogfood.Services
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync(AsyncPackage package);
    }
}
