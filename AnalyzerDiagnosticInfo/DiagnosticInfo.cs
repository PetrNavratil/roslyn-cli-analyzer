using Microsoft.CodeAnalysis;

namespace AnalyzerDiagnosticInfo;

public class DiagnosticInfo
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public bool EnabledByDefault { get; set; }
    public DiagnosticSeverity DefaultSeverity { get; set; }
    public ReportDiagnostic EffectiveSeverity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public AnalyzerInfo? AnalyzerInfo { get; set; }
    
    public required AnalyzerSeverity FinalSeverity { get; set; }

    public bool Enabled => FinalSeverity != AnalyzerSeverity.None;

}

public class AnalyzerInfo
{
    public string ReferenceId { get; set; }
    public required string ReferenceDisplay { get; set; }
    public required string AnalyzerName { get; set; }

}

public class ProjectDiagnostics
{
    public string Name { get; set; } = string.Empty;
    public string? Path { get; set; }
    public IEnumerable<Diagnostic> Diagnostics = Array.Empty<Diagnostic>();
}
