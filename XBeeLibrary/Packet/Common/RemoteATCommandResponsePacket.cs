using Common.Logging;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kveer.XBeeApi.Packet.Common
{

	/**
	 * This class represents a Remote AT Command Response packet. Packet is built 
	 * using the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>If a module receives a remote command response RF data frame in response 
	 * to a Remote AT Command Request, the module will send a Remote AT Command 
	 * Response message out the UART. Some commands may send back multiple frames--
	 * for example, Node Discover ({@code ND}) command.</p>
	 * 
	 * <p>This packet is received in response of a {@code RemoteATCommandPacket}.
	 * </p>
	 * 
	 * <p>Response also includes an {@code ATCommandStatus} object with the status 
	 * of the AT command.</p>
	 * 
	 * @see RemoteATCommandPacket
	 * @see com.digi.xbee.api.models.ATCommandStatus
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class RemoteATCommandResponsePacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 15; // 1 (Frame type) + 1 (frame ID) + 8 (32-bit address) + 2 (16-bit address) + 2 (AT command) + 1 (status)

		// Variables.
		private XBee64BitAddress sourceAddress64;

		private XBee16BitAddress sourceAddress16;

		/// <summary>
		/// Gets the AT command response status.
		/// </summary>
		public ATCommandStatus Status { get; private set; }

		/// <summary>
		/// Gets the AT command.
		/// </summary>
		public string Command { get; private set; }

		private byte[] commandValue;

		private ILog logger;

		/**
		 * Creates an new {@code RemoteATCommandResponsePacket} object from the 
		 * given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a Remote AT Command Response packet ({@code 0x97}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed Remote AT Command Response packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.REMOTE_AT_COMMAND_RESPONSE.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static RemoteATCommandResponsePacket createPacket(byte[] payload)
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
			return new RemoteATCommandResponsePacket(frameID, sourceAddress64,
					sourceAddress16, command, ATCommandStatus.UNKNOWN.Get(status), commandData);
		}

		/**
		 * Class constructor. Instantiates a new 
		 * {@code RemoteATCommandResponsePacket} object with the given parameters.
		 * 
		 * @param frameID frame ID.
		 * @param sourceAddress64 64-bit address of the remote radio returning 
		 *                        response.
		 * @param sourceAddress16 16-bit network address of the remote.
		 * @param command The AT command.
		 * @param status The command status.
		 * @param commandValue The AT command response value.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code sourceAddress64 == null} or
		 *                              if {@code sourceAddress16 == null} or
		 *                              if {@code command == null} or
		 *                              if {@code status == null}.
		 * 
		 * @see com.digi.xbee.api.models.ATCommandStatus
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteATCommandResponsePacket(byte frameID, XBee64BitAddress sourceAddress64, XBee16BitAddress sourceAddress16,
				String command, ATCommandStatus status, byte[] commandValue)
			: base(APIFrameType.REMOTE_AT_COMMAND_RESPONSE)
		{

			if (sourceAddress64 == null)
				throw new ArgumentNullException("64-bit source address cannot be null.");
			if (sourceAddress16 == null)
				throw new ArgumentNullException("16-bit source address cannot be null.");
			if (command == null)
				throw new ArgumentNullException("AT command cannot be null.");
			if (status == null)
				throw new ArgumentNullException("AT command status cannot be null.");
			if (frameID < 0 || frameID > 255)
				throw new ArgumentException("Frame ID must be between 0 and 255.");

			this.frameID = frameID;
			this.sourceAddress64 = sourceAddress64;
			this.sourceAddress16 = sourceAddress16;
			this.Command = command;
			this.Status = status;
			this.commandValue = commandValue;
			this.logger = LogManager.GetLogger<RemoteATCommandResponsePacket>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (MemoryStream data = new MemoryStream())
				{
					try
					{
						data.Write(sourceAddress64.Value, 0, sourceAddress64.Value.Length);
						data.Write(sourceAddress16.Value, 0, sourceAddress16.Value.Length);
						var rawCmd = ByteUtils.StringToByteArray(Command);
						data.Write(rawCmd, 0, rawCmd.Length);
						data.WriteByte(Status.GetId());
						if (commandValue != null)
							data.Write(commandValue, 0, commandValue.Length);
					}
					catch (IOException e)
					{
						logger.Error(e.Message, e);
					}
					return data.ToArray();
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

		/**
		 * Returns the 64-bit source address. 
		 * 
		 * @return The 64-bit source address.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public XBee64BitAddress get64bitSourceAddress()
		{
			return sourceAddress64;
		}

		/**
		 * Returns the 16-bit source address.
		 * 
		 * @return The 16-bit source address.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public XBee16BitAddress get16bitSourceAddress()
		{
			return sourceAddress16;
		}


		/**
		 * Sets the AT command response value as String.
		 * 
		 * @param commandValue The AT command response value as String.
		 */
		public void setCommandValue(String commandValue)
		{
			if (commandValue == null)
				this.commandValue = null;
			else
				this.commandValue = Encoding.UTF8.GetBytes(commandValue);
		}

		/**
		 * Sets the AT response response value.
		 * 
		 * @param commandValue The AT command response value.
		 */
		public void setCommandValue(byte[] commandValue)
		{
			this.commandValue = commandValue;
		}

		/**
		 * Retrieves the AT command response value.
		 * 
		 * @return The AT command response value.
		 */
		public byte[] getCommandValue()
		{
			return commandValue;
		}

		/**
		 * Returns the AT command response value as String.
		 * 
		 * @return The AT command response value as String, {@code null} if no 
		 *         value is set.
		 */
		public string getCommandValueAsString()
		{
			if (commandValue == null)
				return null;
			return Encoding.UTF8.GetString(commandValue);
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
				parameters.Add("64-bit source address", HexUtils.PrettyHexString(sourceAddress64.ToString()));
				parameters.Add("16-bit source address", HexUtils.PrettyHexString(sourceAddress16.ToString()));
				parameters.Add("AT Command", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Encoding.UTF8.GetBytes(Command))) + " (" + Command + ")");
				parameters.Add("Status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(Status.GetId(), 1)) + " (" + Status.GetDescription() + ")");
				if (commandValue != null)
				{
					ATStringCommands cmd;
					if (Enum.TryParse<ATStringCommands>(Command, out cmd))
						parameters.Add("Response", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(commandValue)) + " (" + Encoding.UTF8.GetString(commandValue) + ")");
					else
						parameters.Add("Response", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(commandValue)));
				}
				return parameters;
			}
		}
	}
}