using System.Globalization;

namespace CampusEats.Converters;

public class BoolToFavoriteConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isFavorite && isFavorite ? "❤️" : "🤍";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
