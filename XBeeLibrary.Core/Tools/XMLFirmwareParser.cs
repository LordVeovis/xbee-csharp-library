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
using System.IO;
using System.Xml.Linq;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Tools
{
	/// <summary>
	/// Class to parse an XML firmware configuration file.
	/// </summary>
	public class XMLFirmwareParser
	{
		// Constants.
		private const string ERROR_INVALID_PATH = "Radio firmware path is not valid.";
		private const string ERROR_INVALID_XML_CONTENTS = "Invalid XML contents in file '{0}': {1}";
		private const string ERROR_INVALID_XML_CONTENTS_STRING= "Invalid data: {1}";
		private const string ERROR_FIRMWARE_NOT_FOUND = "Could not find any firmware item in the XML file.";
		private const string MSG_FIRMWARES_NOT_FOUND = "TAG '<firmwares>' couldn't be found.";

		/// <summary>
		/// Parses the given file and returns the XBee firmware objects found.
		/// </summary>
		/// <param name="file">File to parse.</param>
		/// <param name="fullParse">Determines if the parsing process will also include the categories with 
		/// settings and configuration commands (true) or will only parse the firmware's basic 
		/// information (false).</param>
		/// <returns>A list with the XBee firmware objects found in the file, <c>null</c> if error.</returns>
		/// <exception cref="ParsingException">If there is any problem parsing the firmware XML file.</exception>
		public static List<XBeeFirmware> ParseFile(FileInfo file, bool fullParse)
		{
			List<XBeeFirmware> xbeeFirmwares = new List<XBeeFirmware>();

			XDocument document = XDocument.Load(file.FullName);

			// Verify if the file has the expected contents and there is any 
			// XBee firmware defined inside.
			XElement firmwaresElement = document.Root;
			if (firmwaresElement == null || !firmwaresElement.Name.ToString().Equals(XMLFirmwareConstants.ITEM_FIRMWARES))
				throw new ParsingException(string.Format(ERROR_INVALID_XML_CONTENTS, file.FullName, MSG_FIRMWARES_NOT_FOUND));

			List<XElement> firmwaresList = new List<XElement>();
			foreach (XElement element in firmwaresElement.Elements())
			{
				if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_FIRMWARE))
					firmwaresList.Add(element);
			}
			if (firmwaresList == null || firmwaresList.Count == 0)
				throw new ParsingException(ERROR_FIRMWARE_NOT_FOUND);

			foreach (XElement firmwareElement in firmwaresList)
			{
				// This is the list of necessary items to define a firmware. If any of them is 
				// not defined discard this firmware.
				string firmwareVersion = firmwareElement.Attribute(XMLFirmwareConstants.ATTRIBUTE_FW_VERSION).Value;
				if (firmwareVersion == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_FAMILY) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_PRODUCT_NAME) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_HW_VERSION) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_COMPATIBILITY_NUM) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_CONFIG_BUFFER_LOC) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_FLASH_PAGE_SIZE) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_CRC_BUFFER_LEN) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_FUNCTION) == null)
					continue;

				string family = firmwareElement.Element(XMLFirmwareConstants.ITEM_FAMILY).Value;
				string productName = firmwareElement.Element(XMLFirmwareConstants.ITEM_PRODUCT_NAME).Value;
				string hwVersion = firmwareElement.Element(XMLFirmwareConstants.ITEM_HW_VERSION).Value;
				if (!hwVersion.ToUpper().StartsWith(ParsingUtils.HEXADECIMAL_PREFIX))
				{
					if (!ParsingUtils.IsInteger(hwVersion))
					{
						hwVersion = MXIHardwareVersionDictionary.GetDictionaryValue(hwVersion.ToUpper());
					}
					else
					{
						string prefix = hwVersion.Length > 1 ? "0x" : "0x0";
						hwVersion = prefix + hwVersion;
					}
				}
				string compatibilityNumber = firmwareElement.Element(XMLFirmwareConstants.ITEM_COMPATIBILITY_NUM).Value;
				string configBufferLocation = firmwareElement.Element(XMLFirmwareConstants.ITEM_CONFIG_BUFFER_LOC).Value;
				string flashPageSize = firmwareElement.Element(XMLFirmwareConstants.ITEM_FLASH_PAGE_SIZE).Value;
				string crcBufferLength = firmwareElement.Element(XMLFirmwareConstants.ITEM_CRC_BUFFER_LEN).Value;
				string functionSet = firmwareElement.Element(XMLFirmwareConstants.ITEM_FUNCTION).Value;

				// Check if Region item exists.
				string region = "99";
				if (firmwareElement.Element(XMLFirmwareConstants.ITEM_REGION) != null)
					region = firmwareElement.Element(XMLFirmwareConstants.ITEM_REGION).Value;

				// Generate the XBee firmware object.
				XBeeFirmware xbeeFirmware = new XBeeFirmware(family, productName, hwVersion,
					compatibilityNumber, firmwareVersion, configBufferLocation, flashPageSize,
					crcBufferLength, functionSet)
				{
					// Set the region of the firmware.
					Region = region,

					// Set the content of the firmware.
					DefinitionFileContent = new StreamReader(file.OpenRead()).ReadToEnd()
				};

				// Set the path of the definition file in the XBee firmware.
				xbeeFirmware.SetDefinitionFilePath(file.FullName);

				// Check if Modem item exists.
				if (firmwareElement.Element(XMLFirmwareConstants.ITEM_MODEM) != null)
				{
					XElement modemElement = firmwareElement.Element(XMLFirmwareConstants.ITEM_MODEM);
					if (modemElement.Element(XMLFirmwareConstants.ITEM_MODEM_VERISON) != null)
					{
						xbeeFirmware.ModemVersion = modemElement.Element(XMLFirmwareConstants.ITEM_MODEM_VERISON).Value;
						if (modemElement.Element(XMLFirmwareConstants.ITEM_MODEM_URL) != null)
							xbeeFirmware.ModemUrl = modemElement.Element(XMLFirmwareConstants.ITEM_MODEM_URL).Value;
					}
				}

				// If a full parse is not needed, continue.
				if (!fullParse)
				{
					xbeeFirmwares.Add(xbeeFirmware);
					continue;
				}

				xbeeFirmware.Initialized = true;

				// Parse and add the categories with their corresponding settings.
				XElement categoriesElement = firmwareElement.Element(XMLFirmwareConstants.ITEM_CATEGORIES);
				if (categoriesElement != null)
				{
					List<XBeeCategory> xbeeCategories = ParseCategories(categoriesElement, null, xbeeFirmware);
					if (xbeeCategories != null && xbeeCategories.Count > 0)
						xbeeFirmware.Categories = xbeeCategories;
				}

				xbeeFirmwares.Add(xbeeFirmware);
			}

			return xbeeFirmwares;
		}

		/// <summary>
		/// Parses the given file and returns the XBee firmware objects found.
		/// </summary>
		/// <param name="firmwareString">String containing the firmware configuration information 
		/// to parse.</param>
		/// <param name="fullParse">Determines if the parsing process will also include the categories
		/// with settings and configuration commands (true) or will only parse the firmware's basic 
		/// information (false).</param>
		/// <returns>A list with the XBee firmware objects found in the file, <c>null</c> if error.</returns>
		/// <exception cref="ParsingException">If there is any problem parsing the firmware XML file.</exception>
		public static List<XBeeFirmware> ParseString(string firmwareString, bool fullParse)
		{
			List<XBeeFirmware> xbeeFirmwares = new List<XBeeFirmware>();

			XDocument document = XDocument.Parse(firmwareString);

			// Verify if the file has the expected contents and there is any 
			// XBee firmware defined inside.
			XElement firmwaresElement = document.Root;
			if (firmwaresElement == null || !firmwaresElement.Name.ToString().Equals(XMLFirmwareConstants.ITEM_FIRMWARES))
				throw new ParsingException(string.Format(ERROR_INVALID_XML_CONTENTS_STRING, MSG_FIRMWARES_NOT_FOUND));

			List<XElement> firmwaresList = new List<XElement>();
			foreach (XElement element in firmwaresElement.Elements())
			{
				if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_FIRMWARE))
					firmwaresList.Add(element);
			}
			if (firmwaresList == null || firmwaresList.Count == 0)
				throw new ParsingException(ERROR_FIRMWARE_NOT_FOUND);

			foreach (XElement firmwareElement in firmwaresList)
			{
				// This is the list of necessary items to define a firmware. If any of them is 
				// not defined discard this firmware.
				string firmwareVersion = firmwareElement.Attribute(XMLFirmwareConstants.ATTRIBUTE_FW_VERSION).Value;
				if (firmwareVersion == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_FAMILY) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_PRODUCT_NAME) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_HW_VERSION) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_COMPATIBILITY_NUM) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_CONFIG_BUFFER_LOC) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_FLASH_PAGE_SIZE) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_CRC_BUFFER_LEN) == null
					|| firmwareElement.Element(XMLFirmwareConstants.ITEM_FUNCTION) == null)
					continue;

				string family = firmwareElement.Element(XMLFirmwareConstants.ITEM_FAMILY).Value;
				string productName = firmwareElement.Element(XMLFirmwareConstants.ITEM_PRODUCT_NAME).Value;
				string hwVersion = firmwareElement.Element(XMLFirmwareConstants.ITEM_HW_VERSION).Value;
				if (!hwVersion.ToUpper().StartsWith(ParsingUtils.HEXADECIMAL_PREFIX))
				{
					if (!ParsingUtils.IsInteger(hwVersion))
					{
						hwVersion = MXIHardwareVersionDictionary.GetDictionaryValue(hwVersion.ToUpper());
					}
					else
					{
						string prefix = hwVersion.Length > 1 ? "0x" : "0x0";
						hwVersion = prefix + hwVersion;
					}
				}
				string compatibilityNumber = firmwareElement.Element(XMLFirmwareConstants.ITEM_COMPATIBILITY_NUM).Value;
				string configBufferLocation = firmwareElement.Element(XMLFirmwareConstants.ITEM_CONFIG_BUFFER_LOC).Value;
				string flashPageSize = firmwareElement.Element(XMLFirmwareConstants.ITEM_FLASH_PAGE_SIZE).Value;
				string crcBufferLength = firmwareElement.Element(XMLFirmwareConstants.ITEM_CRC_BUFFER_LEN).Value;
				string functionSet = firmwareElement.Element(XMLFirmwareConstants.ITEM_FUNCTION).Value;

				// Check if Region item exists.
				string region = "99";
				if (firmwareElement.Element(XMLFirmwareConstants.ITEM_REGION) != null)
					region = firmwareElement.Element(XMLFirmwareConstants.ITEM_REGION).Value;

				// Generate the XBee firmware object.
				XBeeFirmware xbeeFirmware = new XBeeFirmware(family, productName, hwVersion,
					compatibilityNumber, firmwareVersion, configBufferLocation, flashPageSize,
					crcBufferLength, functionSet)
				{
					// Set the region of the firmware.
					Region = region,

					// Set the content of the firmware.
					DefinitionFileContent = firmwareString
				};

				// Check if Modem item exists.
				if (firmwareElement.Element(XMLFirmwareConstants.ITEM_MODEM) != null)
				{
					XElement modemElement = firmwareElement.Element(XMLFirmwareConstants.ITEM_MODEM);
					if (modemElement.Element(XMLFirmwareConstants.ITEM_MODEM_VERISON) != null)
					{
						xbeeFirmware.ModemVersion = modemElement.Element(XMLFirmwareConstants.ITEM_MODEM_VERISON).Value;
						if (modemElement.Element(XMLFirmwareConstants.ITEM_MODEM_URL) != null)
							xbeeFirmware.ModemUrl = modemElement.Element(XMLFirmwareConstants.ITEM_MODEM_URL).Value;
					}
				}

				// If a full parse is not needed, continue.
				if (!fullParse)
				{
					xbeeFirmwares.Add(xbeeFirmware);
					continue;
				}

				xbeeFirmware.Initialized = true;

				// Parse and add the categories with their corresponding settings.
				XElement categoriesElement = firmwareElement.Element(XMLFirmwareConstants.ITEM_CATEGORIES);
				if (categoriesElement != null)
				{
					List<XBeeCategory> xbeeCategories = ParseCategories(categoriesElement, null, xbeeFirmware);
					if (xbeeCategories != null && xbeeCategories.Count > 0)
						xbeeFirmware.Categories = xbeeCategories;
				}

				xbeeFirmwares.Add(xbeeFirmware);
			}

			return xbeeFirmwares;
		}

		/// <summary>
		/// Reads the XML firmware description file of the XBee firmware and fills the XBee categories 
		/// and configuration commands of the XBee firmware object.
		/// </summary>
		/// <param name="xbeeFirmware">The XBee firmware to fill.</param>
		/// <exception cref="ParsingException">If there is any problem parsing the firmware contents.</exception>
		public static void ParseFirmwareContents(XBeeFirmware xbeeFirmware)
		{
			xbeeFirmware.Initialized = true;

			XDocument document = null;
			string fullFileName = "";

			if (xbeeFirmware.DefinitionFileContent != null)
			{
				document = XDocument.Parse(xbeeFirmware.DefinitionFileContent);
			}
			else
			{
				FileInfo definitionFile = new FileInfo(xbeeFirmware.DefinitionFileLocation);
				document = XDocument.Load(definitionFile.FullName);
				fullFileName = definitionFile.FullName;
			}

			XElement firmwaresElement = document.Root;
			if (firmwaresElement == null || !firmwaresElement.Name.ToString().Equals(XMLFirmwareConstants.ITEM_FIRMWARES))
			{
				if (xbeeFirmware.DefinitionFileContent != null)
					throw new ParsingException(string.Format(ERROR_INVALID_XML_CONTENTS_STRING, MSG_FIRMWARES_NOT_FOUND));
				else
					throw new ParsingException(string.Format(ERROR_INVALID_XML_CONTENTS, fullFileName, MSG_FIRMWARES_NOT_FOUND));
			}

			List<XElement> firmwaresList = new List<XElement>();
			foreach (XElement element in firmwaresElement.Elements())
			{
				if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_FIRMWARE))
					firmwaresList.Add(element);
			}
			if (firmwaresList == null || firmwaresList.Count == 0)
				throw new ParsingException(ERROR_FIRMWARE_NOT_FOUND);

			XElement selectedFirmwareElement = null;
			foreach (XElement firmwareElement in firmwaresList)
			{
				string firmwareVersion = firmwareElement.Attribute(XMLFirmwareConstants.ATTRIBUTE_FW_VERSION).Value;
				if (firmwareVersion.Equals(xbeeFirmware.FirmwareVersion))
				{
					selectedFirmwareElement = firmwareElement;
					break;
				}
			}
			if (selectedFirmwareElement == null)
				return;

			// Parse and add the categories with their corresponding settings.
			XElement categoriesElement = selectedFirmwareElement.Element(XMLFirmwareConstants.ITEM_CATEGORIES);
			if (categoriesElement != null)
			{
				List<XBeeCategory> xbeeCategories = ParseCategories(categoriesElement, null, xbeeFirmware);
				if (xbeeCategories != null && xbeeCategories.Count > 0)
					xbeeFirmware.Categories = xbeeCategories;
			}
		}

		/// <summary>
		/// Returns the first <see cref="XBeeFirmware"/> object found in the given path with the 
		/// provided name.
		/// </summary>
		/// <param name="fileName">The name of the firmware file to obtain the <see cref="XBeeFirmware"/> 
		/// object from.</param>
		/// <param name="path">The path to look for the XBee firmware.</param>
		/// <returns>The <see cref="XBeeFirmware"/> object corresponding to the file.</returns>
		/// <exception cref="ParsingException">If there is any problem getting the XBee firmware from the file name.
		/// </exception>
		public static XBeeFirmware GetFirmwareFromFileName(string fileName, string path)
		{
			FileAttributes fileAttrs = File.GetAttributes(path);

			DirectoryInfo containerDir;
			if ((fileAttrs & FileAttributes.Directory) == FileAttributes.Directory)
			{
				// This is a directory.
				containerDir = new DirectoryInfo(path);
			}
			else
			{
				// This is a file or the selected element does not exist.
				throw new ParsingException(ERROR_INVALID_PATH);
			}

			FileInfo firmwareFile = GetFirmwareFromFileName(fileName, containerDir);
			if (firmwareFile == null)
				throw new ParsingException(ERROR_FIRMWARE_NOT_FOUND);

			List<XBeeFirmware> xbeeFirmwareList = ParseFile(firmwareFile, false);
			if (xbeeFirmwareList.Count > 0)
				return xbeeFirmwareList[0];
			else
				throw new ParsingException(ERROR_FIRMWARE_NOT_FOUND);
		}

		/// <summary>
		/// Parses the given XML element returning a list with the XBee categories found.
		/// </summary>
		/// <param name="categoriesElement">The XML element to parse.</param>
		/// <param name="parentCategory">The XBee category where the categories should be located, 
		/// <c>null</c> if they are root categories.</param>
		/// <param name="ownerFirmware">The owner firmware of the categories to parse.</param>
		/// <returns>A list with the XBee categories found.</returns>
		private static List<XBeeCategory> ParseCategories(XElement categoriesElement, XBeeCategory parentCategory,
			XBeeFirmware ownerFirmware)
		{
			List<XBeeCategory> xbeeCategories = new List<XBeeCategory>();

			List<XElement> categoriesList = new List<XElement>();
			foreach (XElement element in categoriesElement.Elements())
			{
				if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_CATEGORY))
					categoriesList.Add(element);
			}
			if (categoriesList == null || categoriesList.Count == 0)
				return null;

			foreach (XElement categoryElement in categoriesList)
			{
				string name = categoryElement.Attribute(XMLFirmwareConstants.ATTRIBUTE_NAME).Value;
				string description = null;

				// Necessary items to parse and create an XBee category, if they are not defined 
				// continue the parsing process.
				if (name == null || categoryElement.Element(XMLFirmwareConstants.ITEM_DESCRIPTION) == null)
					continue;

				// Parse the possible categories inside a category.
				description = categoryElement.Element(XMLFirmwareConstants.ITEM_DESCRIPTION).Value;
				XBeeCategory xbeeCategory = new XBeeCategory(name, description, parentCategory, ownerFirmware);

				if (categoryElement.Element(XMLFirmwareConstants.ITEM_SETTINGS) != null)
				{
					List<AbstractXBeeSetting> settings = ParseSettings(categoryElement.Element(XMLFirmwareConstants.ITEM_SETTINGS),
						xbeeCategory, ownerFirmware);
					if (settings != null && settings.Count > 0)
						xbeeCategory.Settings = settings;
				}
				// Parse the possible settings inside a category.
				if (categoryElement.Element(XMLFirmwareConstants.ITEM_CATEGORIES) != null)
				{
					List<XBeeCategory> parsedCategories = ParseCategories(categoryElement.Element(XMLFirmwareConstants.ITEM_CATEGORIES),
						xbeeCategory, ownerFirmware);
					if (parsedCategories != null && parsedCategories.Count > 0)
						xbeeCategory.Categories = parsedCategories;
				}

				xbeeCategories.Add(xbeeCategory);
			}

			// Sort the categories list to put in first place the Special category (if the 
			// list contains it).
			xbeeCategories.Sort(new XBeeCategoriesSorter());

			return xbeeCategories;
		}
		
		/// <summary>
		/// Parses the given XML element returning a list with the XBee settings found.
		/// </summary>
		/// <param name="settingsElement">The XML element to parse.</param>
		/// <param name="parentCategory">The XBee category where the settings should be located.</param>
		/// <param name="ownerFirmware">The owner firmware of the settings to parse.</param>
		/// <returns>A list with the XBee settings found.</returns>
		private static List<AbstractXBeeSetting> ParseSettings(XElement settingsElement, XBeeCategory parentCategory,
			XBeeFirmware ownerFirmware)
		{
			List<AbstractXBeeSetting> xbeeSettings = new List<AbstractXBeeSetting>();

			List<XElement> atSettingsList = new List<XElement>();
			foreach (XElement element in settingsElement.Elements())
			{
				if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_SETTING))
					atSettingsList.Add(element);
			}
			List<XElement> bufferSettingsList = new List<XElement>();
			foreach (XElement element in settingsElement.Elements())
			{
				if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_BUFFER_SETTING))
					bufferSettingsList.Add(element);
			}


			if ((atSettingsList == null || atSettingsList.Count == 0)
				&& (bufferSettingsList == null || bufferSettingsList.Count == 0))
				return null;

			// We do not know if the AT and buffer settings can live in the same category, so suppose YES so 
			// do it the hard way.
			// Parse all the AT settings.
			foreach (XElement settingElement in atSettingsList)
			{
				AbstractXBeeSetting xbeeSetting = ParseATSetting(settingElement, parentCategory, ownerFirmware);
				if (xbeeSetting != null)
					xbeeSettings.Add(xbeeSetting);
			}

			return xbeeSettings;
		}
		
		/// <summary>
		/// Parse the given setting element assuming it is an AT setting. Return the setting object 
		/// corresponding to the XML element.
		/// </summary>
		/// <param name="settingElement">The XML element to parse.</param>
		/// <param name="parentCategory">The parent category (where the setting will be placed).</param>
		/// <param name="ownerFirmware">The owner firmware of the settings to parse.</param>
		/// <returns>The setting object corresponding to the given XML element.</returns>
		private static AbstractXBeeSetting ParseATSetting(XElement settingElement, XBeeCategory parentCategory,
			XBeeFirmware ownerFirmware)
		{
			AbstractXBeeSetting xbeeSetting = null;

			// As this is an AT command setting, read the AT command.
			string atCommand = null;
			if (settingElement.Attribute(XMLFirmwareConstants.ATTRIBUTE_COMMAND) != null)
				atCommand = settingElement.Attribute(XMLFirmwareConstants.ATTRIBUTE_COMMAND).Value;

			string name = null;
			string description = null;
			string defaultValue = null;
			string controlType = null;

			int numNetworks = 1;

			// Necessary items to parse and create an AT setting, if they are not defined, return a 
			// null setting.
			if (settingElement.Element(XMLFirmwareConstants.ITEM_NAME) == null
				|| settingElement.Element(XMLFirmwareConstants.ITEM_DESCRIPTION) == null
				|| settingElement.Element(XMLFirmwareConstants.ITEM_CONTROL_TYPE) == null)
				return null;

			name = settingElement.Element(XMLFirmwareConstants.ITEM_NAME).Value.Trim();
			description = settingElement.Element(XMLFirmwareConstants.ITEM_DESCRIPTION).Value.Trim();
			controlType = settingElement.Element(XMLFirmwareConstants.ITEM_CONTROL_TYPE).Value.Trim();
			// Check if the setting has the number of networks field.
			if (settingElement.Element(XMLFirmwareConstants.ITEM_NETWORKS) != null)
				int.TryParse(settingElement.Element(XMLFirmwareConstants.ITEM_NETWORKS).Value.Trim(), out numNetworks);

			// There are settings that may not have a default value defined.
			if (settingElement.Element(XMLFirmwareConstants.ITEM_DEFAULT_VALUE) != null)
			{
				defaultValue = settingElement.Element(XMLFirmwareConstants.ITEM_DEFAULT_VALUE).Value;
				if (defaultValue.ToLower().StartsWith("0x"))
					defaultValue = defaultValue.Substring(2);
			}

			// Only the button setting is allowed to not have an AT command associated, so 
			// if the read AT command is <c>null</c>, return null.
			if (!controlType.Equals(XMLFirmwareConstants.SETTING_TYPE_BUTTON) && atCommand == null)
			{
				return null;
			}

			// Generate the setting object and fill it depending on the control type.
			if (controlType.Equals(XMLFirmwareConstants.SETTING_TYPE_BUTTON))
			{
				xbeeSetting = new XBeeSettingButton(name, description, parentCategory, ownerFirmware, numNetworks);
				FillButtonSetting(settingElement, (XBeeSettingButton)xbeeSetting);
			}
			else if (controlType.Equals(XMLFirmwareConstants.SETTING_TYPE_COMBO))
			{
				xbeeSetting = new XBeeSettingCombo(atCommand, name, description, defaultValue, parentCategory, ownerFirmware, numNetworks);
				FillComboSetting(settingElement, (XBeeSettingCombo)xbeeSetting);
			}
			else if (controlType.Equals(XMLFirmwareConstants.SETTING_TYPE_NONE))
			{
				xbeeSetting = new XBeeSettingNoControl(atCommand, name, description, defaultValue, parentCategory, ownerFirmware, numNetworks);
				FillNoControlSetting(settingElement, (XBeeSettingNoControl)xbeeSetting);
			}
			else if (controlType.Equals(XMLFirmwareConstants.SETTING_TYPE_NON_EDITABLE_STRING))
			{
				xbeeSetting = new XBeeSettingNoControl(atCommand, name, description, defaultValue, parentCategory, ownerFirmware, numNetworks);
				FillNoControlSetting(settingElement, (XBeeSettingNoControl)xbeeSetting);
				((XBeeSettingNoControl)xbeeSetting).Format = Format.ASCII;
			}
			else if (controlType.Equals(XMLFirmwareConstants.SETTING_TYPE_NUMBER))
			{
				xbeeSetting = new XBeeSettingNumber(atCommand, name, description, defaultValue, parentCategory, ownerFirmware, numNetworks);
				FillNumberSetting(settingElement, (XBeeSettingNumber)xbeeSetting);
			}
			else if (controlType.Equals(XMLFirmwareConstants.SETTING_TYPE_TEXT))
			{
				xbeeSetting = new XBeeSettingText(atCommand, name, description, defaultValue, parentCategory, ownerFirmware, numNetworks);
				FillTextSetting(settingElement, (XBeeSettingText)xbeeSetting);
			}

			return xbeeSetting;
		}

		/// <summary>
		/// Reads the text setting specific parameters from the XML element and fills
		/// the given text setting with them.
		/// </summary>
		/// <param name="settingElement">The XML element to read the specific parameters from.</param>
		/// <param name="textSetting">The text setting to be filled.</param>
		private static void FillTextSetting(XElement settingElement, XBeeSettingText textSetting)
		{
			if (settingElement.Element(XMLFirmwareConstants.ITEM_MIN_CHARS) != null
				&& ParsingUtils.IsInteger(settingElement.Element(XMLFirmwareConstants.ITEM_MIN_CHARS).Value.Trim()))
				textSetting.MinChars = int.Parse(settingElement.Element(XMLFirmwareConstants.ITEM_MIN_CHARS).Value.Trim());

			if (settingElement.Element(XMLFirmwareConstants.ITEM_MAX_CHARS) != null
				&& ParsingUtils.IsInteger(settingElement.Element(XMLFirmwareConstants.ITEM_MAX_CHARS).Value.Trim()))
				textSetting.MaxChars = int.Parse(settingElement.Element(XMLFirmwareConstants.ITEM_MAX_CHARS).Value.Trim());

			if (settingElement.Element(XMLFirmwareConstants.ITEM_FORMAT) != null)
			{
				string formatString = settingElement.Element(XMLFirmwareConstants.ITEM_FORMAT).Value.ToUpper();
				Format format = Format.UNKNOWN.Get(formatString);
				if (format == Format.UNKNOWN)
					format = Format.ASCII;

				textSetting.Format = format;
			}
			if (settingElement.Element(XMLFirmwareConstants.ITEM_EXCEPTION) != null)
				textSetting.ExceptionValue = settingElement.Element(XMLFirmwareConstants.ITEM_EXCEPTION).Value;
		}
		
		/// <summary>
		/// Reads the number setting specific parameters from the XML element and fills the given 
		/// number setting with them.
		/// </summary>
		/// <param name="settingElement">The XML element to read the specific parameters from.</param>
		/// <param name="numberSetting">The number setting to be filled.</param>
		private static void FillNumberSetting(XElement settingElement, XBeeSettingNumber numberSetting)
		{
			if (settingElement.Element(XMLFirmwareConstants.ITEM_RANGE_MIN) != null
				&& settingElement.Element(XMLFirmwareConstants.ITEM_RANGE_MAX) != null)
			{
				string rangeMinValue = settingElement.Element(XMLFirmwareConstants.ITEM_RANGE_MIN).Value.Trim();
				string rangeMaxValue = settingElement.Element(XMLFirmwareConstants.ITEM_RANGE_MAX).Value.Trim();
				AddRange(numberSetting, rangeMinValue, rangeMaxValue);
			}
			if (settingElement.Element(XMLFirmwareConstants.ITEM_RANGES) != null)
			{
				XElement rangesElement = settingElement.Element(XMLFirmwareConstants.ITEM_RANGES);
				List<XElement> rangesList = new List<XElement>();
				foreach (XElement element in rangesElement.Elements())
				{
					if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_RANGE))
						rangesList.Add(element);
				}
				foreach (XElement rangeElement in rangesList)
				{
					if (rangeElement.Element(XMLFirmwareConstants.ITEM_RANGE_MIN) == null
						|| rangeElement.Element(XMLFirmwareConstants.ITEM_RANGE_MAX) == null)
						continue;

					string rangeMinValue = rangeElement.Element(XMLFirmwareConstants.ITEM_RANGE_MIN).Value.Trim();
					string rangeMaxValue = rangeElement.Element(XMLFirmwareConstants.ITEM_RANGE_MAX).Value.Trim();
					AddRange(numberSetting, rangeMinValue, rangeMaxValue);
				}
			}

			if (settingElement.Element(XMLFirmwareConstants.ITEM_UNITS) != null)
				numberSetting.Units = settingElement.Element(XMLFirmwareConstants.ITEM_UNITS).Value.Trim();

			List<string> additionalValues = new List<string>();
			XElement additionalValuesElement = settingElement.Element(XMLFirmwareConstants.ITEM_ADDITIONAL_VALUES);
			if (additionalValuesElement == null)
				return;

			List<XElement> additionalValuesList = new List<XElement>();
			foreach (XElement element in additionalValuesElement.Elements())
			{
				if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_ADDITIONAL_VALUE))
					additionalValuesList.Add(element);
			}
			if (additionalValuesList == null || additionalValuesList.Count == 0)
				return;

			foreach (XElement additionalValue in additionalValuesList)
				additionalValues.Add(additionalValue.Value);

			numberSetting.AdditionalValues = additionalValues;
		}

		/// <summary>
		/// Adds a new range to the list of ranges of the numeric setting.
		/// </summary>
		/// <param name="numberSetting">The XBee number setting to add the range to.</param>
		/// <param name="rangeMinValue">The minimum valid value of the range.</param>
		/// <param name="rangeMaxValue">The maximum valid value of the range.</param>
		private static void AddRange(XBeeSettingNumber numberSetting, string rangeMinValue, string rangeMaxValue)
		{
			Range range = new Range(rangeMinValue, rangeMaxValue);

			if (!range.RangeMin.ToLower().StartsWith("0x"))
			{
				// [XCTUNG-1180] Range may contain a reference to another AT command.
				if (ParsingUtils.IsHexadecimal(range.RangeMin))
				{
					range.RangeMin = "0x" + range.RangeMin;
				}
				else
				{
					// The min range depends on another AT command, so add the dependency.
					numberSetting.OwnerFirmware.AddDependency(range.RangeMin, numberSetting.AtCommand);
				}
			}
			if (!range.RangeMax.ToLower().StartsWith("0x"))
			{
				// [XCTUNG-1180] Range may contain a reference to another AT command.
				if (ParsingUtils.IsHexadecimal(range.RangeMax))
				{
					range.RangeMax = "0x" + range.RangeMax;
				}
				else
				{
					// The max range depends on another AT command, so add the dependency.
					numberSetting.OwnerFirmware.AddDependency(range.RangeMax, numberSetting.AtCommand);
				}
			}
			numberSetting.AddRange(range);
		}
		
		/// <summary>
		/// Reads the no_control setting specific parameters from the XML element and fills the given 
		/// <paramref name="noControlSetting"/> setting with them.
		/// </summary>
		/// <param name="settingElement">The XML element to read the specific parameters from.</param>
		/// <param name="noControlSetting">The no_control setting to be filled.</param>
		private static void FillNoControlSetting(XElement settingElement, XBeeSettingNoControl noControlSetting)
		{
			if (settingElement.Element(XMLFirmwareConstants.ITEM_FORMAT) != null)
			{
				string formatString = settingElement.Element(XMLFirmwareConstants.ITEM_FORMAT).Value.ToUpper();
				Format format = Format.UNKNOWN.Get(formatString);
				if (format == Format.UNKNOWN)
					format = Format.NOFORMAT;

				noControlSetting.Format = format;
			}
		}
		
		/// <summary>
		/// Reads the combo setting specific parameters from the XML element and fills the given 
		/// combo setting with them.
		/// </summary>
		/// <param name="settingElement">The XML element to read the specific parameters from.</param>
		/// <param name="comboSetting">The combo setting to be filled.</param>
		private static void FillComboSetting(XElement settingElement, XBeeSettingCombo comboSetting)
		{
			List<string> comboItems = new List<string>();

			XElement comboItemsElement = settingElement.Element(XMLFirmwareConstants.ITEM_ITEMS);
			if (comboItemsElement == null)
				return;

			List<XElement> comboItemsList = new List<XElement>();
			foreach (XElement element in comboItemsElement.Elements())
			{
				if (element.Name.ToString().Equals(XMLFirmwareConstants.ITEM_ITEM))
					comboItemsList.Add(element);
			}
			if (comboItemsList == null || comboItemsList.Count == 0)
				return;

			for (int i = 0; i < comboItemsList.Count; i++)
			{
				string index = ParsingUtils.IntegerToHexString(i, 1);
				while (index.StartsWith("0"))
				{
					if (index.Length == 1)
						break;

					index = index.Substring(1);
				}
				comboItems.Add(comboItemsList[i].Value + " [" + index + "]");
			}
			comboSetting.Items = comboItems;
		}
		
		/// <summary>
		/// Reads the button setting specific parameters from the XML element and fills the given button 
		/// setting with them.
		/// </summary>
		/// <param name="settingElement">The XML element to read the specific parameters from.</param>
		/// <param name="buttonSetting">The button setting to be filled.</param>
		private static void FillButtonSetting(XElement settingElement, XBeeSettingButton buttonSetting)
		{
			if (settingElement.Element(XMLFirmwareConstants.ITEM_FUNCTION_NUMBER) != null
				&& ParsingUtils.IsInteger(settingElement.Element(XMLFirmwareConstants.ITEM_FUNCTION_NUMBER).Value.Trim()))
				buttonSetting.FunctionNumber = int.Parse(settingElement.Element(XMLFirmwareConstants.ITEM_FUNCTION_NUMBER).Value.Trim());
		}

		/// <summary>
		/// Returns the first <see cref="FileInfo"/> object with the provided name found in the
		/// specified directory.
		/// </summary>
		/// <param name="fileName">The name of the file to get its corresponding <see cref="FileInfo"/> 
		/// object.</param>
		/// <param name="containerDir">The directory to look for the file.</param>
		/// <returns>The <see cref="FileInfo"/> corresponding to the file with the provided name.</returns>
		private static FileInfo GetFirmwareFromFileName(string fileName, DirectoryInfo containerDir)
		{
			FileSystemInfo[] files = containerDir.GetFileSystemInfos();
			foreach (FileSystemInfo file in files)
			{
				if (file is DirectoryInfo)
				{
					FileInfo firmwareFile = GetFirmwareFromFileName(fileName, (DirectoryInfo)file);
					if (firmwareFile != null)
						return firmwareFile;
				}
				else if (file is FileInfo && file.Name.ToLower().Equals(fileName.ToLower()))
				{
					return (FileInfo)file;
				}
			}
			return null;
		}
	}
}
