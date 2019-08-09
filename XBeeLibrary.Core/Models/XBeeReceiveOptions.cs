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

using System;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates all the possible options that have been set while receiving an XBee packet.
	/// </summary>
	[Flags]
	public enum XBeeReceiveOptions
	{
		// Enumeration entries.
		/// <summary>
		/// No special receive options.
		/// </summary>
		NONE = 0x00,

		/// <summary>
		/// Packet was acknowledged.
		/// </summary>
		/// <remarks>Not valid for Wi-Fi protocol</remarks>
		PACKET_ACKNOWLEDGED = 0x01,

		/// <summary>
		/// Packet was a broadcast packet.
		/// </summary>
		/// <remarks>Not valid for Wi-Fi protocol</remarks>
		BROADCAST_PACKET = 0x02,

		/// <summary>
		/// Packet encrypted with APS encryption.
		/// </summary>
		/// <remarks>Only valid for ZigBee XBee protocol.</remarks>
		APS_ENCRYPTED = 0x20,

		/// <summary>
		/// Packet was sent from an end device, if known.
		/// </summary>
		/// <remarks>Only valid for ZigBee XBee protocol.</remarks>
		SENT_FROM_END_DEVICE = 0x40,
	}
}