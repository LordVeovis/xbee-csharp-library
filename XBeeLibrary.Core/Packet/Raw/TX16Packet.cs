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

namespace XBeeLibrary.Core.Packet.Raw
{
	/// <summary>
	/// This class represents a TX (Transmit) 16 Request packet. Packet is built using the parameters 
	/// of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>A TX Request message will cause the module to transmit data as an RF Packet.</remarks>
	/// <seealso cref="XBeeAPIPacket"/>
	public class TX16Packet : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 5; // 1 (Frame type) + 1 (frame ID) + 2 (address) + 1 (transmit options)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="TX16Packet"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="frameID">The Frame ID.</param>
		/// <param name="destAddress16">The 16-bit address of the destination device.</param>
		/// <param name="transmitOptions">The bitfield of supported transmission options.</param>
		/// <param name="rfData">The RF Data that is sent to the destination device.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="destAddress16"/> == null</c>.</exception>
		/// <seealso cref="XBeeTransmitOptions"/>
		/// <seealso cref="XBee16BitAddress"/>
		public TX16Packet(byte frameID, XBee16BitAddress destAddress16, byte transmitOptions, byte[] rfData)
			: base(APIFrameType.TX_16)
		{
			FrameID = frameID;
			DestAddress16 = destAddress16 ?? throw new ArgumentNullException("16-bit destination address cannot be null.");
			TransmitOptions = transmitOptions;
			RFData = rfData;
			logger = LogManager.GetLogger<TX16Packet>();
		}

		// Properties.
		/// <summary>
		/// The 16 bit destination address.
		/// </summary>
		/// <seealso cref="XBee16BitAddress"/>
		public XBee16BitAddress DestAddress16 { get; private set; }

		/// <summary>
		/// The transmit options bitfield.
		/// </summary>
		public byte TransmitOptions { get; private set; }

		/// <summary>
		/// The RF data to send.
		/// </summary>
		public byte[] RFData { get; set; }

		/// <summary>
		/// Indicates whether the API packet needs API Frame ID or not.
		/// </summary>
		public override bool NeedsAPIFrameID => true;

		/// <summary>
		/// Indicates whether the packet is a broadcast packet.
		/// </summary>
		public override bool IsBroadcast
		{
			get
			{
				return DestAddress16.Equals(XBee16BitAddress.BROADCAST_ADDRESS);
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
				using (var os = new MemoryStream())
				{
					try
					{
						os.Write(DestAddress16.Value, 0, DestAddress16.Value.Length);
						os.WriteByte(TransmitOptions);
						if (RFData != null)
							os.Write(RFData, 0, RFData.Length);
					}
					catch (IOException e)
					{
						logger.Error(e.Message, e);
					}
					return os.ToArray();
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
					{ "16-bit dest. address", HexUtils.PrettyHexString(DestAddress16.ToString()) },
					{ "Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitOptions, 1)) }
				};
				if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="TX16Packet"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a TX16 Request packet (<c>0x01</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed TX16 Request packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.TX_16.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static TX16Packet CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("TX16 Request packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 2 (address) + 1 (transmit options)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete TX16 Request packet.");
			if ((payload[0] & 0xFF) != APIFrameType.TX_16.GetValue())
				throw new ArgumentException("Payload is not a TX16 Request packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// 2 bytes of address, starting at 2nd byte.
			XBee16BitAddress destAddress16 = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// Transmit options byte.
			byte transmitOptions = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
			}

			return new TX16Packet(frameID, destAddress16, transmitOptions, data);
		}
	}
}