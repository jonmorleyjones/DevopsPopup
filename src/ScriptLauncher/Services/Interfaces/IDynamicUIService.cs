using ScriptLauncher.Models;
using ScriptLauncher.ViewModels;

namespace ScriptLauncher.Services.Interfaces;

/// <summary>
/// Service for building dynamic UI from script field definitions.
/// </summary>
public interface IDynamicUIService
{
    /// <summary>
    /// Builds a form layout from field definitions.
    /// </summary>
    View BuildForm(IEnumerable<FieldDefinition> fields, ScriptPopupViewModel viewModel);

    /// <summary>
    /// Creates a single field control.
    /// </summary>
    View CreateField(FieldDefinition field, FieldViewModel fieldViewModel);
}
