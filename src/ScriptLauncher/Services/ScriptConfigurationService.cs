using System.Text.Json;
using ScriptLauncher.Models;
using ScriptLauncher.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScriptLauncher.Services;

/// <summary>
/// Loads and manages script configurations from JSON files.
/// </summary>
public class ScriptConfigurationService : IScriptConfigurationService
{
    private readonly ILogger<ScriptConfigurationService> _logger;
    private readonly string _scriptsDirectory;
    private readonly List<ScriptDefinition> _scripts = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public IReadOnlyList<ScriptDefinition> Scripts => _scripts.AsReadOnly();

    public event EventHandler? ScriptsReloaded;

    public ScriptConfigurationService(ILogger<ScriptConfigurationService> logger)
    {
        _logger = logger;

        // Default scripts directory is in app data
        _scriptsDirectory = Path.Combine(
            FileSystem.AppDataDirectory,
            "Configuration",
            "scripts");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    public async Task LoadScriptsAsync()
    {
        _scripts.Clear();

        // Ensure directory exists
        if (!Directory.Exists(_scriptsDirectory))
        {
            _logger.LogWarning("Scripts directory not found, creating: {Directory}", _scriptsDirectory);
            Directory.CreateDirectory(_scriptsDirectory);

            // Copy default scripts if available
            await CopyDefaultScriptsAsync();
        }

        var jsonFiles = Directory.GetFiles(_scriptsDirectory, "*.json");
        _logger.LogInformation("Found {Count} script configuration files", jsonFiles.Length);

        foreach (var file in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var script = JsonSerializer.Deserialize<ScriptDefinition>(json, _jsonOptions);

                if (script != null)
                {
                    _scripts.Add(script);
                    _logger.LogInformation("Loaded script: {Name} ({Id})", script.Name, script.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load script configuration: {File}", file);
            }
        }

        ScriptsReloaded?.Invoke(this, EventArgs.Empty);
    }

    public ScriptDefinition? GetScript(string scriptId)
    {
        return _scripts.FirstOrDefault(s =>
            s.Id.Equals(scriptId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task ReloadScriptAsync(string scriptId)
    {
        var file = Path.Combine(_scriptsDirectory, $"{scriptId}.json");

        if (!File.Exists(file))
        {
            _logger.LogWarning("Script file not found: {File}", file);
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(file);
            var script = JsonSerializer.Deserialize<ScriptDefinition>(json, _jsonOptions);

            if (script != null)
            {
                // Remove old version
                _scripts.RemoveAll(s => s.Id == scriptId);
                _scripts.Add(script);
                _logger.LogInformation("Reloaded script: {Name}", script.Name);

                ScriptsReloaded?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload script: {ScriptId}", scriptId);
        }
    }

    private async Task CopyDefaultScriptsAsync()
    {
        // Try to copy from bundled resources
        try
        {
            var assembly = typeof(ScriptConfigurationService).Assembly;
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(n => n.Contains("Configuration.scripts") && n.EndsWith(".json"));

            foreach (var resourceName in resourceNames)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                var fileName = resourceName.Split('.')[^2] + ".json";
                var destPath = Path.Combine(_scriptsDirectory, fileName);

                using var fileStream = File.Create(destPath);
                await stream.CopyToAsync(fileStream);

                _logger.LogInformation("Copied default script: {FileName}", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not copy default scripts");
        }
    }
}
