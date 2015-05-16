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
	 * This class represents a Remote AT Command Request packet. Packet is built
	 * using the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>Used to query or set module parameters on a remote device. For parameter 
	 * changes on the remote device to take effect, changes must be applied, either 
	 * by setting the apply changes options bit, or by sending an {@code AC} command 
	 * to the remote node.</p>
	 * 
	 * <p>Remote Command options are set as a bitfield.</p>
	 * 
	 * <p>If configured, command response is received as a 
	 * {@code RemoteATCommandResponse packet}.</p>
	 * 
	 * @see RemoteATCommandResponsePacket
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class RemoteATCommandPacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 15; // 1 (Frame type) + 1 (frame ID) + 8 (64-bit address) + 2 (16-bit address) + 1 (transmit options byte) + 2 (AT command)

		// Variables.
		private XBee64BitAddress destAddress64;

		private XBee16BitAddress destAddress16;

		/// <summary>
		/// Gets the transmit options bitfield.
		/// </summary>
		public byte transmitOptions { get; private set; }

		/// <summary>
		/// Gets the AT Command.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// Gets or sets the AT command parameter.
		/// </summary>
		public byte[] parameter { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code RemoteATCommandPacket} object from the given 
		 * payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a Remote AT Command packet ({@code 0x17}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed Remote AT Command Request packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.REMOTE_AT_COMMAND_REQUEST.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static RemoteATCommandPacket createPacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "Remote AT Command packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 8 (64-bit address) + 2 (16-bit address) + 1 (transmit options byte) + 2 (AT command)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete Remote AT Command packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.REMOTE_AT_COMMAND_REQUEST.GetValue(), "Payload is not a Remote AT Command packet.");

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

			return new RemoteATCommandPacket(frameID, destAddress64, destAddress16, transmitOptions,
					command, parameterData);
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteATCommandRequest}
		 * object with the given parameters.
		 * 
		 * @param frameID The Frame ID.
		 * @param destAddress64 64-bit address of the destination device.
		 * @param destAddress16 16-bit address of the destination device.
		 * @param transmitOptions Bitfield of supported transmission options.
		 * @param command AT command.
		 * @param parameter AT command parameter as String.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code destAddress64 == null} or
		 *                              if {@code destAddress16 == null} or
		 *                              if {@code command == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeTransmitOptions
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteATCommandPacket(byte frameID, XBee64BitAddress destAddress64, XBee16BitAddress destAddress16,
				byte transmitOptions, String command, string parameter)
			: base(APIFrameType.REMOTE_AT_COMMAND_REQUEST)
		{
			;

			if (destAddress64 == null)
				throw new ArgumentNullException("64-bit destination address cannot be null.");
			if (destAddress16 == null)
				throw new ArgumentNullException("16-bit destination address cannot be null.");
			if (command == null)
				throw new ArgumentNullException("AT command cannot be null.");

			this.frameID = frameID;
			this.destAddress64 = destAddress64;
			this.destAddress16 = destAddress16;
			this.transmitOptions = transmitOptions;
			this.Command = command;
			if (parameter != null)
				this.parameter = Encoding.UTF8.GetBytes(parameter);
			this.logger = LogManager.GetLogger<RemoteATCommandPacket>();
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteATCommandRequest} 
		 * object with the given parameters.
		 * 
		 * @param frameID Frame ID.
		 * @param destAddress64 64-bit address of the destination device.
		 * @param destAddress16 16-bit address of the destination device.
		 * @param transmitOptions Bitfield of supported transmission options.
		 * @param command AT command.
		 * @param parameter AT command parameter.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code destAddress64 == null} or
		 *                              if {@code destAddress16 == null} or
		 *                              if {@code command == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeTransmitOptions
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteATCommandPacket(byte frameID, XBee64BitAddress destAddress64, XBee16BitAddress destAddress16,
				byte transmitOptions, string command, byte[] parameter)
			: base(APIFrameType.REMOTE_AT_COMMAND_REQUEST)
		{
			if (destAddress64 == null)
				throw new ArgumentNullException("64-bit destination address cannot be null.");
			if (destAddress16 == null)
				throw new ArgumentNullException("16-bit destination address cannot be null.");
			if (command == null)
				throw new ArgumentNullException("AT command cannot be null.");

			this.frameID = frameID;
			this.destAddress64 = destAddress64;
			this.destAddress16 = destAddress16;
			this.transmitOptions = transmitOptions;
			this.Command = command;
			this.parameter = parameter;
			this.logger = LogManager.GetLogger<RemoteATCommandPacket>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (MemoryStream data = new MemoryStream())
				{
					try
					{
						data.Write(destAddress64.Value, 0, destAddress64.Value.Length);
						data.Write(destAddress16.Value, 0, destAddress16.Value.Length);
						data.WriteByte(transmitOptions);
						var rawCmd = ByteUtils.StringToByteArray(Command);
						data.Write(rawCmd, 0, rawCmd.Length);
						if (parameter != null)
							data.Write(parameter, 0, parameter.Length);
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

		public override bool IsBroadcast
		{
			get
			{
				return get64bitDestinationAddress().Equals(XBee64BitAddress.BROADCAST_ADDRESS)
						|| get16bitDestinationAddress().Equals(XBee16BitAddress.BROADCAST_ADDRESS);
			}
		}

		/**
		 * Returns the 64 bit destination address.
		 * 
		 * @return The 64 bit destination address.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public XBee64BitAddress get64bitDestinationAddress()
		{
			return destAddress64;
		}

		/**
		 * Returns the 16 bit destination address.
		 * 
		 * @return The 16 bit destination address.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public XBee16BitAddress get16bitDestinationAddress()
		{
			return destAddress16;
		}

		/// <summary>
		/// Gets or sets the AT command parameter as String.
		/// </summary>
		public string StringParameter
		{
			get
			{
				if (parameter == null)
					return null;
				return Encoding.UTF8.GetString(parameter);

			}
			set
			{
				if (value == null)
					this.parameter = null;
				else
					this.parameter = Encoding.UTF8.GetBytes(value);
			}
		}

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add("64-bit dest. address", HexUtils.PrettyHexString(destAddress64.ToString()));
				parameters.Add("16-bit dest. address", HexUtils.PrettyHexString(destAddress16.ToString()));
				parameters.Add("Command options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(transmitOptions, 1)));
				parameters.Add("AT Command", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(Encoding.UTF8.GetBytes(Command))) + " (" + Command + ")");
				if (parameter != null)
				{
					ATStringCommands cmd;
					if (Enum.TryParse<ATStringCommands>(Command, out cmd))
						parameters.Add("Parameter", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(parameter)) + " (" + Encoding.UTF8.GetString(parameter) + ")");
					else
						parameters.Add("Parameter", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(parameter)));
				}
				return parameters;
			}
		}
	}
}