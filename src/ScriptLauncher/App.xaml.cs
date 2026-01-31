using ScriptLauncher.Services.Interfaces;
using ScriptLauncher.Views;

namespace ScriptLauncher;

public partial class App : Application
{
    private readonly IHotkeyService _hotkeyService;
    private readonly IWindowService _windowService;

    public App(
        IHotkeyService hotkeyService,
        IWindowService windowService,
        MainPage mainPage)
    {
        InitializeComponent();

        _hotkeyService = hotkeyService;
        _windowService = windowService;

        // Subscribe to hotkey events
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;

        // Set the main page
        MainPage = new NavigationPage(mainPage);
    }

    private void OnHotkeyPressed(object? sender, HotkeyEventArgs e)
    {
        // Show the popup for the triggered script
        _windowService.ShowPopupAsync(e.ScriptId);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
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
        _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
        _hotkeyService.Dispose();
        base.CleanUp();
    }
}
