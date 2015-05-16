using Common.Logging;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Kveer.XBeeApi.Packet
{
	/**
	 * This class represents a basic and unknown XBee packet where the payload is
	 * set as a byte array without a defined structure.
	 * 
	 * @see XBeeAPIPacket
	 */
	public class UnknownXBeePacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 1; // 1 (Frame type)

		/// <summary>
		/// Gets or sets the received RF data.
		/// </summary>
		public byte[] RFData { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code UnknownXBeePacket} from the given payload.
		 * 
		 * @param payload The API frame payload. The first byte will be the frame 
		 *                type.
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed Unknown packet.
		 * 
		 * @throws ArgumentException if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static UnknownXBeePacket CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "Unknown packet payload cannot be null.");
			// 1 (Frame type)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete Unknown packet.");

			// payload[0] is the frame type.
			byte apiID = payload[0];
			int index = 1;

			byte[] commandData = null;
			if (index < payload.Length)
			{
				commandData = new byte[payload.Length - index];
				Array.Copy(payload, index, commandData, 0, commandData.Length);
				//commandData = Arrays.copyOfRange(payload, index, payload.Length);
			}

			return new UnknownXBeePacket(apiID, commandData);
		}

		/**
		 * Class constructor. Instantiates an XBee packet with the given packet 
		 * data.
		 * 
		 * @param apiIDValue The XBee API integer value of the packet.
		 * @param rfData The XBee RF Data.
		 * 
		 * @throws ArgumentException if {@code apiIDValue < 0} or
		 *                                  if {@code apiIDValue > 255}.
		 */
		public UnknownXBeePacket(byte apiIDValue, byte[] rfData)
			: base(apiIDValue)
		{
			this.RFData = rfData;
			this.logger = LogManager.GetLogger<UnknownXBeePacket>();
		}

		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (var data = new MemoryStream())
				{
					try
					{
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
				return false;
			}
		}

		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				if (RFData != null)
					parameters.Add("RF Data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}
	}
}