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

using System.Text.RegularExpressions;

namespace InterfacesConfigurationSample.Utils.Validators
{
    internal class IPValidator : IValidationRule
    {
        // Constants.
        private const string IP_PATTERN = @"^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])$";

        // Properties.
        /// <inheritdoc/>
        public string Description => "IP must match syntax 'XXX.XXX.XXX.XXX'";

        /// <inheritdoc/>
        public bool Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            Regex regex = new Regex(IP_PATTERN, RegexOptions.IgnoreCase);
            return regex.IsMatch(value);
        }
    }
}
