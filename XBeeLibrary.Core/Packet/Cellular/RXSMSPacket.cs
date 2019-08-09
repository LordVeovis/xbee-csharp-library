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
using System.Text;
using System.Text.RegularExpressions;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.Cellular
{
	/// <summary>
	/// This class represents a RX (Receive) SMS packet. Packet is built using the parameters of the 
	/// constructor or providing a valid API payload.
	/// </summary>
	/// <see cref="TXSMSPacket"/>
	/// <see cref="XBeeAPIPacket"/>
	public class RXSMSPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 1 /* (Frame type) */ + TXSMSPacket.PHONE_NUMBER_LENGTH;

		private const string ERROR_PAYLOAD_NULL = "RX SMS packet payload cannot be null.";
		private const string ERROR_INCOMPLETE_PACKET = "Incomplete RX SMS packet.";
		private const string ERROR_NOT_RXSMS = "Payload is not a RX SMS packet.";

		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RXSMSPacket"/> object with the given 
		/// parameters.
		/// </summary>
		/// <param name="phoneNumber">The phone number. Only numbers and '+' prefix allowed.</param>
		/// <param name="data">The received data.</param>
		/// <exception cref="ArgumentException">If 
		/// <c><paramref name="phoneNumber"/> <![CDATA[>]]> <see cref="TXSMSPacket.PHONE_NUMBER_LENGTH"/></c> 
		/// or if <paramref name="phoneNumber"/> is invalid.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="phoneNumber"/> == null</c>.</exception>
		public RXSMSPacket(string phoneNumber, string data)
			: base(APIFrameType.RX_SMS)
		{
			if (phoneNumber == null)
				throw new ArgumentNullException(TXSMSPacket.ERROR_PHONE_NUMBER_NULL);
			if (phoneNumber.Length > TXSMSPacket.PHONE_NUMBER_LENGTH)
				throw new ArgumentException(TXSMSPacket.ERROR_PHONE_NUMBER_LENGTH);
			if (!Regex.IsMatch(phoneNumber, TXSMSPacket.PHONE_NUMBER_PATTERN))
				throw new ArgumentException(TXSMSPacket.ERROR_PHONE_NUMBER_INVALID);

			PhoneNumberByteArray = new byte[TXSMSPacket.PHONE_NUMBER_LENGTH];
			byte[] phoneNumberBytes = Encoding.UTF8.GetBytes(phoneNumber);
			Array.Copy(phoneNumberBytes, PhoneNumberByteArray, phoneNumberBytes.Length);
			Data = data;
			logger = LogManager.GetLogger<RXSMSPacket>();
		}

		// Properties.
		/// <summary>
		/// The phone number. Only numbers and '+' prefix allowed.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the value to be set is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If the length of the value to be set is greater than 
		/// <see cref="TXSMSPacket.PHONE_NUMBER_LENGTH"/> or if the value to be set is invalid.</exception>
		public string PhoneNumber
		{

			get
			{
				return Encoding.UTF8.GetString(PhoneNumberByteArray).Replace("\0", "");
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException(TXSMSPacket.ERROR_PHONE_NUMBER_NULL);
				if (value.Length > TXSMSPacket.PHONE_NUMBER_LENGTH)
					throw new ArgumentException(TXSMSPacket.ERROR_PHONE_NUMBER_LENGTH);
				if (!Regex.IsMatch(value, TXSMSPacket.PHONE_NUMBER_PATTERN))
					throw new ArgumentException(TXSMSPacket.ERROR_PHONE_NUMBER_INVALID);
				byte[] phoneNumberBytes = Encoding.UTF8.GetBytes(value);
				PhoneNumberByteArray = new byte[TXSMSPacket.PHONE_NUMBER_LENGTH];
				Array.Copy(phoneNumberBytes, PhoneNumberByteArray, phoneNumberBytes.Length);
			}
		}

		/// <summary>
		/// The phone number byte array.
		/// </summary>
		public byte[] PhoneNumberByteArray { get; private set; }

		/// <summary>
		/// The received data.
		/// </summary>
		public string Data { get; set; }

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
				using (MemoryStream ms = new MemoryStream())
				{
					try
					{
						ms.Write(PhoneNumberByteArray, 0, TXSMSPacket.PHONE_NUMBER_LENGTH);
						if (Data != null)
							ms.Write(Encoding.UTF8.GetBytes(Data), 0, Data.Length);
					}
					catch (IOException e)
					{
						logger.Error(e.Message, e);
					}
					return ms.ToArray();
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
					{ "Phone number", ByteUtils.ByteArrayToString(PhoneNumberByteArray).Replace("\0", "") }
				};
				if (Data != null)
					parameters.Add("Data", Data);
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <see cref="RXSMSPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a RX SMS packet (<c>0x9F</c>). The byte array must be in <see cref="Models.OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed RX SMS packet.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="payload"/>[0] != APIFrameType.RX_SMS.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static RXSMSPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException(ERROR_PAYLOAD_NULL);
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException(ERROR_INCOMPLETE_PACKET);
			if ((payload[0] & 0xFF) != APIFrameType.RX_SMS.GetValue())
				throw new ArgumentException(ERROR_NOT_RXSMS);

			// payload[0] is the frame type.
			int index = 1;

			// Bytes of phone number.
			byte[] phoneNumber = new byte[TXSMSPacket.PHONE_NUMBER_LENGTH];
			Array.Copy(payload, index, phoneNumber, 0, TXSMSPacket.PHONE_NUMBER_LENGTH);
			index += TXSMSPacket.PHONE_NUMBER_LENGTH;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				int dataLength = payload.Length - index;
				data = new byte[dataLength];
				Array.Copy(payload, index, data, 0, dataLength);
			}

			return new RXSMSPacket(Encoding.UTF8.GetString(phoneNumber).Replace("\0", ""),
				data == null ? null : Encoding.UTF8.GetString(data));
		}
	}
}