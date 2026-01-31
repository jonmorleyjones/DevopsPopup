using CommunityToolkit.Mvvm.ComponentModel;

namespace ScriptLauncher.ViewModels;

/// <summary>
/// Base class for all ViewModels.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _title;
}
