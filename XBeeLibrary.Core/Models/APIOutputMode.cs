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
using System.Linq;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates the different API output modes. The API output mode establishes
	/// the way data will be output through the serial interface of an XBee device.
	/// </summary>
	public enum APIOutputMode : byte
	{
		// Enumeration entries.
		MODE_NATIVE = 0x00,
		MODE_EXPLICIT = 0x01,
		MODE_EXPLICIT_ZDO_PASSTHRU = 0x03,
		MODE_UNKNOWN = 0xFF
	}

	public static class APIOutputModeExtensions
	{
		static IDictionary<APIOutputMode, string> lookupTable = new Dictionary<APIOutputMode, string>();

		static APIOutputModeExtensions()
		{
			lookupTable.Add(APIOutputMode.MODE_NATIVE, "Native");
			lookupTable.Add(APIOutputMode.MODE_EXPLICIT, "Explicit");
			lookupTable.Add(APIOutputMode.MODE_EXPLICIT_ZDO_PASSTHRU, "Explicit with ZDO Passthru");
			lookupTable.Add(APIOutputMode.MODE_UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the API output mode value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>API output mode value.</returns>
		public static byte GetValue(this APIOutputMode source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the API output mode description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>API output mode description.</returns>
		public static string GetDescription(this APIOutputMode source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the <see cref="APIOutputMode"/> associated with the specified ID <paramref name="value"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">ID value to retrieve <see cref="APIOutputMode"/>.</param>
		/// <returns>The <see cref="APIOutputMode"/> for the specified ID <paramref name="value"/>,
		/// <see cref="APIOutputMode.MODE_UNKNOWN"/> if it does not exist.</returns>
		public static APIOutputMode Get(this APIOutputMode source, byte value)
		{
			var values = Enum.GetValues(typeof(APIOutputMode)).OfType<APIOutputMode>();

			if (values.Cast<byte>().Contains(value))
				return (APIOutputMode)value;

			return APIOutputMode.MODE_UNKNOWN;
		}

		/// <summary>
		/// Gets the string representation of the API output mode.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>String representation of the API output mode.</returns>
		public static string ToDisplayString(this APIOutputMode source)
		{
			return HexUtils.ByteToHexString((byte)source) + ": " + lookupTable[source];
		}
	}
}
