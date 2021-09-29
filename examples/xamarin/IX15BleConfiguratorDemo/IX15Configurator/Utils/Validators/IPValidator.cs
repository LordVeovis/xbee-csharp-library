using System.Text.RegularExpressions;

namespace IX15Configurator.Utils.Validators
{
    class IPValidator : IValidationRule
    {
        // Constants.
        private const string IP_PATTERN = @"^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\/[0-9]{1,2}$";

        // Properties.
        /// <inheritdoc/>
        public string Description => "IP must match syntax 'XXX.XXX.XXX.XXX/YY'";

        /// <inheritdoc/>
        public bool Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var regex = new Regex(IP_PATTERN, RegexOptions.IgnoreCase);
            return regex.IsMatch(value);
        }
    }
}
