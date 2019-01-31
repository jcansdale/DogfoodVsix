
using System;

namespace Dogfood.Exports
{
    public interface IMainThreadInitializable
    {
        void InitializeOnMainThread(IServiceProvider serviceProvider);
    }
}
