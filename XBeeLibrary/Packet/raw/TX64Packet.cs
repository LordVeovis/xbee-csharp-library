using Common.Logging;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Kveer.XBeeApi.Packet.Raw
{
	/**
	 * This class represents a TX (Transmit) 64 Request packet. Packet is built 
	 * using the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>A TX Request message will cause the module to transmit data as an RF 
	 * Packet.</p>
	 * 
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class TX64Packet : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 11; // 1 (Frame type) + 1 (frame ID) + 8 (address) + 1 (transmit options)

		/// <summary>
		/// Gets the transmit options bitfield.
		/// </summary>
		public byte TransmitOptions { get; private set; }

		private XBee64BitAddress destAddress64;

		/// <summary>
		/// Gets or sets the received RF data.
		/// </summary>
		public byte[] RFData { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code TX64Packet} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a TX64 Request packet ({@code 0x00}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed TX (transmit) 64 Request packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.TX_64.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static TX64Packet CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "TX64 Request packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 8 (address) + 1 (transmit options)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete TX64 Request packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.TX_64.GetValue(), "Payload is not a TX64 Request packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			var array = new byte[8];
			Array.Copy(payload, index, array, 0, array.Length);
			// 8 bytes of address, starting at 2nd byte.
			XBee64BitAddress destAddress64 = new XBee64BitAddress(array);
			index = index + 8;

			// Transmit options byte.
			byte transmitOptions = payload[index];
			index = index + 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
				//data = Arrays.copyOfRange(payload, index, payload.Length);
			}

			return new TX64Packet(frameID, destAddress64, transmitOptions, data);
		}

		/**
		 * Class constructor. Instantiates a new {@code TX64Packet} object with
		 * the given parameters.
		 * 
		 * @param frameID Frame ID.
		 * @param destAddress64 64-bit address of the destination device.
		 * @param transmitOptions Bitfield of supported transmission options.
		 * @param rfData RF Data that is sent to the destination device.
		 * 
		 * @throws ArgumentException if {@code frameID < 0} or
		 *                                  if {@code frameID > 255} or
		 *                                  if {@code transmitOptions < 0} or
		 *                                  if {@code transmitOptions > 255}.
		 * @throws ArgumentNullException if {@code destAddress64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeTransmitOptions
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public TX64Packet(byte frameID, XBee64BitAddress destAddress64, byte transmitOptions, byte[] rfData)
			: base(APIFrameType.TX_64)
		{
			Contract.Requires<ArgumentNullException>(destAddress64 != null, "64-bit destination address cannot be null.");

			this.frameID = frameID;
			this.destAddress64 = destAddress64;
			this.TransmitOptions = transmitOptions;
			this.RFData = rfData;
			this.logger = LogManager.GetLogger<TX64Packet>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (var os = new MemoryStream())
				{
					try
					{
						os.Write(destAddress64.Value, 0, destAddress64.Value.Length);
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
				return get64bitDestinationAddress().Equals(XBee64BitAddress.BROADCAST_ADDRESS);
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


		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add("64-bit dest. address", HexUtils.PrettyHexString(destAddress64.ToString()));
				parameters.Add("Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitOptions, 1)));
				if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}
	}
}