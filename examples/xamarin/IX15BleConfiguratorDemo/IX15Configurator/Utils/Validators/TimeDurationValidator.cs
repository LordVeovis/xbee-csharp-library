using System.Text.RegularExpressions;

namespace IX15Configurator.Utils.Validators
{
    class TimeDurationValidator : IValidationRule
    {
        // Constants.
        private const string TIME_PATTERN = @"^([0-9]+w)?([0-9]+d)?([0-9]+h)?([0-9]+m)?([0-9]+s)?$";

        // Properties.
        /// <inheritdoc/>
        public string Description => "Time must match syntax 'number{w|d|h|m|s}...'";

        /// <inheritdoc/>
        public bool Validate(string value)
        {
            if (value == null)
                return false;

            var regex = new Regex(TIME_PATTERN, RegexOptions.IgnoreCase);
            return regex.IsMatch(value);
        }
    }
}
