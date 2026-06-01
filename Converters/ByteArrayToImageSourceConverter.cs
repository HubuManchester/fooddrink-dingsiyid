using System.Globalization;

namespace CampusEats.Converters;

public class ByteArrayToImageSourceConverter : IMultiValueConverter
{
    public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        // values[0] 是 ImageData (byte[]), values[1] 是 ImageName (string)
        if (values?.Length >= 2)
        {
            // 优先使用 ImageData
            if (values[0] is byte[] imageData && imageData.Length > 0)
            {
                return ImageSource.FromStream(() => new MemoryStream(imageData));
            }
            
            // 否则使用 ImageName
            if (values[1] is string imageName && !string.IsNullOrEmpty(imageName))
            {
                return ImageSource.FromFile(imageName);
            }
        }
        
        return null;
    }

    public object?[]? ConvertBack(object? value, Type[]? targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
