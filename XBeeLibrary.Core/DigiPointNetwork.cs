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

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class represents a DigiPoint Network.
	/// </summary>
	/// <remarks>The network allows the discovery of remote devices in the same network as the local 
	/// one and stores them.</remarks>
	/// <seealso cref="DigiMeshNetwork"/>
	/// <seealso cref="Raw802Network"/>
	/// <seealso cref="XBeeDevice"/>
	/// <seealso cref="ZigBeeNetwork"/>
	public class DigiPointNetwork : XBeeNetwork
	{
		/// <summary>
		/// Initializes a new instance of <see cref="DigiPointNetwork"/>.
		/// </summary>
		/// <param name="device">A local DigiPoint device to get the network from.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="device"/> == null</c>.</exception>
		/// <seealso cref="DigiPointDevice"/>
		/// <seealso cref="XBeeNetwork(AbstractXBeeDevice)"/>
		internal DigiPointNetwork(DigiPointDevice device)
			: base(device) { }
	}
}