using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace AnalyzerReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Attempt to set the version of MSBuild.
            var instance = MSBuildLocator.RegisterDefaults();

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            using var workspace = MSBuildWorkspace.Create();

            // Print message for WorkspaceFailed event to help diagnosing project load failures.
            workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

            var solutionPath = args[0];
            Console.WriteLine($"Loading solution '{solutionPath}'");

            // Attach progress reporter so we print projects as they are loaded.
            var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            Console.WriteLine($"Finished loading solution '{solutionPath}'");

            // Get all analyzers in the project
            var diagnosticDescriptors = solution.Projects
                // select explicit project
                .Where(x => x.Id.Id.ToString() != "f10591ad-bf0b-4737-ab94-6f2f38c4fd9c")
                .SelectMany(project => project.AnalyzerReferences)
                .SelectMany(analyzerReference => analyzerReference.GetAnalyzersForAllLanguages())
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .Distinct().OrderBy(x => x.Id);

            Console.WriteLine($"{nameof(DiagnosticDescriptor.Id),-15} {nameof(DiagnosticDescriptor.Title)}");
            foreach (var diagnosticDescriptor in diagnosticDescriptors)
            {
                Console.WriteLine($"{diagnosticDescriptor.Id,-15} {diagnosticDescriptor.Title}");
            }
            
            
            // Get all analyzers in the project
            var documents = solution.Projects
                .SelectMany(project => project.AnalyzerConfigDocuments);

            Console.WriteLine($"{nameof(DiagnosticDescriptor.Id),-15} {nameof(DiagnosticDescriptor.Title)}");
            foreach (var diagnosticDescriptor in documents)
            {
                Console.WriteLine($"{diagnosticDescriptor.Id,-15} {diagnosticDescriptor.FilePath}");
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}
