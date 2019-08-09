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

using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Tools;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an XBee firmware.
	/// </summary>
	public class XBeeFirmware : ICloneable
	{
		// Constants
		private const string SERIAL_SETTING_BAUD_RATE = "BD";
		private const string SERIAL_SETTING_STOP_BITS = "SB";
		private const string SERIAL_SETTING_PARITY = "NB";
		private const string SERIAL_SETTING_FLOW_CONTROL_CTS = "D7";

		private const int DEFAULT_SERIAL_BAUD_RATE = 9600;
		private const int DEFAULT_SERIAL_DATA_BITS = 8;
		private const int DEFAULT_SERIAL_STOP_BITS = 1;
		private const int DEFAULT_SERIAL_PARITY = 0;
		private const int DEFAULT_SERIAL_FLOW_CONTROL = 0;

		private const int FLOWCONTROL_RTSCTS_IN = 1;
		private const int FLOWCONTROL_RTSCTS_OUT = 2;

		public const string EXTENSION_EHX2 = ".ehx2";
		public const string EXTENSION_EBIN = ".ebin";

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeFirmware"/> object with the provided parameters.
		/// </summary>
		/// <param name="family">XBee product family name.</param>
		/// <param name="productName">XBee product name.</param>
		/// <param name="hardwareVersion">Hardware version the firmware is compatible with.</param>
		/// <param name="compatibilityNumber">Compatibility number of the XBee firmware.</param>
		/// <param name="firmwareVersion">XBee firmware version.</param>
		/// <param name="configBufferLocation">Configuration buffer location of the XBee firmware.</param>
		/// <param name="flashPageSize">Flash page size (in bytes) of the XBee firmware.</param>
		/// <param name="crcBufferLength">CRC buffer length of the XBee firmware.</param>
		/// <param name="function">XBee function.</param>
		public XBeeFirmware(string family, string productName, string hardwareVersion,
			string compatibilityNumber, string firmwareVersion, string configBufferLocation,
			string flashPageSize, string crcBufferLength, string function)
		{
			Family = family;
			ProductName = productName;
			HardwareVersion = hardwareVersion;
			CompatibilityNumber = compatibilityNumber;
			FirmwareVersion = firmwareVersion;
			ConfigBufferLocation = configBufferLocation;
			FlashPageSize = flashPageSize;
			CrcBufferLength = crcBufferLength;
			Function = function;

			logger = LogManager.GetLogger<XBeeFirmware>();
		}

		// Properties.
		/// <summary>
		/// XBee product family name.
		/// </summary>
		public string Family { get; private set; }

		/// <summary>
		/// XBee product name.
		/// </summary>
		public string ProductName { get; private set; }

		/// <summary>
		/// Hardware version the firmware is compatible with.
		/// </summary>
		public string HardwareVersion { get; set; }

		/// <summary>
		/// Compatibility number of the XBee firmware.
		/// </summary>
		public string CompatibilityNumber { get; private set; }

		/// <summary>
		/// Version of the XBee firmware.
		/// </summary>
		public string FirmwareVersion { get; private set; }

		/// <summary>
		/// Configuration buffer location of the XBee firmware.
		/// </summary>
		public string ConfigBufferLocation { get; private set; }

		/// <summary>
		/// Flash page size (in bytes) of the XBee firmware.
		/// </summary>
		public string FlashPageSize { get; private set; }

		/// <summary>
		/// CRC buffer length of the XBee firmware.
		/// </summary>
		public string CrcBufferLength { get; private set; }

		/// <summary>
		/// XBee function.
		/// </summary>
		public string Function { get; private set; }

		/// <summary>
		/// Path where the XML definition file is located.
		/// </summary>
		public string DefinitionFileLocation { get; private set; }

		/// <summary>
		/// String containing the full content of the XML definition file.
		/// </summary>
		public string DefinitionFileContent { get; set; }

		/// <summary>
		/// Path where the binary file is located.
		/// </summary>
		public string BinaryFileLocation { get; set; }

		/// <summary>
		/// Region string identifier of the firmware.
		/// </summary>
		public string Region { get; set; } = "99";

		/// <summary>
		/// Version of the modem firmware associated to the XBee firmware.
		/// </summary>
		public string ModemVersion { get; set; }

		/// <summary>
		/// URL where the modem firmware associated to the XBee firmware 
		/// is stored.
		/// </summary>
		public string ModemUrl { get; set; }

		/// <summary>
		/// Indicates whether the firmware is initialized (categories and settings have been parsed 
		/// and listed) or not.
		/// </summary>
		public bool Initialized { get; set; } = false;

		/// <summary>
		/// Indicates whether the settings have been read at least 1 time or 
		/// not.
		/// </summary>
		public bool FirstTimeRead { get; set; } = true;

		/// <summary>
		/// Indicates the index of the current active network (index starts 
		/// at 1).
		/// </summary>
		public int ActiveNetworkIndex { get; set; } = 1;

		/// <summary>
		/// List of configuration categories of the firmware.
		/// </summary>
		public List<XBeeCategory> Categories { get; set; } = new List<XBeeCategory>();

		/// <summary>
		/// Dictionary containing the dependencies. The key correspondonds to the AT command of the 
		/// setting that has a dependency. The value is a list of AT commands corresponding to the 
		/// settings that depend on the key one.
		/// </summary>
		private Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

		/// <summary>
		/// Sets the path where the XML definition file of this XBee firmware is located.
		/// </summary>
		/// <param name="definitionFileLocation">The XML definition file path.</param>
		public void SetDefinitionFilePath(string definitionFileLocation)
		{
			DefinitionFileLocation = definitionFileLocation.Replace("\\", "/");
			BinaryFileLocation = definitionFileLocation.Replace(ParsingUtils.XML_EXTENSION, "").Replace(ParsingUtils.MXI_EXTENSION, "");
		}
		
		/// <summary>
		/// Returns the path where the binary file of this XBee firmware is located, including the 
		/// extension of the binary.
		/// </summary>
		/// <returns>The path (with extension) where the binary file of this XBee firmware is located</returns>
		public string GetBinaryFileWithExtensionPath()
		{
			if (BinaryFileLocation != null)
			{
				FileInfo binaryFile = new FileInfo(BinaryFileLocation + EXTENSION_EBIN);
				if (!binaryFile.Exists)
					binaryFile = new FileInfo(BinaryFileLocation + EXTENSION_EHX2);

				if (binaryFile.Exists)
					return binaryFile.FullName;
			}
			return null;
		}

		/// <summary>
		/// Parses and fills the contents of this XBee firmware.
		/// </summary>
		public void FillContents()
		{
			XMLFirmwareParser.ParseFirmwareContents(this);
		}

		/// <summary>
		/// Returns whether this object is equal to the given one.
		/// </summary>
		/// <param name="obj">The object to compare if it is equal to this one.</param>
		/// <returns><c>true</c> if this object is equal to the given one, <c>false</c> 
		/// otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is XBeeFirmware))
				return false;

			if (((XBeeFirmware)obj).FirmwareVersion.Equals(FirmwareVersion)
				&& ((XBeeFirmware)obj).HardwareVersion.Equals(HardwareVersion))
				return true;

			return false;
		}

		/// <summary>
		/// Returns the Hash code of this object.
		/// </summary>
		/// <returns>The Hash code of this object.</returns>
		public override int GetHashCode()
		{
			return HardwareVersion.GetHashCode() + FirmwareVersion.GetHashCode();
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return "[" + FirmwareVersion + "]" + Function;
		}

		/// <summary>
		/// Returns the list of settings defined in the firmware.
		/// </summary>
		/// <returns>The list of settings defined in the firmware.</returns>
		public List<AbstractXBeeSetting> GetAtSettings()
		{
			List<AbstractXBeeSetting> atSettings = new List<AbstractXBeeSetting>();
			foreach (XBeeCategory category in Categories)
				AddATSettings(category, atSettings);

			return atSettings;
		}

		/// <summary>
		/// Returns the list of settings that don't support multiple networks. If the firmware only 
		/// supports 1 network, this method returns the same list as <see cref="GetAtSettings"/>.
		/// </summary>
		/// <returns>The list of settings that don't support multiple networks.</returns>
		public List<AbstractXBeeSetting> GetCommonAtSettings()
		{
			List<AbstractXBeeSetting> atCommonSettings = new List<AbstractXBeeSetting>();
			foreach (XBeeCategory category in Categories)
				AddCommonATSettings(category, atCommonSettings);

			return atCommonSettings;
		}


		/// <summary>
		/// Returns the list of settings that can be configured for multiple networks.
		/// </summary>
		/// <returns>The list of settings that can be configured for multiple networks.</returns>
		public List<AbstractXBeeSetting> GetDualAtSettings()
		{
			List<AbstractXBeeSetting> atDualSettings = new List<AbstractXBeeSetting>();
			foreach (XBeeCategory category in Categories)
				AddDualATSettings(category, atDualSettings);

			return atDualSettings;
		}

		/// <summary>
		/// Returns the total number of settings contained in the XBee firmware.
		/// </summary>
		/// <returns>The total number of setings of the firmware.</returns>
		public int GetATSettingsNumber()
		{
			int atSettingsNumber = 0;
			foreach (XBeeCategory category in Categories)
				AddATSettingsNumber(category, atSettingsNumber);
			return atSettingsNumber;
		}

		/// <summary>
		/// Returns the setting corresponding to the provided <paramref name="atCommand"/> in any category 
		/// of the firmware.
		/// </summary>
		/// <param name="atCommand">The AT command corresponding to the setting to get.</param>
		/// <returns>The setting corresponding to the provided AT command.</returns>
		public AbstractXBeeSetting GetAtSetting(string atCommand)
		{
			foreach (XBeeCategory category in Categories)
			{
				AbstractXBeeSetting atSetting = GetAtSetting(atCommand, category);
				if (atSetting != null)
					return atSetting;
			}
			return null;
		}

		/// <summary>
		/// Returns the XBee value (value stored in the XBee module) of the baud rate setting of the firmware.
		/// </summary>
		/// <returns>The XBee value of the baud rate setting of the firmware.</returns>
		public int GetSerialBaudRate()
		{
			AbstractXBeeSetting atSetting = GetAtSetting(SERIAL_SETTING_BAUD_RATE);
			if (atSetting != null)
			{
				// Do not specify the network stack of the BD parameter by the moment (it's a common value).
				// TODO: [DUAL] When this setting is implemented individually for each network stack, update this code to
				//       specify the network ID from which this parameter should be read.
				string settingValue = atSetting.GetXBeeValue();
				if (!ParsingUtils.IsHexadecimal(atSetting.GetXBeeValue()))
					return DEFAULT_SERIAL_BAUD_RATE;

				switch (ParsingUtils.HexStringToInt(settingValue))
				{
					case 0:
						return 1200;
					case 1:
						return 2400;
					case 2:
						return 4800;
					case 3:
						return 9600;
					case 4:
						return 19200;
					case 5:
						return 38400;
					case 6:
						return 57600;
					case 7:
						return 115200;
					case 8:
						return 230400;
					case 9:
						return 460800;
					case 10:
						return 921600;
					default:
						return DEFAULT_SERIAL_BAUD_RATE;
				}
			}
			return DEFAULT_SERIAL_BAUD_RATE;
		}

		/// <summary>
		/// Returns the number of serial data bits.
		/// </summary>
		/// <returns>The number of serial data bits.</returns>
		public int GetSerialDataBits()
		{
			return DEFAULT_SERIAL_DATA_BITS;
		}

		/// <summary>
		/// Returns the XBee value (value stored in the XBee module) of the stop bits setting of the 
		/// firmware.
		/// </summary>
		/// <returns>The XBee value of the stop bits setting of the firmware.</returns>
		public int GetSerialStopBits()
		{
			AbstractXBeeSetting atSetting = GetAtSetting(SERIAL_SETTING_STOP_BITS);
			if (atSetting != null)
			{
				// Do not specify the network stack of the SB parameter by the moment (it's a common value).
				// TODO: [DUAL] When this setting is implemented individually for each network stack, update this code to
				//       specify the network ID from which this parameter should be read.
				string settingValue = atSetting.GetXBeeValue();
				if (!ParsingUtils.IsInteger(atSetting.GetXBeeValue()))
					return DEFAULT_SERIAL_STOP_BITS;

				switch (int.Parse(settingValue))
				{
					case 0:
						return 1;
					case 1:
						return 2;
					default:
						return DEFAULT_SERIAL_STOP_BITS;
				}
			}
			return DEFAULT_SERIAL_STOP_BITS;
		}

		/// <summary>
		/// Returns the XBee value (value stored in the XBee module) of the serial parity setting of the firmware.
		/// </summary>
		/// <returns>The XBee value of the serial parity setting of the firmware.</returns>
		public int GetSerialParity()
		{
			AbstractXBeeSetting atSetting = GetAtSetting(SERIAL_SETTING_PARITY);
			if (atSetting != null)
			{
				// Do not specify the network stack of the NB parameter by the moment (it's a common value).
				// TODO: [DUAL] When this setting is implemented individually for each network stack, update this code to
				//       specify the network ID from which this parameter should be read.
				string settingValue = atSetting.GetXBeeValue();
				if (!ParsingUtils.IsInteger(atSetting.GetXBeeValue()))
					return DEFAULT_SERIAL_PARITY;

				switch (int.Parse(settingValue))
				{
					case 0:
						return 0;
					case 1:
						return 2;
					case 2:
						return 1;
					case 3:
						return 3;
					case 4:
						return 4;
					default:
						return DEFAULT_SERIAL_PARITY;
				}
			}
			return DEFAULT_SERIAL_PARITY;
		}

		/// <summary>
		/// Returns whether the XBee firmware has serial interface or not.
		/// </summary>
		/// <returns><c>true</c> if the firmware has serial interface, <c>false</c> otherwise.</returns>
		public bool HasSerialInterface()
		{
			if (GetAtSetting(SERIAL_SETTING_BAUD_RATE) == null)
				return false;

			return true;
		}

		/// <summary>
		/// Returns whether this firmware addresses multiple networks or not.
		/// </summary>
		/// <returns><c>true</c> if the firmware addresses more than 1 network, 
		/// <c>false</c> otherwise.</returns>
		public bool SupportsMultipleNetworks()
		{
			// Look in each setting of each category.
			foreach (XBeeCategory category in Categories)
			{
				foreach (AbstractXBeeSetting setting in category.Settings)
				{
					if (setting.SupportsMultipleNetworks())
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Retrieves the number of networks addressed by this firmware.
		/// </summary>
		/// <returns>The number of networks addressed by this firmware.</returns>
		public int GetNumberOfNetworks()
		{
			int numNetworks = 1;
			foreach (XBeeCategory category in Categories)
			{
				foreach (AbstractXBeeSetting setting in category.Settings)
				{
					if (setting.NumNetworks > numNetworks)
						numNetworks = setting.NumNetworks;
				}
			}
			return numNetworks;
		}

		/// <summary>
		/// Adds a setting dependency.
		/// </summary>
		/// <remarks>
		/// [XCTUNG-1180] Range may contain a reference to another AT command.
		/// </remarks>
		/// <param name="atCommand">Setting that has a dependency.</param>
		/// <param name="dependsOn">Setting that depends on <c>atCommand</c>.</param>
		public void AddDependency(string atCommand, string dependsOn)
		{
			if (!dependencies.TryGetValue(atCommand, out List<string> deps))
				deps = new List<string>();

			if (!deps.Contains(dependsOn))
			{
				deps.Add(dependsOn);
				dependencies[atCommand] = deps;
			}
		}

		/// <summary>
		/// Gets the dependencies of the given setting.
		/// </summary>
		/// <param name="atCommand">Setting of which obtain the dependencies.</param>
		/// <returns>List of AT commands that depend on the given one.</returns>
		public List<string> GetDependencies(string atCommand)
		{
			if (!dependencies.TryGetValue(atCommand, out List<string> deps))
				return new List<string>();

			return deps;
		}
		
		/// <summary>
		/// Returns a string containing the supported binaries separated by '|' char.
		/// </summary>
		/// <returns>A string containing the supported binaries.</returns>
		public static string GetSupportedBinaries()
		{
			return (EXTENSION_EBIN + "|" + EXTENSION_EHX2).Replace(".", "");
		}

		/// <summary>
		/// Creates a shallow copy of the XBeeFirmware.
		/// </summary>
		/// <returns>The copy of the XBeeFirmware</returns>
		public object Clone()
		{
			return MemberwiseClone() as XBeeFirmware;
		}

		/// <summary>
		/// Clones and returns the XBee firmware object.
		/// </summary>
		/// <returns>The cloned XBee firmware object.</returns>
		public XBeeFirmware CloneFirmware()
		{
			// Verify if the firmware needs to fill its contents.
			if (!Initialized)
			{
				try
				{
					FillContents();
				}
				catch (ParsingException e)
				{
					logger.Error(e.Message, e);
				}
			}

			XBeeFirmware clonedFirmware = (XBeeFirmware)Clone();

			// Clone the categories and set them to the cloned firmware.
			List<XBeeCategory> clonedCategories = new List<XBeeCategory>();
			foreach (XBeeCategory category in Categories)
				clonedCategories.Add(category.CloneCategory(null, clonedFirmware));
			clonedFirmware.Categories = clonedCategories;

			return clonedFirmware;
		}

		/// <summary>
		/// Adds a setting to the list of settings of the firmware.
		/// </summary>
		/// <param name="category">The configuration category the setting belongs to.</param>
		/// <param name="atSettings">The setting to add.</param>
		/// <seealso cref="AbstractXBeeSetting"/>
		/// <seealso cref="XBeeCategory"/>
		private void AddATSettings(XBeeCategory category, List<AbstractXBeeSetting> atSettings)
		{
			foreach (AbstractXBeeSetting xbeeSetting in category.Settings)
				atSettings.Add(xbeeSetting);

			foreach (XBeeCategory subCategory in category.Categories)
				AddATSettings(subCategory, atSettings);
		}

		/// <summary>
		/// Adds the common settings of the prvided <paramref name="category"/> to the given 
		/// <paramref name="atCommonSettings"/> list.
		/// </summary>
		/// <param name="category">The category to look for common settings.</param>
		/// <param name="atCommonSettings">List of settings where common ones will be added.</param>
		/// <seealso cref="AbstractXBeeSetting"/>
		/// <seealso cref="XBeeCategory"/>
		private void AddCommonATSettings(XBeeCategory category, List<AbstractXBeeSetting> atCommonSettings)
		{
			foreach (AbstractXBeeSetting xbeeSetting in category.Settings)
			{
				if (!xbeeSetting.SupportsMultipleNetworks())
					atCommonSettings.Add(xbeeSetting);
			}

			foreach (XBeeCategory subCategory in category.Categories)
				AddCommonATSettings(subCategory, atCommonSettings);
		}

		/// <summary>
		/// Adds the dual settings of the prvided <paramref name="category"/> to the given 
		/// <paramref name="atDualSettings"/> list.
		/// </summary>
		/// <param name="category">The category to look for dual settings.</param>
		/// <param name="atDualSettings">List of settings where dual ones will be added.</param>
		/// <seealso cref="AbstractXBeeSetting"/>
		/// <seealso cref="XBeeCategory"/>
		private void AddDualATSettings(XBeeCategory category, List<AbstractXBeeSetting> atDualSettings)
		{
			foreach (AbstractXBeeSetting xbeeSetting in category.Settings)
			{
				if (xbeeSetting.SupportsMultipleNetworks())
					atDualSettings.Add(xbeeSetting);
			}

			foreach (XBeeCategory subCategory in category.Categories)
				AddDualATSettings(subCategory, atDualSettings);
		}

		/// <summary>
		/// Adds the number of settings contained in the given <paramref name="category"/> to the 
		/// provided <paramref name="atSettingsNumber"/> value.
		/// </summary>
		/// <param name="category">The category to get the number of settings that it contains.</param>
		/// <param name="atSettingsNumber">The value to append the number of settings of the category.</param>
		/// <seealso cref="XBeeCategory"/>
		private void AddATSettingsNumber(XBeeCategory category, int atSettingsNumber)
		{
			foreach (AbstractXBeeSetting xbeeSetting in category.Settings)
				atSettingsNumber += 1;

			foreach (XBeeCategory subCategory in category.Categories)
				AddATSettingsNumber(subCategory, atSettingsNumber);
		}

		/// <summary>
		/// Returns the setting corresponding to the provided <paramref name="atCommand"/> and located 
		/// in the given <paramref name="category"/>.
		/// </summary>
		/// <param name="atCommand">The AT command corresponding to the setting to get.</param>
		/// <param name="category">The configuration category where the setting should be found.</param>
		/// <returns>The setting corresponding to the provided AT command and category.</returns>
		/// <seealso cref="AbstractXBeeSetting"/>
		/// <seealso cref="XBeeCategory"/>
		private AbstractXBeeSetting GetAtSetting(string atCommand, XBeeCategory category)
		{
			foreach (AbstractXBeeSetting xbeeSetting in category.Settings)
			{
				if (xbeeSetting.AtCommand != null
					&& xbeeSetting.AtCommand.ToUpper().Equals(atCommand.ToUpper()))
					return xbeeSetting;
			}

			foreach (XBeeCategory subCategory in category.Categories)
			{
				AbstractXBeeSetting atSetting = GetAtSetting(atCommand, subCategory);
				if (atSetting != null)
					return atSetting;
			}
			return null;
		}
	}
}
