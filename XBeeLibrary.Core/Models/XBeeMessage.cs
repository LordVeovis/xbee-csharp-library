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
using System.Text;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an XBee message containing the remote XBee device the message belongs 
	/// to, the content (data) of the message and a flag indicating if the message is a broadcast 
	/// message (was received or is being sent via broadcast). 
	/// </summary>
	/// <remarks>This class is used within the XBee C# Library to read data sent by remote devices.</remarks>
	public class XBeeMessage
	{
		/// <summary>
		/// Initializes new instance of class <see cref="XBeeMessage"/>.
		/// </summary>
		/// <param name="remoteXBeeDevice">The remote XBee device the message belongs to.</param>
		/// <param name="data">Byte array containing the data of the message.</param>
		/// <seealso cref="RemoteXBeeDevice"/>
		public XBeeMessage(RemoteXBeeDevice remoteXBeeDevice, byte[] data)
			: this(remoteXBeeDevice, data, false) { }

		/// <summary>
		/// Initializes new instance of class <see cref="XBeeMessage"/>.
		/// </summary>
		/// <param name="remoteXBeeDevice">The remote XBee device the message belongs to.</param>
		/// <param name="data">Byte array containing the data of the message.</param>
		/// <param name="isBroadcast">Indicates if the message was received via broadcast.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="remoteXBeeDevice"/> is <c>null</c> 
		/// or if <paramref name="data"/> is <c>null</c>.</exception>
		/// <seealso cref="RemoteXBeeDevice"/>
		public XBeeMessage(RemoteXBeeDevice remoteXBeeDevice, byte[] data, bool isBroadcast)
		{
			Device = remoteXBeeDevice ?? throw new ArgumentNullException("Remote XBee device cannot be null.");
			Data = data ?? throw new ArgumentNullException("Data cannot be null.");
			IsBroadcast = isBroadcast;
		}

		// Properties.
		/// <summary>
		/// The remote XBee device this message is associated to.
		/// </summary>
		public RemoteXBeeDevice Device { get; private set; }

		/// <summary>
		/// The data as byte array containing the data of the message.
		/// </summary>
		public byte[] Data { get; private set; }

		/// <summary>
		/// Gets whether or not the message was received via broadcast.
		/// </summary>
		public bool IsBroadcast { get; private set; }

		/// <summary>
		/// Gets the data of the message in string format.
		/// </summary>
		/// <returns>The data of the message in string format.</returns>
		public string DataString
		{
			get
			{
				return Encoding.UTF8.GetString(Data, 0, Data.Length);
			}
		}
	}
}