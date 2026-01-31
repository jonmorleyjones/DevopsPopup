using ScriptLauncher.Models;

namespace ScriptLauncher.Services.Interfaces;

/// <summary>
/// Service for loading and managing script configurations.
/// </summary>
public interface IScriptConfigurationService
{
    /// <summary>
    /// Gets all loaded script definitions.
    /// </summary>
    IReadOnlyList<ScriptDefinition> Scripts { get; }

    /// <summary>
    /// Loads all script configurations from the configuration directory.
    /// </summary>
    Task LoadScriptsAsync();

    /// <summary>
    /// Gets a script by its ID.
    /// </summary>
    ScriptDefinition? GetScript(string scriptId);

    /// <summary>
    /// Reloads a specific script configuration.
    /// </summary>
    Task ReloadScriptAsync(string scriptId);

    /// <summary>
    /// Fired when scripts are reloaded.
    /// </summary>
    event EventHandler? ScriptsReloaded;
}
