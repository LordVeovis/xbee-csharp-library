using Common.Logging;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Kveer.XBeeApi.Packet.Raw
{
	/// <summary>
	/// This class represents an RX (Receive) 64 Request packet. Packet is built using the parameters of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks>When the module receives an RF packet, it is sent out the UART using this message type.
	/// This packet is the response to TX (transmit) 64 Request packets.</remarks>
	/// <seealso cref="TX64Packet"/>
	public class RX64Packet : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 11; // 1 (Frame type) + 8 (64-bit address) + 1 (signal strength) + 1 (receive options)

		/// <summary>
		/// Gets the 64-bit sender/source address. 
		/// </summary>
		public XBee64BitAddress SourceAddress64 { get; private set; }

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

		/// <summary>
		/// Initializes a new instance of class <see cref="RX64Packet"/> from the specified <paramref name="payload"/>.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding to a RX64 packet (0x80). The byte array must be in <code>OperatingMode.API</code> mode.</param>
		/// <returns>Parsed RX 64 packet.</returns>
		/// <exception cref="ArgumentNullException">if <paramref name="payload"/> is null.</exception>
		/// <exception cref="ArgumentException">if first byte of <paramref name="payload"/> is not <code>APIFrameType.RX_64</code> or
		/// if the Length of <paramref name="payload"/> is lower than the <code>MIN_API_PAYLOAD_LENGTH</code> or
		/// if <paramref name="rssi"/> is negative or
		/// if <paramref name="rssi"/> is greater than 100.</exception>
		public static RX64Packet CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "RX64 packet payload cannot be null.");
			// 1 (Frame type) + 8 (64-bit address) + 1 (signal strength) + 1 (receive options)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete RX64 packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.RX_64.GetValue(), "Payload is not a RX64 packet.");

			// payload[0] is the frame type.
			int index = 1;

			var array = new byte[8];
			Array.Copy(payload, index, array, 0, array.Length);
			// 8 bytes of 64-bit address.
			XBee64BitAddress sourceAddress64 = new XBee64BitAddress(array);
			index = index + 8;

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

			return new RX64Packet(sourceAddress64, signalStrength, receiveOptions, data);
		}

		/// <summary>
		/// Initializes a new instance of <see cref="RX64Packet"/>.
		/// </summary>
		/// <param name="sourceAddress64">64-bit address of the sender.</param>
		/// <param name="rssi">Received signal strength indicator.</param>
		/// <param name="receiveOptions">Bitfield indicating the receive options.</param>
		/// <param name="rfData">Received RF data.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="sourceAddress64"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">if <paramref name="rssi"/> is greater than 100.</exception>
		/// <seealso cref="XBeeReceiveOptions"/>
		public RX64Packet(XBee64BitAddress sourceAddress64, byte rssi, byte receiveOptions, byte[] rfData)
			: base(APIFrameType.RX_64)
		{
			Contract.Requires<ArgumentNullException>(sourceAddress64 != null, "64-bit source address cannot be null.");
			Contract.Requires<ArgumentOutOfRangeException>(rssi <= 100, "RSSI value must be between 0 and 100.");

			this.SourceAddress64 = sourceAddress64;
			this.RSSI = rssi;
			this.ReceiveOptions = receiveOptions;
			this.RFData = rfData;
			this.logger = LogManager.GetLogger<RX64Packet>();
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

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add(new KeyValuePair<string, string>("64-bit source address", HexUtils.PrettyHexString(SourceAddress64.ToString())));
				parameters.Add(new KeyValuePair<string, string>("RSSI", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(RSSI, 1))));
				parameters.Add(new KeyValuePair<string, string>("Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(ReceiveOptions, 1))));
				if (RFData != null)
					parameters.Add(new KeyValuePair<string, string>("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData))));
				return parameters;
			}
		}
	}
}