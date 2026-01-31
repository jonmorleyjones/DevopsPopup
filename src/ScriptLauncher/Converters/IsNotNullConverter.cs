using System.Globalization;

namespace ScriptLauncher.Converters;

/// <summary>
/// Converts a value to a boolean indicating whether it is not null or empty.
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => false,
            string s => !string.IsNullOrEmpty(s),
            _ => true
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
