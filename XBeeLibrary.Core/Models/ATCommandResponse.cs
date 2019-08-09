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
	/// This class represents the response of an AT Command sent by the connected XBee device or by a remote 
	/// device after executing an AT Command.
	/// </summary>
	/// <remarks>Among the executed command, this object contains the response data and the command status.</remarks>
	/// <seealso cref="ATCommand"/>
	/// <seealso cref="ATCommandStatus"/>
	public class ATCommandResponse
	{
		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommandResponse"/>.
		/// </summary>
		/// <param name="command">The <see cref="ATCommand"/> that generated the response.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/> is <c>null</c>.</exception>
		/// <seealso cref="ATCommand"/>
		public ATCommandResponse(ATCommand command) : this(command, null, ATCommandStatus.OK) { }

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommandResponse"/>.
		/// </summary>
		/// <param name="command">The <see cref="ATCommand"/> that generated the response.</param>
		/// <param name="status">The <see cref="ATCommandStatus"/> containing the response status.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/> is <c>null</c>.</exception>
		/// <seealso cref="ATCommand"/>
		/// <seealso cref="ATCommandStatus"/>
		public ATCommandResponse(ATCommand command, ATCommandStatus status) : this(command, null, status) { }

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommandResponse"/>.
		/// </summary>
		/// <param name="command">The <see cref="ATCommand"/> that generated the response.</param>
		/// <param name="response">The command response in byte array format.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="command"/> is <c>null</c>.</exception>
		/// <seealso cref="ATCommand"/>
		public ATCommandResponse(ATCommand command, byte[] response) : this(command, response, ATCommandStatus.OK) { }

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommandResponse"/>.
		/// </summary>
		/// <param name="command">The <see cref="ATCommand"/> that generated the response.</param>
		/// <param name="response">The command response in byte array format.</param>
		/// <param name="status">The <see cref="ATCommandStatus"/> containing the response status.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="command"/> is <c>null</c>.</exception>
		/// <seealso cref="ATCommand"/>
		/// <seealso cref="ATCommandStatus"/>
		public ATCommandResponse(ATCommand command, byte[] response, ATCommandStatus status)
		{
			Command = command ?? throw new ArgumentNullException("Command cannot be null.");
			Response = response;
			Status = status;
		}

		// Properties.
		/// <summary>
		/// The AT command that generated the response.
		/// </summary>
		/// <seealso cref="ATCommand"/>
		public ATCommand Command { get; private set; }

		/// <summary>
		/// The AT command status of the response.
		/// </summary>
		public ATCommandStatus Status { get; private set; }

		/// <summary>
		/// The AT command response data in byte array format, if any.
		/// </summary>
		public byte[] Response { get; private set; }

		/// <summary>
		/// The AT command response data as string, if any.
		/// </summary>
		public string ResponseString
		{
			get
			{
				if (Response == null)
					return null;

				return Encoding.UTF8.GetString(Response, 0, Response.Length);
			}
		}
	}
}