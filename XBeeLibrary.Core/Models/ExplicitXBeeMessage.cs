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

using System;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an Explicit XBee message containing the remote XBee device the message 
	/// belongs to, the content (data) of the message, a flag indicating if the message is a broadcast 
	/// message (was received or is being sent via broadcast) and all the application layer fields: 
	/// source endpoint, destination endpoint, cluster ID and profile ID.
	/// </summary>
	/// <remarks>
	/// This class is used within the XBee C# Library to read explicit data sent by remote devices.
	/// </remarks>
	public class ExplicitXBeeMessage : XBeeMessage
	{
		/// <summary>
		/// Class constructor. Instantiates a new object of type <see cref="ExplicitXBeeMessage"/> with 
		/// the given parameters.
		/// </summary>
		/// <param name="remoteXBeeDevice">The remote XBee device the message belongs to (device that 
		/// sent the message).</param>
		/// <param name="sourceEndpoint">Endpoint of the source that initiated the transmission.</param>
		/// <param name="destEndpoint">Endpoint of the destination the message was addressed to.</param>
		/// <param name="clusterID">Cluster ID the packet was addressed to.</param>
		/// <param name="profileID">Profile ID the packet was addressed to.</param>
		/// <param name="data">Byte array containing the data of the message.</param>
		/// <exception cref="ArgumentException">If <c>sourceEndpoint.Length != 2</c> 
		/// or if <c>destEndpoint.Length != 2</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c>remoteXBeeDevice == null</c> 
		/// or if <c>data == null</c>.</exception>
		public ExplicitXBeeMessage(RemoteXBeeDevice remoteXBeeDevice, byte sourceEndpoint, byte destEndpoint,
			byte[] clusterID, byte[] profileID, byte[] data)
			: this(remoteXBeeDevice, sourceEndpoint, destEndpoint, clusterID, profileID, data, false) { }

		/// <summary>
		/// Class constructor. Instantiates a new object of type <c>ExplicitXBeeMessage</c>
		/// with the given parameters.
		/// </summary>
		/// <param name="remoteXBeeDevice">The remote XBee device the message belongs to
		/// (device that sent the message).</param>
		/// <param name="sourceEndpoint">Endpoint of the source that initiated the transmission.</param>
		/// <param name="destEndpoint">Endpoint of the destination the message was addressed to.</param>
		/// <param name="clusterID">Cluster ID the packet was addressed to.</param>
		/// <param name="profileID">Profile ID the packet was addressed to.</param>
		/// <param name="data">Byte array containing the data of the message.</param>
		/// <param name="isBroadcast">Indicates if the message was received via broadcast.</param>
		/// <exception cref="ArgumentException">If <c>sourceEndpoint.Length != 2</c> or
		/// if <c>destEndpoint.Length != 2</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c>remoteXBeeDevice == null</c> or
		/// if <c>data == null</c>.</exception>
		public ExplicitXBeeMessage(RemoteXBeeDevice remoteXBeeDevice, byte sourceEndpoint, byte destEndpoint,
			byte[] clusterID, byte[] profileID, byte[] data, bool isBroadcast)
			: base(remoteXBeeDevice, data, isBroadcast)
		{
			if (clusterID.Length != 2)
				throw new ArgumentException("Cluster ID must be 2 bytes.");
			if (profileID.Length != 2)
				throw new ArgumentException("Profile ID must be 2 bytes.");

			SourceEndpoint = sourceEndpoint;
			DestEndpoint = destEndpoint;
			ClusterID = clusterID;
			ProfileID = profileID;
		}

		// Properties.
		/// <summary>
		/// The endpoint of the source that initiated the transmission.
		/// </summary>
		public byte SourceEndpoint { get; private set; }

		/// <summary>
		/// The endpoint of the destination the message was addressed to.
		/// </summary>
		public byte DestEndpoint { get; private set; }

		/// <summary>
		/// The cluster ID the packet was addressed to.
		/// </summary>
		public byte[] ClusterID { get; private set; }

		/// <summary>
		/// The profile ID the packet was addressed to.
		/// </summary>
		public byte[] ProfileID { get; private set; }
	}
}