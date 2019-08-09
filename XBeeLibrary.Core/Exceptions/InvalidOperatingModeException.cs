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
using XBeeLibrary.Core.Models;

namespace XBeeLibrary.Core.Exceptions
{
	/// <summary>
	/// This exception will be thrown when performing any action with the XBee device and its operating 
	/// mode is different than <see cref="OperatingMode.API"/> and <see cref="OperatingMode.API_ESCAPE"/>.
	/// </summary>
	public class InvalidOperatingModeException : CommunicationException
	{
		// Constants.
		private const string DEFAULT_MESSAGE = "The operating mode of the XBee device is not supported by the library.";

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidOperatingModeException"/> class.
		/// </summary>
		public InvalidOperatingModeException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidOperatingModeException"/> class with the 
		/// specified operating <paramref name="mode"/>.
		/// </summary>
		/// <param name="mode">The unsupported operating mode.</param>
		public InvalidOperatingModeException(OperatingMode mode) : base("Unsupported operating mode: " + mode) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidOperatingModeException"/> class with a 
		/// specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for this exception.</param>
		public InvalidOperatingModeException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidOperatingModeException"/> class with a 
		/// specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null 
		/// reference if no inner exception is specified.</param>
		public InvalidOperatingModeException(string message, Exception innerException) : base(message, innerException) { }
	}
}
