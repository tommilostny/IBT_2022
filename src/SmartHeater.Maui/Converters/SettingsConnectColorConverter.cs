using System.Globalization;

namespace SmartHeater.Maui.Converters;

public class SettingsConnectColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool?)value switch
        {
            null => Color.FromRgb(255, 255, 0),
            true => Color.FromRgb(0, 255, 0),
            false => Color.FromRgb(255, 0, 0)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
