namespace IX15Configurator.Utils.Validators
{
    public interface IValidationRule
    {
        // Properties.
        /// <summary>
        /// The validation error description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Validates the given value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns><c>true</c> if the value is valid, <c>false</c> otherwise.</returns>
        bool Validate(string value);
    }
}
