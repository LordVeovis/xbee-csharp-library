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
using XBeeLibrary.Core.IO;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.Raw
{
	/// <summary>
	/// This class represents an RX64 Address IO packet. Packet is built using the parameters of the 
	/// constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>I/O data is sent out the UART using an API frame.</remarks>
	/// <seealso cref="XBeeAPIPacket"/>
	public class RX64IOPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 11; // 1 (Frame type) + 8 (64-bit address) + 1 (RSSI) + 1 (receive options)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RX64IOPacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="sourceAddress64">The 64-bit address of the sender device.</param>
		/// <param name="rssi">The received signal strength indicator.</param>
		/// <param name="receiveOptions">The bitField of receive options.</param>
		/// <param name="rfData">The received RF data.</param>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="rssi"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="rssi"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="sourceAddress64"/> == null</c>.</exception>
		/// <seealso cref="XBeeReceiveOptions"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RX64IOPacket(XBee64BitAddress sourceAddress64, byte rssi, byte receiveOptions, byte[] rfData)
			: base(APIFrameType.RX_IO_64)
		{
			if (rssi < 0 || rssi > 255)
				throw new ArgumentOutOfRangeException("RSSI value must be between 0 and 255.");

			SourceAddress64 = sourceAddress64 ?? throw new ArgumentNullException("64-bit source address cannot be null.");
			RSSI = rssi;
			ReceiveOptions = receiveOptions;
			RFData = rfData;
			if (rfData != null && rfData.Length >= 5)
				IoSample = new IOSample(rfData);
			else
				IoSample = null;
			logger = LogManager.GetLogger<RX64IOPacket>();
		}

		// Properties.
		/// <summary>
		/// Gets the 64-bit sender/source address.
		/// </summary>
		/// <seealso cref="XBee64BitAddress"/>
		public XBee64BitAddress SourceAddress64 { get; private set; }

		/// <summary>
		/// The IO sample corresponding to the data contained in the packet.
		/// </summary>
		/// <seealso cref="IOSample"/>
		public IOSample IoSample { get; private set; }

		/// <summary>
		/// The Received Signal Strength Indicator (RSSI).
		/// </summary>
		public byte RSSI { get; private set; }

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
				return ByteUtils.IsBitEnabled(ReceiveOptions, 1)
						|| ByteUtils.IsBitEnabled(ReceiveOptions, 2);
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
						os.Write(SourceAddress64.Value, 0, SourceAddress64.Value.Length);
						os.WriteByte(RSSI);
						os.WriteByte(ReceiveOptions);
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
					new KeyValuePair<string, string>("64-bit source address", HexUtils.PrettyHexString(SourceAddress64.ToString())),
					new KeyValuePair<string, string>("RSSI", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(RSSI, 1))),
					new KeyValuePair<string, string>("Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ReceiveOptions, 1)))
				};
				if (IoSample != null)
				{
					parameters.Add(new KeyValuePair<string, string>("Number of samples", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(1, 1)))); // There is always 1 sample.
					parameters.Add(new KeyValuePair<string, string>("Digital channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IoSample.DigitalMask, 2))));
					parameters.Add(new KeyValuePair<string, string>("Analog channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IoSample.AnalogMask, 2))));
					for (int i = 0; i < 16; i++)
					{
						if (IoSample.HasDigitalValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(new KeyValuePair<string, string>(IOLine.UNKNOWN.GetDIO(i).GetName() + " digital value", IoSample.GetDigitalValue(IOLine.UNKNOWN.GetDIO(i)).GetName()));
					}
					for (int i = 0; i < 6; i++)
					{
						if (IoSample.HasAnalogValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(new KeyValuePair<string, string>(IOLine.UNKNOWN.GetDIO(i).GetName() + " analog value", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IoSample.GetAnalogValue(IOLine.UNKNOWN.GetDIO(i)), 2))));
					}
				}
				else if (RFData != null)
					parameters.Add(new KeyValuePair<string, string>("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData))));
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="RX64IOPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to an RX64 Address IO packet (<c>0x82</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed RX64 Address IO packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.RX_IO_64.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static RX64IOPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("RX64 Address IO packet payload cannot be null.");
			// 1 (Frame type) + 8 (64-bit address) + 1 (RSSI) + 1 (receive options)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete RX64 Address IO packet.");
			if ((payload[0] & 0xFF) != APIFrameType.RX_IO_64.GetValue())
				throw new ArgumentException("Payload is not a RX64 Address IO packet.");

			// payload[0] is the frame type.
			int index = 1;

			// 8 bytes of 64-bit address.
			var address = new byte[8];
			Array.Copy(payload, index, address, 0, address.Length);

			XBee64BitAddress sourceAddress64 = new XBee64BitAddress(address);
			index = index + 8;

			// Received Signal Strength Indicator byte.
			byte rssi = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// Received Options byte.
			byte receiveOptions = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
			}

			return new RX64IOPacket(sourceAddress64, rssi, receiveOptions, data);
		}
	}
}