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
 * This class represents a Receive Packet. Packet is built using the parameters
 * of the constructor or providing a valid API payload.
 * 
 * <p>When the module receives an RF packet, it is sent out the UART using this 
 * message type.</p>
 * 
 * <p>This packet is received when external devices send transmit request 
 * packets to this module.</p>
 * 
 * <p>Among received data, some options can also be received indicating 
 * transmission parameters.</p>
 * 
 * @see TransmitPacket
 * @see com.digi.xbee.api.models.XBeeReceiveOptions
 * @see com.digi.xbee.api.packet.XBeeAPIPacket
 */
	public class ReceivePacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 12; // 1 (Frame type) + 8 (32-bit address) + 2 (16-bit address) + 1 (receive options)

		// Variables.
		private XBee64BitAddress sourceAddress64;

		private XBee16BitAddress sourceAddress16;

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
		 * Creates a new {@code ReceivePacket} object from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a Receive packet ({@code 0x90}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed ZigBee Receive packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.RECEIVE_PACKET.getValue()} or
		 *                                  if {@code payload.Length < {@value #MIN_API_PAYLOAD_LENGTH}} or
		 *                                  if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static ReceivePacket createPacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Receive packet payload cannot be null.");

			// 1 (Frame type) + 8 (32-bit address) + 2 (16-bit address) + 1 (receive options)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Receive packet.");

			if ((payload[0] & 0xFF) != APIFrameType.RECEIVE_PACKET.GetValue())
				throw new ArgumentException("Payload is not a Receive packet.");

			// payload[0] is the frame type.
			int index = 1;

			var array = new byte[8];
			Array.Copy(payload, index, array, 0, array.Length);
			// 2 bytes of 16-bit address.
			XBee64BitAddress sourceAddress64 = new XBee64BitAddress(array);
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

			return new ReceivePacket(sourceAddress64, sourceAddress16, receiveOptions, data);
		}

		/**
		 * Class constructor. Instantiates a new {@code ReceivePacket} object
		 * with the given parameters.
		 * 
		 * @param sourceAddress64 64-bit address of the sender.
		 * @param sourceAddress16 16-bit address of the sender.
		 * @param receiveOptions Bitfield indicating the receive options.
		 * @param rfData Received RF data.
		 * 
		 * @throws ArgumentException if {@code receiveOptions < 0} or
		 *                                  if {@code receiveOptions > 255}.
		 * @throws ArgumentNullException if {@code sourceAddress64 == null} or 
		 *                              if {@code sourceAddress16 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBeeReceiveOptions
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public ReceivePacket(XBee64BitAddress sourceAddress64, XBee16BitAddress sourceAddress16, byte receiveOptions, byte[] rfData)
			: base(APIFrameType.RECEIVE_PACKET)
		{

			if (sourceAddress64 == null)
				throw new ArgumentNullException("64-bit source address cannot be null.");
			if (sourceAddress16 == null)
				throw new ArgumentNullException("16-bit source address cannot be null.");

			this.sourceAddress64 = sourceAddress64;
			this.sourceAddress16 = sourceAddress16;
			this.ReceiveOptions = receiveOptions;
			this.RFData = rfData;
			this.logger = LogManager.GetLogger<ReceivePacket>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (MemoryStream data = new MemoryStream())
				{
					try
					{
						data.Write(sourceAddress64.Value, 0, sourceAddress64.Value.Length);
						data.Write(sourceAddress16.Value, 0, sourceAddress16.Value.Length);
						data.WriteByte(ReceiveOptions);
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
				parameters.Add("64-bit source address", HexUtils.PrettyHexString(sourceAddress64.ToString()));
				parameters.Add("16-bit source address", HexUtils.PrettyHexString(sourceAddress16.ToString()));
				parameters.Add("Receive options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ReceiveOptions, 1)));
				if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}
	}
}