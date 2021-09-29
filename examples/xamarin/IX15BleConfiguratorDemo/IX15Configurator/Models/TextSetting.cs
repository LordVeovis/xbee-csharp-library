using IX15Configurator.Utils.Validators;

namespace IX15Configurator.Models
{
    class TextSetting : AbstractSetting
    {
        /// <summary>
        /// Class constructor. Instantiates a new <c>TextSetting</c> with
        /// the given parameters.
        /// </summary>
        /// <param name="name">The setting name.</param>
        /// <param name="command">The setting command.</param>
        /// <param name="defaultValue">The setting default value.</param>
        /// <param name="validations">List of validators for the setting.</param>
        public TextSetting(string name, string command, string defaultValue, params IValidationRule[] validations) : base(name, command, defaultValue, validations)
        {

        }
    }
}
