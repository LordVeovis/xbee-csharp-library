/*
 * Copyright 2019, Digi International Inc.
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
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet.Common
{
	/// <summary>
	/// This class represents an Explicit Addressing Command packet. Packet is built using the 
	/// parameters of the constructor or providing a valid API payload.
	/// </summary>
	/// <remarks> Allows application layer fields (endpoint and cluster ID) to be specified for a data 
	/// transmission. Similar to the Transmit Request, but also requires application layer addressing 
	/// fields to be specified (endpoints, cluster ID, profile ID). An Explicit Addressing Request API
	/// frame causes the module to send data as an RF packet to the specified destination, using the 
	/// specified source and destination endpoints, cluster ID, and profile ID.
	/// 
	/// The 64-bit destination address should be set to <c>0x000000000000FFFF</c> for a broadcast 
	/// transmission (to all devices).
	/// 
	/// The coordinator can be addressed by either setting the 64-bit address to all <c>0x00</c> and 
	/// the 16-bit address to <c>0xFFFE</c>, or by setting the 64-bit address to the coordinator's 
	/// 64-bit address and the 16-bit address to <c>0x0000</c>.
	/// 
	/// For all other transmissions, setting the 16-bit address to the correct 16-bit address can help 
	/// improve performance when transmitting to multiple destinations.
	/// 
	/// If a 16-bit address is not known, this field should be set to <c>0xFFFE</c> (unknown).
	/// 
	/// The Transmit Status frame (<see cref="APIFrameType.TRANSMIT_STATUS"/>) will indicate the 
	/// discovered 16-bit address, if successful (<see cref="TransmitStatusPacket"/>).
	/// 
	/// The broadcast radius can be set from <c>0</c> up to <c>NH</c>. If set to <c>0</c>, the value 
	/// of <c>NH</c> specifies the broadcast radius (recommended). This parameter is only used for 
	/// broadcast transmissions.
	/// 
	/// The maximum number of payload bytes can be read with the <c>NP</c> command. Note: if source 
	/// routing is used, the RF payload will be reduced by two bytes per intermediate hop in the 
	/// source route.
	/// 
	/// Several transmit options can be set using the transmit options bitfield.</remarks>
	/// <seealso cref="XBeeTransmitOptions"/>
	/// <seealso cref="XBee16BitAddress.COORDINATOR_ADDRESS"/>
	/// <seealso cref="XBee16BitAddress.UNKNOWN_ADDRESS"/>
	/// <seealso cref="XBee64BitAddress.BROADCAST_ADDRESS"/>
	/// <seealso cref="XBee64BitAddress.COORDINATOR_ADDRESS"/>
	/// <seealso cref="ExplicitRxIndicatorPacket"/>
	/// <seealso cref="XBeeAPIPacket"/>
	public class ExplicitAddressingPacket : XBeeAPIPacket
	{
		// Constants.
		private const int MIN_API_PAYLOAD_LENGTH = 20; // 1 (Frame type) + 1 (frame ID) + 8 (64-bit address) + 2 (16-bit address) + 1 (source endpoint) + 1 (destination endpoint) + 2 (cluster ID) + 2 (profile ID) + 1 (broadcast radius) + 1 (options)

		// Variables.
		private ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ExplicitAddressingPacket"/> object with the 
		/// given parameters.
		/// </summary>
		/// <param name="frameID">The frame ID.</param>
		/// <param name="destAddress64">The 64-bit address of the destination device.</param>
		/// <param name="destAddress16">The 16-bit address of the destination device.</param>
		/// <param name="sourceEndpoint">The source endpoint for the transaction.</param>
		/// <param name="destEndpoint">The destination endpoint for the transaction.</param>
		/// <param name="clusterID">The cluster ID used in the transaction.</param>
		/// <param name="profileID">The profile ID used in the transaction.</param>
		/// <param name="broadcastRadius">The maximum number of hops a broadcast transmission can traverse.
		/// Set to 0 to use the network maximum hops value.</param>
		/// <param name="transmitOptions">The bitfield of supported transmission options.</param>
		/// <param name="rfData">The RF Data that is sent to the destination device.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="frameID"/> <![CDATA[<]]> 0</c>
		/// or if <c><paramref name="frameID"/> <![CDATA[>]]> 255</c>
		/// or if <c>profileID.Length != 2</c> or if <c>clusterID.Length != 2</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="destAddress64"/> == null</c> 
		/// or if <c><paramref name="destAddress16"/> == null</c>.</exception>
		/// <seealso cref="XBeeTransmitOptions"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public ExplicitAddressingPacket(byte frameID, XBee64BitAddress destAddress64, XBee16BitAddress destAddress16,
			byte sourceEndpoint, byte destEndpoint, byte[] clusterID, byte[] profileID, byte broadcastRadius,
			byte transmitOptions, byte[] rfData)
			: base(APIFrameType.EXPLICIT_ADDRESSING_COMMAND_FRAME)
		{
			if (profileID.Length != 2)
				throw new ArgumentException("Profile ID length must be 2.");
			if (clusterID.Length != 2)
				throw new ArgumentException("Cluster ID length must be 2.");

			FrameID = frameID;
			DestAddress64 = destAddress64 ?? throw new ArgumentNullException("64-bit destination address cannot be null.");
			DestAddress16 = destAddress16 ?? throw new ArgumentNullException("16-bit destination address cannot be null.");
			SourceEndpoint = sourceEndpoint;
			DestEndpoint = destEndpoint;
			ClusterID = clusterID;
			ProfileID = profileID;
			BroadcastRadius = broadcastRadius;
			TransmitOptions = transmitOptions;
			RFData = rfData;
			logger = LogManager.GetLogger<TransmitPacket>();
		}

		// Properties.
		/// <summary>
		/// The 64-bit destination address.
		/// </summary>
		/// <seealso cref="XBee64BitAddress"/>
		public XBee64BitAddress DestAddress64 { get; private set; }

		/// <summary>
		/// The 16-bit destination address.
		/// </summary>
		/// <seealso cref="XBee16BitAddress"/>
		public XBee16BitAddress DestAddress16 { get; private set; }

		/// <summary>
		/// The source endpoint of the transmission.
		/// </summary>
		public byte SourceEndpoint { get; private set; }

		/// <summary>
		/// The destination endpoint of the transmission.
		/// </summary>
		public byte DestEndpoint { get; private set; }

		/// <summary>
		/// The cluster ID used in the transmission.
		/// </summary>
		public byte[] ClusterID { get; private set; }

		/// <summary>
		/// The profile ID used in the transmission.
		/// </summary>
		public byte[] ProfileID { get; private set; }

		/// <summary>
		/// The broadcast radius.
		/// </summary>
		public byte BroadcastRadius { get; private set; }

		/// <summary>
		/// The transmit options bitfield.
		/// </summary>
		public byte TransmitOptions { get; private set; }

		/// <summary>
		/// The received RF data.
		/// </summary>
		public byte[] RFData { get; set; }

		/// <summary>
		/// Indicates whether the API packet needs API Frame ID or not.
		/// </summary>
		public override bool NeedsAPIFrameID => true;

		/// <summary>
		/// Indicates whether the packet is a broadcast packet.
		/// </summary>
		public override bool IsBroadcast
		{
			get
			{
				return DestAddress64.Equals(XBee64BitAddress.BROADCAST_ADDRESS)
						|| DestAddress16.Equals(XBee16BitAddress.BROADCAST_ADDRESS);
			}
		}

		/// <summary>
		/// Gets the XBee API packet specific data.
		/// </summary>
		/// <remarks>This does not include the frame ID if it is needed.</remarks>
		protected override byte[] APIPacketSpecificData
		{
			get
			{
				using (MemoryStream data = new MemoryStream())
				{
					try
					{
						data.Write(DestAddress64.Value, 0, DestAddress64.Value.Length);
						data.Write(DestAddress16.Value, 0, DestAddress16.Value.Length);
						data.WriteByte(SourceEndpoint);
						data.WriteByte(DestEndpoint);
						data.Write(ClusterID, 0, ClusterID.Length);
						data.Write(ProfileID, 0, ProfileID.Length);
						data.WriteByte(BroadcastRadius);
						data.WriteByte(TransmitOptions);
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

		/// <summary>
		/// Gets a map with the XBee packet parameters and their values.
		/// </summary>
		/// <returns>A sorted map containing the XBee packet parameters with their values.</returns>
		protected override LinkedDictionary<string, string> APIPacketParameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>
				{
					{ "Frame ID", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(FrameID, 1)) + " (" + FrameID + ")" },
					{ "64-bit dest. address", HexUtils.PrettyHexString(DestAddress64.ToString()) },
					{ "16-bit dest. address", HexUtils.PrettyHexString(DestAddress16.ToString()) },
					{ "Source endpoint", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(SourceEndpoint, 1)) },
					{ "Dest. endpoint", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(DestEndpoint, 1)) },
					{ "Cluster ID", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(ClusterID)) },
					{ "Profile ID", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(ProfileID)) },
					{ "Broadcast radius", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(BroadcastRadius, 1)) + " (" + BroadcastRadius + ")" },
					{ "Options", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(TransmitOptions, 1)) }
				};
				if (RFData != null)
					parameters.Add("RF data", HexUtils.PrettyHexString(HexUtils.ByteArrayToHexString(RFData)));
				return parameters;
			}
		}

		/// <summary>
		///  Creates a new <see cref="ExplicitAddressingPacket"/> object from the given payload.
		/// </summary>
		/// <param name="payload">The API frame payload. It must start with the frame type corresponding to an 
		/// Explicit Addressing packet (<c>0x11</c>). The byte array must be in <see cref="OperatingMode.API"/> mode.</param>
		/// <returns>Parsed Explicit Addressing packet.</returns>
		/// <exception cref="ArgumentException">If <c>payload[0] != APIFrameType.EXPLICIT_ADDRESSING_COMMAND_FRAME.GetValue()</c> 
		/// or if <c>payload.length <![CDATA[<]]> <see cref="MIN_API_PAYLOAD_LENGTH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="payload"/> == null</c>.</exception>
		public static ExplicitAddressingPacket CreatePacket(byte[] payload)
		{
			if (payload == null)
				throw new ArgumentNullException("Explicit Addressing packet payload cannot be null.");
			// 1 (Frame type) + 1 (frame ID) + 8 (64-bit address) + 2 (16-bit address) + 1 (source endpoint) + 1 (destination endpoint) + 2 (cluster ID) + 2 (profile ID) + 1 (broadcast radius) + 1 (options)
			if (payload.Length < MIN_API_PAYLOAD_LENGTH)
				throw new ArgumentException("Incomplete Explicit Addressing packet.");
			if ((payload[0] & 0xFF) != APIFrameType.EXPLICIT_ADDRESSING_COMMAND_FRAME.GetValue())
				throw new ArgumentException("Payload is not a Explicit Addressing packet.");

			// payload[0] is the frame type.
			int index = 1;

			// Frame ID byte.
			byte frameID = payload[index];
			index = index + 1;

			var array = new byte[8];
			Array.Copy(payload, index, array, 0, array.Length);
			// 8 bytes of 64-bit address.
			XBee64BitAddress destAddress64 = new XBee64BitAddress(array);
			index = index + 8;

			// 2 bytes of 16-bit address.
			XBee16BitAddress destAddress16 = new XBee16BitAddress(payload[index], payload[index + 1]);
			index = index + 2;

			// Source endpoint byte.
			byte sourceEndpoint = payload[index];
			index = index + 1;

			// Destination endpoint byte.
			byte destEndpoint = payload[index];
			index = index + 1;

			// 2 bytes of cluster ID.
			byte[] clusterID = new byte[2];
			Array.Copy(payload, index, clusterID, 0, 2);
			index = index + 2;

			// 2 bytes of profile ID.
			byte[] profileID = new byte[2];
			Array.Copy(payload, index, profileID, 0, 2);
			index = index + 2;

			// Broadcast radious byte.
			byte broadcastRadius = payload[index];
			index = index + 1;

			// Options byte.
			byte options = payload[index];
			index = index + 1;

			// Get RF data.
			byte[] rfData = null;
			if (index < payload.Length)
			{
				rfData = new byte[payload.Length - index];
				Array.Copy(payload, index, rfData, 0, rfData.Length);
			}

			return new ExplicitAddressingPacket(frameID, destAddress64, destAddress16, sourceEndpoint,
				destEndpoint, clusterID, profileID, broadcastRadius, options, rfData);
		}
	}
}