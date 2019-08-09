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
using XBeeLibrary.Core.Models;

namespace XBeeLibrary.Core.Events
{
	/// <summary>
	/// Provides the explicit XBee message for the explicit data received event.
	/// </summary>
	public class ExplicitDataReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Instantiates a <see cref="ExplicitDataReceivedEventArgs"/> object with the provided parameters.
		/// </summary>
		/// <param name="explicitXBeeMessage">The received Explicit XBee message.</param>
		public ExplicitDataReceivedEventArgs(ExplicitXBeeMessage explicitXBeeMessage)
		{
			ExplicitDataReceived = explicitXBeeMessage;
		}

		// Properties.
		/// <summary>
		/// The <see cref="ExplicitXBeeMessage"/> object containing the data, the 
		/// <see cref="RemoteXBeeDevice"/> that sent the data, a flag indicating whether the data was 
		/// sent via broadcast or not and the application layer fields (source endpoint, destination 
		/// endpoint, cluster ID and profile ID).
		/// </summary>
		/// <seealso cref="ExplicitXBeeMessage"/>
		public ExplicitXBeeMessage ExplicitDataReceived { get; private set; }
	}
}
