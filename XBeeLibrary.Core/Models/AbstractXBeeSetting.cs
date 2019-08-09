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

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class provides common functionality for all XBee settings.
	/// </summary>
	public abstract class AbstractXBeeSetting : ICloneable
	{
		// Constants.
		public const int TYPE_NUMBER = 0;
		public const int TYPE_COMBO = 1;
		public const int TYPE_NO_CONTROL = 2;
		public const int TYPE_TEXT = 3;
		public const int TYPE_BUTTON = 4;

		public const string COMMAND_BUFFER = "::";
		public const string COMMAND_BUTTON = "()";

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="AbstractXBeeSetting"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="parentCategory">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		protected AbstractXBeeSetting(string name, string description, string defaultValue,
			XBeeCategory parentCategory, XBeeFirmware ownerFirmware)
			: this(null, name, description, defaultValue, parentCategory, ownerFirmware, 1) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="AbstractXBeeSetting"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="parentCategory">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		/// <param name="numNetworks">The number of networks the setting can be configured for.</param>
		protected AbstractXBeeSetting(string name, string description, string defaultValue,
			XBeeCategory parentCategory, XBeeFirmware ownerFirmware, int numNetworks)
			: this(null, name, description, defaultValue, parentCategory, ownerFirmware, numNetworks) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="AbstractXBeeSetting"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="atCommand">The AT command corresponding to the setting.</param>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="parentCategory">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		protected AbstractXBeeSetting(string atCommand, string name, string description,
			string defaultValue, XBeeCategory parentCategory, XBeeFirmware ownerFirmware)
			: this(atCommand, name, description, defaultValue, parentCategory, ownerFirmware, 1) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="AbstractXBeeSetting"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="atCommand">The AT command corresponding to the setting.</param>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="defaultValue">Default value of the setting.</param>
		/// <param name="parentCategory">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		/// <param name="numNetworks">The number of networks the setting can be configured for.</param>
		protected AbstractXBeeSetting(string atCommand, string name, string description, string defaultValue,
			XBeeCategory parentCategory, XBeeFirmware ownerFirmware, int numNetworks)
		{
			AtCommand = atCommand;
			Name = name;
			Description = description;
			DefaultValue = defaultValue;
			ParentCategory = parentCategory;
			OwnerFirmware = ownerFirmware;
			NumNetworks = numNetworks;

			// Fill values with default value.
			for (int i = 0; i < numNetworks; i++)
				CurrentValues.Add(defaultValue);

			// Fill XBee values with default value.
			for (int i = 0; i < numNetworks; i++)
				XBeeValues.Add(defaultValue);
		}

		// Properties.
		/// <summary>
		/// AT command corresponding to the setting.
		/// </summary>
		public string AtCommand { get; set; }

		/// <summary>
		/// Name of the setting.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description of the setting.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Default value of the setting.
		/// </summary>
		public string DefaultValue { get; set; }

		/// <summary>
		/// Number of networks the setting can be configured for.
		/// </summary>
		public int NumNetworks { get; set; }

		/// <summary>
		/// Type of setting.
		/// </summary>
		/// <remarks>One of:
		/// <list type="bullet">
		/// <item><description>TYPE_NUMBER</description></item>
		/// <item><description>TYPE_COMBO</description></item>
		/// <item><description>TYPE_NO_CONTROL</description></item>
		/// <item><description>TYPE_TEXT</description></item>
		/// <item><description>TYPE_BUTTON</description></item>
		/// </list></remarks>
		public int Type { get; protected set; }

		/// <summary>
		/// Parent category of the setting.
		/// </summary>
		public XBeeCategory ParentCategory { get; set; }

		/// <summary>
		/// XBee firmware the setting belongs to.
		/// </summary>
		public XBeeFirmware OwnerFirmware { get; set; }

		/// <summary>
		/// Indicates whether the setting is enabled (can be edited) or not.
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Indicates whether the setting is visible or not.
		/// </summary>
		public bool Visible { get; set; } = true;

		/// <summary>
		/// Indicates whether the setting should be flashed as a custom default setting or not.
		/// </summary>
		public bool CustomDefault { get; set; } = false;

		/// <summary>
		/// Validation error message of the setting.
		/// </summary>
		public string ValidationErrorMessage { get; protected set; }

		/// <summary>
		/// List of current values, 1 for each network the setting can be configured for.
		/// </summary>
		public List<string> CurrentValues { get; set; } = new List<string>();

		/// <summary>
		/// List of XBee values (values stored in the XBee device), 1 for each network the setting 
		/// can be configured for.
		/// </summary>
		public List<string> XBeeValues { get; set; } = new List<string>();

		/// <summary>
		/// Returns whether or not the current value of the setting is valid.
		/// </summary>
		/// <returns><c>true</c> if the value of the setting is valid, <c>false</c> 
		/// otherwise.</returns>
		public abstract bool ValidateSetting();

		/// <summary>
		/// Returns whether or not the current value of the provided network index is valid.
		/// </summary>
		/// <returns><c>true</c> if the value is valid, <c>false</c> otherwise.</returns>
		public abstract bool ValidateSetting(int networkIndex);

		/// <summary>
		/// Returns the current value of the setting.
		/// </summary>
		/// <returns>The current value of the setting.</returns>
		public string GetCurrentValue()
		{
			return CurrentValues[0];
		}

		/// <summary>
		/// Returns the current value of the setting for the given network index.
		/// </summary>
		/// <param name="networkIndex">Network index to retrieve the setting value from. Index starts at 1.</param>
		/// <returns>The current value of the setting for the given network index.</returns>
		public string GetCurrentValue(int networkIndex)
		{
			if (networkIndex == 0)
				networkIndex = 1;

			if (networkIndex > NumNetworks)
				return CurrentValues[0];

			return CurrentValues[networkIndex - 1];
		}

		/// <summary>
		/// Sets the current value of the setting.
		/// </summary>
		/// <param name="currentValue">The current value to set to the setting.</param>
		public void SetCurrentValue(string currentValue)
		{
			CurrentValues[0] = currentValue;
			CustomDefault = false;
		}

		/// <summary>
		/// Sets the current value of the setting for the given network index.
		/// </summary>
		/// <param name="currentValue">The current value for the given network index.</param>
		/// <param name="networkIndex">Index of the network this value is for. Index starts 
		/// at 1.</param>
		public void SetCurrentValue(string currentValue, int networkIndex)
		{
			if (networkIndex == 0)
				networkIndex = 1;

			if (networkIndex > NumNetworks)
				CurrentValues[0] = currentValue;
			else
				CurrentValues[networkIndex - 1] = currentValue;

			CustomDefault = false;
		}

		/// <summary>
		/// Returns the value of the setting that is stored in the XBee device.
		/// </summary>
		/// <returns>The XBee setting value.</returns>
		public string GetXBeeValue()
		{
			return XBeeValues[0];
		}

		/// <summary>
		/// Returns the current value of the setting that is stored in the XBee device for the given network index.
		/// </summary>
		/// <param name="networkIndex">Network index to retrieve the setting value from. Index starts at 1.</param>
		/// <returns>The XBee setting value for the given network index.</returns>
		public string GetXBeeValue(int networkIndex)
		{
			if (networkIndex == 0)
				networkIndex = 1;

			if (networkIndex > NumNetworks)
				return XBeeValues[0];

			return XBeeValues[networkIndex - 1];
		}

		/// <summary>
		/// Sets the value of the setting that is stored in the XBee device.
		/// </summary>
		/// <param name="xbeeValue">The XBee setting value to set to the setting.</param>
		public void SetXBeeValue(string xbeeValue)
		{
			XBeeValues[0] = xbeeValue;
		}

		/// <summary>
		/// Sets the value of the setting that is stored in the XBee device for the given network index.
		/// </summary>
		/// <param name="xbeeValue">The XBee setting value for the given network index.</param>
		/// <param name="networkIndex">Index of the network this value is for. Index starts at 1.</param>
		public void SetXBeeValue(string xbeeValue, int networkIndex)
		{
			if (networkIndex == 0)
				networkIndex = 1;

			if (networkIndex > NumNetworks)
				XBeeValues[0] = xbeeValue;
			else
				XBeeValues[networkIndex - 1] = xbeeValue;
		}

		/// <summary>
		/// Returns whether this setting addresses multiple networks or not.
		/// </summary>
		/// <returns><c>true</c> if the setting addresses more than 1 network, <c>false</c> 
		/// otherwise.</returns>
		public bool SupportsMultipleNetworks()
		{
			return NumNetworks > 1;
		}

		/// <summary>
		/// Returns the setting label to be displayed in the configuration editor.
		/// </summary>
		/// <returns>The label for the setting.</returns>
		public string GetLabel()
		{
			string label = "";
			if (AtCommand != null)
				label = "[" + AtCommand + "]";
			else
				label = Name;

			return label;
		}

		/// <summary>
		/// Returns the string representation (name) of the setting.
		/// </summary>
		/// <returns>The string representation (name) of the setting.</returns>
		public string Tostring()
		{
			return Name;
		}

		/// <summary>
		/// Creates a shallow copy of the AbstractXBeeSetting.
		/// </summary>
		/// <returns>The copy of the AbstractXBeeSetting</returns>
		public object Clone()
		{
			return MemberwiseClone() as AbstractXBeeSetting;
		}

		/// <summary>
		/// Clones and returns the setting object.
		/// </summary>
		/// <param name="parentCategory">The parent category where the cloned setting should be placed.</param>
		/// <param name="ownerFirmware">The owner firmware of the cloned setting.</param>
		/// <returns>The cloned setting object.</returns>
		public AbstractXBeeSetting CloneSetting(XBeeCategory parentCategory, XBeeFirmware ownerFirmware)
		{
			AbstractXBeeSetting clonedSetting = (AbstractXBeeSetting)Clone();
			clonedSetting.ParentCategory = parentCategory;
			clonedSetting.OwnerFirmware = ownerFirmware;

			// Clone the list of current values.
			List<string> clonedCurrentValues = new List<string>();
			clonedCurrentValues.AddRange(CurrentValues);
			clonedSetting.CurrentValues = clonedCurrentValues;

			// Clone the list of XBee values.
			List<string> clonedXBeeValues = new List<string>();
			clonedXBeeValues.AddRange(XBeeValues);
			clonedSetting.XBeeValues = clonedXBeeValues;

			return clonedSetting;
		}
	}
}
