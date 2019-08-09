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

namespace XBeeLibrary.Core.Utils
{
	public static class XMLFirmwareConstants
	{
		// Constants.
		public const string ITEM_FIRMWARES = "firmwares";
		public const string ITEM_FIRMWARE = "firmware";
		public const string ITEM_FAMILY = "family";
		public const string ITEM_PRODUCT_NAME = "product_name";
		public const string ITEM_HW_VERSION = "hw_version";
		public const string ITEM_REGION = "region";
		public const string ITEM_COMPATIBILITY_NUM = "compatibility_number";
		public const string ITEM_CONFIG_BUFFER_LOC = "config_buffer_loc";
		public const string ITEM_FLASH_PAGE_SIZE = "flash_page_size";
		public const string ITEM_CRC_BUFFER_LEN = "crc_buffer_len";
		public const string ITEM_FUNCTION = "function";
		public const string ITEM_SETTINGS = "settings";
		public const string ITEM_SETTING = "setting";
		public const string ITEM_BUFFER_SETTING = "buffer_setting";
		public const string ITEM_CATEGORIES = "categories";
		public const string ITEM_CATEGORY = "category";
		public const string ITEM_CONFIG_COMMANDS = "config_commands";
		public const string ITEM_CONFIG_COMMAND = "config_command";
		public const string ITEM_NAME = "name";
		public const string ITEM_DESCRIPTION = "description";
		public const string ITEM_AT_COMMAND = "at_command";
		public const string ITEM_CONTROL_TYPE = "control_type";
		public const string ITEM_DEFAULT_VALUE = "default_value";
		public const string ITEM_RANGE_MIN = "range_min";
		public const string ITEM_RANGE_MAX = "range_max";
		public const string ITEM_UNITS = "units";
		public const string ITEM_EXCEPTION = "exception";
		public const string ITEM_MIN_CHARS = "min_chars";
		public const string ITEM_MAX_CHARS = "max_chars";
		public const string ITEM_FUNCTION_NUMBER = "function_number";
		public const string ITEM_BUFFER_LOCATION = "buffer_location";
		public const string ITEM_BYTES_NUMBER = "bytes_number";
		public const string ITEM_BYTE_INDEX = "byte_index";
		public const string ITEM_BYTES_RETURNED = "bytes_returned";
		public const string ITEM_FORMAT = "format";
		public const string ITEM_ITEMS = "items";
		public const string ITEM_ITEM = "item";
		public const string ITEM_ADDITIONAL_VALUES = "additional_values";
		public const string ITEM_ADDITIONAL_VALUE = "value";
		public const string ITEM_NETWORKS = "networks";
		public const string ITEM_RANGES = "ranges";
		public const string ITEM_RANGE = "range";
		public const string ITEM_MODEM = "modem";
		public const string ITEM_MODEM_VERISON = "modem_version";
		public const string ITEM_MODEM_URL = "modem_url";

		public const string ATTRIBUTE_FW_VERSION = "fw_version";
		public const string ATTRIBUTE_NAME = "name";
		public const string ATTRIBUTE_COMMAND = "command";

		public const string SETTING_TYPE_TEXT = "text";
		public const string SETTING_TYPE_COMBO = "combo";
		public const string SETTING_TYPE_NUMBER = "number";
		public const string SETTING_TYPE_NONE = "none";
		public const string SETTING_TYPE_NON_EDITABLE_STRING = "nestring"; // Non-editable string.
		public const string SETTING_TYPE_BUTTON = "button";
	}
}
