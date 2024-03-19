using Microsoft.CodeAnalysis.MSBuild;

namespace AnalyzerDiagnosticInfo;

public class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
{
    public void Report(ProjectLoadProgress value)
    {
        var projectDisplay = Path.GetFileName(value.FilePath);
        if (value.TargetFramework != null)
        {
            projectDisplay += $" ({value.TargetFramework})";
        }

        Console.WriteLine($"{value.Operation,-15} {value.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
    }
}
