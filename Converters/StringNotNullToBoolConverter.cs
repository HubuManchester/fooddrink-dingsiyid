using System.Globalization;

namespace CampusEats.Converters;

public class StringNotNullToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string strValue)
        {
            return !string.IsNullOrWhiteSpace(strValue);
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
