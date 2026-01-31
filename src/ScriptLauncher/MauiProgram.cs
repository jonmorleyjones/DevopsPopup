using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ScriptLauncher.Services;
using ScriptLauncher.Services.Interfaces;
using ScriptLauncher.ViewModels;
using ScriptLauncher.Views;

namespace ScriptLauncher;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register Services
        builder.Services.AddSingleton<IScriptConfigurationService, ScriptConfigurationService>();
        builder.Services.AddSingleton<IScriptExecutionService, ScriptExecutionService>();
        builder.Services.AddSingleton<IHotkeyService, HotkeyService>();
        builder.Services.AddSingleton<IDynamicUIService, DynamicUIService>();
        builder.Services.AddSingleton<IWindowService, WindowService>();

        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<ScriptPopupViewModel>();

        // Register Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ScriptPopupPage>();

        return builder.Build();
    }
}
