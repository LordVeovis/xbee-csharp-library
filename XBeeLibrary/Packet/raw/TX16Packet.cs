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
	 * This class represents a TX (Transmit) 16 Request packet. Packet is built 
	 * using the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>A TX Request message will cause the module to transmit data as an RF 
	 * Packet.</p>
	 * 
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class TX16Packet : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 5; // 1 (Frame type) + 1 (frame ID) + 2 (address) + 1 (transmit options)

		/// <summary>
		/// Gets the transmit options bitfield.
		/// </summary>
		public byte TransmitOptions { get; private set; }

		private XBee16BitAddress destAddress16;

		/// <summary>
		/// Gets or sets the received RF data.
		/// </summary>
		public byte[] RFData { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code TX16Packet} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a TX16 Request packet ({@code 0x01}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed TX (transmit) 16 Request packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.TX_16.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static TX16Packet CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "TX16 Request packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 2 (address) + 1 (transmit options)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete TX16 Request packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.TX_16.GetValue(), "Payload is not a TX16 Request packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// 2 bytes of address, starting at 2nd byte.
			XBee16BitAddress destAddress16 = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// Transmit options byte.
			byte transmitOptions = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
				//data = Arrays.copyOfRange(payload, index, payload.Length);
			}

			return new TX16Packet(frameID, destAddress16, transmitOptions, data);
		}

		/**
		 * Class constructor. Instantiates a new {@code TX16Packet} object with
		 * the given parameters.
		 * 
		 * @param frameID Frame ID.
		 * @param destAddress16 16-bit address of the destination device.
		 * @param transmitOptions Bitfield of supported transmission options.
		 * @param rfData RF Data that is sent to the destination device.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code destAddress == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeTransmitOptions
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public TX16Packet(byte frameID, XBee16BitAddress destAddress16, byte transmitOptions, byte[] rfData)
			: base(APIFrameType.TX_16)
		{
			Contract.Requires<ArgumentNullException>(destAddress16 != null, "16-bit destination address cannot be null.");

			this.frameID = frameID;
			this.destAddress16 = destAddress16;
			this.TransmitOptions = transmitOptions;
			this.RFData = rfData;
			this.logger = LogManager.GetLogger<TX16Packet>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (var os = new MemoryStream())
				{
					try
					{
						os.Write(destAddress16.Value, 0, destAddress16.Value.Length);
						os.WriteByte(TransmitOptions);
						if (RFData != null)
							os.Write(RFData, 0, RFData.Length);
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

		public override bool IsBroadcast
		{
			get
			{
				return get16bitDestinationAddress().Equals(XBee16BitAddress.BROADCAST_ADDRESS);
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

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add("16-bit dest. address", HexUtils.PrettyHexString(destAddress16.ToString()));
				parameters.Add("Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitOptions, 1)));
				if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}
	}
}