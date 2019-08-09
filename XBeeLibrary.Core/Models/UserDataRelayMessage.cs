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

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents a User Data Relay message containing the source interface from which the 
	/// message was sent and the data.
	/// </summary>
	public class UserDataRelayMessage
	{
		/// <summary>
		/// Instantiates a new <see cref="UserDataRelayMessage"/> object with the given parameters.
		/// </summary>
		/// <param name="sourceInterface">Source interface.</param>
		/// <param name="data">Data.</param>
		/// <exception cref="ArgumentException">If the source interface is unknown.</exception>
		public UserDataRelayMessage(XBeeLocalInterface sourceInterface, byte[] data)
		{
			if (sourceInterface == XBeeLocalInterface.UNKNOWN)
				throw new ArgumentException("Source interface cannot be unknown.");

			SourceInterface = sourceInterface;
			Data = data;
		}

		// Properties.
		/// <summary>
		/// The source interface of the User Data Relay message.
		/// </summary>
		public XBeeLocalInterface SourceInterface { get; private set; }

		/// <summary>
		/// The data contained in the User Data Relay message.
		/// </summary>
		public byte[] Data { get; private set; }
	}
}