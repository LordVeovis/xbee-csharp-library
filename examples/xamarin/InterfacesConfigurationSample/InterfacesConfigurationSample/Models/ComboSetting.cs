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

using System.Collections.Generic;

namespace InterfacesConfigurationSample.Models
{
    internal class ComboSetting : AbstractSetting
    {
        // Properties
        /// <summary>
        /// Gets the table of values.
        /// </summary>
        public Dictionary<string, string> Values { get; }

        // Properties
        /// <summary>
        /// Gets the list of available values.
        /// </summary>
        public IList<string> AvailableValues => new List<string>(Values.Keys);

        /// <summary>
        /// Gets the list of display values.
        /// </summary>
        public IList<string> DisplayValues => new List<string>(Values.Values);

        /// <summary>
        /// The setting display value.
        /// </summary>
        public string DisplayValue => Values[Value];

        /// <summary>
        /// The setting value.
        /// </summary>
        public new string Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                RaisePropertyChangedEvent(nameof(DisplayValue));
            }
        }

        /// <summary>
        /// Class constructor. Instantiates a new <c>ComboSetting</c> with
        /// the given parameters.
        /// </summary>
        /// <param name="name">The setting name.</param>
        /// <param name="defaultValue">The setting default value.</param>
        /// <param name="availableValues">Dictionary of available values with their display value for the setting.</param>
        public ComboSetting(string name, string defaultValue, Dictionary<string, string> availableValues) : base(SettingType.COMBO, name, defaultValue, null)
        {
            this.Values = availableValues;
        }
    }
}
