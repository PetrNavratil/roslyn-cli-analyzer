using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace AnalyzerDiagnosticInfo.SolutionManager;

public static class SolutionManager
{
    public static Task<Solution> LoadSolutionAsync(string solutionPath)
    {
        var buildWorkspace = CreateBuildWorkspace();
        return buildWorkspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
    }

    public static Task<Project> LoadProjectAsync(string projectPath)
    {
        var buildWorkspace = CreateBuildWorkspace();
        return buildWorkspace.OpenProjectAsync(projectPath, new ConsoleProgressReporter());
    }

    private static MSBuildWorkspace CreateBuildWorkspace()
    {
        var buildInstance = MSBuildLocator.RegisterDefaults();
        Console.WriteLine($"Using MSBuild at '{buildInstance.MSBuildPath}' to load projects.");
        var buildWorkspace = MSBuildWorkspace.Create();

        // Print message for WorkspaceFailed event to help diagnosing project load failures.
        buildWorkspace.WorkspaceFailed += (o, e) => Console.WriteLine($"{e.Diagnostic.Message}");
        return buildWorkspace;
    }
}
