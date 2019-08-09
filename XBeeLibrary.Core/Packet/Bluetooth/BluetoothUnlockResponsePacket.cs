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

namespace XBeeLibrary.Core.Packet.Bluetooth
{
	/// <summary>
	/// This class represents a Bluetooth Unlock Response packet.
	/// </summary>
	/// <seealso cref="XBeeAPIPacket"/>
	public class BluetoothUnlockResponsePacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 2; // 1 (Frame type) + 1 (Step)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a <see cref="BluetoothUnlockResponsePacket"/> packet
		/// with the given parameters.
		/// </summary>
		/// <param name="phase">The SRP phase.</param>
		/// <param name="data">The data.</param>
		/// <exception cref="ArgumentException">If <paramref name="phase"/> is <see cref="SrpPhase.UNKNOWN"/>.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="data"/> is <c>null</c>.</exception>
		/// <seealso cref="Models.SrpPhase"/>
		public BluetoothUnlockResponsePacket(SrpPhase phase, byte[] data)
			: base(APIFrameType.BLE_UNLOCK_RESPONSE)
		{
			if (phase == SrpPhase.UNKNOWN)
				throw new ArgumentException("SRP phase cannot be unknown.");
			SrpPhase = phase;
			Data = data ?? throw new ArgumentNullException("Data cannot be null.");
			logger = LogManager.GetLogger<BluetoothUnlockResponsePacket>();
		}

		/// <summary>
		/// Class constructor. Instantiates a <see cref="BluetoothUnlockResponsePacket"/> packet
		/// with the given parameters.
		/// </summary>
		/// <param name="error">SRP error</param>
		/// <seealso cref="Models.SrpError"/>
		public BluetoothUnlockResponsePacket(SrpError error)
			: base(APIFrameType.BLE_UNLOCK_RESPONSE)
		{
			SrpError = error;
			logger = LogManager.GetLogger<BluetoothUnlockResponsePacket>();
		}

		// Properties.
		/// <summary>
		/// The SRP Phase.
		/// </summary>
		/// <seealso cref="Models.SrpPhase"/>
		public SrpPhase SrpPhase { get; private set; } = SrpPhase.UNKNOWN;

		/// <summary>
		/// The SRP error, if any.
		/// </summary>
		/// <seealso cref="Models.SrpError"/>
		public SrpError SrpError { get; private set; } = SrpError.UNKNOWN;

		/// <summary>
		/// The SRP data.
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
						if (SrpPhase != SrpPhase.UNKNOWN)
						{
							data.WriteByte(SrpPhase.GetValue());
							data.Write(Data, 0, Data.Length);
						}
						else
						{
							data.WriteByte(SrpError.GetValue());
						}
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
				if (SrpPhase != SrpPhase.UNKNOWN)
				{
					parameters.Add("SRP phase", HexUtils.PrettyHexString(HexUtils.ByteToHexString(SrpPhase.GetValue())));
					parameters.Add("Data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Data)));
				}
				else
				{
					parameters.Add("SRP error", HexUtils.PrettyHexString(HexUtils.ByteToHexString(SrpError.GetValue())));
				}
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <seealso cref="BluetoothUnlockResponsePacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a Bluetooth Unlock Response packet (<c>0xAC</c>). The byte array must be in 
		/// <see cref="OperatingMode.API"/> mode.</param>
		/// <returns>Parsed Bluetooth Unlock packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.BLE_UNLOCK_RESPONSE.GetValue()</c>
		/// or if <c>payload.Length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static BluetoothUnlockResponsePacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Bluetooth Unlock Response packet payload cannot be null.");
			// 1 (Frame type) + 1 (Step)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Bluetooth Unlock Response packet.");
			if ((payload[0] & 0xFF) != APIFrameType.BLE_UNLOCK_RESPONSE.GetValue())
				throw new ArgumentException("Payload is not a Bluetooth Unlock Response packet.");

			// payload[0] is the frame type.
			int index = 1;

			// SRP phase.
			SrpPhase phase = SrpPhase.UNKNOWN.Get(payload[index]);

			// If the phase is unknown, the packet contains an error.
			if (phase == SrpPhase.UNKNOWN)
				return new BluetoothUnlockResponsePacket(SrpError.UNKNOWN.Get(payload[index]));

			index = index + 1;

			// Data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
			}

			return new BluetoothUnlockResponsePacket(phase, data);
		}
	}
}