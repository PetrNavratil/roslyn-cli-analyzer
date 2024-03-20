namespace AnalyzerDiagnosticInfo.ConfigManager;

public class ProjectConfig
{
    public string Name { get; set; } = string.Empty;
    public string? Path { get; set; }
    public IEnumerable<DiagnosticInfo> Diagnostics { get; set; } = Array.Empty<DiagnosticInfo>();

    public IEnumerable<DiagnosticInfo> ActiveDiagnostics => Diagnostics.Where(x => x.Enabled);
}
