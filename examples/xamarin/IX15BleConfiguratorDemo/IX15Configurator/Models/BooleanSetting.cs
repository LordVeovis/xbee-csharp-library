namespace IX15Configurator.Models
{
    class BooleanSetting : AbstractSetting
    {
        /// <summary>
        /// Class constructor. Instantiates a new <c>BooleanSetting</c> with
        /// the given parameters.
        /// </summary>
        /// <param name="name">The setting name.</param>
        /// <param name="command">The setting command.</param>
        /// <param name="defaultValue">The setting default value.</param>
        public BooleanSetting(string name, string command, string defaultValue) : base(name, command, defaultValue, null)
        {
            
        }
    }
}
