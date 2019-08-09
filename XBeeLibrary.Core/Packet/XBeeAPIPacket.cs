/*
 * Copyright 2019, Digi International Inc.
 * Copyright 2014, 2015, Sébastien Rault.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet
{
	/// <summary>
	/// This abstract class provides the basic structure of a ZigBee API frame.
	/// </summary>
	/// <remarks>Derived classes should implement their own methods to generate the API data and 
	/// frame ID in case they support it.
	/// 
	/// Basic operations such as frame type retrieval are performed in this class.</remarks>
	public abstract class XBeeAPIPacket : XBeePacket
	{
		// Constants.
		public const int NO_FRAME_ID = 9999;

		// Variables.
		private ushort frameID;
		private ILog logger;

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeeAPIPacket"/> with the 
		/// specified <paramref name="frameType"/>.
		/// </summary>
		/// <param name="frameType">XBee packet frame type.</param>
		protected XBeeAPIPacket(APIFrameType frameType)
			: base()
		{
			FrameType = frameType;
			FrameTypeValue = frameType.GetValue();

			logger = LogManager.GetLogger<XBeeAPIPacket>();
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeeAPIPacket"/> with the 
		/// specified <paramref name="frameTypeValue"/>.
		/// </summary>
		/// <param name="frameTypeValue">XBee packet frame type integer value.</param>
		protected XBeeAPIPacket(byte frameTypeValue)
			: base()
		{
			FrameTypeValue = frameTypeValue;
			FrameType = APIFrameType.UNKNOWN.Get(frameTypeValue);

			logger = LogManager.GetLogger<XBeeAPIPacket>();
		}

		// Properties.
		/// <summary>
		/// The Frame ID of the API packet.
		/// </summary>
		/// <remarks>If the frame ID is not configured or if the API packet does not need a Frame ID 
		/// (<c>if (!NeedsAPIFrameID)</c>), this property returns <see cref="NO_FRAME_ID"/>.</remarks>
		/// <exception cref="ArgumentException">If <c><paramref name="value"/> <![CDATA[<]]> 0 </c> 
		/// or if <c><paramref name="value"/> <![CDATA[>]]> 255</c>.</exception>
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
				if (value < 0 || value > 255)
					throw new ArgumentException("Frame ID must be between 0 and 255.");

				if (NeedsAPIFrameID)
					frameID = value;
			}
		}

		/// <summary>
		/// The XBee packet frame type.
		/// </summary>
		/// <seealso cref="APIFrameType"/>
		public APIFrameType FrameType { get; private set; }

		/// <summary>
		/// The XBee packet frame type byte value.
		/// </summary>
		public byte FrameTypeValue { get; private set; }

		/// <summary>
		/// The XBee API packet data.
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
		/// Indicates whether the API packet needs API Frame ID or not.
		/// </summary>
		public abstract bool NeedsAPIFrameID { get; }

		/// <summary>
		/// Indicates whether the packet is a broadcast packet.
		/// </summary>
		public abstract bool IsBroadcast { get; }

		/// <summary>
		/// Gets the XBee API packet specific data.
		/// </summary>
		/// <remarks>This does not include the frame ID if it is needed.</remarks>
		protected abstract byte[] APIPacketSpecificData { get; }

		/// <summary>
		/// A sorted dictionary with the packet parameters and their values.
		/// </summary>
		protected override LinkedDictionary<string, string> PacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>
				{
					new KeyValuePair<string, string>("Frame type", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(FrameTypeValue, 1)) + " (" + FrameType.GetName() + ")")
				};

				if (NeedsAPIFrameID)
				{
					if (FrameID == NO_FRAME_ID)
						parameters.Add(new KeyValuePair<string, string>("Frame ID", "(NO FRAME ID)"));
					else
						parameters.Add(new KeyValuePair<string, string>("Frame ID", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(FrameID, 1)) + " (" + FrameID + ")"));
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

		/// <summary>
		/// Indicates whether the given ID is the current frame ID.
		/// </summary>
		/// <param name="id">The frame id to check.</param>
		/// <returns><c>true</c> if frame ID is equal to the <paramref name="id"/> provided, <c>false</c> 
		/// otherwise or if the frame does not need an ID.</returns>
		/// <seealso cref="NeedsAPIFrameID"/>
		public bool CheckFrameID(int id)
		{
			return NeedsAPIFrameID && FrameID == id;
		}

		/// <summary>
		/// Returns the packet data.
		/// </summary>
		/// <returns>The packet data.</returns>
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
	}
}