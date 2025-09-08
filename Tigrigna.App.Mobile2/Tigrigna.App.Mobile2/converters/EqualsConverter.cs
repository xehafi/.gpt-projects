using System.Globalization;

namespace Tigrigna.App.Mobile2.Converters;

public class EqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value?.ToString() ?? "") == (parameter?.ToString() ?? "");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
