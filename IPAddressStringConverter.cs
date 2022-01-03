using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace Chat
{
    [ValueConversion(typeof(IPAddress), typeof(string))]
    public class IPAddressStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IPAddress)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IPAddress.Parse((string)value);
        }
    }
}
