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
	/// Provides the XBee message for the data received event.
	/// </summary>
	public class DataReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Instantiates a <see cref="DataReceivedEventArgs"/> object with the provided parameters.
		/// </summary>
		/// <param name="xbeeMessage">The XBee message.</param>
		public DataReceivedEventArgs(XBeeMessage xbeeMessage)
		{
			DataReceived = xbeeMessage;
		}

		// Properties.
		/// <summary>
		/// The <see cref="XBeeMessage"/> object containing the data, the <see cref="RemoteXBeeDevice"/> 
		/// that sent the data and a flag indicating whether the data was sent via broadcast or not.
		/// </summary>
		/// <seealso cref="XBeeMessage"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		public XBeeMessage DataReceived { get; private set; }
	}
}
