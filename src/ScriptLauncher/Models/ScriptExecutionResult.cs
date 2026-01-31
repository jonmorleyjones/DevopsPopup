namespace ScriptLauncher.Models;

/// <summary>
/// Result of a script execution.
/// </summary>
public class ScriptExecutionResult
{
    public string ScriptId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int? ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string ErrorOutput { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public Dictionary<string, object>? ParsedOutput { get; set; }
}
