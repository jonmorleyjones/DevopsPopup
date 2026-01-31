using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScriptLauncher.Models;
using ScriptLauncher.Services.Interfaces;

namespace ScriptLauncher.ViewModels;

/// <summary>
/// ViewModel for the script popup page.
/// </summary>
public partial class ScriptPopupViewModel : BaseViewModel
{
    private readonly IScriptExecutionService _executionService;
    private readonly IWindowService _windowService;

    [ObservableProperty]
    private ScriptDefinition? _script;

    [ObservableProperty]
    private ObservableCollection<FieldViewModel> _fields = new();

    [ObservableProperty]
    private bool _isExecuting;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private bool _showStatus;

    public string SubmitButtonText => Script?.UI.Buttons?.Submit?.Label ?? "Execute";
    public string CancelButtonText => Script?.UI.Buttons?.Cancel?.Label ?? "Cancel";

    public ScriptPopupViewModel(
        IScriptExecutionService executionService,
        IWindowService windowService)
    {
        _executionService = executionService;
        _windowService = windowService;
    }

    /// <summary>
    /// Initializes the popup with a script definition.
    /// </summary>
    public void Initialize(ScriptDefinition script)
    {
        Script = script;
        Title = script.UI.Title ?? script.Name;

        Fields.Clear();
        foreach (var fieldDef in script.UI.Fields)
        {
            var fieldVm = new FieldViewModel(fieldDef);
            Fields.Add(fieldVm);
        }

        StatusMessage = null;
        ShowStatus = false;
        IsExecuting = false;

        OnPropertyChanged(nameof(SubmitButtonText));
        OnPropertyChanged(nameof(CancelButtonText));
    }

    /// <summary>
    /// Gets a field by ID.
    /// </summary>
    public FieldViewModel? GetField(string fieldId)
    {
        return Fields.FirstOrDefault(f => f.Id == fieldId);
    }

    /// <summary>
    /// Checks if the form can be submitted.
    /// </summary>
    public bool CanExecute => !IsExecuting;

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task ExecuteAsync()
    {
        if (Script == null) return;

        // Validate all fields
        var isValid = true;
        foreach (var field in Fields)
        {
            var errors = field.Validate();
            if (errors.Count > 0) isValid = false;
        }

        if (!isValid)
        {
            StatusMessage = "Please fix the validation errors";
            IsSuccess = false;
            ShowStatus = true;
            return;
        }

        IsExecuting = true;
        StatusMessage = "Executing...";
        ShowStatus = true;
        IsSuccess = false;

        try
        {
            // Build parameters dictionary
            var parameters = Fields.ToDictionary(
                f => f.Id,
                f => f.Value
            );

            var result = await _executionService.ExecuteAsync(Script, parameters);

            if (result.Success)
            {
                StatusMessage = Script.SuccessMessage ?? "Completed successfully!";
                IsSuccess = true;

                // Handle success action
                await HandleSuccessActionAsync(result);

                // Clear form after delay
                await Task.Delay(1500);
                ClearForm();
            }
            else
            {
                var errorMsg = result.ErrorMessage ?? result.ErrorOutput;
                if (string.IsNullOrWhiteSpace(errorMsg))
                {
                    errorMsg = $"Script failed with exit code {result.ExitCode}";
                }
                StatusMessage = $"Error: {errorMsg}";
                IsSuccess = false;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsExecuting = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _windowService.HidePopup();
    }

    private void ClearForm()
    {
        foreach (var field in Fields)
        {
            field.Clear();
        }

        StatusMessage = null;
        ShowStatus = false;

        // Focus first field
        // (This would need to be handled by the View)
    }

    private async Task HandleSuccessActionAsync(ScriptExecutionResult result)
    {
        if (Script?.OnSuccess == null) return;

        switch (Script.OnSuccess.Action.ToLowerInvariant())
        {
            case "openurl":
                if (!string.IsNullOrEmpty(Script.OnSuccess.Pattern))
                {
                    var url = ReplaceTokens(Script.OnSuccess.Pattern, result);
                    try
                    {
                        await Launcher.OpenAsync(new Uri(url));
                    }
                    catch
                    {
                        // Ignore URL open errors
                    }
                }
                break;

            case "copy":
                if (!string.IsNullOrEmpty(Script.OnSuccess.Pattern))
                {
                    var text = ReplaceTokens(Script.OnSuccess.Pattern, result);
                    await Clipboard.SetTextAsync(text);
                }
                break;

            case "close":
                _windowService.HidePopup();
                break;
        }
    }

    private string ReplaceTokens(string pattern, ScriptExecutionResult result)
    {
        var output = pattern;

        // Replace field values
        foreach (var field in Fields)
        {
            output = output.Replace($"{{{field.Id}}}", field.StringValue);
        }

        // Replace parsed output values
        if (result.ParsedOutput != null)
        {
            foreach (var (key, value) in result.ParsedOutput)
            {
                output = output.Replace($"{{output:{key}}}", value?.ToString() ?? "");
            }
        }

        // Replace environment variables
        output = Environment.ExpandEnvironmentVariables(output);

        return output;
    }
}
