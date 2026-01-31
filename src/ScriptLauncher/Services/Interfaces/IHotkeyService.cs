using ScriptLauncher.Models;

namespace ScriptLauncher.Services.Interfaces;

/// <summary>
/// Service for registering and handling global hotkeys.
/// </summary>
public interface IHotkeyService : IDisposable
{
    /// <summary>
    /// Fired when a registered hotkey is pressed.
    /// </summary>
    event EventHandler<HotkeyEventArgs>? HotkeyPressed;

    /// <summary>
    /// Starts listening for hotkeys.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stops listening for hotkeys.
    /// </summary>
    void Stop();

    /// <summary>
    /// Registers a hotkey for a script.
    /// </summary>
    bool RegisterHotkey(string scriptId, HotkeyDefinition hotkey);

    /// <summary>
    /// Unregisters a hotkey for a script.
    /// </summary>
    bool UnregisterHotkey(string scriptId);

    /// <summary>
    /// Unregisters all hotkeys.
    /// </summary>
    void UnregisterAll();

    /// <summary>
    /// Gets all registered hotkeys.
    /// </summary>
    IReadOnlyDictionary<string, HotkeyDefinition> GetRegisteredHotkeys();
}

/// <summary>
/// Event arguments for hotkey events.
/// </summary>
public class HotkeyEventArgs : EventArgs
{
    public string ScriptId { get; init; } = string.Empty;
    public HotkeyDefinition Hotkey { get; init; } = new();
}
