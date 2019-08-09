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

namespace XBeeLibrary.Core.Connection
{
	/// <summary>
	/// This enumeration lists all the available connections used to communicate with XBee devices.
	/// </summary>
	public enum ConnectionType : byte
	{
		SERIAL = 0x00,
		BLUETOOTH = 0x01,
		UNKNOWN = 0xFE
	}

	public static class ConnectionTypeExtensions
	{
		static IDictionary<ConnectionType, string> lookupTable = new Dictionary<ConnectionType, string>();

		static ConnectionTypeExtensions()
		{
			lookupTable.Add(ConnectionType.SERIAL, "Serial connection");
			lookupTable.Add(ConnectionType.BLUETOOTH, "Bluetooth connection");
		}

		/// <summary>
		/// Gets the <see cref="ConnectionType"/> associated with the specified ID <paramref name="value"/>.
		/// </summary>
		/// <param name="dumb"></param>
		/// <param name="value">ID value to retrieve <see cref="ConnectionType"/>.</param>
		/// <returns>The <see cref="ConnectionType"/> for the specified ID <paramref name="value"/>, 
		/// <see cref="ConnectionType.UNKNOWN"/> if it does not exist.</returns>
		public static ConnectionType Get(this ConnectionType dumb, byte value)
		{
			var values = Enum.GetValues(typeof(ConnectionType)).OfType<ConnectionType>();

			if (values.Cast<byte>().Contains(value))
				return (ConnectionType)value;

			return ConnectionType.UNKNOWN;
		}

		/// <summary>
		/// Gets the connection type value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The connection type value.</returns>
		public static byte GetValue(this ConnectionType source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the connection type name.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The connection type name.</returns>
		public static string GetName(this ConnectionType source)
		{
			return lookupTable.ContainsKey(source) ? lookupTable[source] : source.ToString();
		}

		/// <summary>
		/// Returns the <see cref="ConnectionType"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="ConnectionType"/> in string format.</returns>
		public static string ToDisplayString(this ConnectionType source)
		{
			return string.Format("({0}) {1}", HexUtils.ByteArrayToHexString(ByteUtils.IntToByteArray((byte)source)), GetName(source));
		}
	}
}