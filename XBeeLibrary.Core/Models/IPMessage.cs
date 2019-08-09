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
using System.Net;
using System.Text;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an IP message containing the IP address the message belongs to, the 
	/// source and destination ports, the IP protocol, and the content (data) of the message.
	/// </summary>
	/// <remarks>This class is used within the library to read data sent to IP devices.</remarks>
	public class IPMessage
	{
		/// <summary>
		/// Class constructor. Instantiates a new object of type <see cref="IPMessage"/> with the 
		/// given parameters.
		/// </summary>
		/// <param name="ipAddress">The IP address the message comes from.</param>
		/// <param name="sourcePort">TCP or UDP source port of the transmission.</param>
		/// <param name="destPort">TCP or UDP destination port of the transmission.</param>
		/// <param name="protocol">IP protocol used in the transmission.</param>
		/// <param name="data">Byte array containing the data of the message.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="ipAddress"/> is <c>null</c> 
		/// or if <paramref name="data"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="sourcePort"/> is <c>null</c> or invalid, 
		/// or if <paramref name="destPort"/> is <c>null</c> or invalid, 
		/// or if <paramref name="protocol"/> is <see cref="IPProtocol.UNKNOWN"/>.</exception>
		public IPMessage(IPAddress ipAddress, int sourcePort, int destPort, IPProtocol protocol, byte[] data)
		{
			if (protocol == IPProtocol.UNKNOWN)
				throw new ArgumentException("Protocol cannot be unknown.");
			if (sourcePort < 0 || sourcePort > 65535)
				throw new ArgumentException("Source port must be between 0 and 65535.");
			if (destPort < 0 || destPort > 65535)
				throw new ArgumentException("Destination port must be between 0 and 65535.");

			IPAddress = ipAddress ?? throw new ArgumentNullException("IP address cannot be null.");
			SourcePort = sourcePort;
			DestPort = destPort;
			Protocol = protocol;
			Data = data ?? throw new ArgumentNullException("Data cannot be null.");
		}

		// Properties.
		/// <summary>
		/// The IP address this message is associated to.
		/// </summary>
		/// <seealso cref="System.Net.IPAddress"/>
		public IPAddress IPAddress { get; private set; }

		/// <summary>
		/// The source port of the transmission.
		/// </summary>
		public int SourcePort { get; private set; }

		/// <summary>
		/// The destination port of the transmission.
		/// </summary>
		public int DestPort { get; private set; }

		/// <summary>
		/// The IP protocol used in the transmission.
		/// </summary>
		/// <seealso cref="IPProtocol"/>
		public IPProtocol Protocol { get; private set; }

		/// <summary>
		/// The byte array containing the data of the message.
		/// </summary>
		public byte[] Data { get; private set; }

		/// <summary>
		/// The data of the message in string format.
		/// </summary>
		public string DataString => Encoding.UTF8.GetString(Data);
	}
}