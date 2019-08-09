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

using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.Common
{
	/// <summary>
	/// This class represents a Receive Packet. Packet is built using the parameters of the 
	/// constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>When the module receives an RF packet, it is sent out the UART using this message type.
	/// 
	/// This packet is received when external devices send transmit request packets to this module.
	/// 
	/// Among received data, some options can also be received indicating transmission parameters.</remarks>
	/// <seealso cref="TransmitPacket"/>
	/// <seealso cref="XBeeReceiveOptions"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class ReceivePacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 12; // 1 (Frame type) + 8 (64-bit address) + 2 (16-bit address) + 1 (receive options)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ReceivePacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="sourceAddress64">The 64-bit address of the sender device.</param>
		/// <param name="sourceAddress16">The 16-bit address of the sender device.</param>
		/// <param name="receiveOptions">The bitField of receive options.</param>
		/// <param name="rfData">The received RF data.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="receiveOptions"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="receiveOptions"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="sourceAddress64"/> == null</c> 
		/// or if <c><paramref name="sourceAddress16"/> == null</c>.</exception>
		/// <seealso cref="XBeeReceiveOptions"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public ReceivePacket(XBee64BitAddress sourceAddress64, XBee16BitAddress sourceAddress16, byte receiveOptions, byte[] rfData)
			: base(APIFrameType.RECEIVE_PACKET)
		{
			SourceAddress64 = sourceAddress64 ?? throw new ArgumentNullException("64-bit source address cannot be null.");
			SourceAddress16 = sourceAddress16 ?? throw new ArgumentNullException("16-bit source address cannot be null.");
			ReceiveOptions = receiveOptions;
			RFData = rfData;
			logger = LogManager.GetLogger<ReceivePacket>();
		}

		// Properties.
		/// <summary>
		/// The 64 bit sender/source address.
		/// </summary>
		/// <seealso cref="XBee64BitAddress"/>
		public XBee64BitAddress SourceAddress64 { get; private set; }

		/// <summary>
		/// The 16 bit sender/source address.
		/// </summary>
		/// <seealso cref="XBee16BitAddress"/>
		public XBee16BitAddress SourceAddress16 { get; private set; }

		/// <summary>
		/// The receive options bitfield.
		/// </summary>
		public byte ReceiveOptions { get; private set; }

		/// <summary>
		/// The received RF data.
		/// </summary>
		public byte[] RFData { get; set; }

		/// <summary>
		/// Indicates whether the API packet needs API Frame ID or not.
		/// </summary>
		public override bool NeedsAPIFrameID => false;

		/// <summary>
		/// Indicates whether the packet is a broadcast packet.
		/// </summary>
		public override bool IsBroadcast
		{
			get
			{
				return ByteUtils.IsBitEnabled(ReceiveOptions, 1);
			}
		}

		/// <summary>
		/// Gets the XBee API packet specific data.
		/// </summary>
		/// <remarks>This does not include the frame ID if it is needed.</remarks>
		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (MemoryStream data = new MemoryStream())
				{
					try
					{
						data.Write(SourceAddress64.Value, 0, SourceAddress64.Value.Length);
						data.Write(SourceAddress16.Value, 0, SourceAddress16.Value.Length);
						data.WriteByte(ReceiveOptions);
						if (RFData != null)
							data.Write(RFData, 0, RFData.Length);
					}
					catch (IOException e)
					{
						logger.Error(e.Message, e);
					}
					return data.ToArray();
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
					{ "64-bit source address", HexUtils.PrettyHexString(SourceAddress64.ToString()) },
					{ "16-bit source address", HexUtils.PrettyHexString(SourceAddress16.ToString()) },
					{ "Receive options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ReceiveOptions, 1)) }
				};
				if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="ReceivePacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a Receive packet (<c>0x90</c>). The byte array must be in 
		/// <see cref="OperatingMode.API"/> mode.</param>
		/// <returns>Parsed Receive packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.RECEIVE_PACKET.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static ReceivePacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Receive packet payload cannot be null.");

			// 1 (Frame type) + 8 (32-bit address) + 2 (16-bit address) + 1 (receive options)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Receive packet.");

			if ((payload[0] & 0xFF) != APIFrameType.RECEIVE_PACKET.GetValue())
				throw new ArgumentException("Payload is not a Receive packet.");

			// payload[0] is the frame type.
			int index = 1;

			var array = new byte[8];
			Array.Copy(payload, index, array, 0, array.Length);
			// 8 bytes of 64-bit address.
			XBee64BitAddress sourceAddress64 = new XBee64BitAddress(array);
			index = index + 8;

			// 2 bytes of 16-bit address.
			XBee16BitAddress sourceAddress16 = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// Receive options
			byte receiveOptions = payload[index];
			index = index + 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
			}

			return new ReceivePacket(sourceAddress64, sourceAddress16, receiveOptions, data);
		}
	}
}