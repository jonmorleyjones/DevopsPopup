using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScriptLauncher.Models;
using ScriptLauncher.Services.Interfaces;

namespace ScriptLauncher.ViewModels;

/// <summary>
/// ViewModel for the main/settings page.
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly IScriptConfigurationService _configService;
    private readonly IHotkeyService _hotkeyService;
    private readonly IWindowService _windowService;

    [ObservableProperty]
    private ObservableCollection<ScriptDefinition> _scripts = new();

    [ObservableProperty]
    private ScriptDefinition? _selectedScript;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isLoading;

    public MainViewModel(
        IScriptConfigurationService configService,
        IHotkeyService hotkeyService,
        IWindowService windowService)
    {
        _configService = configService;
        _hotkeyService = hotkeyService;
        _windowService = windowService;

        Title = "Script Launcher";

        _configService.ScriptsReloaded += OnScriptsReloaded;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading scripts...";

        try
        {
            await _configService.LoadScriptsAsync();
            RefreshScripts();

            // Register hotkeys
            RegisterHotkeys();

            // Start hotkey listener
            await _hotkeyService.StartAsync();

            StatusMessage = $"Loaded {Scripts.Count} script(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnScriptsReloaded(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(RefreshScripts);
    }

    private void RefreshScripts()
    {
        Scripts.Clear();
        foreach (var script in _configService.Scripts)
        {
            Scripts.Add(script);
        }
    }

    private void RegisterHotkeys()
    {
        _hotkeyService.UnregisterAll();

        foreach (var script in _configService.Scripts)
        {
            if (script.Hotkey != null)
            {
                _hotkeyService.RegisterHotkey(script.Id, script.Hotkey);
            }
        }
    }

    [RelayCommand]
    private async Task ReloadScriptsAsync()
    {
        IsLoading = true;
        StatusMessage = "Reloading scripts...";

        try
        {
            await _configService.LoadScriptsAsync();
            RefreshScripts();
            RegisterHotkeys();

            StatusMessage = $"Reloaded {Scripts.Count} script(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowScript(ScriptDefinition? script)
    {
        if (script == null) return;
        _windowService.ShowPopupAsync(script.Id);
    }

    [RelayCommand]
    private void MinimizeToTray()
    {
        _windowService.HideToTray();
    }

    [RelayCommand]
    private void Exit()
    {
        _windowService.Exit();
    }

    [RelayCommand]
    private async Task OpenScriptsFolderAsync()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "Configuration", "scripts");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        try
        {
            await Launcher.OpenAsync(new Uri($"file://{path}"));
        }
        catch
        {
            StatusMessage = $"Scripts folder: {path}";
        }
    }
}
