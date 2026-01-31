using System.Text.Json.Serialization;

namespace ScriptLauncher.Models;

/// <summary>
/// Defines a keyboard hotkey combination.
/// </summary>
public class HotkeyDefinition
{
    [JsonPropertyName("modifiers")]
    public List<string> Modifiers { get; set; } = new();

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets a display-friendly string representation of the hotkey.
    /// </summary>
    public string DisplayString => string.Join("+", Modifiers.Concat(new[] { Key }));

    public bool HasCtrl => Modifiers.Any(m =>
        m.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
        m.Equals("Control", StringComparison.OrdinalIgnoreCase));

    public bool HasShift => Modifiers.Any(m =>
        m.Equals("Shift", StringComparison.OrdinalIgnoreCase));

    public bool HasAlt => Modifiers.Any(m =>
        m.Equals("Alt", StringComparison.OrdinalIgnoreCase));

    public bool HasWin => Modifiers.Any(m =>
        m.Equals("Win", StringComparison.OrdinalIgnoreCase) ||
        m.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
        m.Equals("Cmd", StringComparison.OrdinalIgnoreCase) ||
        m.Equals("Command", StringComparison.OrdinalIgnoreCase));
}
