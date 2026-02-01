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
        try
        {
            System.Diagnostics.Debug.WriteLine("Starting MauiProgram.CreateMauiApp");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    // Default system fonts will be used
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            System.Diagnostics.Debug.WriteLine("Registering services...");

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

            System.Diagnostics.Debug.WriteLine("Building app...");
            var app = builder.Build();
            System.Diagnostics.Debug.WriteLine("App built successfully");
            return app;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FATAL ERROR in CreateMauiApp: {ex}");
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "scriptlauncher_error.txt"), ex.ToString());
            throw;
        }
    }
}
