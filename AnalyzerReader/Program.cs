using System.Collections.Immutable;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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
            // TODO: tu je warning, co ma CA flag a pritom to je 3rd party vec :(
            // TODO: aha, to tam pridava 3rd party vec, ale zaroven to je v 7+ .net all configu #bordel
            // TODO: ok, ted to tam je 3x :D
            var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            // var solution = await workspace.OpenProjectAsync(solutionPath, new ConsoleProgressReporter());
            
            Console.WriteLine($"Finished loading solution '{solutionPath}'");

            // // Get all analyzers in the project
            // var diagnosticDescriptors = solution.Projects
            //     .Where(x => x.Id.Id.ToString() != "f10591ad-bf0b-4737-ab94-6f2f38c4fd9c")
            //     .SelectMany(project =>
            //     {
            //         // tu jde vytahnout v jakem jazyce ten projekt je a na zaklade toho vytahnout analyzery
            //         Console.WriteLine(project.Language);
            //         return project.AnalyzerReferences;
            //     })
            //     // TODO: tady jde passnout jazyk .. C#, VB
            //     .SelectMany(analyzerReference => analyzerReference.GetAnalyzersForAllLanguages())
            //     .SelectMany(analyzer => analyzer.SupportedDiagnostics)
            //     .Distinct().OrderBy(x => x.Id);

            // get all analyzers for each project
            foreach (var project in solution.Projects)
            {
                // var compilation = await project.GetCompilationAsync();
                // AnalyzerConfigOptionsProvider optionsProvider = new AnalyzerConfigOptionsProvider(
                //     new AnalyzerConfigOptions(ImmutableArray<AnalyzerConfigOptionsResult>.Empty));
                //
                // // Get all analyzers for the compilation
                // var analyzers = compilation.SyntaxTrees
                //     .SelectMany(tree => compilation.GetSemanticModel(tree).GetDiagnostics().Select(diagnostic => diagnostic.Id))
                //     .Distinct()
                //     .Select(id => compilation.GetAnalyzer(id))
                //     .Where(analyzer => analyzer != null)
                //     .ToImmutableArray();
                // Console.WriteLine($"{project.Name}\t{project.Language}");
                foreach (var analyzerReference in project.AnalyzerReferences)
                {
                    // toto jsou vsechny analyzery a generatory, co jsou v projektu pouzite - DLLka, nugety package proste
                    Console.WriteLine($"\t{analyzerReference.Display}\t{analyzerReference.Id}");
                    foreach (var diagnosticAnalyzer in analyzerReference.GetAnalyzers(project.Language))
                    {
                        Console.WriteLine($"\t\t{diagnosticAnalyzer}");
                        foreach (var diagnosticDescriptor in diagnosticAnalyzer.SupportedDiagnostics.Distinct())
                        {
                            Console.WriteLine($"\t\t\t{diagnosticDescriptor.Id}\t{diagnosticDescriptor.Title}\t{diagnosticDescriptor.Category} {diagnosticDescriptor}");
                        }
                    }
                }
            }
            
            
            
            

            // Console.WriteLine($"{nameof(DiagnosticDescriptor.Id),-15} {nameof(DiagnosticDescriptor.Title)}");
            // foreach (var diagnosticDescriptor in diagnosticDescriptors)
            // {
            //     Console.WriteLine($"{diagnosticDescriptor.Id,-15} {diagnosticDescriptor.Title}\t{diagnosticDescriptor.Category}");
            // }
            
            
            // Get all analyzers in the project
            var documents = solution.Projects
            // var documents = solution.AnalyzerConfigDocuments;
                .SelectMany(project => project.AnalyzerConfigDocuments);
            
            Console.WriteLine($"{nameof(DiagnosticDescriptor.Id),-15} {nameof(DiagnosticDescriptor.Title)}");
            foreach (var diagnosticDescriptor in documents)
            {
            
                Console.WriteLine($"{diagnosticDescriptor.Id,-15} {diagnosticDescriptor.FilePath}");
                Console.WriteLine($"{diagnosticDescriptor.Name} {diagnosticDescriptor.Folders.Count}");
                // var text = await diagnosticDescriptor.GetTextAsync();
                // foreach (var line in text.Lines)
                // {
                //     Console.WriteLine($"{line.Text}");
                // }

                var parsed = new EditorConfig.Core.EditorConfigParser();
                var config = parsed.Parse(diagnosticDescriptor.FilePath);
                Console.WriteLine(config.Properties.Count);
                foreach (var kv in config.Properties)
                {
                    Console.WriteLine($"{kv.Key}\t{kv.Value}");
                }

            }
            
            // Console.WriteLine(diagnosticDescriptors.Count());
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
