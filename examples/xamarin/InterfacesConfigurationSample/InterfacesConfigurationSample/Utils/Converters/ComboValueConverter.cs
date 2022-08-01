/*
 * Copyright 2022, Digi International Inc.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using InterfacesConfigurationSample.Models;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace InterfacesConfigurationSample.Utils.Converters
{
    class ComboValueConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = (string)value;
            Picker comboBox = parameter as Picker;
            ComboSetting comboSetting = comboBox.BindingContext as ComboSetting;
            return comboSetting.Values[stringValue.Trim()];
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringDisplayValue = (string)value;
            Picker comboBox = parameter as Picker;
            ComboSetting comboSetting = comboBox.BindingContext as ComboSetting;
            foreach (string keyVar in comboSetting.Values.Keys)
            {
                if (comboSetting.Values[keyVar].Trim().Equals(stringDisplayValue))
                {
                    return keyVar;
                }
            }
            return null;
        }
    }
}
