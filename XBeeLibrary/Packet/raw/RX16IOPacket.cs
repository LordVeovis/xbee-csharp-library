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
	/// <summary>
	/// This class represents an RX16 Address IO packet. Packet is built using the parameters of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>I/O data is sent out the UART using an API frame.</remarks>
	public class RX16IOPacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 5; // 1 (Frame type) + 2 (16-bit address) + 1 (RSSI) + 1 (receive options)

		// Variables.
		private XBee16BitAddress sourceAddress16;

		/// <summary>
		/// Gets the IO sample corresponding to the data contained in the packet.
		/// </summary>
		public IOSample IOSample { get; private set; }

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
		 * Creates a new {@code RX16IOPacket} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a RX16 Address IO packet ({@code 0x83}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed RX16 Address IO packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.RX_16.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code rssi < 0} or
		 *                                  if {@code rssi > 100} or
		 *                                  if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255} or
		 *                                  if {@code rfData.Length < 5}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static RX16IOPacket CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "RX16 Address IO packet payload cannot be null.");
			// 1 (Frame type) + 2 (16-bit address) + 1 (RSSI) + 1 (receive options)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete RX16 Address IO packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.RX_IO_16.GetValue(), "Payload is not a RX16 Address IO packet.");

			// payload[0] is the frame type.
			int index = 1;

			// 2 bytes of 16-bit address.
			XBee16BitAddress sourceAddress16 = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// Received Signal Strength Indicator byte.
			byte rssi = (byte)(payload[index] & 0xFF);
			index = index + 1;

			// Received Signal Strength Indicator byte.
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

			return new RX16IOPacket(sourceAddress16, rssi, receiveOptions, data);
		}

		/**
		 * Class constructor. Instantiates a new {@code RX16IOPacket} object with 
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
		 *                                  if {@code receiveOptions > 255} or
		 *                                  if {@code rfData.Length < 5}.
		 * @throws ArgumentNullException if {@code sourceAddress16 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeReceiveOptions
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public RX16IOPacket(XBee16BitAddress sourceAddress16, byte rssi, byte receiveOptions, byte[] rfData)
			: base(APIFrameType.RX_IO_16)
		{
			Contract.Requires<ArgumentNullException>(sourceAddress16 != null, "16-bit source address cannot be null.");
			Contract.Requires<ArgumentOutOfRangeException>(rssi >= 0 && rssi <= 100, "RSSI value must be between 0 and 100.");

			this.sourceAddress16 = sourceAddress16;
			this.RSSI = rssi;
			this.ReceiveOptions = receiveOptions;
			this.RFData = rfData;
			if (rfData != null)
				IOSample = new IOSample(rfData);
			else
				IOSample = null;
			this.logger = LogManager.GetLogger<RX16IOPacket>();
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

		/**
		 * Returns the 16-bit sender/source address. 
		 * 
		 * @return The 16-bit sender/source address.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public XBee16BitAddress get16bitSourceAddress()
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
				if (IOSample != null)
				{
					parameters.Add(new KeyValuePair<string, string>("Number of samples", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(1, 1)))); // There is always 1 sample.
					parameters.Add(new KeyValuePair<string, string>("Digital channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.DigitalMask, 2))));
					parameters.Add(new KeyValuePair<string, string>("Analog channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.AnalogMask, 2))));
					for (int i = 0; i < 16; i++)
					{
						if (IOSample.HasDigitalValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(new KeyValuePair<string, string>(IOLine.UNKNOWN.GetDIO(i).GetName() + " digital value", IOSample.GetDigitalValue(IOLine.UNKNOWN.GetDIO(i)).GetName()));
					}
					for (int i = 0; i < 6; i++)
					{
						if (IOSample.HasAnalogValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(new KeyValuePair<string, string>(IOLine.UNKNOWN.GetDIO(i).GetName() + " analog value", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.GetAnalogValue(IOLine.UNKNOWN.GetDIO(i)), 2))));
					}
				}
				else if (RFData != null)
					parameters.Add(new KeyValuePair<string, string>("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData))));
				return parameters;
			}
		}
	}
}