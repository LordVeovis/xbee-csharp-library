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
	/// This class represents a Remote AT Command Request packet. Packet is built using the parameters 
	/// of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>Used to query or set module parameters on a remote device. For parameter changes on the 
	/// remote device to take effect, changes must be applied, either by setting the apply changes 
	/// options bit, or by sending an <c>AC</c> command to the remote node.
	/// 
	/// Remote Command options are set as a bitfield.
	/// 
	/// If configured, command response is received as a <see cref="RemoteATCommandResponsePacket"/>.</remarks>
	/// <seealso cref="RemoteATCommandResponsePacket"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class RemoteATCommandPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 15; // 1 (Frame type) + 1 (frame ID) + 8 (64-bit address) + 2 (16-bit address) + 1 (transmit options byte) + 2 (AT command)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteATCommandPacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="frameID">The Frame ID.</param>
		/// <param name="destAddress64">The 64-bit address of the destination device.</param>
		/// <param name="destAddress16">The 16-bit address of the destination device.</param>
		/// <param name="transmitOptions">The bitfield of supported transmission options.</param>
		/// <param name="command">The AT command.</param>
		/// <param name="parameter">The AT command parameter as string.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="destAddress64"/> == null</c> 
		/// or if <c><paramref name="destAddress16"/> == null</c> 
		/// or if <c><paramref name="command"/> == null</c>.</exception>
		/// <seealso cref="XBeeTransmitOptions"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteATCommandPacket(byte frameID, XBee64BitAddress destAddress64, XBee16BitAddress destAddress16,
			byte transmitOptions, string command, string parameter)
			: base(APIFrameType.REMOTE_AT_COMMAND_REQUEST)
		{
			FrameID = frameID;
			DestAddress64 = destAddress64 ?? throw new ArgumentNullException("64-bit destination address cannot be null.");
			DestAddress16 = destAddress16 ?? throw new ArgumentNullException("16-bit destination address cannot be null.");
			TransmitOptions = transmitOptions;
			Command = command ?? throw new ArgumentNullException("AT command cannot be null.");
			if (parameter != null)
				Parameter = Encoding.UTF8.GetBytes(parameter);
			logger = LogManager.GetLogger<RemoteATCommandPacket>();
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteATCommandPacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="frameID">The Frame ID.</param>
		/// <param name="destAddress64">The 64-bit address of the destination device.</param>
		/// <param name="destAddress16">The 16-bit address of the destination device.</param>
		/// <param name="transmitOptions">The bitfield of supported transmission options.</param>
		/// <param name="command">The AT command.</param>
		/// <param name="parameter">The AT command parameter.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="destAddress64"/> == null</c> 
		/// or if <c><paramref name="destAddress16"/> == null</c>
		/// or if <c><paramref name="command"/> == null</c>.</exception>
		/// <seealso cref="XBeeTransmitOptions"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteATCommandPacket(byte frameID, XBee64BitAddress destAddress64, XBee16BitAddress destAddress16,
			byte transmitOptions, string command, byte[] parameter)
			: base(APIFrameType.REMOTE_AT_COMMAND_REQUEST)
		{
			FrameID = frameID;
			DestAddress64 = destAddress64 ?? throw new ArgumentNullException("64-bit destination address cannot be null.");
			DestAddress16 = destAddress16 ?? throw new ArgumentNullException("16-bit destination address cannot be null.");
			TransmitOptions = transmitOptions;
			Command = command ?? throw new ArgumentNullException("AT command cannot be null.");
			Parameter = parameter;
			logger = LogManager.GetLogger<RemoteATCommandPacket>();
		}

		// Properties.
		/// <summary>
		/// The 64 bit destination address.
		/// </summary>
		/// <seealso cref="XBee64BitAddress"/>
		public XBee64BitAddress DestAddress64 { get; private set; }

		/// <summary>
		/// The 16 bit destination address.
		/// </summary>
		/// <seealso cref="XBee16BitAddress"/>
		public XBee16BitAddress DestAddress16 { get; private set; }

		/// <summary>
		/// The transmit options bitfield.
		/// </summary>
		public byte TransmitOptions { get; private set; }

		/// <summary>
		/// The AT Command.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// The AT command parameter.
		/// </summary>
		public byte[] Parameter { get; set; }

		/// <summary>
		/// The AT command parameter as String.
		/// </summary>
		public string StringParameter
		{
			get
			{
				if (Parameter == null)
					return null;
				return Encoding.UTF8.GetString(Parameter, 0, Parameter.Length);

			}
			set
			{
				if (value == null)
					Parameter = null;
				else
					Parameter = Encoding.UTF8.GetBytes(value);
			}
		}

		/// <summary>
		/// Indicates whether the API packet needs API Frame ID or not.
		/// </summary>
		public override bool NeedsAPIFrameID => true;

		/// <summary>
		/// Indicates whether the packet is a broadcast packet.
		/// </summary>
		public override bool IsBroadcast
		{
			get
			{
				return DestAddress64.Equals(XBee64BitAddress.BROADCAST_ADDRESS)
						|| DestAddress16.Equals(XBee16BitAddress.BROADCAST_ADDRESS);
			}
		}

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
						data.Write(DestAddress64.Value, 0, DestAddress64.Value.Length);
						data.Write(DestAddress16.Value, 0, DestAddress16.Value.Length);
						data.WriteByte(TransmitOptions);
						var rawCmd = ByteUtils.StringToByteArray(Command);
						data.Write(rawCmd, 0, rawCmd.Length);
						if (Parameter != null)
							data.Write(Parameter, 0, Parameter.Length);
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
					{ "64-bit dest. address", HexUtils.PrettyHexString(DestAddress64.ToString()) },
					{ "16-bit dest. address", HexUtils.PrettyHexString(DestAddress16.ToString()) },
					{ "Command options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitOptions, 1)) },
					{ "AT Command", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Encoding.UTF8.GetBytes(Command))) + " (" + Command + ")" }
				};
				if (Parameter != null)
				{
					if (Enum.TryParse(Command, out ATStringCommands cmd))
						parameters.Add("Parameter", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Parameter)) + " (" + Encoding.UTF8.GetString(Parameter, 0, Parameter.Length) + ")");
					else
						parameters.Add("Parameter", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Parameter)));
				}
				return parameters;
			}
		}

		/// <summary>
		/// Creates a new <seealso cref="RemoteATCommandPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to a Remote AT Command packet (<c>0x17</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed Remote AT Command packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.REMOTE_AT_COMMAND_REQUEST.GetValue()</c> 
		/// or if <c>payload.Length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static RemoteATCommandPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Remote AT Command packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 8 (64-bit address) + 2 (16-bit address) + 1 (transmit options byte) + 2 (AT command)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Remote AT Command packet.");
			if ((payload[0] & 0xFF) != APIFrameType.REMOTE_AT_COMMAND_REQUEST.GetValue())
				throw new ArgumentException("Payload is not a Remote AT Command packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			var array = new byte[8];
			Array.Copy(payload, index, array, 0, array.Length);
			// 8 bytes of 64-bit address.
			XBee64BitAddress destAddress64 = new XBee64BitAddress(array);
			index = index + 8;

			// 2 bytes of 16-bit address.
			XBee16BitAddress destAddress16 = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// Options byte.
			byte transmitOptions = payload[index];
			index = index + 1;

			// 2 bytes of AT command.
			byte[] commandByte = new byte[] { payload[index], payload[index + 1] };
			string command = Encoding.UTF8.GetString(commandByte, 0, commandByte.Length);
			index = index + 2;

			// Get data.
			byte[] parameterData = null;
			if (index < payload.Length)
			{
				parameterData = new byte[payload.Length - index];
				Array.Copy(payload, index, parameterData, 0, parameterData.Length);
			}

			return new RemoteATCommandPacket(frameID, destAddress64, destAddress16, transmitOptions,
					command, parameterData);
		}
	}
}