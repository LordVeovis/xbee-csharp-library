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
	/// This class represents a TX (Transmit) IPv4 packet. Packet is built
	/// using the parameters of the constructor or providing a valid API payload.
	/// </summary>
	/// <seealso cref="RXIPv4Packet"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class TXIPv4Packet : XBeeAPIPacket
	{
		// Constants.
		// This option will close the socket after the transmission.
		public const int OPTIONS_CLOSE_SOCKET = 2;
		// This option will leave socket open after the transmission.
		public const int OPTIONS_LEAVE_SOCKET_OPEN = 0;

		private const int MIN_API_PAYLOAD_LENGTH = 12; // 1 (Frame type) + 1 (frame ID) + 4 (dest address) + 2 (dest port) + 2 (source port) + 1 (protocol) + 1 (transmit options)

		private const string ERROR_PAYLOAD_NULL = "TX IPv4 packet payload cannot be null.";
		private const string ERROR_INCOMPLETE_PACKET = "Incomplete TX IPv4 packet.";
		private const string ERROR_NOT_TXIPV4 = "Payload is not a TX IPv4 packet.";
		private const string ERROR_DEST_ADDR_NULL = "Destination address cannot be null.";
		private const string ERROR_PROTOCOL_UNKNOWN = "Protocol cannot be UNKNOWN.";
		private const string ERROR_FRAME_ID_ILLEGAL = "Frame ID must be between 0 and 255.";
		private const string ERROR_PORT_ILLEGAL = "Port must be between 0 and 65535.";
		private static readonly string ERROR_OPTIONS_INVALID = "Transmit options can only be " + OPTIONS_CLOSE_SOCKET +
			" or " + OPTIONS_LEAVE_SOCKET_OPEN + ".";

		// Variables.
		private IPAddress destAddress;

		private int destPort;
		private int sourcePort;
		private int transmitOptions;

		private IPProtocol protocol;
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="TXIPv4Packet"/> object with the given 
		/// parameters.
		/// </summary>
		/// <param name="frameID">Frame ID.</param>
		/// <param name="destAddress">32-bit IP address of the destination device.</param>
		/// <param name="destPort">Destination port number.</param>
		/// <param name="sourcePort">Source port number.</param>
		/// <param name="protocol">Protocol used for transmitted data.</param>
		/// <param name="transmitOptions">Transmit options bitfield.</param>
		/// <param name="data">Transmit data bytes.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0 </c> 
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255 </c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c> 
		/// or if <c><paramref name="sourcePort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="sourcePort"/> <![CDATA[>]]> 65535</c> 
		/// or if <paramref name="transmitOptions"/> is invalid
		/// or if <paramref name="protocol"/> is unknown.</exception>
		/// <seealso cref="IPAddress"/>
		/// <seealso cref="IPProtocol"/>
		/// <see cref="IPAddress"/>
		/// <see cref="IPProtocol"/>
		public TXIPv4Packet(byte frameID, IPAddress destAddress, int destPort, int sourcePort,
			IPProtocol protocol, int transmitOptions, byte[] data) :
			base(APIFrameType.TX_IPV4)
		{
			if (destPort < 0 || destPort > 65535)
				throw new ArgumentException(ERROR_PORT_ILLEGAL);
			if (sourcePort < 0 || sourcePort > 65535)
				throw new ArgumentException(ERROR_PORT_ILLEGAL);
			if (transmitOptions != OPTIONS_CLOSE_SOCKET && transmitOptions != OPTIONS_LEAVE_SOCKET_OPEN)
				throw new ArgumentException(ERROR_OPTIONS_INVALID);
			if (protocol == IPProtocol.UNKNOWN)
				throw new ArgumentNullException(ERROR_PROTOCOL_UNKNOWN);

			FrameID = frameID;
			this.destAddress = destAddress ?? throw new ArgumentNullException(ERROR_DEST_ADDR_NULL);
			this.destPort = destPort;
			this.sourcePort = sourcePort;
			this.protocol = protocol;
			this.transmitOptions = transmitOptions;
			Data = data;
			logger = LogManager.GetLogger<TXIPv4Packet>();
		}

		/// <summary>
		/// The 32-bit destination IP address.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the value to set is <c>null</c>.</exception>
		/// <seealso cref="IPAddress"/>
		public IPAddress DestAddress
		{
			get => destAddress;
			set => destAddress = value ?? throw new ArgumentNullException(ERROR_DEST_ADDR_NULL);
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
		/// The transmit options. Must be <seealso cref="OPTIONS_CLOSE_SOCKET"/>
		/// or <seealso cref="OPTIONS_LEAVE_SOCKET_OPEN"/>.
		/// </summary>
		/// <exception cref="ArgumentException">If the value to set is invalid.</exception>
		public int TransmitOptions
		{
			get => transmitOptions;
			set
			{
				if (value != OPTIONS_CLOSE_SOCKET && value != OPTIONS_LEAVE_SOCKET_OPEN)
					throw new ArgumentException(ERROR_OPTIONS_INVALID);
				transmitOptions = value;
			}
		}

		/// <summary>
		/// The transmission data.
		/// </summary>
		public byte[] Data { get; set; }

		/// <summary>
		/// Indicates whether the API packet needs API Frame ID or not.
		/// </summary>
		public override bool NeedsAPIFrameID => true;

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
						ms.Write(destAddress.GetAddressBytes(), 0, 4);
						ms.WriteByte((byte)(destPort >> 8));
						ms.WriteByte((byte)destPort);
						ms.WriteByte((byte)(sourcePort >> 8));
						ms.WriteByte((byte)sourcePort);
						ms.WriteByte((byte)protocol.GetID());
						ms.WriteByte((byte)transmitOptions);

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
					{ "Destination address", destAddress.ToString() },
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
					},
					{ "Transmit options", HexUtils.ByteToHexString((byte)transmitOptions) }
				};
				if (Data != null)
					parameters.Add("Data", HexUtils.PrettyHexString(Data));
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="TXIPv4Packet"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a TX IPv4 packet (<c>0x20</c>). The byte array must be in <see cref="OperatingMode.API"/> mode.</param>
		/// <returns>Parsed TX IPv4 packet.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="payload"/>[0] != APIFrameType.TX_IPV4.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static TXIPv4Packet CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException(ERROR_PAYLOAD_NULL);
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException(ERROR_INCOMPLETE_PACKET);
			if ((payload[0] & 0xFF) != APIFrameType.TX_IPV4.GetValue())
				throw new ArgumentException(ERROR_NOT_TXIPV4);

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index += 1;

			// 4 bytes of IP 32-bit destination address.
			byte[] addressBytes = new byte[4];
			Array.Copy(payload, index, addressBytes, 0, addressBytes.Length);
			IPAddress destAddress = new IPAddress(addressBytes);
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

			// Transmit options byte.
			int transmitOptions = payload[index] & 0xFF;
			index += 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				int dataLength = payload.Length - index;
				data = new byte[dataLength];
				Array.Copy(payload, index, data, 0, dataLength);
			}
			return new TXIPv4Packet(frameID, destAddress, destPort, sourcePort, protocol, transmitOptions, data);
		}
	}
}
