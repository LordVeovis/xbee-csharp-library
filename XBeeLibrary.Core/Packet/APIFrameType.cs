/*
 * Copyright 2019, Digi International Inc.
 * Copyright 2014, 2015, Sébastien Rault.
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

namespace XBeeLibrary.Core.Packet
{
	/// <summary>
	/// This enumeration lists all the available frame types used in any XBee protocol.
	/// </summary>
	public enum APIFrameType : byte
	{
		// Enumeration entries.
		TX_64 = 0x00,
		TX_16 = 0x01,
		AT_COMMAND = 0x08,
		AT_COMMAND_QUEUE = 0x09,
		TRANSMIT_REQUEST = 0x10,
		EXPLICIT_ADDRESSING_COMMAND_FRAME = 0x11,
		REMOTE_AT_COMMAND_REQUEST = 0x17,
		TX_SMS = 0x1F,
		TX_IPV4 = 0x20,
		TX_REQUEST_TLS_PROFILE = 0x23,
		BLE_UNLOCK = 0x2C,
		USER_DATA_RELAY = 0x2D,
		RX_64 = 0x80,
		RX_16 = 0x81,
		RX_IO_64 = 0x82,
		RX_IO_16 = 0x83,
		AT_COMMAND_RESPONSE = 0x88,
		TX_STATUS = 0x89,
		MODEM_STATUS = 0x8A,
		TRANSMIT_STATUS = 0x8B,
		RECEIVE_PACKET = 0x90,
		EXPLICIT_RX_INDICATOR = 0x91,
		IO_DATA_SAMPLE_RX_INDICATOR = 0x92,
		REMOTE_AT_COMMAND_RESPONSE = 0x97,
		RX_SMS = 0x9F,
		BLE_UNLOCK_RESPONSE = 0xAC,
		USER_DATA_RELAY_OUTPUT = 0xAD,
		RX_IPV4 = 0xB0,
		UNKNOWN = 0xFF
	}

	public static class APIFrameTypeExtensions
	{
		static IDictionary<APIFrameType, string> lookupTable = new Dictionary<APIFrameType, string>();

		static APIFrameTypeExtensions()
		{
			lookupTable.Add(APIFrameType.TX_64, "TX (Transmit) Request 64-bit address");
			lookupTable.Add(APIFrameType.TX_16, "TX (Transmit) Request 16-bit address");
			lookupTable.Add(APIFrameType.AT_COMMAND, "AT Command");
			lookupTable.Add(APIFrameType.AT_COMMAND_QUEUE, "AT Command Queue");
			lookupTable.Add(APIFrameType.TRANSMIT_REQUEST, "Transmit Request");
			lookupTable.Add(APIFrameType.EXPLICIT_ADDRESSING_COMMAND_FRAME, "Explicit Addressing Command Frame");
			lookupTable.Add(APIFrameType.REMOTE_AT_COMMAND_REQUEST, "Remote AT Command Request");
			lookupTable.Add(APIFrameType.TX_SMS, "TX SMS");
			lookupTable.Add(APIFrameType.TX_IPV4, "TX IPv4");
			lookupTable.Add(APIFrameType.TX_REQUEST_TLS_PROFILE, "TX Request with TLS Profile");
			lookupTable.Add(APIFrameType.BLE_UNLOCK, "Bluetooth Unlock");
			lookupTable.Add(APIFrameType.USER_DATA_RELAY, "User Data Relay");
			lookupTable.Add(APIFrameType.RX_64, "RX (Receive) Packet 64-bit Address");
			lookupTable.Add(APIFrameType.RX_16, "RX (Receive) Packet 16-bit Address");
			lookupTable.Add(APIFrameType.RX_IO_64, "IO Data Sample RX 64-bit Address Indicator");
			lookupTable.Add(APIFrameType.RX_IO_16, "IO Data Sample RX 16-bit Address Indicator");
			lookupTable.Add(APIFrameType.AT_COMMAND_RESPONSE, "AT Command Response");
			lookupTable.Add(APIFrameType.TX_STATUS, "TX (Transmit) Status");
			lookupTable.Add(APIFrameType.MODEM_STATUS, "Modem Status");
			lookupTable.Add(APIFrameType.TRANSMIT_STATUS, "Transmit Status");
			lookupTable.Add(APIFrameType.RECEIVE_PACKET, "Receive Packet");
			lookupTable.Add(APIFrameType.EXPLICIT_RX_INDICATOR, "Explicit RX Indicator");
			lookupTable.Add(APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR, "IO Data Sample RX Indicator");
			lookupTable.Add(APIFrameType.REMOTE_AT_COMMAND_RESPONSE, "Remote Command Response");
			lookupTable.Add(APIFrameType.RX_SMS, "RX SMS");
			lookupTable.Add(APIFrameType.BLE_UNLOCK_RESPONSE, "Bluetooth Unlock Response");
			lookupTable.Add(APIFrameType.USER_DATA_RELAY_OUTPUT, "User Data Relay Output");
			lookupTable.Add(APIFrameType.RX_IPV4, "RX IPv4");
			lookupTable.Add(APIFrameType.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the <see cref="APIFrameType"/> associated with the specified ID <paramref name="value"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">ID value to retrieve <see cref="APIFrameType"/>.</param>
		/// <returns>The <see cref="APIFrameType"/> for the specified ID <paramref name="value"/>, 
		/// <see cref="APIFrameType.UNKNOWN"/> if it does not exist.</returns>
		/// <seealso cref="APIFrameType.UNKNOWN"/>
		public static APIFrameType Get(this APIFrameType source, byte value)
		{
			var values = Enum.GetValues(typeof(APIFrameType)).OfType<APIFrameType>();

			if (values.Cast<byte>().Contains(value))
				return (APIFrameType)value;

			return APIFrameType.UNKNOWN;
		}

		/// <summary>
		/// Gets the API frame type value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The API frame type value.</returns>
		public static byte GetValue(this APIFrameType source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the API frame type name.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The API frame type name.</returns>
		public static string GetName(this APIFrameType source)
		{
			return lookupTable.ContainsKey(source) ? lookupTable[source] : source.ToString();
		}

		/// <summary>
		/// Returns the <see cref="APIFrameType"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="APIFrameType"/> in string format.</returns>
		public static string ToDisplayString(this APIFrameType source)
		{
			return string.Format("({0}) {1}", HexUtils.ByteArrayToHexString(ByteUtils.IntToByteArray((byte)source)), GetName(source));
		}
	}
}