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

namespace XBeeLibrary.Core.Exceptions
{
	/// <summary>
	/// This exception is thrown when there is an error parsing an MXI or XML firmware definition file.
	/// </summary>
	public class ParsingException : Exception
	{
		/// <summary>
		/// Creates a ParsingException with the specified message.
		/// </summary>
		/// <param name="message">The associated message.</param>
		public ParsingException(string message) : base(message) { }

		/// <summary>
		/// Creates a ParsingException with the specified message and exception.
		/// </summary>
		/// <param name="exception">Exception that caused this one.</param>
		/// <param name="message">The associated message.</param>
		public ParsingException(Exception exception, string message) : base(message, exception) { }
	}
}
