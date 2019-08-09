/*
 * Copyright 2019, Digi International Inc.
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

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an XBee setting that can be interacted with through a text input.
	/// </summary>
	public class XBeeSettingText : AbstractXBeeSetting
	{
		// Constants.
		public const string IP_V4_PATTERN = "([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\."
				+ "([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\."
				+ "([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\."
				+ "([01]?\\d\\d?|2[0-4]\\d|25[0-5])";
		public const string IP_V6_PATTERN = "([0-9a-fA-F]){32}|([0-9a-fA-F]{4}:){7}([0-9a-fA-F]){4}"
				+ "|([0-9a-fA-F]){16}|([0-9a-fA-F]{4}:){3}([0-9a-fA-F]){4}";
		public const string PHONE_PATTERN = "^\\+?[0-9]+$";

		private const string KY_SETTING = "KY";

		private const string HEXADECIMAL_PREFIX_UPPER = "0X";
		private const string HEXADECIMAL_PREFIX_LOWER = "0x";

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingText"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="category">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		/// <seealso cref="XBeeCategory"/>
		/// <seealso cref="XBeeFirmware"/>
		public XBeeSettingText(string name, string description, string defaultValue, 
			XBeeCategory category, XBeeFirmware ownerFirmware)
			: this(name, description, defaultValue, category, ownerFirmware, 1) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingText"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="category">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		/// <param name="numNetworks">The number of networks the setting can be configured for.</param>
		/// <seealso cref="XBeeCategory"/>
		/// <seealso cref="XBeeFirmware"/>
		public XBeeSettingText(string name, string description, string defaultValue, 
			XBeeCategory category, XBeeFirmware ownerFirmware, int numNetworks)
			: base(name, description, defaultValue, category, ownerFirmware, numNetworks)
		{
			Type = TYPE_TEXT;
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingText"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="atCommand">The AT command corresponding to the setting.</param>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="category">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		/// <seealso cref="XBeeCategory"/>
		/// <seealso cref="XBeeFirmware"/>
		public XBeeSettingText(string atCommand, string name, string description,
			string defaultValue, XBeeCategory category, XBeeFirmware ownerFirmware)
			: base(atCommand, name, description, defaultValue, category, ownerFirmware, 1) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingText"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="atCommand">The AT command corresponding to the setting.</param>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="category">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		/// <param name="numNetworks">The number of networks the setting can be configured for.</param>
		/// <seealso cref="XBeeCategory"/>
		/// <seealso cref="XBeeFirmware"/>
		public XBeeSettingText(string atCommand, string name, string description,
			string defaultValue, XBeeCategory category, XBeeFirmware ownerFirmware, int numNetworks)
			: base(atCommand, name, description, defaultValue, category, ownerFirmware, numNetworks)
		{
			Type = TYPE_TEXT;
		}

		// Properties.
		/// <summary>
		/// Minimum number of characters for the value of the setting.
		/// </summary>
		public int MinChars { get; set; }

		/// <summary>
		/// Maximum number of characters for the value of the setting.
		/// </summary>
		public int MaxChars { get; set; }

		/// <summary>
		/// Text format of the setting.
		/// </summary>
		/// <remarks>One of:
		/// <list type="bullet">
		/// <item><description><c>Format.HEX</c></description></item>
		/// <item><description><c>Format.ASCII</c></description></item>
		/// <item><description><c>Format.IPV4</c></description></item>
		/// <item><description><c>Format.IPV6</c></description></item>
		/// <item><description><c>Format.PHONE</c></description></item>
		/// <item><description><c>Format.NOFORMAT</c></description></item>
		/// <item><description><c>Format.UNKNOWN</c></description></item></list></remarks>
		public Format Format { get; set; } = Format.ASCII;

		/// <summary>
		/// Exception value of the setting.
		/// </summary>
		public string ExceptionValue { get; set; }

		/// <summary>
		/// Clones and returns the setting object.
		/// </summary>
		/// <param name="parentCategory">The parent category where the cloned setting should be placed.</param>
		/// <param name="ownerFirmware">The owner firmware of the cloned setting.</param>
		/// <returns>The cloned setting object.</returns>
		/// <seealso cref="AbstractXBeeDevice"/>
		public new AbstractXBeeSetting CloneSetting(XBeeCategory parentCategory, XBeeFirmware ownerFirmware)
		{
			XBeeSettingText clonedSetting = (XBeeSettingText)base.CloneSetting(parentCategory, ownerFirmware);

			// Clone the attributes of this object.
			clonedSetting.MinChars = MinChars;
			clonedSetting.MaxChars = MaxChars;
			clonedSetting.Format = Format;
			clonedSetting.ExceptionValue = ExceptionValue;

			return clonedSetting;
		}

		/// <summary>
		/// Returns whether or not the current value of the setting is valid.
		/// </summary>
		/// <returns><c>true</c> if the value of the setting is valid, <c>false</c> 
		/// otherwise.</returns>
		public override bool ValidateSetting()
		{
			return ValidateSetting(1);
		}

		/// <summary>
		/// Returns whether or not the current value of the provided network index is valid.
		/// </summary>
		/// <returns><c>true</c> if the value is valid, <c>false</c> otherwise.</returns>
		public override bool ValidateSetting(int networkIndex)
		{
			// Special fix to avoid errors on settings that don't have a default value configured.
			// This is a firmware miss-specification error.
			// TODO: Is this necessary FirmwareAddon.isValidateDefaultEmptySettingsEnabled()?
			if ((DefaultValue == null || DefaultValue.Length == 0)
				&& (GetCurrentValue(networkIndex) == null || GetCurrentValue(networkIndex).Length == 0))
				return true;

			// Special fix to avoid errors on KY setting due to firmware miss-specification error.
			if (string.Compare(AtCommand, KY_SETTING, StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (GetCurrentValue(networkIndex) != null && GetCurrentValue(networkIndex).Length > 0)
				{
					if (GetCurrentValue(networkIndex).Trim().Equals("0") || GetCurrentValue(networkIndex).Trim().Equals("1"))
						return true;
				}
			}

			if (GetCurrentValue(networkIndex) == null)
			{
				ValidationErrorMessage = "Value cannot be empty.";
				return false;
			}

			if (GetCurrentValue(networkIndex).Length == 0 && MinChars == 0)
			{
				ValidationErrorMessage = null;
				return true;
			}

			if (MinChars == MaxChars
				&& GetCurrentValue(networkIndex).Length != MinChars)
			{
				ValidationErrorMessage = "Value must have " + MinChars + " characters.";
				return false;
			}

			if (GetCurrentValue(networkIndex).Length < MinChars)
			{
				ValidationErrorMessage = "Value must have at least " + MinChars + " characters.";
				return false;
			}
			if (GetCurrentValue(networkIndex).Length > MaxChars)
			{
				ValidationErrorMessage = "Value must have less than " + (MaxChars + 1) + " characters.";
				return false;
			}
			if (Format == Format.HEX)
			{
				if (!ParsingUtils.IsHexadecimal(GetCurrentValue(networkIndex)))
				{
					ValidationErrorMessage = "Value can only contain hexadecimal characters";
					return false;
				}
			}
			if (Format == Format.IPV4)
			{
				if (!Regex.IsMatch(GetCurrentValue(networkIndex), IP_V4_PATTERN))
				{
					ValidationErrorMessage = "Value must follow the IPv4 format: x.x.x.x (where x must be a decimal value between 0 and 255).";
					return false;
				}
			}
			if (Format == Format.IPV6)
			{
				if (!Regex.IsMatch(GetCurrentValue(networkIndex), IP_V6_PATTERN))
				{
					ValidationErrorMessage = "Value must follow the IPv6 format: 'xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx' " +
							"or 'xxxx:xxxx:xxxx:xxxx' (or without ':'), where x must be an hexadecimal value.";
					return false;
				}
			}
			if (Format == Format.PHONE)
			{
				if (!Regex.IsMatch(GetCurrentValue(networkIndex), PHONE_PATTERN))
				{
					ValidationErrorMessage = "Value can only contain numeric characters (0-9) and plus sign '+' prefix.";
					return false;
				}
			}

			ValidationErrorMessage = null;
			return true;
		}

		/// <summary>
		/// Reformats the default value of the text setting to have the same format than the values 
		/// of the read XBee AT parameters.
		/// </summary>
		private void ReFormatDefaultValue()
		{
			if (DefaultValue == null || DefaultValue.Length == 0)
				return;

			string[] characters;
			if (DefaultValue.Contains(HEXADECIMAL_PREFIX_UPPER))
				characters = DefaultValue.Split(HEXADECIMAL_PREFIX_UPPER.ToCharArray(), StringSplitOptions.None);
			else if (DefaultValue.Contains(HEXADECIMAL_PREFIX_LOWER))
				characters = DefaultValue.Split(HEXADECIMAL_PREFIX_LOWER.ToCharArray());
			else
				characters = new string[] { DefaultValue };

			string newDefaultValue = "";

			for (int i = 0; i < characters.Length; i++)
			{
				if (characters[i].Length == 0)
				{
					continue;
				}
				switch (Format)
				{
					case Format.ASCII:
						int asciiChar;
						if (int.TryParse(characters[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out asciiChar))
							newDefaultValue += asciiChar;
						else
							newDefaultValue += characters[i];
						break;
					default:
						newDefaultValue += characters[i];
						break;
				}
			}

			DefaultValue = newDefaultValue;
			for (int i = 0; i < NumNetworks; i++)
			{
				CurrentValues[i] = DefaultValue;
				XBeeValues[i] = DefaultValue;
			}
		}
	}
}
