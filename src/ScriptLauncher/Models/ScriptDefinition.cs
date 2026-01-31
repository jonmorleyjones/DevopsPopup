using System.Text.Json.Serialization;

namespace ScriptLauncher.Models;

/// <summary>
/// Defines a complete script configuration including UI, hotkey, and execution details.
/// </summary>
public class ScriptDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("hotkey")]
    public HotkeyDefinition? Hotkey { get; set; }

    [JsonPropertyName("script")]
    public ScriptExecutionDefinition Script { get; set; } = new();

    [JsonPropertyName("ui")]
    public ScriptUIDefinition UI { get; set; } = new();

    [JsonPropertyName("successMessage")]
    public string? SuccessMessage { get; set; }

    [JsonPropertyName("onSuccess")]
    public SuccessActionDefinition? OnSuccess { get; set; }
}

/// <summary>
/// Defines how the script should be executed.
/// </summary>
public class ScriptExecutionDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "powershell";

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("command")]
    public string? Command { get; set; }

    [JsonPropertyName("workingDirectory")]
    public string? WorkingDirectory { get; set; }

    [JsonPropertyName("timeout")]
    public int? Timeout { get; set; }

    [JsonPropertyName("runElevated")]
    public bool RunElevated { get; set; }
}

/// <summary>
/// Defines the UI layout for the script popup.
/// </summary>
public class ScriptUIDefinition
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("fields")]
    public List<FieldDefinition> Fields { get; set; } = new();

    [JsonPropertyName("buttons")]
    public ButtonDefinitions? Buttons { get; set; }
}

/// <summary>
/// Defines custom button labels.
/// </summary>
public class ButtonDefinitions
{
    [JsonPropertyName("submit")]
    public ButtonDefinition? Submit { get; set; }

    [JsonPropertyName("cancel")]
    public ButtonDefinition? Cancel { get; set; }
}

/// <summary>
/// Defines a single button.
/// </summary>
public class ButtonDefinition
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}

/// <summary>
/// Defines an action to perform on successful script execution.
/// </summary>
public class SuccessActionDefinition
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("pattern")]
    public string? Pattern { get; set; }
}
