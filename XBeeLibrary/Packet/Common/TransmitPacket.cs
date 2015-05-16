using Common.Logging;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Kveer.XBeeApi.Packet.Common
{

	/**
	 * This class represents a Transmit Packet. Packet is built using the parameters 
	 * of the constructor or providing a valid API payload.
	 * 
	 * <p>A Transmit Request API frame causes the module to send data as an RF 
	 * packet to the specified destination.</p>
	 * 
	 * <p>The 64-bit destination address should be set to {@code 0x000000000000FFFF} 
	 * for a broadcast transmission (to all devices).</p>
	 * 
	 * <p>The coordinator can be addressed by either setting the 64-bit address to 
	 * all {@code 0x00} and the 16-bit address to {@code 0xFFFE}, OR by setting the 
	 * 64-bit address to the coordinator's 64-bit address and the 16-bit address to 
	 * {@code 0x0000}.</p>
	 * 
	 * <p>For all other transmissions, setting the 16-bit address to the correct 
	 * 16-bit address can help improve performance when transmitting to multiple 
	 * destinations.</p>
	 * 
	 * <p>If a 16-bit address is not known, this field should be set to 
	 * {@code 0xFFFE} (unknown).</p> 
	 * 
	 * <p>The Transmit Status frame 
	 * ({@link com.digi.xbee.api.packet.APIFrameType#TRANSMIT_REQUEST}) will 
	 * indicate the discovered 16-bit address, if successful (see 
	 * {@link com.digi.xbee.api.packet.common.TransmitStatusPacket}).</p>
	 * 
	 * <p>The broadcast radius can be set from {@code 0} up to {@code NH}. If set 
	 * to {@code 0}, the value of {@code NH} specifies the broadcast radius
	 * (recommended). This parameter is only used for broadcast transmissions.</p>
	 * 
	 * <p>The maximum number of payload bytes can be read with the {@code NP} 
	 * command.</p>
	 * 
	 * <p>Several transmit options can be set using the transmit options bitfield.
	 * </p>
	 * 
	 * @see com.digi.xbee.api.models.XBeeTransmitOptions
	 * @see com.digi.xbee.api.models.XBee16BitAddress#COORDINATOR_ADDRESS
	 * @see com.digi.xbee.api.models.XBee16BitAddress#UNKNOWN_ADDRESS
	 * @see com.digi.xbee.api.models.XBee64BitAddress#BROADCAST_ADDRESS
	 * @see com.digi.xbee.api.models.XBee64BitAddress#COORDINATOR_ADDRESS
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class TransmitPacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 14; // 1 (Frame type) + 1 (frame ID) + 8 (64-bit address) + 2 (16-bit address) + 1 (broadcast radious) + 1 (options)

		// Variables.
		private XBee64BitAddress destAddress64;

		private XBee16BitAddress destAddress16;

		private byte broadcastRadius;
		private byte transmitOptions;

		/// <summary>
		/// Gets or sets the received RF data.
		/// </summary>
		public byte[] RFData { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code TransmitPacket} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a Transmit packet ({@code 0x10}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed Transmit Request packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.TRANSMIT_REQUEST.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code broadcastRadius < 0} or
		 *                                  if {@code broadcastRadius > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static TransmitPacket createPacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "Transmit packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 8 (64-bit address) + 2 (16-bit address) + 1 (broadcast radious) + 1 (options)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete Transmit packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) != APIFrameType.TRANSMIT_REQUEST.GetValue(), "Payload is not a Transmit packet.");

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
			XBee16BitAddress destAddress16 = new XBee16BitAddress(payload[index] & 0xFF, payload[index + 1] & 0xFF);
			index = index + 2;

			// Broadcast radious byte.
			byte broadcastRadius = payload[index];
			index = index + 1;

			// Options byte.
			byte options = payload[index];
			index = index + 1;

			// Get RF data.
			byte[] rfData = null;
			if (index < payload.Length)
			{
				rfData = new byte[payload.Length - index];
				Array.Copy(payload, index, rfData, 0, rfData.Length);
				//rfData = Arrays.copyOfRange(payload, index, payload.Length);
			}

			return new TransmitPacket(frameID, destAddress64, destAddress16, broadcastRadius, options, rfData);
		}

		/**
		 * Class constructor. Instantiates a new {@code TransmitPacket} object
		 * with the given parameters.
		 * 
		 * @param frameID Frame ID.
		 * @param destAddress64 64-bit address of the destination device.
		 * @param destAddress16 16-bit address of the destination device.
		 * @param broadcastRadius maximum number of hops a broadcast transmission 
		 *                        can occur.
		 * @param transmitOptions Bitfield of supported transmission options.
		 * @param rfData RF Data that is sent to the destination device.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code broadcastRadius < 0} or
		 *                                  if {@code broadcastRadius > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code destAddress64 == null} or
		 *                              if {@code destAddress16 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeTransmitOptions
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public TransmitPacket(byte frameID, XBee64BitAddress destAddress64, XBee16BitAddress destAddress16,
				byte broadcastRadius, byte transmitOptions, byte[] rfData)
			: base(APIFrameType.TRANSMIT_REQUEST)
		{
			if (destAddress64 == null)
				throw new ArgumentNullException("64-bit destination address cannot be null.");
			if (destAddress16 == null)
				throw new ArgumentNullException("16-bit destination address cannot be null.");

			this.frameID = frameID;
			this.destAddress64 = destAddress64;
			this.destAddress16 = destAddress16;
			this.broadcastRadius = broadcastRadius;
			this.transmitOptions = transmitOptions;
			this.RFData = rfData;
			this.logger = LogManager.GetLogger<TransmitPacket>();
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
						data.WriteByte(broadcastRadius);
						data.WriteByte(transmitOptions);
						if (RFData != null)
							data.Write(RFData, 0, RFData.Length);
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
		 * Returns the 64-bit destination address.
		 * 
		 * @return The 64-bit destination address.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public XBee64BitAddress get64bitDestinationAddress()
		{
			return destAddress64;
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
		 * Returns the broadcast radius.
		 * 
		 * @return The broadcast radius.
		 */
		public int getBroadcastRadius()
		{
			return broadcastRadius;
		}

		/**
		 * Returns the transmit options bitfield.
		 * 
		 * @return The transmit options bitfield.
		 * 
		 * @see com.digi.xbee.api.models.XBeeTransmitOptions
		 */
		public int getTransmitOptions()
		{
			return transmitOptions;
		}

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add("64-bit dest. address", HexUtils.PrettyHexString(destAddress64.ToString()));
				parameters.Add("16-bit dest. address", HexUtils.PrettyHexString(destAddress16.ToString()));
				parameters.Add("Broadcast radius", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(broadcastRadius, 1)) + " (" + broadcastRadius + ")");
				parameters.Add("Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(transmitOptions, 1)));
				if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}
	}
}