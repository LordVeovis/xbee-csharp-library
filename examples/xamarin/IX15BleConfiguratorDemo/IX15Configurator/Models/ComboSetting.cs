using System.Collections.Generic;

namespace IX15Configurator.Models
{
    class ComboSetting : AbstractSetting
    {
        // Variables.
        private Dictionary<string, string> availableValues;

        // Properties
        /// <summary>
        /// Gets the list of available values.
        /// </summary>
        public IList<string> AvailableValues
        {
            get
            {
                return new List<string>(this.availableValues.Keys);
            }
        }

        /// <summary>
        /// Gets the list of display values.
        /// </summary>
        public IList<string> DisplayValues
        {
            get
            {
                return new List<string>(this.availableValues.Values);
            }
        }

        /// <summary>
        /// The setting display value.
        /// </summary>
        public string DisplayValue
        {
            get
            {
                return availableValues[Value];
            }
        }

        /// <summary>
        /// The setting value.
        /// </summary>
        public new string Value
        {
            get { return base.Value; }
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
        /// <param name="command">The setting command.</param>
        /// <param name="defaultValue">The setting default value.</param>
        /// <param name="availableValues">Dictionary of available values with their display value for the setting.</param>
        public ComboSetting(string name, string command, string defaultValue, Dictionary<string, string> availableValues) : base(name, command, defaultValue, null)
        {
            this.availableValues = availableValues;
        }
    }
}
