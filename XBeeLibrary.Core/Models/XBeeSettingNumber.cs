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
using System.Collections.Generic;
using System.Numerics;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an XBee setting that can be interacted with through an input and its 
	/// format is a number.
	/// </summary>
	public class XBeeSettingNumber : AbstractXBeeSetting
	{
		// Constants.
		private const string HEXADECIMAL_PREFIX = "0X";
		private const string ZERO = "0";
		private const string LT_SETTING = "LT";

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingNumber"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="category">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs 
		/// to.</param>
		/// <seealso cref="XBeeCategory"/>
		/// <seealso cref="XBeeFirmware"/>
		public XBeeSettingNumber(string name, string description,
			string defaultValue, XBeeCategory category, XBeeFirmware ownerFirmware)
			: this(name, description, defaultValue, category, ownerFirmware, 1) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingNumber"/> object with the 
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
		public XBeeSettingNumber(string name, string description, string defaultValue, 
			XBeeCategory category, XBeeFirmware ownerFirmware, int numNetworks)
			: base(name, description, defaultValue, category, ownerFirmware, numNetworks)
		{
			Type = TYPE_NUMBER;
			ReFormatDefaultValue();
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingNumber"/> object with the 
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
		public XBeeSettingNumber(string atCommand, string name, string description,
			string defaultValue, XBeeCategory category, XBeeFirmware ownerFirmware)
			: this(atCommand, name, description, defaultValue, category, ownerFirmware, 1) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingNumber"/> object with the 
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
		public XBeeSettingNumber(string atCommand, string name, string description,
			string defaultValue, XBeeCategory category, XBeeFirmware ownerFirmware, int numNetworks)
			: base(atCommand, name, description, defaultValue, category, ownerFirmware, numNetworks)
		{
			Type = TYPE_NUMBER;
			ReFormatDefaultValue();
		}

		// Properties.
		/// <summary>
		/// Units of the setting.
		/// </summary>
		public string Units { get; set; }

		/// <summary>
		/// List of allowed ranges for the settings.
		/// </summary>
		private List<Range> Ranges { get; set; } = new List<Range>();

		/// <summary>
		/// List of additional allowed values for the setting.
		/// </summary>
		public List<string> AdditionalValues { get; set; } = new List<string>();

		/// <summary>
		/// Clones and returns the setting object.
		/// </summary>
		/// <param name="parentCategory">The parent category where the cloned setting should be placed.</param>
		/// <param name="ownerFirmware">The owner firmware of the cloned setting.</param>
		/// <returns>The cloned setting object.</returns>
		/// <seealso cref="AbstractXBeeSetting"/>
		public new AbstractXBeeSetting CloneSetting(XBeeCategory parentCategory, XBeeFirmware ownerFirmware)
		{
			XBeeSettingNumber clonedSetting = (XBeeSettingNumber) base.CloneSetting(parentCategory, ownerFirmware);

			// Clone the list of items and set it to the cloned setting.
			List<string> clonedAdditionalValues = new List<string>();
			clonedAdditionalValues.AddRange(AdditionalValues);
			clonedSetting.AdditionalValues = clonedAdditionalValues;

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
			// TODO: Verify if it is necessary to use: FirmwareAddon.isValidateDefaultEmptySettingsEnabled()
			if ((DefaultValue == null || DefaultValue.Length == 0)
				&& (GetCurrentValue(networkIndex) == null || GetCurrentValue(networkIndex).Length == 0))
				return true;

			if (GetCurrentValue(networkIndex) == null || GetCurrentValue(networkIndex).Length == 0)
			{
				ValidationErrorMessage = "Value cannot be empty.";
				return false;
			}

			if (!ParsingUtils.IsHexadecimal(GetCurrentValue(networkIndex)))
			{
				ValidationErrorMessage = "Value is not a valid hexadecimal number.";
				return false;
			}

			// Get the current value.
			BigInteger intValue = ParsingUtils.GetBigInt(GetCurrentValue(networkIndex));

			// Special fix to avoid errors on LT setting due to firmware miss-specification error.
			if (AtCommand != null
				&& string.Compare(AtCommand, LT_SETTING, StringComparison.OrdinalIgnoreCase) == 0
				&& BigInteger.Compare(intValue, new BigInteger(0)) == 0)
			{
				ValidationErrorMessage = null;
				return true;
			}

			// Check if the value is any of the additional values.
			foreach (string additionalValue in AdditionalValues)
			{
				BigInteger validValue = ParsingUtils.GetBigInt(additionalValue);
				if (validValue.CompareTo(intValue) == 0)
				{
					ValidationErrorMessage = null;
					return true;
				}
			}

			// Check if value is in any of the possible ranges.
			foreach (Range range in Ranges)
			{
				BigInteger rangeMinInt;
				BigInteger rangeMaxInt;

				if (ParsingUtils.IsHexadecimal(range.RangeMin))
					rangeMinInt = ParsingUtils.GetBigInt(range.RangeMin);
				else
				{
					// [XCTUNG-1180] Range may contain a reference to another AT command.
					AbstractXBeeSetting setting = OwnerFirmware.GetAtSetting(range.RangeMin);
					rangeMinInt = ParsingUtils.GetBigInt(setting != null ?
						(string.IsNullOrEmpty(setting.GetCurrentValue()) ? "0" : setting.GetCurrentValue()) : GetCurrentValue());
				}

				if (ParsingUtils.IsHexadecimal(range.RangeMax))
					rangeMaxInt = ParsingUtils.GetBigInt(range.RangeMax);
				else
				{
					AbstractXBeeSetting setting = OwnerFirmware.GetAtSetting(range.RangeMax);
					rangeMaxInt = ParsingUtils.GetBigInt(setting != null ?
						(string.IsNullOrEmpty(setting.GetCurrentValue()) ? "0" : setting.GetCurrentValue()) : GetCurrentValue());
				}

				if (BigInteger.Compare(intValue, rangeMinInt) >= 0 && BigInteger.Compare(intValue, rangeMaxInt) <= 0)
				{
					ValidationErrorMessage = null;
					return true;
				}
			}

			ValidationErrorMessage = GenerateValidationErrorMsg();
			return false;
		}

		/// <summary>
		/// Adds a new range to the list of ranges.
		/// </summary>
		/// <param name="range">The range to add to the list.</param>
		/// <seealso cref="Range"/>
		public void AddRange(Range range)
		{
			Ranges.Add(range);
		}

		/// <summary>
		/// Returns the list of valid ranges for this numeric setting.
		/// </summary>
		/// <returns>The list of valid ranges.</returns>
		/// <seealso cref="Range"/>
		public List<Range> GetRanges()
		{
			return Ranges;
		}

		/// <summary>
		/// Reformats the default value of the numeric setting to have the same format
		/// than the values of the read XBee AT parameters.
		/// </summary>
		private void ReFormatDefaultValue()
		{
			if (DefaultValue != null && DefaultValue.ToUpper().StartsWith(HEXADECIMAL_PREFIX))
			{
				DefaultValue = DefaultValue.Substring(HEXADECIMAL_PREFIX.Length);
				while (DefaultValue.Length > 1 && DefaultValue.StartsWith(ZERO))
					DefaultValue = DefaultValue.Substring(1);
			}

			for (int i = 0; i < NumNetworks; i++)
				CurrentValues[i] = DefaultValue;
		}

		/// <summary>
		/// Generates and returns the error message that is displayed when the numeric value is 
		/// not valid.
		/// </summary>
		/// <remarks> The message includes tips with the valid ranges and additional values.</remarks>
		/// <returns>The error message.</returns>
		private string GenerateValidationErrorMsg()
		{
			string errorMessage = "Value out of range. Valid range is ";
			foreach (Range range in Ranges)
				errorMessage += string.Format("[{0} - {1}], ", range.RangeMin, range.RangeMax);

			errorMessage = errorMessage.Substring(0, errorMessage.Length - 2);
			if (AdditionalValues.Count > 0)
			{
				errorMessage += ", or any of the following values: ";
				foreach (string additionalValue in AdditionalValues)
					errorMessage += string.Format("{0}, ", additionalValue);

				errorMessage = errorMessage.Substring(0, errorMessage.Length - 2);
			}
			errorMessage += ".";
			return errorMessage;
		}
	}
}
