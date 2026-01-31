using ScriptLauncher.Services.Interfaces;
using ScriptLauncher.Views;

namespace ScriptLauncher.Services;

/// <summary>
/// Service for managing windows and popups.
/// </summary>
public class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IScriptConfigurationService _configService;
    private Window? _popupWindow;

    public WindowService(
        IServiceProvider serviceProvider,
        IScriptConfigurationService configService)
    {
        _serviceProvider = serviceProvider;
        _configService = configService;
    }

    public async Task ShowPopupAsync(string scriptId)
    {
        var script = _configService.GetScript(scriptId);
        if (script == null) return;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Create the popup page
            var popupPage = _serviceProvider.GetRequiredService<ScriptPopupPage>();
            popupPage.Initialize(script);

            // Create a new window for the popup
            _popupWindow = new Window(popupPage)
            {
                Title = script.UI.Title ?? script.Name,
                Width = script.UI.Width ?? 500,
                Height = 450
            };

            // Center the window on screen
            // Note: Positioning logic is platform-specific
            // This will be handled in platform-specific code if needed

            Application.Current?.OpenWindow(_popupWindow);
        });
    }

    public void HidePopup()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_popupWindow != null)
            {
                Application.Current?.CloseWindow(_popupWindow);
                _popupWindow = null;
            }
        });
    }

    public void ShowMainWindow()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var mainWindow = Application.Current?.Windows.FirstOrDefault();
            if (mainWindow != null)
            {
                // Platform-specific show logic would go here
                // For Windows, use H.NotifyIcon extensions
            }
        });
    }

    public void HideToTray()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var mainWindow = Application.Current?.Windows.FirstOrDefault();
            if (mainWindow != null)
            {
                // Platform-specific hide logic would go here
                // For Windows, use H.NotifyIcon extensions
#if WINDOWS
                // mainWindow.Hide(); // Using H.NotifyIcon extension
#endif
            }
        });
    }

    public void Exit()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current?.Quit();
        });
    }
}
