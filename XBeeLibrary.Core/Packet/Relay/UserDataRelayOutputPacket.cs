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
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.Relay
{
	/// <summary>
	/// This class represents a User Data Relay Output packet.
	/// </summary>
	/// <remarks>The User Data Relay Output packet can be received from any relay interface.
	/// 
	/// The source interface must be one of the interfaces found in the corresponding enumerator.</remarks>
	/// <seealso cref="XBeeAPIPacket"/>
	public class UserDataRelayOutputPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 2; // 1 (Frame type) + 1 (Src. interface)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="UserDataRelayOutputPacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="sourceInterface">The source interface.</param>
		/// <param name="data">The data that is received from the source interface.</param>
		/// <exception cref="ArgumentException">If 
		/// <c><paramref name="sourceInterface"/> == <see cref="XBeeLocalInterface.UNKNOWN"/></c>.</exception>
		/// <seealso cref="XBeeLocalInterface"/>
		public UserDataRelayOutputPacket(XBeeLocalInterface sourceInterface, byte[] data)
			: base(APIFrameType.USER_DATA_RELAY_OUTPUT)
		{
			if (sourceInterface == XBeeLocalInterface.UNKNOWN)
				throw new ArgumentException("Source interface cannot be unknown.");

			SourceInterface = sourceInterface;
			Data = data;
			logger = LogManager.GetLogger<UserDataRelayOutputPacket>();
		}

		// Properties.
		/// <summary>
		/// The source XBee local interface.
		/// </summary>
		/// <seealso cref="XBeeLocalInterface"/>
		public XBeeLocalInterface SourceInterface { get; private set; }

		/// <summary>
		/// The data that is received from the source interface.
		/// </summary>
		public byte[] Data { get; private set; }

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
				using (MemoryStream data = new MemoryStream())
				{
					try
					{
						data.WriteByte(SourceInterface.GetValue());
						if (Data != null)
							data.Write(Data, 0, Data.Length);
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
					{ "Source interface", HexUtils.PrettyHexString(HexUtils.ByteToHexString(SourceInterface.GetValue())) + " (" + SourceInterface.GetDescription() + ")" }
				};
				if (Data != null)
					parameters.Add("Data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Data)));
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="UserDataRelayOutputPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a User Data Relay Output packet (<c>0xAD</c>). The byte array must be in 
		/// <see cref="OperatingMode.API"/> mode.</param>
		/// <returns>Parsed User Data Relay Output packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.USER_DATA_RELAY_OUTPUT.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static UserDataRelayOutputPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("User Data Relay Output packet payload cannot be null.");
			// 1 (Frame type) + 1 (Dest. interface)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete User Data Relay Output packet.");
			if ((payload[0] & 0xFF) != APIFrameType.USER_DATA_RELAY_OUTPUT.GetValue())
				throw new ArgumentException("Payload is not a User Data Relay Ouptut packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Source interface.
			XBeeLocalInterface srcInterface = XBeeLocalInterface.UNKNOWN.Get(payload[index]);
			index = index + 1;

			// Data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
			}

			return new UserDataRelayOutputPacket(srcInterface, data);
		}
	}
}