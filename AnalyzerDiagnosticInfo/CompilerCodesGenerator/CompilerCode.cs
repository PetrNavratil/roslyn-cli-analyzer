using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzerDiagnosticInfo.CompilerCodesGenerator;


public class CompilerCode
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiagnosticSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
}

public class CompilerCodeOutput
{
    public static CompilerCodeOutput Create(
        EnumMemberDeclarationSyntax member,
        Func<string, string> getMessageByName,
        Func<int, Uri> getLinkByValue)
    {
        string name = member.Identifier.ValueText;
        if (name == "Void" || name == "Unknown")
        {
            return new CompilerCodeOutput(name, 0, DiagnosticSeverity.Hidden, "", null);
        }
        else
        {
            int value = int.Parse(member.EqualsValue?.Value.GetText().ToString() ?? "0");
            return new CompilerCodeOutput(
                name[4..],
                value,
                ParseSeverity(name.Substring(0, 3)),
                getMessageByName(name),
                getLinkByValue(value));
        }
    }
    
    private CompilerCodeOutput(string name, int value, DiagnosticSeverity severity, string message, Uri? link)
    {
        Name = name;
        Value = value;
        Severity = severity;
        Message = message;
        Link = link;
    }
    
    public string Name { get; }
    public int Value { get; }
    
    public string Code => $"CS{Value:D4}";
    public DiagnosticSeverity Severity { get; }
    public string Message { get; }
    public Uri? Link { get; set; }
    
    private static DiagnosticSeverity ParseSeverity(string severity)
    {
        return severity switch
        {
            "HDN" => DiagnosticSeverity.Hidden,
            "INF" => DiagnosticSeverity.Info,
            "WRN" => DiagnosticSeverity.Warning,
            "ERR" => DiagnosticSeverity.Error,
            "FTL" => DiagnosticSeverity.Error,
            _ => DiagnosticSeverity.Hidden,
        };
    }
}
