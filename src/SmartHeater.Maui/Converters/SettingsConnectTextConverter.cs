using System.Globalization;

namespace SmartHeater.Maui.Converters;

public class SettingsConnectTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool?)value switch
        {
            null => "Connecting...",
            true => "Connected",
            false => "Not connected"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
