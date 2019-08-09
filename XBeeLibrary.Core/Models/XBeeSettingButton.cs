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

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an XBee setting that can be interacted with through a button.
	/// </summary>
	public class XBeeSettingButton : AbstractXBeeSetting
	{
		// Properties.
		/// <summary>
		/// ID of the function (action) associated to the setting.
		/// </summary>
		public int FunctionNumber { get; set; }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingButton"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="category">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		/// <seealso cref="XBeeCategory"/>
		/// <seealso cref="XBeeFirmware"/>
		public XBeeSettingButton(string name, string description, XBeeCategory category, XBeeFirmware ownerFirmware)
			: this(null, name, description, null, category, ownerFirmware, 1) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingButton"/> object with the 
		/// provided parameters.
		/// </summary>
		/// <param name="name">Name of the setting.</param>
		/// <param name="description">Description of the setting.</param>
		/// <param name="category">Parent category of the setting.</param>
		/// <param name="ownerFirmware">XBee firmware the setting belongs to.</param>
		/// <param name="numNetworks">The number of networks the setting can be configured for.</param>
		/// <seealso cref="XBeeCategory"/>
		/// <seealso cref="XBeeFirmware"/>
		public XBeeSettingButton(string name, string description, XBeeCategory category,
			XBeeFirmware ownerFirmware, int numNetworks)
			: this(null, name, description, null, category, ownerFirmware, numNetworks) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingButton"/> object with the 
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
		public XBeeSettingButton(string atCommand, string name, string description,
			string defaultValue, XBeeCategory category, XBeeFirmware ownerFirmware)
			: this(atCommand, name, description, defaultValue, category, ownerFirmware, 1) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeSettingButton"/> object with the 
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
		public XBeeSettingButton(string atCommand, string name, string description,
			string defaultValue, XBeeCategory category, XBeeFirmware ownerFirmware, int numNetworks)
			: base(atCommand, name, description, defaultValue, category, ownerFirmware, numNetworks)
		{
			Type = TYPE_BUTTON;
		}

		/// <summary>
		/// Clones and returns the setting object.
		/// </summary>
		/// <param name="parentCategory">The parent category where the cloned setting should be placed.</param>
		/// <param name="ownerFirmware">The owner firmware of the cloned setting.</param>
		/// <returns>The cloned setting object.</returns>
		/// <seealso cref="AbstractXBeeSetting"/>
		public new AbstractXBeeSetting CloneSetting(XBeeCategory parentCategory, XBeeFirmware ownerFirmware)
		{
			XBeeSettingButton clonedSetting = (XBeeSettingButton)base.CloneSetting(parentCategory, ownerFirmware);

			// Clone the attributes of this object.
			clonedSetting.FunctionNumber = FunctionNumber;

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
			ValidationErrorMessage = null;
			return true;
		}
	}
}
