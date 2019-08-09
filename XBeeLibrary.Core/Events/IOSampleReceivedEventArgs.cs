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
using XBeeLibrary.Core.IO;

namespace XBeeLibrary.Core.Events
{
	/// <summary>
	/// Provides data for IO sample received event.
	/// </summary>
	public class IOSampleReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Instantiates a <see cref="IOSampleReceivedEventArgs"/> object with the provided parameters.
		/// </summary>
		/// <param name="remoteDevice">The remote XBee device.</param>
		/// <param name="ioSample">The IO sample.</param>
		public IOSampleReceivedEventArgs(RemoteXBeeDevice remoteDevice, IOSample ioSample)
		{
			RemoteDevice = remoteDevice;
			IOSample = ioSample;
		}

		// Properties.
		/// <summary>
		/// The remote XBee device that sent the sample.
		/// </summary>
		/// <seealso cref="RemoteXBeeDevice"/>
		public RemoteXBeeDevice RemoteDevice { get; private set; }

		/// <summary>
		/// The received IO sample.
		/// </summary>
		/// <seealso cref="IOSample"/>
		public IOSample IOSample { get; private set; }
	}
}
