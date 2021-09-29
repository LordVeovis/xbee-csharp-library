using IX15Configurator.Models;
using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace IX15Configurator.Utils.Converters
{
    class IPModeConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = (string)value;
            return DeviceSettings.IP_MODE_VALUES[stringValue.Trim()];
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringDisplayValue = (string)value;
            return DeviceSettings.IP_MODE_VALUES.FirstOrDefault(x => x.Value.Equals(stringDisplayValue.Trim())).Key;
        }
    }
}
