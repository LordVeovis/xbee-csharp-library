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

namespace XBeeLibrary.Core.Exceptions
{
	/// <summary>
	/// This exception will be thrown when performing synchronous operations and the configured time 
	/// expires.
	/// </summary>
	public class TimeoutException : CommunicationException
	{
		// Constants.
		private const string DEFAULT_MESSAGE = "There was a timeout while executing the requested operation.";

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class.
		/// </summary>
		public TimeoutException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified 
		/// error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public TimeoutException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified error 
		/// message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a 
		/// null reference if no inner exception is specified.</param>
		public TimeoutException(string message, Exception innerException) : base(message, innerException) { }
	}
}
