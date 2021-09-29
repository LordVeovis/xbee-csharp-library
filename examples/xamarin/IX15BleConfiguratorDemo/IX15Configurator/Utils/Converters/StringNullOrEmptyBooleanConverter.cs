using System;
using System.Globalization;
using Xamarin.Forms;

namespace IX15Configurator.Utils.Converters
{
    public class StringNullOrEmptyBooleanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = (string)value;
            return !string.IsNullOrWhiteSpace(stringValue);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
