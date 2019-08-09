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
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.IO;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;
namespace XBeeLibrary.Core.Packet.Common
{
	/// <summary>
	/// This class represents an IO Data Sample RX Indicator packet. Packet is built using the 
	/// parameters of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>When the module receives an IO sample frame from a remote device, it sends the 
	/// sample out the UART using this frame type (when AO= 0). Only modules running API firmware 
	/// will send IO samples out the UART.
	/// 
	/// Among received data, some options can also be received indicating transmission parameters.</remarks>
	/// <seealso cref="XBeeReceiveOptions"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class IODataSampleRxIndicatorPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 12; // 1 (Frame type) + 8 (64-bit address) + 2 (16-bit address) + 1 (receive options)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a <see cref="IODataSampleRxIndicatorPacket"/> with the given parameters.
		/// </summary>
		/// <param name="sourceAddress64">The 64-bit address of the sender.</param>
		/// <param name="sourceAddress16">The 16-bit address of the sender.</param>
		/// <param name="receiveOptions">The receive options.</param>
		/// <param name="rfData">The received RF data.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="receiveOptions"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="receiveOptions"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="sourceAddress64"/> is <c>null</c>
		/// or if <paramref name="sourceAddress16"/> is <c>null</c>.</exception>
		/// <seealso cref="XBeeReceiveOptions"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public IODataSampleRxIndicatorPacket(XBee64BitAddress sourceAddress64, XBee16BitAddress sourceAddress16, 
			byte receiveOptions, byte[] rfData)
			: base(APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR)
		{
			SourceAddress64 = sourceAddress64 ?? throw new ArgumentNullException("64-bit source address cannot be null.");
			SourceAddress16 = sourceAddress16 ?? throw new ArgumentNullException("16-bit source address cannot be null.");
			ReceiveOptions = receiveOptions;
			RFData = rfData;
			if (rfData != null && rfData.Length >= 5)
				IOSample = new IOSample(rfData);
			else
				// TODO: Should we throw an exception here?
				IOSample = null;
			logger = LogManager.GetLogger<IODataSampleRxIndicatorPacket>();
		}

		// Properties.
		/// <summary>
		/// The 64-bit sender/source address.
		/// </summary>
		/// <seealso cref="XBee64BitAddress"/>
		public XBee64BitAddress SourceAddress64 { get; private set; }

		/// <summary>
		/// The 16-bit sender/source address.
		/// </summary>
		/// <seealso cref="XBee16BitAddress"/>
		public XBee16BitAddress SourceAddress16 { get; private set; }

		/// <summary>
		/// The IO sample corresponding to the data contained in the packet.
		/// </summary>
		/// <seealso cref="IO.IOSample"/>
		public IOSample IOSample { get; private set; }

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
				using (MemoryStream os = new MemoryStream())
				{
					try
					{
						os.Write(SourceAddress64.Value, 0, SourceAddress64.Value.Length);
						os.Write(SourceAddress16.Value, 0, SourceAddress16.Value.Length);
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
					{ "64-bit source address", HexUtils.PrettyHexString(SourceAddress64.ToString()) },
					{ "16-bit source address", HexUtils.PrettyHexString(SourceAddress16.ToString()) },
					{ "Receive options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ReceiveOptions, 1)) }
				};
				if (IOSample != null)
				{
					parameters.Add("Number of samples", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(1, 1))); // There is always 1 sample.
					parameters.Add("Digital channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.DigitalMask, 2)));
					parameters.Add("Analog channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.AnalogMask, 1)));
					for (int i = 0; i < 16; i++)
					{
						if (IOSample.HasDigitalValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(IOLine.UNKNOWN.GetDIO(i).GetName() + " digital value", IOSample.GetDigitalValue(IOLine.UNKNOWN.GetDIO(i)).GetName());
					}
					for (int i = 0; i < 6; i++)
					{
						if (IOSample.HasAnalogValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(IOLine.UNKNOWN.GetDIO(i).GetName() + " analog value", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.GetAnalogValue(IOLine.UNKNOWN.GetDIO(i)), 2)));
					}
					if (IOSample.HasPowerSupplyValue)
						try
						{
							parameters.Add("Power supply value", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.PowerSupplyValue, 2)));
						}
						catch (OperationNotSupportedException) { }
				}
				else if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <seealso cref="IODataSampleRxIndicatorPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to an IO Data Sample RX Indicator packet (<c>0x92</c>). The byte array must be in 
		/// <see cref="OperatingMode.API"/> mode.</param>
		/// <returns>Parsed IO Data Sample RX Indicator packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR.GetValue()</c>
		/// or if <c>payload.Length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static IODataSampleRxIndicatorPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("IO Data Sample RX Indicator packet payload cannot be null.");
			// 1 (Frame type) + 8 (32-bit address) + 2 (16-bit address) + 1 (receive options)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete IO Data Sample RX Indicator packet.");
			if ((payload[0] & 0xFF) != APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR.GetValue())
				throw new ArgumentException("Payload is not a IO Data Sample RX Indicator packet.");

			// payload[0] is the frame type.
			int index = 1;

			var addr = new byte[8];
			Array.Copy(payload, index, addr, 0, addr.Length);
			// 8 bytes of 64-bit address.
			XBee64BitAddress sourceAddress64 = new XBee64BitAddress(addr);
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
			return new IODataSampleRxIndicatorPacket(sourceAddress64, sourceAddress16, receiveOptions, data);
		}
	}
}