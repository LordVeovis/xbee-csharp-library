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

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This enumeration lists all the possible options that can be set while transmitting an XBee data 
	/// packet.
	/// </summary>
	public enum XBeeTransmitOptions
	{
		// Enumeration entries.
		/// <summary>
		/// No special transmit options.
		/// </summary>
		NONE = 0x00,

		/// <summary>
		/// Disables acknowledgments on all unicasts.
		/// </summary>
		/// <remarks>Only valid for DigiMesh, 802.15.4 and Point-to-multipoint protocols.</remarks>
		DISABLE_ACK = 0x01,

		/// <summary>
		/// Disables the retries and router repair in the frame.
		/// </summary>
		/// <remarks>Only valid for ZigBee protocol.</remarks>
		DISABLE_RETRIES_AND_REPAIR = 0x01,

		/// <summary>
		/// Doesn't attempt Route Discovery.
		/// Disables Route Discovery on all DigiMesh unicasts.
		/// </summary>
		/// <remarks>Only valid for DigiMesh protocol.</remarks>
		DONT_ATTEMPT_RD = 0x02,

		/// <summary>
		/// Sends packet with broadcast <c>PAN ID</c>. Packet will be sent to all devices in the same 
		/// channel ignoring the <c>PAN ID</c>. It cannot be combined with other options.
		/// </summary>
		/// <remarks>Only valid for 802.15.4 XBee protocol.</remarks>
		USE_BROADCAST_PAN_ID = 0x04,

		/// <summary>
		/// Enables unicast NACK messages. NACK message is enabled on the packet.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 protocol.</remarks>
		ENABLE_UNICAST_NACK = 0x04,

		/// <summary>
		/// Enables unicast trace route messages. Trace route is enabled on the packets.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 protocol.</remarks>
		ENABLE_UNICAST_TRACE_ROUTE = 0x04,

		/// <summary>
		/// Enables APS encryption, only if <c>EE=1</c>. Enabling APS encryption decreases the maximum 
		/// number of RF payload bytes by 4 (below the value reported by <c>NP</c>).
		/// </summary>
		/// <remarks>Only valid for ZigBee XBee protocol.</remarks>
		ENABLE_APS_ENCRYPTION = 0x20,

		/// <summary>
		/// Uses the extended transmission timeout. Setting the extended timeout bit causes the stack to 
		/// set the extended transmission timeout for the destination address.
		/// </summary>
		/// <remarks>Only valid for ZigBee XBee protocol.</remarks>
		USE_EXTENDED_TIMEOUT = 0x40,

		/// <summary>
		/// Transmission is performed using point-to-Multipoint mode.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
		POINT_MULTIPOINT_MODE = 0x40,

		/// <summary>
		/// Transmission is performed using repeater mode.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
		REPEATER_MODE = 0x80,

		/// <summary>
		/// Transmission is performed using DigiMesh mode.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
		DIGIMESH_MODE = 0xC0
	}
}