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
	 * This class represents an RX (Receive) 16 Request packet. Packet is built 
	 * using the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>When the module receives an RF packet, it is sent out the UART using this 
	 * message type</p>
	 * 
	 * <p>This packet is the response to TX (transmit) 16 Request packets.</p>
	 * 
	 * @see TX16Packet
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 *
	 */
	public class RX16Packet : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 5; // 1 (Frame type) + 2 (16-bit address) + 1 (signal strength) + 1 (receive options)

		// Variables.
		private XBee16BitAddress sourceAddress16;

		/// <summary>
		/// Gets the Received Signal Strength Indicator (RSSI).
		/// </summary>
		public byte RSSI { get; private set; }
		/// <summary>
		/// Gets the receive options bitfield.
		/// </summary>
		public byte ReceiveOptions { get; private set; }

		/// <summary>
		/// Gets or sets the received RF data
		/// </summary>
		public byte[] RFData { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code RX16Packet} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a RX16 packet ({@code 0x81}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed RX 16 packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.RX_16.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code rssi < 0} or
		 *                                  if {@code rssi > 100} or
		 *                                  if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static RX16Packet CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "RX16 packet payload cannot be null.");
			// 1 (Frame type) + 2 (16-bit address) + 1 (signal strength) + 1 (receive options)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete RX16 packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.RX_16.GetValue(), "Payload is not a RX16 packet.");

			// payload[0] is the frame type.
			int index = 1;

			// 2 bytes of 16-bit address.
			XBee16BitAddress sourceAddress16 = new XBee16BitAddress(payload[index] & 0xFF, payload[index + 1] & 0xFF);
			index = index + 2;

			// Signal strength byte.
			byte signalStrength = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// Receive options byte.
			byte receiveOptions = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
				//data = Arrays.copyOfRange(payload, index, payload.Length);
			}

			return new RX16Packet(sourceAddress16, signalStrength, receiveOptions, data);
		}

		/**
		 * Class constructor. Instantiates a new {@code RX16Packet} object with
		 * the given parameters.
		 * 
		 * @param sourceAddress16 16-bit address of the sender.
		 * @param rssi Received signal strength indicator.
		 * @param receiveOptions Bitfield indicating the receive options.
		 * @param rfData Received RF data.
		 * 
		 * @throws ArgumentException if {@code rssi < 0} or
		 *                                  if {@code rssi > 100} or
		 *                                  if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255}.
		 * @throws ArgumentNullException if {@code sourceAddress16 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeReceiveOptions
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public RX16Packet(XBee16BitAddress sourceAddress16, byte rssi, byte receiveOptions, byte[] rfData)
			: base(APIFrameType.RX_16)
		{
			Contract.Requires<ArgumentNullException>(sourceAddress16 != null, "16-bit source address cannot be null.");
			Contract.Requires<ArgumentException>(rssi >= 0 && rssi <= 100, "RSSI value must be between 0 and 100.");

			this.sourceAddress16 = sourceAddress16;
			this.RSSI = rssi;
			this.ReceiveOptions = receiveOptions;
			this.RFData = rfData;
			this.logger = LogManager.GetLogger<RX16Packet>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (var os = new MemoryStream())
				{
					try
					{
						os.Write(sourceAddress16.Value, 0, sourceAddress16.Value.Length);
						os.WriteByte(RSSI);
						os.WriteByte(ReceiveOptions);
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
				return false;
			}
		}

		public override bool IsBroadcast
		{
			get
			{
				return ByteUtils.IsBitEnabled(ReceiveOptions, 1)
						|| ByteUtils.IsBitEnabled(ReceiveOptions, 2);
			}
		}

		/// <summary>
		/// Gets the 16-bit sender/source address. 
		/// </summary>
		/// <returns></returns>
		public XBee16BitAddress Get16bitSourceAddress()
		{
			return sourceAddress16;
		}

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add(new KeyValuePair<string, string>("16-bit source address", HexUtils.PrettyHexString(sourceAddress16.ToString())));
				parameters.Add(new KeyValuePair<string, string>("RSSI", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(RSSI, 1))));
				parameters.Add(new KeyValuePair<string, string>("Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ReceiveOptions, 1))));
				if (RFData != null)
					parameters.Add(new KeyValuePair<string, string>("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData))));
				return parameters;
			}
		}
	}
}