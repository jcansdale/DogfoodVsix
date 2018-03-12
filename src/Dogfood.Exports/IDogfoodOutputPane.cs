using System;

namespace Dogfood.Exports
{
    public interface IDogfoodOutputPane : IProgress<string>
    {
        void Activate();
    }
}