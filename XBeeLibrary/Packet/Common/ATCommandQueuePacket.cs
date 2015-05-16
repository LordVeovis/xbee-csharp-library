using Common.Logging;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Kveer.XBeeApi.Packet.Common
{
	/**
	 * This class represents an AT Command Queue XBee packet. Packet is built
	 * using the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>Used to query or set module parameters on the local device. In contrast
	 * to the {@link ATCommandPacket} API packet, new parameter values are queued 
	 * and not applied until either an {@code ATCommandPacket} is sent or the 
	 * {@code applyChanges()} method of the {@code XBeeDevice} is issued.</p>
	 * 
	 * <p>Register queries (reading parameter values) are returned immediately.</p>
	 * 
	 * <p>Command response is received as an {@code ATCommandResponsePacket}.</p>
	 * 
	 * @see ATCommandPacket
	 * @see ATCommandResponsePacket
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class ATCommandQueuePacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 4; // 1 (Frame type) + 1 (frame ID) + 2 (AT command)

		/// <summary>
		/// Gets the AT command.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// Gets or sets the AT command parameter.
		/// </summary>
		public byte[] Parameter { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code ATCommandQueuePacket} object from the given 
		 * payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a AT Command Queue packet 
		 *                ({@code 0x09}). The byte array must be in 
		 *                {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed AT Command Queue packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.AT_COMMAND_QUEUE.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static ATCommandQueuePacket CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "AT Command Queue packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 2 (AT command)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete AT Command Queue packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) != APIFrameType.AT_COMMAND_QUEUE.GetValue(), "Payload is not an AT Command Queue packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			// 2 bytes of AT command, starting at 2nd byte.
			string command = Encoding.UTF8.GetString(new byte[] { payload[index], payload[index + 1] });
			index = index + 2;

			// Get data.
			byte[] parameterData = null;
			if (index < payload.Length)
			{
				parameterData = new byte[payload.Length - index];
				Array.Copy(payload, index, parameterData, 0, parameterData.Length);
				//parameterData = Arrays.copyOfRange(payload, index, payload.Length);
			}

			return new ATCommandQueuePacket(frameID, command, parameterData);
		}

		/**
		 * Class constructor. Instantiates a new {@code ATCommandQueuePacket} 
		 * object with the given parameters.
		 * 
		 * @param frameID XBee API frame ID.
		 * @param command AT command.
		 * @param parameter AT command parameter as String, {@code null} if it is 
		 *                  not required.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code command == null}.
		 */
		public ATCommandQueuePacket(byte frameID, String command, String parameter)
			: this(frameID, command, parameter == null ? null : Encoding.UTF8.GetBytes(parameter))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code ATCommandQueuePacket} 
		 * object with the given parameters.
		 * 
		 * @param frameID XBee API frame ID.
		 * @param command AT command.
		 * @param parameter AT command parameter {@code null} if it is not required.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code command == null}.
		 */
		public ATCommandQueuePacket(byte frameID, String command, byte[] parameter)
			: base(APIFrameType.AT_COMMAND_QUEUE)
		{

			if (command == null)
				throw new ArgumentNullException("AT command cannot be null.");

			this.frameID = frameID;
			this.Command = command;
			this.Parameter = parameter;
			this.logger = LogManager.GetLogger<ATCommandQueuePacket>();
		}

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

		public override bool NeedsAPIFrameID
		{
			get
			{
				return true;
			}
		}

		public string StringParameter
		{
			get
			{
				if (Parameter == null)
					return null;
				return Encoding.UTF8.GetString(Parameter);
			}
			set
			{
				if (value == null)
					this.Parameter = null;
				else
					this.Parameter = Encoding.UTF8.GetBytes(value);
			}
		}

		public override bool IsBroadcast
		{
			get
			{
				return false;
			}
		}

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add("AT Command", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Encoding.UTF8.GetBytes(Command))) + " (" + Command + ")");
				if (Parameter != null)
				{
					ATStringCommands cmd;

					if (Enum.TryParse<ATStringCommands>(Command, out cmd))
						parameters.Add("Parameter", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Parameter)) + " (" + Encoding.UTF8.GetString(Parameter) + ")");
					else
						parameters.Add("Parameter", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Parameter)));
				}
				return parameters;
			}
		}
	}
}