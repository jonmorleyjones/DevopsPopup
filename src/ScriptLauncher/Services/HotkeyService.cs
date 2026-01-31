using ScriptLauncher.Models;
using ScriptLauncher.Services.Interfaces;
using SharpHook;
using SharpHook.Native;
using Microsoft.Extensions.Logging;

namespace ScriptLauncher.Services;

/// <summary>
/// Cross-platform global hotkey service using SharpHook.
/// </summary>
public class HotkeyService : IHotkeyService
{
    private readonly ILogger<HotkeyService> _logger;
    private readonly Dictionary<string, HotkeyDefinition> _registeredHotkeys = new();
    private TaskPoolGlobalHook? _hook;
    private bool _isRunning;
    private bool _disposed;

    // Track modifier key states
    private bool _ctrlPressed;
    private bool _shiftPressed;
    private bool _altPressed;
    private bool _winPressed;

    public event EventHandler<HotkeyEventArgs>? HotkeyPressed;

    public HotkeyService(ILogger<HotkeyService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync()
    {
        if (_isRunning) return;

        _hook = new TaskPoolGlobalHook();
        _hook.KeyPressed += OnKeyPressed;
        _hook.KeyReleased += OnKeyReleased;

        _isRunning = true;
        _logger.LogInformation("Hotkey service starting...");

        // Run the hook asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                await _hook.RunAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hotkey hook crashed");
            }
        });

        // Give the hook time to initialize
        await Task.Delay(100);
        _logger.LogInformation("Hotkey service started");
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _hook?.Dispose();
        _hook = null;
        _isRunning = false;
        _logger.LogInformation("Hotkey service stopped");
    }

    public bool RegisterHotkey(string scriptId, HotkeyDefinition hotkey)
    {
        if (_registeredHotkeys.ContainsKey(scriptId))
        {
            _logger.LogWarning("Hotkey already registered for script: {ScriptId}", scriptId);
            return false;
        }

        _registeredHotkeys[scriptId] = hotkey;
        _logger.LogInformation("Registered hotkey {Hotkey} for script: {ScriptId}",
            hotkey.DisplayString, scriptId);
        return true;
    }

    public bool UnregisterHotkey(string scriptId)
    {
        if (_registeredHotkeys.Remove(scriptId))
        {
            _logger.LogInformation("Unregistered hotkey for script: {ScriptId}", scriptId);
            return true;
        }
        return false;
    }

    public void UnregisterAll()
    {
        _registeredHotkeys.Clear();
        _logger.LogInformation("Unregistered all hotkeys");
    }

    public IReadOnlyDictionary<string, HotkeyDefinition> GetRegisteredHotkeys()
    {
        return _registeredHotkeys.AsReadOnly();
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        // Update modifier states
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcLeftControl:
            case KeyCode.VcRightControl:
                _ctrlPressed = true;
                return;
            case KeyCode.VcLeftShift:
            case KeyCode.VcRightShift:
                _shiftPressed = true;
                return;
            case KeyCode.VcLeftAlt:
            case KeyCode.VcRightAlt:
                _altPressed = true;
                return;
            case KeyCode.VcLeftMeta:
            case KeyCode.VcRightMeta:
                _winPressed = true;
                return;
        }

        // Check if any registered hotkey matches
        var pressedKey = KeyCodeToString(e.Data.KeyCode);
        if (string.IsNullOrEmpty(pressedKey)) return;

        foreach (var (scriptId, hotkey) in _registeredHotkeys)
        {
            if (IsHotkeyMatch(hotkey, pressedKey))
            {
                _logger.LogDebug("Hotkey matched for script: {ScriptId}", scriptId);

                // Fire event on UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    HotkeyPressed?.Invoke(this, new HotkeyEventArgs
                    {
                        ScriptId = scriptId,
                        Hotkey = hotkey
                    });
                });

                break;
            }
        }
    }

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        // Update modifier states
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcLeftControl:
            case KeyCode.VcRightControl:
                _ctrlPressed = false;
                break;
            case KeyCode.VcLeftShift:
            case KeyCode.VcRightShift:
                _shiftPressed = false;
                break;
            case KeyCode.VcLeftAlt:
            case KeyCode.VcRightAlt:
                _altPressed = false;
                break;
            case KeyCode.VcLeftMeta:
            case KeyCode.VcRightMeta:
                _winPressed = false;
                break;
        }
    }

    private bool IsHotkeyMatch(HotkeyDefinition hotkey, string pressedKey)
    {
        // Check if key matches (case insensitive)
        if (!pressedKey.Equals(hotkey.Key, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check modifier states match
        if (hotkey.HasCtrl != _ctrlPressed) return false;
        if (hotkey.HasShift != _shiftPressed) return false;
        if (hotkey.HasAlt != _altPressed) return false;
        if (hotkey.HasWin != _winPressed) return false;

        return true;
    }

    private static string? KeyCodeToString(KeyCode keyCode)
    {
        // Convert KeyCode to string representation
        var name = keyCode.ToString();

        // Handle letter keys (VcA, VcB, etc.)
        if (name.StartsWith("Vc") && name.Length == 3)
        {
            return name[2].ToString();
        }

        // Handle number keys
        if (name.StartsWith("Vc") && name.Length == 4 && name[2] >= '0' && name[2] <= '9')
        {
            return name[2].ToString();
        }

        // Handle function keys
        if (name.StartsWith("VcF") && name.Length >= 3)
        {
            return name[2..];
        }

        // Handle special keys
        return keyCode switch
        {
            KeyCode.VcSpace => "Space",
            KeyCode.VcEnter => "Enter",
            KeyCode.VcEscape => "Escape",
            KeyCode.VcTab => "Tab",
            KeyCode.VcBackspace => "Backspace",
            KeyCode.VcDelete => "Delete",
            KeyCode.VcInsert => "Insert",
            KeyCode.VcHome => "Home",
            KeyCode.VcEnd => "End",
            KeyCode.VcPageUp => "PageUp",
            KeyCode.VcPageDown => "PageDown",
            KeyCode.VcUp => "Up",
            KeyCode.VcDown => "Down",
            KeyCode.VcLeft => "Left",
            KeyCode.VcRight => "Right",
            _ => null
        };
    }

    public void Dispose()
    {
        if (_disposed) return;

        Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
