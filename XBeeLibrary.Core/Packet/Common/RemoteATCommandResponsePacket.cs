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
	/// This class represents a Remote AT Command Response packet. Packet is built using the parameters 
	/// of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>If a module receives a remote command response RF data frame in response to a Remote 
	/// AT Command Request, the module will send a Remote AT Command Response message out the UART. 
	/// Some commands may send back multiple frames--for example, Node Discover (<c>ND</c>) command.
	/// 
	/// This packet is received in response of a {@code RemoteATCommandPacket}.
	/// 
	/// Response also includes an <see cref="ATCommandStatus"/> object with the status of the 
	/// AT command.</remarks>
	/// <seealso cref="RemoteATCommandPacket"/>
	/// <seealso cref="ATCommandStatus"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class RemoteATCommandResponsePacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 15; // 1 (Frame type) + 1 (frame ID) + 8 (32-bit address) + 2 (16-bit address) + 2 (AT command) + 1 (status)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteATCommandResponsePacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="frameID">The frame ID.</param>
		/// <param name="sourceAddress64">The 64-bit address of the sender device.</param>
		/// <param name="sourceAddress16">The 16-bit address of the sender device.</param>
		/// <param name="command">The AT command.</param>
		/// <param name="status">The AT command response status.</param>
		/// <param name="commandValue">The AT command response value.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="sourceAddress64"/> == null</c> 
		/// or if <c><paramref name="sourceAddress16"/> == null</c> 
		/// or if <c><paramref name="command"/> == null</c>.</exception>
		/// <seealso cref="XBeeReceiveOptions"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteATCommandResponsePacket(byte frameID, XBee64BitAddress sourceAddress64, XBee16BitAddress sourceAddress16,
			string command, ATCommandStatus status, byte[] commandValue)
			: base(APIFrameType.REMOTE_AT_COMMAND_RESPONSE)
		{
			if (frameID < 0 || frameID > 255)
				throw new ArgumentException("Frame ID must be between 0 and 255.");

			FrameID = frameID;
			SourceAddress64 = sourceAddress64 ?? throw new ArgumentNullException("64-bit source address cannot be null.");
			SourceAddress16 = sourceAddress16 ?? throw new ArgumentNullException("16-bit source address cannot be null.");
			Command = command ?? throw new ArgumentNullException("AT command cannot be null.");
			Status = status;
			CommandValue = commandValue;
			logger = LogManager.GetLogger<RemoteATCommandResponsePacket>();
		}

		// Properties.
		/// <summary>
		/// The 64 bit sender/source address.
		/// </summary>
		/// <seealso cref="XBee64BitAddress"/>
		public XBee64BitAddress SourceAddress64 { get; private set; }

		/// <summary>
		/// The 16 bit sender/source address.
		/// </summary>
		/// <seealso cref="XBee16BitAddress"/>
		public XBee16BitAddress SourceAddress16 { get; private set; }

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
				using (MemoryStream data = new MemoryStream())
				{
					try
					{
						data.Write(SourceAddress64.Value, 0, SourceAddress64.Value.Length);
						data.Write(SourceAddress16.Value, 0, SourceAddress16.Value.Length);
						var rawCmd = ByteUtils.StringToByteArray(Command);
						data.Write(rawCmd, 0, rawCmd.Length);
						data.WriteByte(Status.GetId());
						if (CommandValue != null)
							data.Write(CommandValue, 0, CommandValue.Length);
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
					{ "64-bit source address", HexUtils.PrettyHexString(SourceAddress64.ToString()) },
					{ "16-bit source address", HexUtils.PrettyHexString(SourceAddress16.ToString()) },
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
		/// Creates a new <see cref="RemoteATCommandResponsePacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a Remote AT Command Response packet (<c>0x97</c>). The byte array must be in 
		/// <see cref="OperatingMode.API"/> mode.</param>
		/// <returns>Parsed Remote AT Command Response packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.REMOTE_AT_COMMAND_RESPONSE.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static RemoteATCommandResponsePacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Remote AT Command Response packet payload cannot be null.");

			// 1 (Frame type) + 1 (frame ID) + 8 (32-bit address) + 2 (16-bit address) + 2 (AT command) + 1 (status)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Remote AT Command Response packet.");

			if ((payload[0] & 0xFF) != APIFrameType.REMOTE_AT_COMMAND_RESPONSE.GetValue())
				throw new ArgumentException("Payload is not a Remote AT Command Response packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			var addr = new byte[8];
			Array.Copy(payload, index, addr, 0, addr.Length);
			// 8 bytes of 64-bit address.
			XBee64BitAddress sourceAddress64 = new XBee64BitAddress(addr);
			index = index + 8;

			// 2 bytes of 16-bit address.
			XBee16BitAddress sourceAddress16 = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// 2 bytes of AT command.
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
			return new RemoteATCommandResponsePacket(frameID, sourceAddress64,
					sourceAddress16, command, ATCommandStatus.UNKNOWN.Get(status), commandData);
		}
	}
}