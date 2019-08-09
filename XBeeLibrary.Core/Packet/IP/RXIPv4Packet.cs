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
using System.Net;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.IP
{
	/// <summary>
	/// This class represents an RX (Receive) IPv4 packet. Packet is built
	/// using the parameters of the constructor or providing a valid API payload.
	/// </summary>
	/// <seealso cref="TXIPv4Packet"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class RXIPv4Packet : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 11; // 1 (Frame type) + 4 (source address) + 2 (dest port) + 2 (source port) + 1 (protocol) + 1 (status)
		private const string ERROR_PAYLOAD_NULL = "RX IPv4 packet payload cannot be null.";
		private const string ERROR_INCOMPLETE_PACKET = "Incomplete RX IPv4 packet.";
		private const string ERROR_NOT_RXIPV4 = "Payload is not a RX IPv4 packet.";
		private const string ERROR_SOURCE_ADDR_NULL = "Source address cannot be null.";
		private const string ERROR_PROTOCOL_UNKNOWN = "Protocol cannot be UNKNOWN.";
		private const string ERROR_PORT_ILLEGAL = "Port must be between 0 and 65535.";

		// Variables.
		private IPAddress sourceAddress;

		private int destPort;
		private int sourcePort;

		private IPProtocol protocol;

		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <c>RXIPv4Packet</c> object with
		/// the given parameters.
		/// </summary>
		/// <param name="sourceAddress">32-bit IP address of the source device.</param>
		/// <param name="destPort">Destination port number.</param>
		/// <param name="sourcePort">Source port number.</param>
		/// <param name="protocol">Protocol used for transmitted data.</param>
		/// <param name="data">Receive data bytes.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c> 
		/// or if <c><paramref name="sourcePort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="sourcePort"/> <![CDATA[>]]> 65535</c> 
		/// or if <paramref name="protocol"/> is unknown.</exception>
		/// <seealso cref="IPAddress"/>
		/// <seealso cref="IPProtocol"/>
		public RXIPv4Packet(IPAddress sourceAddress, int destPort, int sourcePort,
			IPProtocol protocol, byte[] data) :
			base(APIFrameType.RX_IPV4)
		{
			if (destPort < 0 || destPort > 65535)
				throw new ArgumentException(ERROR_PORT_ILLEGAL);
			if (sourcePort < 0 || sourcePort > 65535)
				throw new ArgumentException(ERROR_PORT_ILLEGAL);
			if (protocol == IPProtocol.UNKNOWN)
				throw new ArgumentException(ERROR_PROTOCOL_UNKNOWN);

			this.sourceAddress = sourceAddress ?? throw new ArgumentNullException(ERROR_SOURCE_ADDR_NULL);
			this.destPort = destPort;
			this.sourcePort = sourcePort;
			this.protocol = protocol;
			Data = data;
			logger = LogManager.GetLogger<RXIPv4Packet>();
		}

		// Properties.
		/// <summary>
		/// The 32-bit source IP address.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the value to set is <c>null</c>.</exception>
		/// <seealso cref="IPAddress"/>
		public IPAddress SourceAddress
		{
			get => sourceAddress;
			set => sourceAddress = value ?? throw new ArgumentNullException(ERROR_SOURCE_ADDR_NULL);
		}

		/// <summary>
		/// The destination port. The port must be a number between 0 and 65535.
		/// </summary>
		/// <exception cref="ArgumentException">If the value to set is not between 0 and 65535.</exception>
		public int DestPort
		{
			get => destPort;
			set
			{
				if (value < 0 || value > 65535)
					throw new ArgumentException(ERROR_PORT_ILLEGAL);
				destPort = value;
			}
		}

		/// <summary>
		/// The source port. The port must be a number between 0 and 65535.
		/// </summary>
		/// <exception cref="ArgumentException">If the value to set is not between 0 and 65535.</exception>
		public int SourcePort
		{
			get => sourcePort;
			set
			{
				if (value < 0 || value > 65535)
					throw new ArgumentException(ERROR_PORT_ILLEGAL);
				sourcePort = value;
			}
		}

		/// <summary>
		/// The network protocol.
		/// </summary>
		/// <seealso cref="IPProtocol"/>
		/// <exception cref="ArgumentException">If the value to set is unknown.</exception>
		public IPProtocol Protocol
		{
			get => protocol;
			set
			{
				if (value == IPProtocol.UNKNOWN)
					throw new ArgumentException(ERROR_PROTOCOL_UNKNOWN);
				protocol = value;
			}
		}

		/// <summary>
		/// The received data.
		/// </summary>
		public byte[] Data { get; set; }

		/// <summary>
		/// Indicates whether the API packet needs API Frame ID or not.
		/// </summary>
		public override bool NeedsAPIFrameID => false;

		/// <summary>
		/// Indicates whether the packet is a broadcast packet.
		/// </summary>
		public override bool IsBroadcast => false;

		/// <summary>
		/// Gets the XBee API packet specific data.
		/// </summary>
		/// <remarks>This does not include the frame ID if it is needed.</remarks>
		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (MemoryStream ms = new MemoryStream())
				{
					try
					{
						ms.Write(sourceAddress.GetAddressBytes(), 0, 4);
						ms.WriteByte((byte)(destPort >> 8));
						ms.WriteByte((byte)destPort);
						ms.WriteByte((byte)(sourcePort >> 8));
						ms.WriteByte((byte)sourcePort);
						ms.WriteByte((byte)protocol.GetID());
						ms.WriteByte(0x00); // Status byte, reserved.
						if (Data != null)
							ms.Write(Data, 0, Data.Length);
					}
					catch (IOException e)
					{
						logger.Error(e.Message, e);
					}
					return ms.ToArray();
				}
			}
		}

		/// <summary>
		/// Gets a map with the XBee packet parameters and their values.
		/// </summary>
		/// <returns>A sorted map containing the XBee packet parameters with their values.</returns>
		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>
				{
					{ "Source address", sourceAddress.ToString() },
					{
						"Destination port",
						HexUtils.PrettyHexString(
					ByteUtils.ShortToByteArray((short)destPort)) + " (" + destPort.ToString() + ")"
					},
					{
						"Source port",
						HexUtils.PrettyHexString(
					ByteUtils.ShortToByteArray((short)sourcePort)) + " (" + sourcePort.ToString() + ")"
					},
					{
						"Protocol",
						HexUtils.PrettyHexString(
					HexUtils.ByteToHexString((byte)protocol)) + " (" + protocol.ToDisplayString() + ")"
					}
				};
				if (Data != null)
					parameters.Add("Data", HexUtils.PrettyHexString(Data));
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <c>RXIPv4Packet</c> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to an RX IPv4 packet (<c>0xB0</c>). The byte array must be in <see cref="OperatingMode.API"/> mode.</param>
		/// <returns>Parsed RX IPv4 packet.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="payload"/>[0] != APIFrameType.RX_IPV4.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static RXIPv4Packet CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException(ERROR_PAYLOAD_NULL);
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException(ERROR_INCOMPLETE_PACKET);
			if ((payload[0] & 0xFF) != APIFrameType.RX_IPV4.GetValue())
				throw new ArgumentException(ERROR_NOT_RXIPV4);

			// payload[0] is the frame type.
			int index = 1;

			// 4 bytes of IP 32-bit source address.
			byte[] addressBytes = new byte[4];
			Array.Copy(payload, index, addressBytes, 0, addressBytes.Length);
			IPAddress sourceAddress = new IPAddress(addressBytes);
			index += 4;

			// 2 bytes of destination port.
			int destPort = (payload[index] & 0xFF) << 8 | payload[index + 1] & 0xFF;
			index += 2;

			// 2 bytes of source port.
			int sourcePort = (payload[index] & 0xFF) << 8 | payload[index + 1] & 0xFF;
			index += 2;

			// Protocol byte.
			IPProtocol protocol = IPProtocol.UNKNOWN.Get(payload[index] & 0xFF);
			index += 1;

			// Status byte, reserved.
			index += 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				int dataLength = payload.Length - index;
				data = new byte[dataLength];
				Array.Copy(payload, index, data, 0, dataLength);
			}
			return new RXIPv4Packet(sourceAddress, destPort, sourcePort, protocol, data);
		}
	}
}
