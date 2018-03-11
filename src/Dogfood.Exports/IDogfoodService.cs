using System;
using System.Threading.Tasks;

namespace Dogfood.Exports
{
    public interface IDogfoodService
    {
        Task Reinstall(string vsixFile, IProgress<string> progress);
    }
}