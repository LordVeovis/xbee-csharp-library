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

using Xamarin.Forms;

namespace InterfacesConfigurationSample.Models
{
    public class SettingContainerTemplateSelector : DataTemplateSelector
    {
        // Properties.
        /// <summary>
        /// Data template corresponding to a text setting.
        /// </summary>
        public DataTemplate TextTemplate { get; set; }

        /// <summary>
        /// Data template corresponding to a combo setting.
        /// </summary>
        public DataTemplate ComboTemplate { get; set; }

        /// <summary>
        /// Data template corresponding to a boolean setting.
        /// </summary>
        public DataTemplate BooleanTemplate { get; set; }

        /// <summary>
        /// Data template corresponding to setting without controls.
        /// </summary>
        public DataTemplate NoControlTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            switch (((AbstractSetting)item).Type)
            {
                case SettingType.TEXT:
                    return TextTemplate;
                case SettingType.COMBO:
                    return ComboTemplate;
                case SettingType.BOOLEAN:
                    return BooleanTemplate;
                default:
                    return NoControlTemplate;
            }
        }
    }
}
