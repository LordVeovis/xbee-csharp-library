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
	/// This exception will be thrown when the answer received from an AT command is empty and 
	/// it shouldn't.
	/// </summary>
	public class ATCommandEmptyException : CommunicationException
	{
		// Constants.
		const string DEFAULT_MESSAGE = "Response received for command '{0}' was empty.";

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandEmptyException"/> class.
		/// </summary>
		/// <param name="atCommand">The AT command that originated the exception.</param>
		public ATCommandEmptyException(string atCommand) : base(string.Format(DEFAULT_MESSAGE, atCommand))
		{
			ATCommand = atCommand;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandEmptyException"/> class with a 
		/// specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for this exception.</param>
		/// <param name="atCommand">The AT command that originated the exception.</param>
		public ATCommandEmptyException(string message, string atCommand) : base(message)
		{
			ATCommand = atCommand;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandEmptyException"/> class with a specified 
		/// error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for this exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a 
		/// null reference if no inner exception is specified.</param>
		/// <param name="atCommand">The AT command that originated the exception.</param>
		public ATCommandEmptyException(string message, Exception innerException, string atCommand) : base(message, innerException)
		{
			ATCommand = atCommand;
		}

		// Properties.
		/// <summary>
		/// The AT command of the exception.
		/// </summary>
		public string ATCommand { get; private set; }
	}
}
