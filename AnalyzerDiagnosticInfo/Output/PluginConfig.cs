using Newtonsoft.Json;

namespace AnalyzerDiagnosticInfo.Output;

public class PluginConfig
{
    [JsonIgnore]
    private const string MaterialIcon = "csharp";
    [JsonIgnore]
    private const string DefaultSlug = "csharp_analyzer";
    [JsonIgnore]
    private const string DefaultTitle = "C# analyzer using Roslyn API";
    
    public string Title { get; set; } = DefaultTitle;
    public string Slug { get; set; } = DefaultSlug;
    public string Icon { get; set; } = MaterialIcon;
    public IEnumerable<PluginConfigAudit> Audits { get; set; } = Array.Empty<PluginConfigAudit>();
}

public class PluginConfigAudit
{
    public string Slug { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? DocsUrl { get; set; }
}
