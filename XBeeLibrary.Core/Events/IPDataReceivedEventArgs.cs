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
	/// Provides the IP Data received event.
	/// </summary>
	public class IPDataReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Instantiates a <see cref="IPDataReceivedEventArgs"/> object with the provided parameters.
		/// </summary>
		/// <param name="ipMessage">The received IP Data message.</param>
		public IPDataReceivedEventArgs(IPMessage ipMessage)
		{
			IPDataReceived = ipMessage;
		}

		// Properties.
		/// <summary>
		/// The received IP Data message that contains the data, the IP address that sent the data, 
		/// the source and destination ports and the <see cref="IPProtocol"/> of the transmission.
		/// </summary>
		/// <seealso cref="IPMessage"/>
		public IPMessage IPDataReceived { get; private set; }
	}
}
