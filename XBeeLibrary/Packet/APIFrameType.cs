using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kveer.XBeeApi.Packet
{
	/// <summary>
	/// This enumeration lists all the available frame types used in any XBee protocol.
	/// </summary>
	public enum APIFrameType : byte
	{
		TX_64 = 0x00,
		TX_16 = 0x01,
		AT_COMMAND = 0x08,
		AT_COMMAND_QUEUE = 0x09,
		TRANSMIT_REQUEST = 0x10,
		REMOTE_AT_COMMAND_REQUEST = 0x17,
		RX_64 = 0x80,
		RX_16 = 0x81,
		RX_IO_64 = 0x82,
		RX_IO_16 = 0x83,
		AT_COMMAND_RESPONSE = 0x88,
		TX_STATUS = 0x89,
		MODEM_STATUS = 0x8A,
		TRANSMIT_STATUS = 0x8B,
		RECEIVE_PACKET = 0x90,
		IO_DATA_SAMPLE_RX_INDICATOR = 0x92,
		REMOTE_AT_COMMAND_RESPONSE = 0x97,
		GENERIC = 0xFF,
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
			lookupTable.Add(APIFrameType.REMOTE_AT_COMMAND_REQUEST, "Remote AT Command Request");
			lookupTable.Add(APIFrameType.RX_64, "RX (Receive) Packet 64-bit Address");
			lookupTable.Add(APIFrameType.RX_16, "RX (Receive) Packet 16-bit Address");
			lookupTable.Add(APIFrameType.RX_IO_64, "IO Data Sample RX 64-bit Address Indicator");
			lookupTable.Add(APIFrameType.RX_IO_16, "IO Data Sample RX 16-bit Address Indicator");
			lookupTable.Add(APIFrameType.AT_COMMAND_RESPONSE, "AT Command Response");
			lookupTable.Add(APIFrameType.TX_STATUS, "TX (Transmit) Status");
			lookupTable.Add(APIFrameType.MODEM_STATUS, "Modem Status");
			lookupTable.Add(APIFrameType.TRANSMIT_STATUS, "Transmit Status");
			lookupTable.Add(APIFrameType.RECEIVE_PACKET, "Receive Packet");
			lookupTable.Add(APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR, "IO Data Sample RX Indicator");
			lookupTable.Add(APIFrameType.REMOTE_AT_COMMAND_RESPONSE, "Remote Command Response");
			lookupTable.Add(APIFrameType.GENERIC, "Generic");
		}

		/// <summary>
		/// Gets the <see cref="APIFrameType"/> associated with the specified ID <paramref name="value"/>.
		/// </summary>
		/// <param name="dumb"></param>
		/// <param name="value">ID value to retrieve <see cref="APIFrameType"/>.</param>
		/// <returns>The <see cref="APIFrameType"/> for the specified ID <paramref name="value"/>, null if it does not exist.</returns>
		public static APIFrameType Get(this APIFrameType dumb, byte value)
		{
			var values = Enum.GetValues(typeof(APIFrameType));

			if (values.OfType<byte>().Contains(value))
				return (APIFrameType)value;

			return 0;
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
			return lookupTable[source];
		}

		public static string ToDisplayString(this APIFrameType source)
		{
			return string.Format("({0}) {1}", HexUtils.ByteArrayToHexString(ByteUtils.IntToByteArray((byte)source)), GetName(source));
		}
	}
}