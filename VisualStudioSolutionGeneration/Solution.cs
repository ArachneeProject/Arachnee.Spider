using System.IO;
using Sharpmake;

[module: Sharpmake.Include("Spider_Project.cs")]

namespace SharpmakeGeneration
{
    // Represents the solution that will be generated.
    [Generate]
    class GeneratedSolution : CSharpSolution
    {
        public GeneratedSolution()
        {
            Name = "Arachnee.Spider";
            AddTargets(GeneratedSolution.Target);
        }

        // Entry point
        [Main]
        public static void SharpmakeMain(Arguments sharpmakeArgs)
        {
            sharpmakeArgs.Generate<GeneratedSolution>();
        }

        [Configure]
        public void ConfigureAll(Solution.Configuration conf, Target target)
        {
            conf.SolutionFileName = "Arachnee.Spider";
            conf.SolutionPath = Path.Combine("[solution.SharpmakeCsPath]", "..");

            // Add projects here
            conf.AddProject<Spider_Project>(target);
			
			// Tests projects
            
        }

        public static Target Target
        {
            get
            {
                return new Target(

                    // 32/64 bits
                    Platform.win64 | Platform.win32,

                    // Visual Studio environment
                    DevEnv.vs2017,

                    // Configuration
                    Optimization.Debug | Optimization.Release,

                    // .NET Framework
                    framework: DotNetFramework.v4_5_2
                );
            }
        }
    }
}