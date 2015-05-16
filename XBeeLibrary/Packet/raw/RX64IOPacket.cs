using Common.Logging;
using Kveer.XBeeApi.IO;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Kveer.XBeeApi.Packet.Raw
{
	/**
	 * This class represents an RX64 Address IO packet. Packet is built using the 
	 * parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>I/O data is sent out the UART using an API frame.</p>
	 * 
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class RX64IOPacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 11; // 1 (Frame type) + 8 (64-bit address) + 1 (RSSI) + 1 (receive options)

		/// <summary>
		/// Gets the 64-bit sender/source address.
		/// </summary>
		public XBee64BitAddress SourceAddress64 { get; private set; }

		private IOSample ioSample;

		/// <summary>
		/// Gets the Received Signal Strength Indicator (RSSI).
		/// </summary>
		public byte RSSI { get; private set; }

		/// <summary>
		/// Gets the receive options bitfield.
		/// </summary>
		public byte ReceiveOptions { get; private set; }

		/// <summary>
		/// Gets or sets the received RF data.
		/// </summary>
		public byte[] RFData { get; set; }

		private ILog logger;

		/**
		 * Creates an new {@code RX64IOPacket} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a RX64 Address IO packet ({@code 0x82}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed RX64 Address IO packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.RX_64.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code rssi < 0} or
		 *                                  if {@code rssi > 100} or
		 *                                  if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255} or 
		 *                                  if {@code rfData.Length < 5}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static RX64IOPacket CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "RX64 Address IO packet payload cannot be null.");
			// 1 (Frame type) + 8 (64-bit address) + 1 (RSSI) + 1 (receive options)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete RX64 Address IO packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) != APIFrameType.RX_IO_64.GetValue(), "Payload is not a RX64 Address IO packet.");

			// payload[0] is the frame type.
			int index = 1;

			// 8 bytes of 64-bit address.
			var address = new byte[8];
			Array.Copy(payload, index, address, 0, address.Length);
			//XBee64BitAddress sourceAddress64 = new XBee64BitAddress(Arrays.copyOfRange(payload, index, index + 8));
			XBee64BitAddress sourceAddress64 = new XBee64BitAddress(address);
			index = index + 8;

			// Received Signal Strength Indicator byte.
			byte rssi = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// Received Options byte.
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

			return new RX64IOPacket(sourceAddress64, rssi, receiveOptions, data);
		}

		/**
		 * Class constructor. Instantiates a new {@code RX64IOPacket} object with
		 * the given parameters.
		 * 
		 * @param sourceAddress64 64-bit address of the sender.
		 * @param rssi Received signal strength indicator.
		 * @param receiveOptions Bitfield indicating the receive options.
		 * @param rfData Received RF data.
		 * 
		 * @throws ArgumentException if {@code rssi < 0} or
		 *                                  if {@code rssi > 100} or
		 *                                  if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255} or 
		 *                                  if {@code rfData.Length < 5}.
		 * @throws ArgumentNullException if {@code sourceAddress64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeReceiveOptions
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RX64IOPacket(XBee64BitAddress sourceAddress64, byte rssi, byte receiveOptions, byte[] rfData)
			: base(APIFrameType.RX_IO_64)
		{
			Contract.Requires<ArgumentNullException>(sourceAddress64 != null, "64-bit source address cannot be null.");
			Contract.Requires<ArgumentOutOfRangeException>(rssi >= 0 && rssi <= 100);

			this.SourceAddress64 = sourceAddress64;
			this.RSSI = rssi;
			this.ReceiveOptions = receiveOptions;
			this.RFData = rfData;
			if (rfData != null)
				ioSample = new IOSample(rfData);
			else
				ioSample = null;
			this.logger = LogManager.GetLogger<RX64IOPacket>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (var os = new MemoryStream())
				{
					try
					{
						os.Write(SourceAddress64.Value, 0, SourceAddress64.Value.Length);
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

		/**
		 * Returns the IO sample corresponding to the data contained in the packet.
		 * 
		 * @return The IO sample of the packet, {@code null} if the packet has not 
		 *         any data or if the sample could not be generated correctly.
		 * 
		 * @see com.digi.xbee.api.io.IOSample
		 */
		public IOSample getIOSample()
		{
			return ioSample;
		}

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add(new KeyValuePair<string, string>("64-bit source address", HexUtils.PrettyHexString(SourceAddress64.ToString())));
				parameters.Add(new KeyValuePair<string, string>("RSSI", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(RSSI, 1))));
				parameters.Add(new KeyValuePair<string, string>("Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ReceiveOptions, 1))));
				if (ioSample != null)
				{
					parameters.Add(new KeyValuePair<string, string>("Number of samples", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(1, 1)))); // There is always 1 sample.
					parameters.Add(new KeyValuePair<string, string>("Digital channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ioSample.DigitalMask, 2))));
					parameters.Add(new KeyValuePair<string, string>("Analog channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ioSample.AnalogMask, 2))));
					for (int i = 0; i < 16; i++)
					{
						if (ioSample.HasDigitalValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(new KeyValuePair<string, string>(IOLine.UNKNOWN.GetDIO(i).GetName() + " digital value", ioSample.GetDigitalValue(IOLine.UNKNOWN.GetDIO(i)).GetName()));
					}
					for (int i = 0; i < 6; i++)
					{
						if (ioSample.HasAnalogValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(new KeyValuePair<string, string>(IOLine.UNKNOWN.GetDIO(i).GetName() + " analog value", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ioSample.GetAnalogValue(IOLine.UNKNOWN.GetDIO(i)), 2))));
					}
				}
				else if (RFData != null)
					parameters.Add(new KeyValuePair<string, string>("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData))));
				return parameters;
			}
		}
	}
}