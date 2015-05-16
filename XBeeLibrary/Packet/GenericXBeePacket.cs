using Common.Logging;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
namespace Kveer.XBeeApi.Packet
{

	/**
	 * This class represents a basic and Generic XBee packet where the payload is
	 * set as a byte array without a defined structure.
	 * 
	 * @see XBeeAPIPacket
	 */
	public class GenericXBeePacket : XBeeAPIPacket
	{

		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 1; // 1 (Frame type)

		/// <summary>
		/// Gets or sets the received RF data.
		/// </summary>
		public byte[] RFData { get; set; }

		private ILog logger;

		/**
		 * Creates a new {@code GenericXBeePacket} from the given payload.
		 * 
		 * @param payload The API frame payload. It must start with the frame type 
		 *                corresponding to a Generic packet ({@code 0xFF}).
		 *                The byte array must be in {@code OperatingMode.API} mode.
		 * 
		 * @return Parsed Generic packet.
		 * 
		 * @throws ArgumentException if {@code payload[0] != APIFrameType.GENERIC.getValue()} or
		 *                                  if {@code payload.Length < }{@value #MIN_API_PAYLOAD_LENGTH}.
		 * @throws ArgumentNullException if {@code payload == null}.
		 */
		public static GenericXBeePacket CreatePacket(byte[] payload)
		{
			Contract.Requires<ArgumentNullException>(payload != null, "Generic packet payload cannot be null.");
			// 1 (Frame type)
			Contract.Requires<ArgumentException>(payload.Length >= MIN_API_PAYLOAD_LENGTH, "Incomplete Generic packet.");
			Contract.Requires<ArgumentException>((payload[0] & 0xFF) == APIFrameType.GENERIC.GetValue(), "Payload is not a Generic packet.");

			// payload[0] is the frame type.
			int index = 1;

			byte[] commandData = null;
			if (index < payload.Length)
			{
				commandData = new byte[payload.Length - index];
				Array.Copy(payload, index, commandData, 0, commandData.Length);
				//commandData = Arrays.copyOfRange(payload, index, payload.Length);
			}

			return new GenericXBeePacket(commandData);
		}

		/**
		 * Class constructor. Instantiates an XBee packet with the given packet 
		 * data.
		 * 
		 * @param rfData The XBee RF Data.
		 */
		public GenericXBeePacket(byte[] rfData)
			: base(APIFrameType.GENERIC)
		{
			this.RFData = rfData;
			this.logger = LogManager.GetLogger<GenericXBeePacket>();
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