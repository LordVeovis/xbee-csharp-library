using IX15Configurator.Models;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace IX15Configurator.Utils.Converters
{
    public class IPModeStaticBooleanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException(String.Format("{0}: The target must be a boolean", GetType().Name));

            string stringValue = (string)value;
            return DeviceSettings.VALUE_IP_MODE_STATIC.Equals(stringValue);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
