using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AnalyzerDiagnosticInfo;

public static class DiagnosticInfoParser
{
    public static IEnumerable<DiagnosticInfo> RetrieveDiagnosticInfos(Project project)
    {
        return project.AnalyzerReferences
            .SelectMany(analyzerReference => analyzerReference
                .GetAnalyzers(project.Language)
                .SelectMany(analyzer =>
                    {
                        return analyzer.SupportedDiagnostics
                            .GroupBy(x => x.Id)
                            .Select(x => x.First())
                            .Select(diagnostic => new DiagnosticInfo
                            {
                                Id = diagnostic.Id,
                                Title = diagnostic.Title,
                                Description = diagnostic.Description,
                                EnabledByDefault = diagnostic.IsEnabledByDefault,
                                DefaultSeverity = diagnostic.DefaultSeverity,
                                EffectiveSeverity = diagnostic.GetEffectiveSeverity(project.CompilationOptions!),
                                FinalSeverity = 
                                    (SeverityMapper.Map(diagnostic.GetEffectiveSeverity(project.CompilationOptions!))
                                 ?? SeverityMapper.Map(diagnostic.DefaultSeverity))!.Value,
                                Category = diagnostic.Category,
                                LinkUrl = string.IsNullOrEmpty(diagnostic.HelpLinkUri)
                                    ? null
                                    : diagnostic.HelpLinkUri,
                                AnalyzerInfo = new AnalyzerInfo
                                {
                                    ReferenceId = analyzerReference.Id.ToString(),
                                    ReferenceDisplay = analyzerReference.Display,
                                    AnalyzerName = analyzer.ToString()
                                }
                            });
                    }
                )
            );
    }

    public static IEnumerable<DiagnosticInfo> RetrieveCompilerDiagnosticInfos(string filePath)
    {
        var readJson = File.ReadAllText(filePath);
        var jsonContent = JsonSerializer.Deserialize<IEnumerable<CompilerError>>(readJson);
        return jsonContent.Select(Map);
    }

    public static DiagnosticInfo Map(CompilerError value)
    {
        return new DiagnosticInfo
        {
            Id = value.Code,
            // TODO: check what makes more sense
            Description = value.Message,
            Title = value.Message,
            DefaultSeverity = DiagnosticSeverity.Warning,
            EffectiveSeverity = ReportDiagnostic.Warn,
            FinalSeverity = AnalyzerSeverity.Warning,
            LinkUrl = value.Link
        };
    }
}

public class CompilerError
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
}
