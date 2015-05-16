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
	 * This class represents an AT Command Response packet. Packet is built using 
	 * the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>In response to an AT Command message, the module will send an AT Command 
	 * Response message. Some commands will send back multiple frames (for example, 
	 * the ND (Node Discover) command).</p>
	 * 
	 * <p>This packet is received in response of an {@code ATCommandPacket}.</p>
	 * 
	 * <p>Response also includes an {@code ATCommandStatus} object with the status 
	 * of the AT command.</p>
	 * 
	 * @see ATCommandPacket
	 * @see com.digi.xbee.api.models.ATCommandStatus
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class ATCommandResponsePacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 5; // 1 (Frame type) + 1 (frame ID) + 2 (AT command) + 1 (status byte)


		/// <summary>
		/// Gets the AT command response status.
		/// </summary>
		public ATCommandStatus Status { get; private set; }

		/// <summary>
		/// Gets the AT command.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// Gets or sets the AT command response value.
		/// </summary>
		public byte[] CommandValue { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code ATCommandResponsePacket} object from the given 
		 * payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a AT Command Response packet ({@code 0x88}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed AT Command Response packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.AT_COMMAND.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static ATCommandResponsePacket createPacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "AT Command Response packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 2 (AT command) + 1 (status byte)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete AT Command Response packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.AT_COMMAND_RESPONSE.GetValue(), "Payload is not an AT Command Response packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			// 2 bytes of AT command, starting at 2nd byte.
			string command = Encoding.UTF8.GetString(new byte[] { payload[index], payload[index + 1] });
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
				//commandData = Arrays.copyOfRange(payload, index, payload.Length);
			}

			// TODO if ATCommandStatus is unknown????
			return new ATCommandResponsePacket(frameID, ATCommandStatus.UNKNOWN.Get(status), command, commandData);
		}

		/**
		 * Class constructor. Instantiates a new {@code ATCommandResponsePacket} 
		 * object with the given parameters.
		 * 
		 * @param frameID The XBee API frame ID.
		 * @param status The AT command response status.
		 * @param command The AT command.
		 * @param commandValue The AT command response value.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code status == null} or 
		 *                              if {@code command == null}.
		 * 
		 * @see com.digi.xbee.api.models.ATCommandStatus
		 */
		public ATCommandResponsePacket(byte frameID, ATCommandStatus status, String command, byte[] commandValue)
			: base(APIFrameType.AT_COMMAND_RESPONSE)
		{
			if (command == null)
				throw new ArgumentNullException("AT command cannot be null.");
			if (status == null)
				throw new ArgumentNullException("AT command status cannot be null.");

			this.frameID = frameID;
			this.Status = status;
			this.Command = command;
			this.CommandValue = commandValue;
			this.logger = LogManager.GetLogger<ATCommandResponsePacket>();
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

		public override bool NeedsAPIFrameID
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets or sets the AT command response value as String.
		/// </summary>
		public string StringCommandValue
		{
			get
			{
				if (CommandValue == null)
					return null;
				return Encoding.UTF8.GetString(CommandValue);

			}
			set
			{
				if (value == null)
					this.CommandValue = null;
				else
					this.CommandValue = Encoding.UTF8.GetBytes(value);
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
				parameters.Add("Status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(Status.GetId(), 1)) + " (" + Status.GetDescription() + ")");
				if (CommandValue != null)
				{
					ATStringCommands cmd;
					if (Enum.TryParse<ATStringCommands>(Command, out cmd))
						parameters.Add("Response", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(CommandValue)) + " (" + Encoding.UTF8.GetString(CommandValue) + ")");
					else
						parameters.Add("Response", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(CommandValue)));
				}
				return parameters;
			}
		}
	}
}