namespace ScriptLauncher.Services.Interfaces;

/// <summary>
/// Service for managing windows.
/// </summary>
public interface IWindowService
{
    /// <summary>
    /// Shows the popup window for a script.
    /// </summary>
    Task ShowPopupAsync(string scriptId);

    /// <summary>
    /// Hides the popup window.
    /// </summary>
    void HidePopup();

    /// <summary>
    /// Shows the main/settings window.
    /// </summary>
    void ShowMainWindow();

    /// <summary>
    /// Hides the main window to system tray.
    /// </summary>
    void HideToTray();

    /// <summary>
    /// Exits the application.
    /// </summary>
    void Exit();
}
