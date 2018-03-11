using System.Collections.Generic;
using EnvDTE;

namespace Dogfood.Exports
{
    public interface IProjectUtilities
    {
        string FindBuiltFile(Project project);
        IEnumerable<Project> FindProjects(Solution solution);
    }
}