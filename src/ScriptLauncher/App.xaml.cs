using ScriptLauncher.Services.Interfaces;
using ScriptLauncher.Views;

namespace ScriptLauncher;

public partial class App : Application
{
    private IHotkeyService? _hotkeyService;
    private IWindowService? _windowService;

    public App(
        IHotkeyService hotkeyService,
        IWindowService windowService)
    {
        InitializeComponent();

        _hotkeyService = hotkeyService;
        _windowService = windowService;
    }

    protected override void OnStart()
    {
        base.OnStart();

        // Subscribe to hotkey events after app has started
        if (_hotkeyService != null)
        {
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        }
    }

    private void OnHotkeyPressed(object? sender, HotkeyEventArgs e)
    {
        // Show the popup for the triggered script
        _windowService?.ShowPopupAsync(e.ScriptId);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Resolve MainPage here after resources are loaded
        var mainPage = Handler.MauiContext.Services.GetRequiredService<MainPage>();
        MainPage = new NavigationPage(mainPage);

        var window = base.CreateWindow(activationState);

        window.Title = "Script Launcher";
        window.Width = 600;
        window.Height = 500;

        // Center window on screen
        var displayInfo = DeviceDisplay.MainDisplayInfo;
        window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
        window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;

        return window;
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        // App is going to background - hotkeys should still work
    }

    protected override void OnResume()
    {
        base.OnResume();
    }

    protected override void CleanUp()
    {
        if (_hotkeyService != null)
        {
            _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
            _hotkeyService.Dispose();
        }
        base.CleanUp();
    }
}
