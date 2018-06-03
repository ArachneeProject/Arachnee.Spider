using System.IO;
using Sharpmake;

namespace SharpmakeGeneration
{
    // Represents the project that will be generated.
    [Generate]
    public class Spider_Project : CSharpProject
    {
        public Spider_Project()
        {
            Name = "Spider";
			SourceRootPath = "[project.SharpmakeCsPath]/../Spider";
            RootPath = "[project.SharpmakeCsPath]/../";
            AddTargets(GeneratedSolution.Target);
        }
        
        [Configure]
        public void ConfigureAll(Project.Configuration conf, Target target)
        {
            conf.Output = Configuration.OutputType.DotNetConsoleApp;
            
            conf.ProjectFileName = @"Spider";
			conf.SolutionFolder = "Spider";
            conf.ProjectPath = @"[project.SharpmakeCsPath]/../Spider";
            conf.TargetPath = RootPath + @"\Outputs\[project.Name]";
            
            conf.ReferencesByName.Add("System");
            conf.ReferencesByNuGetPackage.Add("TMDbLib", "1.2.0-alpha");
			conf.ReferencesByNuGetPackage.Add("Newtonsoft.Json", "9.0.1");
			conf.ReferencesByNuGetPackage.Add("RestSharp", "105.2.3");

            // conf.ReferencesByNuGetPackage.Add("NUnit", "3.9.0");
            // conf.AddPrivateDependency<InteropLibrary>(target);
        }
    }	
}