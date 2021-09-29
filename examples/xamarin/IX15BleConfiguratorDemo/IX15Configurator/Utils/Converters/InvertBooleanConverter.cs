using System;
using System.Globalization;
using Xamarin.Forms;

namespace IX15Configurator.Utils.Converters
{
    class InvertBooleanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException(String.Format("{0}: The target must be a boolean", GetType().Name));

            bool boolValue = (bool)value;
            return !boolValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
