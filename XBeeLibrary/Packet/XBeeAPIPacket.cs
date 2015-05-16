using Common.Logging;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Kveer.XBeeApi.Packet
{
	/// <summary>
	/// This abstract class provides the basic structure of a ZigBee API frame.
	/// 
	/// Derived classes should implement their own methods to generate the API data and frame ID in case they support it.
	/// 
	/// Basic operations such as frame type retrieval are performed in this class.
	/// </summary>
	public abstract class XBeeAPIPacket : XBeePacket
	{

		// Constants.
		public const int NO_FRAME_ID = 9999;

		// Variables.
		protected ushort frameID = NO_FRAME_ID;

		/// <summary>
		/// Gets the XBee packet frame type.
		/// </summary>
		public APIFrameType FrameType { get; private set; }

		/// <summary>
		/// Gets the XBee packet frame type integer value.
		/// </summary>
		public byte FrameTypeValue { get; private set; }

		private ILog logger;

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeeAPIPacket"/> with the specified <paramref name="frameType"/>.
		/// </summary>
		/// <param name="frameType">XBee packet frame type.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="frameType"/> is null.</exception>
		protected XBeeAPIPacket(APIFrameType frameType)
			: base()
		{

			Contract.Requires<ArgumentNullException>(frameType != null, "Frame type cannot be null.");

			this.FrameType = frameType;
			FrameTypeValue = frameType.GetValue();

			this.logger = LogManager.GetLogger<XBeeAPIPacket>();
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeeAPIPacket"/> with the specified <paramref name="frameTypeValue"/>.
		/// </summary>
		/// <param name="frameTypeValue">XBee packet frame type integer value.</param>
		/// <exception cref="ArgumentException">if <paramref name="frameTypeValue"/> is not in a range of a byte value.</exception>
		protected XBeeAPIPacket(byte frameTypeValue)
			: base()
		{
			this.FrameTypeValue = frameTypeValue;
			this.FrameType = APIFrameType.GENERIC.Get(frameTypeValue);

			this.logger = LogManager.GetLogger<XBeeAPIPacket>();
		}

		public override byte[] GetPacketData()
		{
			using (var data = new MemoryStream())
			{
				data.WriteByte(FrameTypeValue);

				byte[] apiData = APIData;
				if (apiData == null)
					apiData = new byte[0];
				if (apiData != null && apiData.Length > 0)
				{
					try
					{
						data.Write(apiData, 0, APIData.Length);
					}
					catch (IOException e)
					{
						logger.Error(e.Message, e);
					}
				}

				return data.ToArray();
			}
		}

		/// <summary>
		/// Gets the XBee API packet data.
		/// </summary>
		/// <remarks>This does not include the frame ID if it is needed.</remarks>
		public byte[] APIData
		{
			get
			{
				using (var data = new MemoryStream())
				{
					byte[] apiData = APIPacketSpecificData;
					if (apiData == null)
						apiData = new byte[0];

					if (NeedsAPIFrameID)
						data.WriteByte((byte)FrameID);

					if (apiData != null && apiData.Length > 0)
					{
						try
						{
							data.Write(apiData, 0, apiData.Length);
						}
						catch (IOException e)
						{
							logger.Error(e.Message, e);
						}
					}

					return data.ToArray();
				}
			}
		}

		/// <summary>
		/// Gets the XBee API packet specific data.
		/// </summary>
		/// <remarks>This does not include the frame ID if it is needed.</remarks>
		protected abstract byte[] APIPacketSpecificData { get; }

		/// <summary>
		/// Indicates whether the API packet needs API Frame ID or not.
		/// </summary>
		public abstract bool NeedsAPIFrameID { get; }

		/// <summary>
		/// Gets or sets the Frame ID of the API packet.
		/// </summary>
		/// <remarks>If the frame ID is not configured or if the API packet does not need a Frame ID (<code>if (!NeedsAPIFrameID())</code>), this property returns <code>NO_FRAME_ID</code>.</remarks>
		public ushort FrameID
		{
			get
			{
				if (NeedsAPIFrameID)
					return frameID;
				return NO_FRAME_ID;
			}
			set
			{
				Contract.Requires<ArgumentOutOfRangeException>(value >= 0 && value <= 255);

				if (NeedsAPIFrameID)
					this.frameID = value;
			}
		}

		/// <summary>
		/// Indicates whether the packet is a broadcast packet.
		/// </summary>
		public abstract bool IsBroadcast { get; }

		/// <summary>
		/// Indicates whether the given ID is the current frame ID.
		/// </summary>
		/// <param name="id">The frame id to check.</param>
		/// <returns>true if frame ID is equal to the <paramref name="id"/> provided, false otherwise or if the frame does not need an ID.</returns>
		/// <seealso cref="NeedsAPIFrameID"/>
		public bool CheckFrameID(int id)
		{
			return NeedsAPIFrameID && FrameID == id;
		}

		protected override LinkedDictionary<string, string> PacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				if (FrameType != null)
					parameters.Add(new KeyValuePair<string, string>("Frame type", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(FrameTypeValue, 1)) + " (" + FrameType.GetName() + ")"));
				else
					parameters.Add(new KeyValuePair<string, string>("Frame type", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(FrameTypeValue, 1))));

				if (NeedsAPIFrameID)
				{
					if (frameID == NO_FRAME_ID)
						parameters.Add(new KeyValuePair<string, string>("Frame ID", "(NO FRAME ID)"));
					else
						parameters.Add(new KeyValuePair<string, string>("Frame ID", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(frameID, 1)) + " (" + frameID + ")"));
				}

				if (APIPacketParameters != null)
					foreach (var kvp in APIPacketParameters)
						parameters.Add(kvp);

				return parameters;
			}
		}

		/// <summary>
		/// Gets a map with the XBee packet parameters and their values.
		/// </summary>
		/// <returns>A sorted map containing the XBee packet parameters with their values.</returns>
		protected abstract LinkedDictionary<string, string> APIPacketParameters { get; }
	}
}