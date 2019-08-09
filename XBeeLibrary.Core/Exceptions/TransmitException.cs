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
	/// This exception will be thrown when receiving a transmit status different than 
	/// <see cref="XBeeTransmitStatus.SUCCESS"/> after sending an XBee API packet.
	/// </summary>
	public class TransmitException : CommunicationException
	{
		// Constants.
		private const string DEFAULT_MESSAGE = "There was a problem transmitting the XBee API packet.";

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class.
		/// </summary>
		/// <param name="transmitStatus">The status of the transmission.</param>
		public TransmitException(XBeeTransmitStatus transmitStatus) : base(DEFAULT_MESSAGE)
		{
			TransmitStatus = transmitStatus;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified 
		/// error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="transmitStatus">The status of the transmission.</param>
		public TransmitException(string message, XBeeTransmitStatus transmitStatus) : base(message)
		{
			TransmitStatus = transmitStatus;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified 
		/// error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a 
		/// null reference if no inner exception is specified.</param>
		/// <param name="transmitStatus">The status of the transmission.</param>
		public TransmitException(string message, Exception innerException, XBeeTransmitStatus transmitStatus) : base(message, innerException)
		{
			TransmitStatus = transmitStatus;
		}

		// Properties.
		/// <summary>
		/// The <see cref="XBeeTransmitStatus"/> of the exception containing information about 
		/// the transmission.
		/// </summary>
		/// <seealso cref="XBeeTransmitStatus"/>
		public XBeeTransmitStatus TransmitStatus { get; private set; }

		/// <summary>
		/// The transmit status message.
		/// </summary>
		public string TransmitStatusMessage { get { return TransmitStatus.GetDescription(); } }

		/// <summary>
		/// The exception message.
		/// </summary>
		public override string Message
		{
			get
			{
				return string.Format("{0} > {1}", base.Message, TransmitStatus.GetDescription());
			}
		}
	}
}
