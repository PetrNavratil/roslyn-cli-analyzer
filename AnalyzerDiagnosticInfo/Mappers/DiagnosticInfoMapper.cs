using Microsoft.CodeAnalysis;

namespace AnalyzerDiagnosticInfo.Mappers;

public static class DiagnosticInfoMapper
{
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
