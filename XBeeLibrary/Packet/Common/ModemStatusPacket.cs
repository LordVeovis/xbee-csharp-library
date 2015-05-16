using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Kveer.XBeeApi.Packet.Common
{

	/**
	 * This class represents a Modem Status packet. Packet is built using the 
	 * parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>RF module status messages are sent from the module in response to specific 
	 * conditions and indicates the state of the modem in that moment.</p>
	 * 
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class ModemStatusPacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 2; // 1 (Frame type) + 1 (Modem status)

		// Variables.
		private ModemStatusEvent modemStatusEvent;

		/**
		 * Creates a new {@code ModemStatusPacket} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a Modem Status packet ({@code 0x8A}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed Modem Status packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.MODEM_STATUS.getValue()} or
		 *                                  if {@code payload.Length < {@value #MIN_API_PAYLOAD_LENGTH}}.
		 * @throws ArgumentNullException if {@code payload == null} or 
		 *                              if {@code modemStatusEvent == null}.
		 */
		public static ModemStatusPacket CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "Modem Status packet payload cannot be null.");
			// 1 (Frame type) + 1 (Modem status)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete Modem Status packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.MODEM_STATUS.GetValue(), "Payload is not a Modem Status packet.");

			// Get the Modem status byte (byte 1).
			int status = payload[1] & 0xFF;

			// Get the Modem Status enum. entry.
			ModemStatusEvent modemStatusEvent = (ModemStatusEvent)status;

			return new ModemStatusPacket(modemStatusEvent);
		}

		/**
		 * Class constructor. Instantiates a new {@code ModemStatusPacket} object
		 * with the given modem status.
		 * 
		 * @param modemStatusEvent Modem status event enum. entry.
		 * 
		 * @throws ArgumentNullException if {@code modemStatusEvent == null}.
		 */
		public ModemStatusPacket(ModemStatusEvent modemStatusEvent)
			: base(APIFrameType.MODEM_STATUS)
		{
			if (modemStatusEvent == null)
				throw new ArgumentNullException("Modem Status event cannot be null.");

			this.modemStatusEvent = modemStatusEvent;
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				byte[] data = new byte[1];
				data[0] = (byte)(modemStatusEvent.GetId() & 0xFF);
				return data;
			}
		}

		public override bool NeedsAPIFrameID
		{
			get
			{
				return false;
			}
		}

		/**
		 * Returns modem status event enum. entry.
		 * 
		 * @return Modem status event enum. entry.
		 */
		public ModemStatusEvent Status
		{
			get
			{
				return modemStatusEvent;
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
				parameters.Add("Status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(modemStatusEvent.GetId(), 1)) + " (" + modemStatusEvent.GetDescription() + ")");
				return parameters;
			}
		}
	}
}