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
namespace XBeeLibrary.Core.Packet
{
	/// <summary>
	/// This class represents an unknown XBee packet where the payload is set as a byte 
	/// array without a defined structure.
	/// </summary>
	/// <seealso cref="XBeeAPIPacket"/>
	public class UnknownXBeePacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 1; // 1 (Frame type)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="UnknownXBeePacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="rfData">The XBee RF Data.</param>
		public UnknownXBeePacket(byte[] rfData)
			: this(rfData, APIFrameType.UNKNOWN.GetValue()) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="UnknownXBeePacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="rfData">The XBee RF Data.</param>
		/// <param name="origApiFrameType">The original API frame type.</param>
		public UnknownXBeePacket(byte[] rfData, byte origApiFrameType)
			: base(origApiFrameType)
		{
			RFData = rfData;
			logger = LogManager.GetLogger<UnknownXBeePacket>();
		}

		// Properties.
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
		public override bool IsBroadcast => false;

		/// <summary>
		/// A sorted dictionary with the packet parameters and their values.
		/// </summary>
		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (var data = new MemoryStream())
				{
					try
					{
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
				var parameters = new LinkedDictionary<string, string>();
				if (RFData != null)
					parameters.Add("RF Data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="UnknownXBeePacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to an Unknown packet (<c>0xFF</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed Unknown packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload.length <![CDATA[<]]> 
		/// <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static UnknownXBeePacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Unknown packet payload cannot be null.");
			// 1 (Frame type)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Unknown packet.");

			// payload[0] is the frame type.
			int index = 1;

			byte[] commandData = null;
			if (index < payload.Length)
			{
				commandData = new byte[payload.Length - index];
				Array.Copy(payload, index, commandData, 0, commandData.Length);
			}

			// Check frame type. If it is not unknown, it is unknown to the library.
			if (payload[0] != APIFrameType.UNKNOWN.GetValue())
				return new UnknownXBeePacket(commandData, payload[0]);
			return new UnknownXBeePacket(commandData);
		}
	}
}