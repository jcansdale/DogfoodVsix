using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Dogfood.Exports
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync(IAsyncServiceProvider asyncServiceProvider);
    }
}
