using System.Collections.Generic;
using EnvDTE;

namespace Dogfood.Exports
{
    public interface IProjectUtilities
    {
        string FindVsixFile(Solution solution);
        string FindBuiltFile(Project project);
        IEnumerable<Project> FindProjects(Solution solution);
    }
}