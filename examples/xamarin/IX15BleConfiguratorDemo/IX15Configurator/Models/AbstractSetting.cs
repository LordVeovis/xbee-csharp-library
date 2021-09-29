using IX15Configurator.Utils.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace IX15Configurator.Models
{
    public abstract class AbstractSetting : INotifyPropertyChanged
    {
        // Variables.
        public event PropertyChangedEventHandler PropertyChanged;

        protected string value;
        protected string defaultValue;
        protected string name;
        protected string command;

        protected bool hasChanged = false;

        // Properties.
        // Collection of validation rules to apply.
        public List<IValidationRule> Validations { get; } = new List<IValidationRule>();

        /// <summary>
        /// The setting name.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// The setting command.
        /// </summary>
        public string Command
        {
            get { return command; }
        }

        /// <summary>
        /// The setting default value.
        /// </summary>
        public string DefaultValue
        {
            get { return defaultValue; }
        }

        /// <summary>
        /// Returns whether the value is valid or not.
        /// </summary>
        public bool IsValid
        {
            get { return Validations.TrueForAll(v => v.Validate(Value)); }
        }

        /// <summary>
        /// The setting value.
        /// </summary>
        public string Value
        {
            get { return value; }
            set
            {
                if (this.value.Equals(value))
                    return;

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
            get { return hasChanged; }
            set
            {
                hasChanged = value;
                RaisePropertyChangedEvent(nameof(HasChanged));
            }
        }

        // Validation descriptions aggregator
        public string ValidationDescriptions
        {
            get { return string.Join(Environment.NewLine, Validations.Select(v => v.Description)); }
        }

        /// <summary>
        /// Class constructor. Instantiates a new <c>AbstractSetting</c> with the
        /// given parameters.
        /// </summary>
        /// <param name="name">The setting name.</param>
        /// <param name="command">The setting command.</param>
        /// <param name="defaultValue">The setting default value.</param>
        /// <param name="validations">List of validators for the setting.</param>
        public AbstractSetting(string name, string command, string defaultValue, params IValidationRule[] validations)
        {
            this.name = name;
            this.command = command;
            this.defaultValue = defaultValue;
            
            if (validations != null)
            {
                foreach (var val in validations)
                    Validations.Add(val);
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
