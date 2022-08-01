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

using InterfacesConfigurationSample.Utils.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace InterfacesConfigurationSample.Models
{
    public abstract class AbstractSetting : INotifyPropertyChanged
    {
        // Variables.
        public event PropertyChangedEventHandler PropertyChanged;

        protected string value;
        protected string defaultValue;
        protected string name;

        protected bool hasChanged = false;

        protected SettingType type;

        // Properties.
        // Collection of validation rules to apply.
        public List<IValidationRule> Validations { get; } = new List<IValidationRule>();

        /// <summary>
        /// The setting type.
        /// </summary>
        public SettingType Type => type;

        /// <summary>
        /// The setting name.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// The setting default value.
        /// </summary>
        public string DefaultValue => defaultValue;

        /// <summary>
        /// Returns whether the value is valid or not.
        /// </summary>
        public bool IsValid => Validations.TrueForAll(v => v.Validate(Value));

        /// <summary>
        /// The setting value.
        /// </summary>
        public string Value
        {
            get => value;
            set
            {
                if (this.value.Equals(value))
                {
                    return;
                }

                this.value = value;
                RaisePropertyChangedEvent(nameof(Value));
                RaisePropertyChangedEvent(nameof(IsValid));
                HasChanged = true;
            }
        }

        /// <summary>
        /// Determines whether the setting has changed or not.
        /// </summary>
        public bool HasChanged
        {
            get => hasChanged;
            set
            {
                hasChanged = value;
                RaisePropertyChangedEvent(nameof(HasChanged));
            }
        }

        // Validation descriptions aggregator
        public string ValidationDescriptions => string.Join(Environment.NewLine, Validations.Select(v => v.Description));

        /// <summary>
        /// Class constructor. Instantiates a new <c>AbstractSetting</c> with the
        /// given parameters.
        /// </summary
        /// <param name="type">The setting type.</param>
        /// <param name="name">The setting name.</param>
        /// <param name="defaultValue">The setting default value.</param>
        /// <param name="validations">List of validators for the setting.</param>
        public AbstractSetting(SettingType type, string name, string defaultValue, params IValidationRule[] validations)
        {
            this.type = type;
            this.name = name;
            this.defaultValue = defaultValue;

            if (validations != null)
            {
                foreach (IValidationRule val in validations)
                {
                    Validations.Add(val);
                }
            }

            value = this.defaultValue;
        }

        /// <summary>
        /// Generates and raises a new event indicating that the provided 
        /// property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that has 
        /// changed.</param>
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }
    }
}
