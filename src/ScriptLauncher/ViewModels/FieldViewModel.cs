using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using ScriptLauncher.Models;

namespace ScriptLauncher.ViewModels;

/// <summary>
/// ViewModel for a single form field.
/// </summary>
public partial class FieldViewModel : ObservableObject
{
    private readonly FieldDefinition _definition;

    [ObservableProperty]
    private object? _value;

    [ObservableProperty]
    private string? _validationError;

    [ObservableProperty]
    private bool _hasError;

    public string Id => _definition.Id;
    public FieldType Type => _definition.Type;
    public string Label => _definition.Label;
    public bool Required => _definition.Required;

    /// <summary>
    /// Gets or sets the value as a string.
    /// </summary>
    public string StringValue
    {
        get => Value?.ToString() ?? string.Empty;
        set => Value = value;
    }

    /// <summary>
    /// Gets or sets the value as a boolean.
    /// </summary>
    public bool BoolValue
    {
        get => Value is bool b && b;
        set => Value = value;
    }

    /// <summary>
    /// Gets or sets the value as a double.
    /// </summary>
    public double DoubleValue
    {
        get => Value is double d ? d : (double.TryParse(Value?.ToString(), out var parsed) ? parsed : 0);
        set => Value = value;
    }

    public FieldViewModel(FieldDefinition definition)
    {
        _definition = definition;

        // Set default value
        if (definition.Default != null)
        {
            Value = definition.Default;
        }
    }

    /// <summary>
    /// Validates the field value.
    /// </summary>
    /// <returns>List of validation errors, empty if valid.</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();
        var stringValue = StringValue;

        // Required validation
        if (_definition.Required && string.IsNullOrWhiteSpace(stringValue))
        {
            errors.Add($"{_definition.Label} is required");
        }

        // Skip other validations if empty and not required
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            UpdateValidationState(errors);
            return errors;
        }

        // Length validations
        if (_definition.Validation?.MinLength.HasValue == true &&
            stringValue.Length < _definition.Validation.MinLength.Value)
        {
            errors.Add($"{_definition.Label} must be at least {_definition.Validation.MinLength} characters");
        }

        if (_definition.Validation?.MaxLength.HasValue == true &&
            stringValue.Length > _definition.Validation.MaxLength.Value)
        {
            errors.Add($"{_definition.Label} must be at most {_definition.Validation.MaxLength} characters");
        }

        // Pattern validation
        if (!string.IsNullOrEmpty(_definition.Validation?.Pattern))
        {
            try
            {
                if (!Regex.IsMatch(stringValue, _definition.Validation.Pattern))
                {
                    var message = _definition.Validation.PatternMessage ??
                        $"{_definition.Label} format is invalid";
                    errors.Add(message);
                }
            }
            catch (ArgumentException)
            {
                // Invalid regex pattern - skip validation
            }
        }

        // Numeric validations for number/slider types
        if (_definition.Type is FieldType.Number or FieldType.Slider)
        {
            if (double.TryParse(stringValue, out var numValue))
            {
                if (_definition.Validation?.Min.HasValue == true &&
                    numValue < _definition.Validation.Min.Value)
                {
                    errors.Add($"{_definition.Label} must be at least {_definition.Validation.Min}");
                }

                if (_definition.Validation?.Max.HasValue == true &&
                    numValue > _definition.Validation.Max.Value)
                {
                    errors.Add($"{_definition.Label} must be at most {_definition.Validation.Max}");
                }
            }
            else
            {
                errors.Add($"{_definition.Label} must be a valid number");
            }
        }

        UpdateValidationState(errors);
        return errors;
    }

    private void UpdateValidationState(List<string> errors)
    {
        HasError = errors.Count > 0;
        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : null;
    }

    /// <summary>
    /// Clears the field value and validation state.
    /// </summary>
    public void Clear()
    {
        Value = _definition.Default;
        ValidationError = null;
        HasError = false;
    }
}
