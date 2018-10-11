using System;
using System.Threading.Tasks;

namespace Dogfood.Exports
{
    public interface IDogfoodService
    {
        Task<bool> Reinstall(string vsixFile, IProgress<string> progress);
    }
}