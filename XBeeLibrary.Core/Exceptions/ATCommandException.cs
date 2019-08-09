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
	/// This exception will be thrown when any problem related to the communication with the XBee 
	/// device occurs.
	/// </summary>
	public class ATCommandException : CommunicationException
	{
		// Constants.
		const string DEFAULT_MESSAGE = "There was a problem sending the AT command packet.";

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandException"/> class.
		/// </summary>
		public ATCommandException(ATCommandStatus atCommandStatus) : base(DEFAULT_MESSAGE)
		{
			CommandStatus = atCommandStatus;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandException"/> class with a specified 
		/// error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="atCommandStatus">The status of the AT command response.</param>
		public ATCommandException(string message, ATCommandStatus atCommandStatus) : base(message)
		{
			CommandStatus = atCommandStatus;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandException"/> class with a specified error 
		/// message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null 
		/// reference if no inner exception is specified.</param>
		/// <param name="atCommandStatus">The status of the AT command response.</param>
		public ATCommandException(string message, Exception innerException, ATCommandStatus atCommandStatus) : base(message, innerException)
		{
			CommandStatus = atCommandStatus;
		}

		// Properties.
		/// <summary>
		/// The <see cref="ATCommandStatus"/> of the exception containing information about the AT 
		/// command response.
		/// </summary>
		public ATCommandStatus CommandStatus { get; private set; }

		/// <summary>
		/// The text containing the status of the AT command response.
		/// </summary>
		public string CommandStatusMessage { get { return CommandStatus.GetDescription(); } }

		/// <summary>
		/// The message reported by this exception.
		/// </summary>
		public override string Message
		{
			get
			{
				return string.Format("{0} > {1}", base.Message, CommandStatusMessage);
			}
		}
	}
}
