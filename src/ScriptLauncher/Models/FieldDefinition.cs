using System.Text.Json.Serialization;

namespace ScriptLauncher.Models;

/// <summary>
/// Defines a single input field in the script's UI.
/// </summary>
public class FieldDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FieldType Type { get; set; } = FieldType.Text;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("default")]
    public object? Default { get; set; }

    [JsonPropertyName("autofocus")]
    public bool Autofocus { get; set; }

    [JsonPropertyName("rows")]
    public int? Rows { get; set; }

    [JsonPropertyName("options")]
    public List<OptionDefinition>? Options { get; set; }

    [JsonPropertyName("suggestions")]
    public List<string>? Suggestions { get; set; }

    [JsonPropertyName("validation")]
    public ValidationDefinition? Validation { get; set; }

    [JsonPropertyName("min")]
    public double? Min { get; set; }

    [JsonPropertyName("max")]
    public double? Max { get; set; }

    [JsonPropertyName("step")]
    public double? Step { get; set; }
}

/// <summary>
/// Defines an option for dropdown, radio, or checkbox group fields.
/// </summary>
public class OptionDefinition
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Validation rules for a field.
/// </summary>
public class ValidationDefinition
{
    [JsonPropertyName("minLength")]
    public int? MinLength { get; set; }

    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; set; }

    [JsonPropertyName("pattern")]
    public string? Pattern { get; set; }

    [JsonPropertyName("patternMessage")]
    public string? PatternMessage { get; set; }

    [JsonPropertyName("min")]
    public double? Min { get; set; }

    [JsonPropertyName("max")]
    public double? Max { get; set; }
}
