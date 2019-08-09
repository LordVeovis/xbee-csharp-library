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
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace XBeeLibrary.Core.Utils
{
	/// <summary>
	/// Utility class containing methods to work with several data type conversions.
	/// </summary>
	public class ParsingUtils
	{
		// Constants.
		private const string HEXES = "0123456789ABCDEF";

		public const string HEXADECIMAL_PREFIX = "0X";
		private const string HEXADECIMAL_PATTERN = @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z";

		public const string MXI_EXTENSION = ".mxi";
		public const string PRO_EXTENSION = ".pro";
		public const string XML_EXTENSION = ".xml";
		public const string XPRO_EXTENSION = ".xpro";

		public const string VALUE_TRUE = "true";
		public const string VALUE_FALSE = "false";

		public const string BLANK_SPACE_CODE = "0x20";
		
		/// <summary>
		/// Returns whether or not the given string value can be casted to Integer.
		/// </summary>
		/// <param name="stringValue">The string value to check.</param>
		/// <returns><c>true</c> if the value can be casted to Integer, <c>false</c> otherwise.</returns>
		public static bool IsInteger(string stringValue)
		{
			if (stringValue == null || stringValue.Trim().Length == 0)
				return false;

			return int.TryParse(stringValue.Trim(), out int i);
		}
		
		/// <summary>
		/// Returns whether or not the given string value can be casted to Integer.
		/// </summary>
		/// <param name="stringValue">The string value to check.</param>
		/// <returns><c>true</c> if the value can be casted to Integer, <c>false</c> otherwise.</returns>
		public static bool IsHexadecimal(string stringValue)
		{
			if (stringValue == null || stringValue.Trim().Length == 0)
				return false;
			
			Regex reg = new Regex(HEXADECIMAL_PATTERN);
			MatchCollection matches = reg.Matches(stringValue);

			return matches.Count > 0;
		}

		/// <summary>
		/// Returns the ASCII representation in string format of the given hexadecimal string.
		/// </summary>
		/// <param name="hexString">The hexadecimal string to convert to ASCII.</param>
		/// <returns>The ASCII representation in string format of the given hexadecimal string. 
		/// <c>Null</c> if the given string is not hexadecimal.</returns>
		public static string HexStringToAscii(string hexString)
		{
			string asciiValue = "";
			try
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i <= hexString.Length - 2; i += 2)
					sb.Append(Convert.ToString(Convert.ToChar(int.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber))));

				asciiValue = sb.ToString();
			}
			catch (FormatException)
			{
				// If the process fails, return null.
				asciiValue = null;
			}
			return asciiValue;
		}
		
		/// <summary>
		/// Returns the big integer value corresponding to the give hexadecimal value string.
		/// The string can come with or without the 0x prefix.
		/// </summary>
		/// <param name="hexValue">Hex string to convert to big integer.</param>
		/// <returns>The big integer value corresponding to the give hexadecimal value
		/// string, or <c>-1</c> if the process failed.</returns>
		public static BigInteger GetBigInt(string hexValue)
		{
			if (hexValue.ToUpper().StartsWith(HEXADECIMAL_PREFIX))
				hexValue = hexValue.Substring(HEXADECIMAL_PREFIX.Length);

			// Add initial 0 to parse value to BigInteger. This step
			// is necessary, otherwise the returning number is negative.
			if (!hexValue.StartsWith("0"))
				hexValue = "0" + hexValue;

			BigInteger value;
			try
			{
				value = BigInteger.Parse(hexValue, NumberStyles.AllowHexSpecifier);
			}
			catch (Exception)
			{
				value = -1;
			}
			return value;
		}
		
		/// <summary>
		/// Retrieves the integer value of the given hexadecimal string. The string can come with 
		/// or without the 0x prefix.
		/// </summary>
		/// <param name="hexValue">Hex string to convert to integer.</param>
		/// <returns>The integer value of the given hexadecimal string.</returns>
		public static int HexStringToInt(string hexValue)
		{
			if (hexValue.ToUpper().StartsWith(HEXADECIMAL_PREFIX))
				hexValue = hexValue.ToUpper().Replace(HEXADECIMAL_PREFIX, "");

			return int.Parse(hexValue, NumberStyles.HexNumber);
		}
		
		/// <summary>
		/// Converts the given hex string into a byte array.
		/// </summary>
		/// <param name="value">Hex string to convert to.</param>
		/// <returns>Byte array of the given hex string.</returns>
		public static byte[] HexStringToByteArray(string value)
		{
			value = value.Trim();
			if (value.ToUpper().StartsWith(HEXADECIMAL_PREFIX))
				value = value.ToUpper().Substring(HEXADECIMAL_PREFIX.Length);

			int len = value.Length;
			if (len % 2 != 0)
			{
				value = "0" + value;
				len = value.Length;
			}
			byte[] data = new byte[len / 2];
			for (int i = 0; i < len; i += 2)
				data[i / 2] = (byte)((int.Parse(value[i].ToString(), NumberStyles.HexNumber) << 4) + int.Parse(value[i + 1].ToString(), NumberStyles.HexNumber));

			return data;
		}
		
		/// <summary>
		/// Converts the given byte array (4 bytes length max) into an integer.
		/// </summary>
		/// <param name="b">Byte array to convert to integer (4 bytes length max).</param>
		/// <returns>Converted integer.</returns>
		public static int ByteArrayToInt(byte[] b)
		{
			byte[] values = b;
			if (b.Length < 4)
			{
				values = new byte[4];
				int diff = 4 - b.Length;
				for (int i = 0; i < diff; i++)
					values[i] = 0;
				for (int i = diff; i < 4; i++)
					values[i] = b[i - diff];
			}
			return ((values[0] & 0xFF) << 24)
					| ((values[1] & 0xFF) << 16)
					| ((values[2] & 0xFF) << 8)
					| (values[3] & 0xFF);
		}
		
		/// <summary>
		/// Retrieves the given integer as hexadecimal string.
		/// </summary>
		/// <param name="value">The integer value to convert to hexadecimal string.</param>
		/// <param name="minBytes">The minimum number of bytes to be represented.</param>
		/// <returns>The integer value as hexadecimal string.</returns>
		public static string IntegerToHexString(int value, int minBytes)
		{
			byte[] intAsByteArray = IntToByteArray(value);
			string intAsHexString = "";
			bool numberFound = false;
			for (int i = 0; i < intAsByteArray.Length; i++)
			{
				if (intAsByteArray[i] == 0x00 && !numberFound && intAsByteArray.Length - i > minBytes)
					continue;
				intAsHexString += ByteArrayToHexString(new byte[] { (byte)(intAsByteArray[i] & 0xFF) });
				numberFound = true;
			}
			return intAsHexString;
		}
		
		/// <summary>
		/// Converts the given integer into a byte array.
		/// </summary>
		/// <param name="value">Integer to convert to.</param>
		/// <returns>Byte array of the given integer (4 bytes length).</returns>
		public static byte[] IntToByteArray(int value)
		{
			return new byte[] {
				(byte)((value >> 24) & 0xFF),
				(byte)((value >> 16) & 0xFF),
				(byte)((value >> 8) & 0xFF),
				(byte)(value & 0xFF)
			};
		}
		
		/// <summary>
		/// Converts the given byte array into a hex string.
		/// </summary>
		/// <param name="value">Byte array to convert to hex string.</param>
		/// <returns>Converted byte array to hex string.</returns>
		public static string ByteArrayToHexString(byte[] value)
		{
			return ByteArrayToHexString(value, 0, value.Length);
		}
		
		/// <summary>
		/// Converts the given byte array into a hex string.
		/// </summary>
		/// <param name="value">Byte array to convert to hex string.</param>
		/// <param name="from">The initial index of the range to be converted, inclusive.</param>
		/// <param name="length">The number of array elements to be converted.</param>
		/// <returns>Converted byte array to hex string.</returns>
		public static string ByteArrayToHexString(byte[] value, int from, int length)
		{
			if (value == null)
				return null;

			if (from < 0 || length < 0)
				return null;

			if (length > value.Length - from)
				length = value.Length - from;

			StringBuilder hex = new StringBuilder(2 * length);
			for (int i = from; i < from + length; i++)
			{
				hex.Append(HEXES[(value[i] & 0xF0) >> 4])
					.Append(HEXES[(value[i] & 0x0F)]);
			}
			return hex.ToString();
		}
	}
}
