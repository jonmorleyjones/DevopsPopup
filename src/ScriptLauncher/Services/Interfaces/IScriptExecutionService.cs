using ScriptLauncher.Models;

namespace ScriptLauncher.Services.Interfaces;

/// <summary>
/// Service for executing scripts.
/// </summary>
public interface IScriptExecutionService
{
    /// <summary>
    /// Executes a script with the given parameters.
    /// </summary>
    Task<ScriptExecutionResult> ExecuteAsync(
        ScriptDefinition script,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
