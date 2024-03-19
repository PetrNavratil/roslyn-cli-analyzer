using System.Collections.Immutable;
using AnalyzerDiagnosticInfo.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json;

namespace AnalyzerDiagnosticInfo;

public static class DiagnosticGenerator
{
    public static async Task<ProjectDiagnostics> PerformDiagnostic(Project project, bool includeHidden)
    {
        Console.WriteLine($"{project.Name}:\t\tGetting compilation");
        var compilation = await project.GetCompilationAsync();
        Console.WriteLine($"{project.Name}:\t\tCompleted compilation");
        Console.WriteLine($"{project.Name}:\t\tRetrieving analyzers");
        var analyzers = project.AnalyzerReferences
            .SelectMany(x => x.GetAnalyzers(project.Language))
            .ToImmutableArray();
        Console.WriteLine($"{project.Name}:\t\tCompleted retrieving analyzers");
        Console.WriteLine($"{project.Name}:\t\tRunning analyzers");
        var allDiagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAllDiagnosticsAsync(CancellationToken.None);

        Console.WriteLine($"{project.Name}:\t\tAnalyzers completed");
        return new ProjectDiagnostics
        {
            Name = project.Name,
            Path = project.FilePath,
            Diagnostics = allDiagnostics
                .WhereIf(x => x.Severity != DiagnosticSeverity.Hidden, !includeHidden)
                .Select(x => new Diagnostic
                {
                    Id = x.Id,
                    Severity = x.Severity,
                    FilePosition = x.Location.GetMappedLineSpan()
                })
        };
    }
}

public class Diagnostic
{
    public required string Id { get; set; }
    public DiagnosticSeverity Severity { get; set; }
    
    [JsonIgnore]
    public required FileLinePositionSpan FilePosition { get; set; }
    public string FilePath => FilePosition.Path;
    public int StartLine => FilePosition.StartLinePosition.Line;
    public int StartColumn => FilePosition.StartLinePosition.Character;
    public int EndLine => FilePosition.EndLinePosition.Line;
    public int EndColumn => FilePosition.EndLinePosition.Character;

    public override string ToString()
    {
        return $"{Id}\t{Severity}\t{FilePath}\t{StartLine}:{StartColumn} - {EndLine}:{EndColumn}";
    }
}
