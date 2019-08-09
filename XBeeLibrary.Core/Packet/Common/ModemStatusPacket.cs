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
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.Common
{
	/// <summary>
	/// This class represents a Modem Status packet. Packet is built using the parameters of the 
	/// constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>RF module status messages are sent from the module in response to specific conditions 
	/// and indicates the state of the modem in that moment.</remarks>
	/// <seealso cref="XBeeAPIPacket"/>
	public class ModemStatusPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 2; // 1 (Frame type) + 1 (Modem status)

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ModemStatusPacket"/> object with the 
		/// given modem status.
		/// </summary>
		/// <param name="modemStatusEvent">The modem status event enumeration entry.</param>
		/// <seealso cref="ModemStatusEvent"/>
		public ModemStatusPacket(ModemStatusEvent modemStatusEvent)
			: base(APIFrameType.MODEM_STATUS)
		{
			Status = modemStatusEvent;
		}

		// Properties.
		/// <summary>
		/// The modem status event enumeration entry.
		/// </summary>
		/// <seealso cref="ModemStatusEvent"/>
		public ModemStatusEvent Status { get; }

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
				byte[] data = new byte[1];
				data[0] = (byte)(Status.GetId() & 0xFF);
				return data;
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
					{ "Status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(Status.GetId(), 1)) + " (" + Status.GetDescription() + ")" }
				};
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="ModemStatusPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a Modem Status packet (<c>0x8A</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed Modem status packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.MODEM_STATUS.GetValue()</c> 
		/// or if <c>payload.Length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static ModemStatusPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Modem Status packet payload cannot be null.");
			// 1 (Frame type) + 1 (Modem status)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Modem Status packet.");
			if ((payload[0] & 0xFF) != APIFrameType.MODEM_STATUS.GetValue())
				throw new ArgumentException("Payload is not a Modem Status packet.");

			// Get the Modem status byte (byte 1).
			int status = payload[1] & 0xFF;

			// Get the Modem Status enumeration entry.
			ModemStatusEvent modemStatusEvent = (ModemStatusEvent)status;

			return new ModemStatusPacket(modemStatusEvent);
		}
	}
}