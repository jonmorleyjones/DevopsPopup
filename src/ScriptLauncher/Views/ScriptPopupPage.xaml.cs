using ScriptLauncher.Models;
using ScriptLauncher.Services.Interfaces;
using ScriptLauncher.ViewModels;

namespace ScriptLauncher.Views;

public partial class ScriptPopupPage : ContentPage
{
    private readonly ScriptPopupViewModel _viewModel;
    private readonly IDynamicUIService _dynamicUIService;

    public ScriptPopupPage(
        ScriptPopupViewModel viewModel,
        IDynamicUIService dynamicUIService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _dynamicUIService = dynamicUIService;
        BindingContext = viewModel;
    }

    /// <summary>
    /// Initializes the page with a script definition.
    /// </summary>
    public void Initialize(ScriptDefinition script)
    {
        _viewModel.Initialize(script);
        BuildDynamicUI();
    }

    private void BuildDynamicUI()
    {
        FieldsContainer.Children.Clear();

        if (_viewModel.Script == null) return;

        var form = _dynamicUIService.BuildForm(
            _viewModel.Script.UI.Fields,
            _viewModel);

        FieldsContainer.Children.Add(form);

        // Focus first field
        FocusFirstField();
    }

    private void FocusFirstField()
    {
        // Find the first focusable element
        var firstEntry = FindFirstEntry(FieldsContainer);
        firstEntry?.Focus();
    }

    private Entry? FindFirstEntry(IView view)
    {
        if (view is Entry entry)
            return entry;

        if (view is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                var found = FindFirstEntry(child);
                if (found != null) return found;
            }
        }

        return null;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        FocusFirstField();
    }

    protected override bool OnBackButtonPressed()
    {
        _viewModel.CancelCommand.Execute(null);
        return true;
    }
}
