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
	/// This class represents a Bluetooth Unlock packet.
	/// </summary>
	/// <seealso cref="XBeeAPIPacket"/>
	public class BluetoothUnlockPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 34; // 1 (Frame type) + 1 (Step) + 32 (Hash length)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a <see cref="BluetoothUnlockPacket"/> with the given parameters.
		/// </summary>
		/// <param name="phase">The SRP phase.</param>
		/// <param name="data">The data.</param>
		/// <exception cref="ArgumentException">If <paramref name="phase"/> is <see cref="SrpPhase.UNKNOWN"/>.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="data"/> is <c>null</c>.</exception>
		/// <seealso cref="Models.SrpPhase"/>
		public BluetoothUnlockPacket(SrpPhase phase, byte[] data) : base(APIFrameType.BLE_UNLOCK)
		{
			if (phase == SrpPhase.UNKNOWN)
				throw new ArgumentException("SRP phase cannot be unknown.");
			SrpPhase = phase;
			Data = data ?? throw new ArgumentNullException("Data cannot be null.");
			logger = LogManager.GetLogger<BluetoothUnlockPacket>();
		}

		// Properties.
		/// <summary>
		/// The SRP Phase.
		/// </summary>
		/// <seealso cref="SrpPhase"/>
		public SrpPhase SrpPhase { get; private set; }

		/// <summary>
		/// The data to send.
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
						data.WriteByte(SrpPhase.GetValue());
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
					{ "SRP phase", HexUtils.PrettyHexString(HexUtils.ByteToHexString(SrpPhase.GetValue())) },
					{ "Data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Data)) }
				};
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <seealso cref="BluetoothUnlockPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a Bluetooth Unlock packet (<c>0x2C</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed Bluetooth Unlock packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.BLE_UNLOCK.GetValue()</c> 
		/// or if <c>payload.Length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static BluetoothUnlockPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Bluetooth Unlock packet payload cannot be null.");
			// 1 (Frame type) + 1 (Step) + 32 (Hash length)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Bluetooth Unlock packet.");
			if ((payload[0] & 0xFF) != APIFrameType.BLE_UNLOCK.GetValue())
				throw new ArgumentException("Payload is not a Bluetooth Unlock packet.");

			// payload[0] is the frame type.
			int index = 1;

			// SRP phase.
			SrpPhase phase = SrpPhase.UNKNOWN.Get(payload[index]);
			index = index + 1;

			// Data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
			}

			return new BluetoothUnlockPacket(phase, data);
		}
	}
}