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
	/// This class represents an AT Command Queue XBee packet. Packet is built using the parameters 
	/// of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>Used to query or set module parameters on the local device. In contrast to the 
	/// <see cref="ATCommandPacket"/> API packet, new parameter values are queued and not applied until 
	/// either an <see cref="ATCommandPacket"/> is sent or the 
	/// <see cref="AbstractXBeeDevice.ApplyChanges"/> method is issued.
	/// 
	/// Register queries (reading parameter values) are returned immediately.
	/// 
	/// Command response is received as an <see cref="ATCommandResponsePacket"/>.</remarks>
	/// <seealso cref="ATCommandPacket"/>
	/// <seealso cref="ATCommandResponsePacket"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class ATCommandQueuePacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 4; // 1 (Frame type) + 1 (frame ID) + 2 (AT command)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a <see cref="ATCommandQueuePacket"/> with the given parameters.
		/// </summary>
		/// <param name="frameID">The XBee API frame ID.</param>
		/// <param name="command">The AT command.</param>
		/// <param name="parameter">AT command parameter as string, <c>null</c> if it is not required.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/> is <c>null</c>.</exception>
		public ATCommandQueuePacket(byte frameID, string command, string parameter)
			: this(frameID, command, parameter == null ? null : Encoding.UTF8.GetBytes(parameter)) { }

		/// <summary>
		/// Class constructor. Instantiates a <see cref="ATCommandQueuePacket"/> with the given parameters.
		/// </summary>
		/// <param name="frameID">The XBee API frame ID.</param>
		/// <param name="command">The AT command.</param>
		/// <param name="parameter">The AT command parameter, <c>null</c> if it is not required.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255</c>.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/> is <c>null</c>.</exception>
		public ATCommandQueuePacket(byte frameID, string command, byte[] parameter)
			: base(APIFrameType.AT_COMMAND_QUEUE)
		{
			FrameID = frameID;
			Command = command ?? throw new ArgumentNullException("AT command cannot be null.");
			Parameter = parameter;
			logger = LogManager.GetLogger<ATCommandQueuePacket>();
		}

		// Properties.
		/// <summary>
		/// The AT command.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// The AT command parameter.
		/// </summary>
		public byte[] Parameter { get; set; }

		/// <summary>
		/// The AT command parameter as string.
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
						if (Parameter != null)
							os.Write(Parameter, 0, Parameter.Length);
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
		/// A sorted dictionary with the packet parameters and their values.
		/// </summary>
		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>
				{
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
		/// Creates a new <seealso cref="ATCommandQueuePacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding 
		/// to an AT Command Queue packet (<c>0x09</c>). The byte array must be in <see cref="OperatingMode.API"/> 
		/// mode.</param>
		/// <returns>Parsed AT Command Queue packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.AT_COMMAND_QUEUE.GetValue()</c> 
		/// or if <c>payload.Length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static ATCommandQueuePacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("AT Command Queue packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 2 (AT command)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete AT Command Queue packet.");
			if ((payload[0] & 0xFF) != APIFrameType.AT_COMMAND_QUEUE.GetValue())
				throw new ArgumentException("Payload is not an AT Command Queue packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			// 2 bytes of AT command, starting at 2nd byte.
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

			return new ATCommandQueuePacket(frameID, command, parameterData);
		}
	}
}