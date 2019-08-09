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
using XBeeLibrary.Core.IO;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.Raw
{
	/// <summary>
	/// This class represents a TX (Transmit) Status packet. Packet is built using the parameters 
	/// of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>When a TX Request is completed, the module sends a TX Status message. This message 
	/// will indicate if the packet was transmitted successfully or if there was a failure.</remarks>
	/// <seealso cref="TX16Packet"/>
	/// <seealso cref="TX64Packet"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class TXStatusPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 3; // 1 (Frame type) + 1 (frame ID) + 1 (status)

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="TXStatusPacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="frameID">The Frame ID.</param>
		/// <param name="transmitStatus">The transmit status.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255</c>.</exception>
		/// <seealso cref="XBeeTransmitStatus"/>
		public TXStatusPacket(byte frameID, XBeeTransmitStatus transmitStatus)
			: base(APIFrameType.TX_STATUS)
		{
			FrameID = frameID;
			TransmitStatus = transmitStatus;
		}

		// Properties.
		/// <summary>
		/// Gets the transmit status.
		/// </summary>
		/// <seealso cref="XBeeTransmitStatus"/>
		public XBeeTransmitStatus TransmitStatus { get; private set; }

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
				return new byte[] { TransmitStatus.GetId() };
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
					{ "Status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitStatus.GetId(), 1)) + " (" + TransmitStatus.GetDescription() + ")" }
				};
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="TXStatusPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a TX Status packet (<c>0x89</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed TX Status packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.TX_STATUS.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static TXStatusPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("TX Status packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 1 (status)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete TX Status packet.");
			if ((payload[0] & 0xFF) != APIFrameType.TX_STATUS.GetValue())
				throw new ArgumentException("Payload is not a TX Status packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			// Status byte.
			byte status = payload[index];

			// TODO if status is unknown????
			return new TXStatusPacket(frameID, XBeeTransmitStatus.UNKNOWN.Get(status));
		}
	}
}