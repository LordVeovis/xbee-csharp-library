using Common.Logging;
using Kveer.XBeeApi.IO;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Kveer.XBeeApi.Packet.Raw
{


	/**
	 * This class represents a TX (Transmit) Status packet. Packet is built using 
	 * the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>When a TX Request is completed, the module sends a TX Status message. 
	 * This message will indicate if the packet was transmitted successfully or if 
	 * there was a failure.</p>
	 * 
	 * @see TX16Packet
	 * @see TX64Packet
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class TXStatusPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 3; // 1 (Frame type) + 1 (frame ID) + 1 (status)

		/// <summary>
		/// Gets the transmit status.
		/// </summary>
		public XBeeTransmitStatus TransmitStatus { get; private set; }

		/**
		 * Creates a new {@code TXStatusPacket} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a TX Status packet ({@code 0x89}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed TX status packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.TX_STATUS.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static TXStatusPacket createPacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "TX Status packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 1 (status)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete TX Status packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) != APIFrameType.TX_STATUS.GetValue(), "Payload is not a TX Status packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index] ;
			index = index + 1;

			// Status byte.
			byte status = payload[index] ;

			// TODO if status is unknown????
			return new TXStatusPacket(frameID, XBeeTransmitStatus.UNKNOWN.Get(status));
		}

		/**
		 * Class constructor. Instantiates a new {@code TXStatusPacket} object
		 * with the given parameters.
		 * 
		 * @param frameID Packet frame ID.
		 * @param transmitStatus Transmit status.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255}.
		 * @throws ArgumentNullException if {@code transmitStatus == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeTransmitStatus
		 */
		public TXStatusPacket(byte frameID, XBeeTransmitStatus transmitStatus)
			: base(APIFrameType.TX_STATUS)
		{

			Contract.Requires<ArgumentNullException>(transmitStatus != null, "Transmit status cannot be null.");

			this.frameID = frameID;
			this.TransmitStatus = transmitStatus;
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				return new byte[] { (byte)TransmitStatus.GetId() };
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
				return false;
			}
		}

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add("Status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitStatus.GetId(), 1)) + " (" + TransmitStatus.GetDescription() + ")");
				return parameters;
			}
		}
	}
}