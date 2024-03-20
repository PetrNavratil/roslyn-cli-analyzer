namespace AnalyzerDiagnosticInfo.Output;

public class AuditOutput
{
    public string Slug { get; set; } = string.Empty;
    public int Value { get; set; }
    public float Score { get; set; }
    public AuditDetail Details { get; set; }
}

public class AuditDetail
{
    public IEnumerable<AuditDetailIssue> Issues { get; set; } = Array.Empty<AuditDetailIssue>();
}

public class AuditDetailIssue
{
    public string Message { get; set; } = string.Empty;
    public AuditDetailIssueSeverity Severity { get; set; }
    
    public AuditDetailIssueSource Source { get; set; }
    
}

public enum AuditDetailIssueSeverity
{
    Info,
    Warning,
    Error
}

public class AuditDetailIssueSource
{
    public string File { get; set; } = string.Empty;
    public AuditDetailIssueLocationPosition Position { get; set; }
}

public class AuditDetailIssueLocationPosition
{
    public int StartLine { get; set; }
    public int StartColumn { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
}
