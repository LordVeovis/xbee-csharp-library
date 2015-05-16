using Common.Logging;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.IO;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
namespace Kveer.XBeeApi.Packet.Common
{

	/**
	 * This class represents an IO Data Sample RX Indicator packet. Packet is built 
	 * using the parameters of the constructor or providing a valid API payload.
	 * 
	 * <p>When the module receives an IO sample frame from a remote device, it 
	 * sends the sample out the UART using this frame type (when AO=0). Only modules
	 * running API firmware will send IO samples out the UART.</p>
	 * 
	 * <p>Among received data, some options can also be received indicating 
	 * transmission parameters.</p>
	 * 
	 * @see com.digi.xbee.api.models.XBeeReceiveOptions
	 * @see com.digi.xbee.api.packet.XBeeAPIPacket
	 */
	public class IODataSampleRxIndicatorPacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 12; // 1 (Frame type) + 8 (32-bit address) + 2 (16-bit address) + 1 (receive options)

		// Variables.
		private XBee64BitAddress sourceAddress64;
		private XBee16BitAddress sourceAddress16;

		/// <summary>
		/// Gets the IO sample corresponding to the data contained in the packet.
		/// </summary>
		public IOSample IOSample { get; private set; }

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
		 * Creates a new {@code IODataSampleRxIndicatorPacket} object from the 
		 * given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a IO Data Sample RX Indicator packet ({@code 0x92}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed ZigBee Receive packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH} or
		 *                                  if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255} or 
		 *                                  if {@code rfData.Length < 5}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static IODataSampleRxIndicatorPacket CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "IO Data Sample RX Indicator packet payload cannot be null.");
			// 1 (Frame type) + 8 (32-bit address) + 2 (16-bit address) + 1 (receive options)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete IO Data Sample RX Indicator packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR.GetValue(), "Payload is not a IO Data Sample RX Indicator packet.");

			// payload[0] is the frame type.
			int index = 1;

			var addr = new byte[8];
			Array.Copy(payload, index, addr, 0, addr.Length);
			// 2 bytes of 16-bit address.
			XBee64BitAddress sourceAddress64 = new XBee64BitAddress(addr);
			index = index + 8;

			// 2 bytes of 16-bit address.
			XBee16BitAddress sourceAddress16 = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// Receive options
			byte receiveOptions = payload[index];
			index = index + 1;

			// Get data.
			byte[] data = null;
			if (index < payload.Length)
			{
				data = new byte[payload.Length - index];
				Array.Copy(payload, index, data, 0, data.Length);
				//data = Arrays.copyOfRange(payload, index, payload.Length);
			}
			return new IODataSampleRxIndicatorPacket(sourceAddress64, sourceAddress16, receiveOptions, data);
		}

		/**
		 * Class constructor. Instantiates a new 
		 * {@code IODataSampleRxIndicatorPacket} object with the given parameters.
		 * 
		 * @param sourceAddress64 64-bit address of the sender.
		 * @param sourceAddress16 16-bit address of the sender.
		 * @param receiveOptions Receive options.
		 * @param rfData Received RF data.
		 * 
		 * @throws ArgumentException if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255} or
		 *                                  if {@code rfData.Length < 5}.
		 * @throws ArgumentNullException if {@code sourceAddress64 == null} or 
		 *                              if {@code sourceAddress16 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeReceiveOptions
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress 
		 */
		public IODataSampleRxIndicatorPacket(XBee64BitAddress sourceAddress64, XBee16BitAddress sourceAddress16, byte receiveOptions, byte[] rfData)
			: base(APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR)
		{

			if (sourceAddress64 == null)
				throw new ArgumentNullException("64-bit source address cannot be null.");
			if (sourceAddress16 == null)
				throw new ArgumentNullException("16-bit source address cannot be null.");

			this.sourceAddress64 = sourceAddress64;
			this.sourceAddress16 = sourceAddress16;
			this.ReceiveOptions = receiveOptions;
			this.RFData = rfData;
			if (rfData != null)
				IOSample = new IOSample(rfData);
			else
				IOSample = null;
			this.logger = LogManager.GetLogger<IODataSampleRxIndicatorPacket>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (MemoryStream os = new MemoryStream())
				{
					try
					{
						os.Write(sourceAddress64.Value, 0, sourceAddress64.Value.Length);
						os.Write(sourceAddress16.Value, 0, sourceAddress16.Value.Length);
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
				return ByteUtils.IsBitEnabled(ReceiveOptions, 1);
			}
		}

		/**
		 * Returns the 64-bit sender/source address. 
		 * 
		 * @return The 64-bit sender/source address.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public XBee64BitAddress get64bitSourceAddress()
		{
			return sourceAddress64;
		}

		/**
		 * Returns the 16-bit sender/source address. 
		 * 
		 * @return 16-bit sender/source address.
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
				parameters.Add("64-bit source address", HexUtils.PrettyHexString(sourceAddress64.ToString()));
				parameters.Add("16-bit source address", HexUtils.PrettyHexString(sourceAddress16.ToString()));
				parameters.Add("Receive options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ReceiveOptions, 1)));
				if (IOSample != null)
				{
					parameters.Add("Number of samples", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(1, 1))); // There is always 1 sample.
					parameters.Add("Digital channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.DigitalMask, 2)));
					parameters.Add("Analog channel mask", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.AnalogMask, 1)));
					for (int i = 0; i < 16; i++)
					{
						if (IOSample.HasDigitalValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(IOLine.UNKNOWN.GetDIO(i).GetName() + " digital value", IOSample.GetDigitalValue(IOLine.UNKNOWN.GetDIO(i)).GetName());
					}
					for (int i = 0; i < 6; i++)
					{
						if (IOSample.HasAnalogValue(IOLine.UNKNOWN.GetDIO(i)))
							parameters.Add(IOLine.UNKNOWN.GetDIO(i).GetName() + " analog value", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.GetAnalogValue(IOLine.UNKNOWN.GetDIO(i)), 2)));
					}
					if (IOSample.HasPowerSupplyValue)
						try
						{
							parameters.Add("Power supply value", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(IOSample.PowerSupplyValue, 2)));
						}
						catch (OperationNotSupportedException) { }
				}
				else if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}
	}
}