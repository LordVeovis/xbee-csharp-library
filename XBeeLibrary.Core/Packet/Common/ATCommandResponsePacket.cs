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
using System.Text;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;
namespace XBeeLibrary.Core.Packet.Common
{
	/// <summary>
	/// This class represents an AT Command Response packet. Packet is built using the parameters of 
	/// the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>In response to an AT Command message, the module will send an AT Command Response 
	/// message. Some commands will send back multiple frames (for example, the ND (Node Discover) command). 
	/// 
	/// This packet is received in response of an <see cref="ATCommandPacket"/>.
	/// 
	/// Response also includes an <see cref="ATCommandStatus"/> object with the status of the AT 
	/// command.</remarks>
	/// <seealso cref="ATCommandPacket"/>
	/// <seealso cref="ATCommandStatus"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class ATCommandResponsePacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 5; // 1 (Frame type) + 1 (frame ID) + 2 (AT command) + 1 (status byte)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a <see cref="ATCommandResponsePacket"/> with the given parameters.
		/// </summary>
		/// <param name="frameID">The XBee API frame ID.</param>
		/// <param name="status">The AT command response status.</param>
		/// <param name="command">The AT command.</param>
		/// <param name="commandValue">The AT command response value.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/> is <c>null</c>.</exception>
		/// <seealso cref="ATCommandStatus"/>
		public ATCommandResponsePacket(byte frameID, ATCommandStatus status, string command, byte[] commandValue)
			: base(APIFrameType.AT_COMMAND_RESPONSE)
		{
			FrameID = frameID;
			Status = status;
			Command = command ?? throw new ArgumentNullException("AT command cannot be null.");
			CommandValue = commandValue;
			logger = LogManager.GetLogger<ATCommandResponsePacket>();
		}

		// Properties.
		/// <summary>
		/// The AT command response status.
		/// </summary>
		/// <seealso cref="ATCommandStatus"/>
		public ATCommandStatus Status { get; private set; }

		/// <summary>
		/// The AT command.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// The AT command response value.
		/// </summary>
		public byte[] CommandValue { get; set; }

		/// <summary>
		/// The AT command response value as String.
		/// </summary>
		public string StringCommandValue
		{
			get
			{
				if (CommandValue == null)
					return null;
				return Encoding.UTF8.GetString(CommandValue, 0, CommandValue.Length);

			}
			set
			{
				if (value == null)
					CommandValue = null;
				else
					CommandValue = Encoding.UTF8.GetBytes(value);
			}
		}

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
				using (MemoryStream os = new MemoryStream())
				{
					try
					{
						var rawCmd = Encoding.UTF8.GetBytes(Command);
						os.Write(rawCmd, 0, rawCmd.Length);
						os.WriteByte(Status.GetId());
						if (CommandValue != null)
							os.Write(CommandValue, 0, CommandValue.Length);
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
					{ "AT Command", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Encoding.UTF8.GetBytes(Command))) + " (" + Command + ")" },
					{ "Status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(Status.GetId(), 1)) + " (" + Status.GetDescription() + ")" }
				};
				if (CommandValue != null)
				{
					if (Enum.TryParse(Command, out ATStringCommands cmd))
						parameters.Add("Response", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(CommandValue)) + " (" + Encoding.UTF8.GetString(CommandValue, 0, CommandValue.Length) + ")");
					else
						parameters.Add("Response", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(CommandValue)));
				}
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <seealso cref="ATCommandResponsePacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to an AT Command Response packet (<c>0x88</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed AT Command Response packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.AT_COMMAND_RESPONSE.GetValue()</c> 
		/// or if <c>payload.Length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static ATCommandResponsePacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("AT Command Response packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 2 (AT command) + 1 (status byte)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete AT Command Response packet.");
			if ((payload[0] & 0xFF) != APIFrameType.AT_COMMAND_RESPONSE.GetValue())
				throw new ArgumentException("Payload is not an AT Command Response packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			// 2 bytes of AT command, starting at 2nd byte.
			byte[] commandByte = new byte[] { payload[index], payload[index + 1] };
			string command = Encoding.UTF8.GetString(commandByte, 0, commandByte.Length);
			index = index + 2;

			// Status byte.
			byte status = payload[index];
			index = index + 1;

			// Get data.
			byte[] commandData = null;
			if (index < payload.Length)
			{
				commandData = new byte[payload.Length - index];
				Array.Copy(payload, index, commandData, 0, commandData.Length);
			}

			// TODO if ATCommandStatus is unknown????
			return new ATCommandResponsePacket(frameID, ATCommandStatus.UNKNOWN.Get(status), command, commandData);
		}
	}
}