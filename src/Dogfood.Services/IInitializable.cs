
using System;

namespace Dogfood.Exports
{
    public interface IInitializable
    {
        void Initialize(IServiceProvider serviceProvider);
    }
}
