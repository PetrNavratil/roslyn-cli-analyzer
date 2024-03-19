using Microsoft.CodeAnalysis;

namespace AnalyzerDiagnosticInfo;

public static class SeverityMapper
{
    public static AnalyzerSeverity? Map(EditorConfigSeverity value)
    {
        return value switch
        {
            EditorConfigSeverity.Default => null,
            EditorConfigSeverity.Error => AnalyzerSeverity.Error,
            EditorConfigSeverity.Warning => AnalyzerSeverity.Warning,
            EditorConfigSeverity.Suggestion => AnalyzerSeverity.Info,
            EditorConfigSeverity.Silent => null,
            EditorConfigSeverity.None => AnalyzerSeverity.None,
            _ => null
        };
    }

    public static AnalyzerSeverity? Map(DiagnosticSeverity value)
    {
        return value switch
        {
            DiagnosticSeverity.Hidden => AnalyzerSeverity.None,
            DiagnosticSeverity.Info => AnalyzerSeverity.Info,
            DiagnosticSeverity.Warning => AnalyzerSeverity.Warning,
            DiagnosticSeverity.Error => AnalyzerSeverity.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public static AnalyzerSeverity? Map(ReportDiagnostic value)
    {
        return value switch
        {
            ReportDiagnostic.Default => null,
            ReportDiagnostic.Error => AnalyzerSeverity.Error,
            ReportDiagnostic.Warn => AnalyzerSeverity.Warning,
            ReportDiagnostic.Info => AnalyzerSeverity.Info,
            ReportDiagnostic.Hidden => AnalyzerSeverity.None,
            ReportDiagnostic.Suppress => AnalyzerSeverity.None,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
