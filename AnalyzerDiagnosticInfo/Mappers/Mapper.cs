using AnalyzerDiagnosticInfo.CompilerCodesGenerator;
using AnalyzerDiagnosticInfo.ConfigManager;
using AnalyzerDiagnosticInfo.Output;
using Microsoft.CodeAnalysis;

namespace AnalyzerDiagnosticInfo.Mappers;

public static class Mapper
{
    public static DiagnosticInfo Map(CompilerCode value)
    {
        return new DiagnosticInfo
        {
            Id = value.Code,
            Description = value.Message,
            Title = value.Name,
            FinalSeverity = Map2(value.Severity)!.Value,
            LinkUrl = value.Link
        };
    }

    public static AuditDetailIssueSeverity Map(DiagnosticSeverity severity)
    {
        return severity switch
        {
            DiagnosticSeverity.Hidden => AuditDetailIssueSeverity.Info,
            DiagnosticSeverity.Info => AuditDetailIssueSeverity.Info,
            DiagnosticSeverity.Warning => AuditDetailIssueSeverity.Warning,
            DiagnosticSeverity.Error => AuditDetailIssueSeverity.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    }

    public static IEnumerable<AuditOutput> Map(ProjectDiagnostics projectDiagnostic, IEnumerable<PluginConfigAudit> audits)
    {
        return Map(projectDiagnostic.Diagnostics, audits);
    }
    
        
    public static IEnumerable<AuditOutput> Map(IEnumerable<ProjectDiagnostics> projectDiagnostics, IEnumerable<PluginConfigAudit> audits)
    {
        return Map(projectDiagnostics.SelectMany(x => x.Diagnostics), audits);
    }

    private static IEnumerable<AuditOutput> Map(IEnumerable<Diagnostic> projectDiagnostics, IEnumerable<PluginConfigAudit> audits)
    {
        var currentDiagnostics =  projectDiagnostics
            .GroupBy(
                x => x.Id,
                (id, diagnostics) =>
                {
                    var diagnosticsList = diagnostics.ToList();
                    return new AuditOutput
                    {
                        Slug = Slugify(id),
                        Value = diagnosticsList.Count,
                        Score = 1,
                        Details = new AuditDetail
                        {
                            Issues = diagnosticsList.Select(x => new AuditDetailIssue
                            {
                                Message = x.Message,
                                Severity = Map(x.Severity),
                                Source = new AuditDetailIssueSource
                                {
                                    File = x.FilePath,
                                    Position = new AuditDetailIssueLocationPosition
                                    {
                                        StartLine = x.StartLine,
                                        StartColumn = x.StartColumn,
                                        EndLine = x.EndLine,
                                        EndColumn = x.EndColumn
                                    }
                                }

                            })
                        }
                    };
                });

        return audits.GroupJoin(
            currentDiagnostics,
            x => x.Slug,
            x => x.Slug,
            (audit, diagnostics) =>
            {
                // if found, the diagnostic already exists
                // if not, create empty diagnostic from the audit
                var diagnostic = diagnostics.FirstOrDefault();
                return diagnostic ?? Map(audit);
            });
    }

    public static AuditOutput Map(PluginConfigAudit value)
    {
        return new AuditOutput
        {
            Slug = value.Slug,
            Value = 0,
            Score = 0,
            Details = new AuditDetail()
        };
    }
    
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

    public static AnalyzerSeverity? Map2(DiagnosticSeverity value)
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

    public static PluginConfigAudit Map(DiagnosticInfo value)
    {
        return new PluginConfigAudit
        {
            Title = value.Title,
            Slug = Slugify(value.Id),
            Description = value.Description,
            DocsUrl = value.LinkUrl
        };
    }

    public static PluginConfig Map(ProjectConfig value)
    {
        return new PluginConfig
        {
            Audits = value.Diagnostics.Select(Map)
        };
    }

    public static string Slugify(string value)
    {
        return value
            .Trim()
            .ToLower()
            .Replace(@"\s+|/", "-")
            .Replace(@"[^a-z\d-]", "");
    }
}
