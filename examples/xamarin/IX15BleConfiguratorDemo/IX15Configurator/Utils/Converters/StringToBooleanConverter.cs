using System;
using System.Globalization;
using Xamarin.Forms;

namespace IX15Configurator.Utils.Converters
{
    class StringToBooleanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException(String.Format("{0}: The target must be a bool"));

            string stringValue = (string)value;
            return bool.Parse(stringValue.Trim());
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException(String.Format("{0}: The target must be a string"));

            bool booleanValue = (bool)value;
            return booleanValue.ToString().ToLower();
        }
    }
}
