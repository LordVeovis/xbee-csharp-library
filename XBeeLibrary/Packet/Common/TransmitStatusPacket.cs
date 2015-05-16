using Common.Logging;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Packet;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kveer.XBeeApi.Packet.Common
{

	/**
	 * This class represents a Transmit Status Packet. Packet is built using the 
	 * parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>When a Transmit Request is completed, the module sends a Transmit Status 
	 * message. This message will indicate if the packet was transmitted 
	 * successfully or if there was a failure.</p>
	 * 
	 * <p>This packet is the response to standard and explicit transmit requests.
	 * </p>
	 * 
	 * @see TransmitPacket
	 */
	public class TransmitStatusPacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 7; // 1 (Frame type) + 1 (frame ID) + 2 (16-bit address) + 1 (retry count) + 1 (delivery status) + 1 (discovery status)

		// Variables.
		private XBee16BitAddress destAddress16;

		private byte tranmistRetryCount;
		private XBeeTransmitStatus transmitStatus;
		private XBeeDiscoveryStatus discoveryStatus;

		private ILog logger;

		/**
		 * Creates a new {@code TransmitStatusPacket} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a Transmit Status packet ({@code 0x8B}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed Transmit Status packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.TRANSMIT_STATUS.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code tranmistRetryCount < 0} or
		 *                                  if {@code tranmistRetryCount > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static TransmitStatusPacket createPacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Transmit Status packet payload cannot be null.");

			// 1 (Frame type) + 1 (frame ID) + 2 (16-bit address) + 1 (retry count) + 1 (delivery status) + 1 (discovery status)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Transmit Status packet.");

			if ((payload[0] & 0xFF) != APIFrameType.TRANSMIT_STATUS.GetValue())
				throw new ArgumentException("Payload is not a Transmit Status packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			// 2 bytes of 16-bit address.
			XBee16BitAddress address = new XBee16BitAddress(payload[index] & 0xFF, payload[index + 1] & 0xFF);
			index = index + 2;

			// Retry count byte.
			byte retryCount = payload[index];
			index = index + 1;

			// Delivery status byte.
			byte deliveryStatus = payload[index];
			index = index + 1;

			// Discovery status byte.
			byte discoveryStatus = payload[index];

			// TODO if XBeeTransmitStatus is unknown????
			return new TransmitStatusPacket(frameID, address, retryCount,
					XBeeTransmitStatus.SUCCESS.Get(deliveryStatus), XBeeDiscoveryStatus.DISCOVERY_STATUS_ADDRESS_AND_ROUTE.Get(discoveryStatus));
		}

		/**
		 * Class constructor. Instantiates a new {@code TransmitStatusPacket} 
		 * object with the given parameters.
		 * 
		 * @param frameID Frame ID.
		 * @param destAddress16 16-bit Network address the packet was delivered to.
		 * @param tranmistRetryCount The number of application transmission retries 
		 *                           that took place.
		 * @param transmitStatus Transmit status.
		 * @param discoveryStatus Discovery status.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code tranmistRetryCount < 0} or
		 *                                  if {@code tranmistRetryCount > 255}.
		 * @throws ArgumentNullException if {@code destAddress16 == null} or
		 *                              if {@code transmitStatus == null} or
		 *                              if {@code discoveryStatus == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeDiscoveryStatus
		 * @see com.digi.xbee.api.models.XBeeTransmitStatus
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public TransmitStatusPacket(byte frameID, XBee16BitAddress destAddress16, byte tranmistRetryCount,
				XBeeTransmitStatus transmitStatus, XBeeDiscoveryStatus discoveryStatus)
			: base(APIFrameType.TRANSMIT_STATUS)
		{
			if (destAddress16 == null)
				throw new ArgumentNullException("16-bit destination address cannot be null.");
			if (transmitStatus == null)
				throw new ArgumentNullException("Delivery status cannot be null.");
			if (discoveryStatus == null)
				throw new ArgumentNullException("Discovery status cannot be null.");

			this.frameID = frameID;
			this.destAddress16 = destAddress16;
			this.tranmistRetryCount = tranmistRetryCount;
			this.transmitStatus = transmitStatus;
			this.discoveryStatus = discoveryStatus;
			this.logger = LogManager.GetLogger<TransmitStatusPacket>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				MemoryStream data = new MemoryStream();
				try
				{
					data.Write(destAddress16.Value, 0, destAddress16.Value.Length);
					data.WriteByte(tranmistRetryCount);
					data.WriteByte(transmitStatus.GetId());
					data.WriteByte(discoveryStatus.GetId());
				}
				catch (IOException e)
				{
					logger.Error(e.Message, e);
				}
				return data.ToArray();
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.packet.XBeeAPIPacket#needsAPIFrameID()
		 */
		//@Override
		public override bool NeedsAPIFrameID
		{
			get
			{
				return true;
			}
		}

		/**
		 * Returns the 16-bit destination address. 
		 * 
		 * @return The 16-bit destination address.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public XBee16BitAddress get16bitDestinationAddress()
		{
			return destAddress16;
		}

		/**
		 * Returns the transmit retry count.
		 * 
		 * @return Transmit retry count.
		 */
		public int getTransmitRetryCount()
		{
			return tranmistRetryCount;
		}

		/**
		 * Returns the transmit status.
		 * 
		 * @return Transmit status.
		 * 
		 * @see com.digi.xbee.api.models.XBeeTransmitStatus
		 */
		public XBeeTransmitStatus getTransmitStatus()
		{
			return transmitStatus;
		}

		/**
		 * Returns the discovery status.
		 * 
		 * @return Discovery status.
		 * 
		 * @see XBeeDiscoveryStatus
		 */
		public XBeeDiscoveryStatus getDiscoveryStatus()
		{
			return discoveryStatus;
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
				parameters.Add("16-bit dest. address", HexUtils.PrettyHexString(destAddress16.ToString()));
				parameters.Add("Tx. retry count", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(tranmistRetryCount, 1)) + " (" + tranmistRetryCount + ")");
				parameters.Add("Delivery status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(transmitStatus.GetId(), 1)) + " (" + transmitStatus.GetDescription() + ")");
				parameters.Add("Discovery status", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(discoveryStatus.GetId(), 1)) + " (" + discoveryStatus.GetDescription() + ")");
				return parameters;
			}
		}
	}
}