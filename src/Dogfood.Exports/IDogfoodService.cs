using System;
using System.Threading.Tasks;

namespace Dogfood.Exports
{
    public interface IDogfoodService
    {
        Task Execute(IProgress<string> progress);
    }
}