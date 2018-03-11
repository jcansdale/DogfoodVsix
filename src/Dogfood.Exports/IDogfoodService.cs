using System;
using System.Threading.Tasks;
using EnvDTE;

namespace Dogfood.Exports
{
    public interface IDogfoodService
    {
        string FindVsixFile(Solution solution);
        Task Reinstall(string vsixFile, IProgress<string> progress);
    }
}