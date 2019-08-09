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
using XBeeLibrary.Core.Packet;

namespace XBeeLibrary.Core.Events
{
	/// <summary>
	/// Provides the XBee packet received event.
	/// </summary>
	public class PacketReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Instantiates a <see cref="PacketReceivedEventArgs"/> object with the provided parameters.
		/// </summary>
		/// <param name="packet">The received packet.</param>
		public PacketReceivedEventArgs(XBeePacket packet)
		{
			ReceivedPacket = packet;
		}

		// Properties.
		/// <summary>
		/// The received <see cref="XBeePacket"/> object.
		/// </summary>
		/// <seealso cref="XBeePacket"/>
		public XBeePacket ReceivedPacket { get; private set; }
	}
}
