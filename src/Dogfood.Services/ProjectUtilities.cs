using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Dogfood.Exports;
using EnvDTE;
using EnvDTE80;
using VSLangProj;

namespace Dogfood.Services
{
    [Export(typeof(IProjectUtilities))]
    public class ProjectUtilities : IProjectUtilities
    {
        public string FindVsixFile(Solution solution)
        {
            var projects = FindProjects(solution);
            foreach (Project project in projects)
            {
                var file = FindBuiltFile(project);
                if (file == null)
                {
                    continue;
                }

                file = Path.ChangeExtension(file, "vsix");
                if (!File.Exists(file))
                {
                    continue;
                }

                return file;
            }

            return null;
        }

        public string FindBuiltFile(Project project)
        {
            try
            {
                switch (project.Kind)
                {
                    case PrjKind.prjKindCSharpProject:
                    case PrjKind.prjKindVBProject:
                        var dir = Path.GetDirectoryName(project.FileName);
                        var outputPath = (string)project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value;
                        var fileName = (project.ConfigurationManager.ActiveConfiguration.OutputGroups.Item("Built").FileNames as object[])[0] as string;
                        var file = Path.Combine(dir, outputPath, fileName);
                        return Path.GetFullPath(file);
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }

        public IEnumerable<Project> FindProjects(Solution solution)
        {
            var projects = Enumerable.Empty<Project>();
            foreach (Project project in solution)
            {
                projects = projects.Concat(FindProjects(project));
            }

            return projects;
        }

        IEnumerable<Project> FindProjects(Project project)
        {
            // HACK: Avoid - Error CS1752 Interop type 'ProjectKinds' cannot be embedded
            var vsProjectKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
            if (project.Kind == /*ProjectKinds.*/ vsProjectKindSolutionFolder)
            {
                var projects = Enumerable.Empty<Project>();
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item.Object is Project subProject)
                    {
                        projects = projects.Concat(FindProjects(subProject));
                    }
                }

                return projects;
            }

            return new[] { project };
        }
    }
}
