using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using Task = System.Threading.Tasks.Task;

namespace Dogfood.Exports
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync(IServiceProvider serviceProvider);
    }
}
