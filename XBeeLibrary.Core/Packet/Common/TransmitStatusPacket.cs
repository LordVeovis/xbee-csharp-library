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
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.Common
{
	/// <summary>
	/// This class represents a Transmit Status Packet. Packet is built using the parameters of the 
	/// constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>When a Transmit Request is completed, the module sends a Transmit Status message. 
	/// This message will indicate if the packet was transmitted successfully or if there was a failure.
	/// 
	/// This packet is the response to standard and explicit transmit requests.</remarks>
	/// <seealso cref="TransmitPacket"/>
	public class TransmitStatusPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 7; // 1 (Frame type) + 1 (frame ID) + 2 (16-bit address) + 1 (retry count) + 1 (delivery status) + 1 (discovery status)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="TransmitStatusPacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="frameID">The frame ID.</param>
		/// <param name="destAddress16">The 16-bit network address the packet was delivered to.</param>
		/// <param name="transmitRetryCount">The number of application transmission retries that took place.</param> 
		/// <param name="transmitStatus">The transmit status.</param>
		/// <param name="discoveryStatus">The discovery status.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="destAddress16"/> == null</c>.</exception>
		/// <seealso cref="XBeeDiscoveryStatus"/>
		/// <seealso cref="XBeeTransmitStatus"/>
		/// <seealso cref="XBee16BitAddress"/>
		public TransmitStatusPacket(byte frameID, XBee16BitAddress destAddress16, byte transmitRetryCount,
			XBeeTransmitStatus transmitStatus, XBeeDiscoveryStatus discoveryStatus)
			: base(APIFrameType.TRANSMIT_STATUS)
		{
			FrameID = frameID;
			DestAddress16 = destAddress16 ?? throw new ArgumentNullException("16-bit destination address cannot be null.");
			TransmitRetryCount = transmitRetryCount;
			TransmitStatus = transmitStatus;
			DiscoveryStatus = discoveryStatus;
			logger = LogManager.GetLogger<TransmitStatusPacket>();
		}

		// Properties.
		/// <summary>
		/// The 16 bit destination address.
		/// </summary>
		/// <seealso cref="XBee16BitAddress"/>
		public XBee16BitAddress DestAddress16 { get; private set; }

		/// <summary>
		/// The transmit retry count.
		/// </summary>
		public byte TransmitRetryCount { get; private set; }

		/// <summary>
		/// The transmit status.
		/// </summary>
		/// <seealso cref="XBeeTransmitStatus"/>
		public XBeeTransmitStatus TransmitStatus { get; private set; }

		/// <summary>
		/// The discovery status.
		/// </summary>
		/// <seealso cref="XBeeDiscoveryStatus"/>
		public XBeeDiscoveryStatus DiscoveryStatus { get; private set; }

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
				MemoryStream data = new MemoryStream();
				try
				{
					data.Write(DestAddress16.Value, 0, DestAddress16.Value.Length);
					data.WriteByte(TransmitRetryCount);
					data.WriteByte(TransmitStatus.GetId());
					data.WriteByte(DiscoveryStatus.GetId());
				}
				catch (IOException e)
				{
					logger.Error(e.Message, e);
				}
				return data.ToArray();
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
					{ "Tx. retry count", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitRetryCount, 1)) + " (" + TransmitRetryCount + ")" },
					{ "Delivery status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitStatus.GetId(), 1)) + " (" + TransmitStatus.GetDescription() + ")" },
					{ "Discovery status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(DiscoveryStatus.GetId(), 1)) + " (" + DiscoveryStatus.GetDescription() + ")" }
				};
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="TransmitStatusPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a Transmit Status packet (<c>0x8B</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed Transmit Status packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.TRANSMIT_STATUS.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static TransmitStatusPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Transmit Status packet payload cannot be null.");

			// 1 (Frame type) + 1 (frame ID) + 2 (16-bit address) + 1 (retry count) + 1 (delivery status) + 1 (discovery status)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Transmit Status packet.");

			if ((payload[0] & 0xFF) != APIFrameType.TRANSMIT_STATUS.GetValue())
				throw new ArgumentException("Payload is not a Transmit Status packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			// 2 bytes of 16-bit address.
			XBee16BitAddress address = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// Retry count byte.
			byte retryCount = payload[index];
			index = index + 1;

			// Delivery status byte.
			byte deliveryStatus = payload[index];
			index = index + 1;

			// Discovery status byte.
			byte discoveryStatus = payload[index];

			// TODO if XBeeTransmitStatus is unknown????
			return new TransmitStatusPacket(frameID, address, retryCount,
					XBeeTransmitStatus.SUCCESS.Get(deliveryStatus), XBeeDiscoveryStatus.DISCOVERY_STATUS_ADDRESS_AND_ROUTE.Get(discoveryStatus));
		}
	}
}