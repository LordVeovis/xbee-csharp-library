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
	/// Enumerates the different XBee local interfaces used in the Relay API packets.
	/// </summary>
	public enum XBeeLocalInterface : byte
	{
		// Enumeration entries.
		SERIAL = 0x00,
		BLUETOOTH = 0x01,
		MICROPYTHON = 0x02,
		UNKNOWN = 0xFF
	}

	public static class XBeeLocalInterfaceExtensions
	{
		static IDictionary<XBeeLocalInterface, string> lookupTable = new Dictionary<XBeeLocalInterface, string>();

		static XBeeLocalInterfaceExtensions()
		{
			lookupTable.Add(XBeeLocalInterface.SERIAL, "Serial port");
			lookupTable.Add(XBeeLocalInterface.BLUETOOTH, "Bluetooth Low Energy");
			lookupTable.Add(XBeeLocalInterface.MICROPYTHON, "MicroPython");
			lookupTable.Add(XBeeLocalInterface.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the XBee local interface value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The XBee local interface value.</returns>
		public static byte GetValue(this XBeeLocalInterface source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the XBee local interface description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The XBee local interface description.</returns>
		public static string GetDescription(this XBeeLocalInterface source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the <see cref="XBeeLocalInterface"/> associated with the specified ID <paramref name="value"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">ID value to retrieve <see cref="XBeeLocalInterface"/>.</param>
		/// <returns>The <see cref="XBeeLocalInterface"/> for the specified ID <paramref name="value"/>,
		/// <see cref="XBeeLocalInterface.UNKNOWN"/> if it does not exist.</returns>
		/// <seealso cref="XBeeLocalInterface.UNKNOWN"/>
		public static XBeeLocalInterface Get(this XBeeLocalInterface source, byte value)
		{
			var values = Enum.GetValues(typeof(XBeeLocalInterface)).OfType<XBeeLocalInterface>();

			if (values.Cast<byte>().Contains(value))
				return (XBeeLocalInterface)value;

			return XBeeLocalInterface.UNKNOWN;
		}

		/// <summary>
		/// Returns the <see cref="XBeeLocalInterface"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="XBeeLocalInterface"/> in string format.</returns>
		public static string ToDisplayString(this XBeeLocalInterface source)
		{
			return HexUtils.ByteToHexString((byte)source) + ": " + lookupTable[source];
		}
	}
}
