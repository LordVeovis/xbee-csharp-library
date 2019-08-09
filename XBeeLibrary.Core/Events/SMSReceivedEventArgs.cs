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
using XBeeLibrary.Core.Models;

namespace XBeeLibrary.Core.Events
{
	/// <summary>
	/// Provides the SMS received event.
	/// </summary>
	public class SMSReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Instantiates a <see cref="SMSReceivedEventArgs"/> object with the provided parameters.
		/// </summary>
		/// <param name="smsMessage">The received SMS message.</param>
		public SMSReceivedEventArgs(SMSMessage smsMessage)
		{
			SMSReceived = smsMessage;
		}

		// Properties.
		/// <summary>
		/// The received SMS that contains the SMS text and the phone number that sent the message.
		/// </summary>
		/// <seealso cref="SMSMessage"/>
		public SMSMessage SMSReceived { get; private set; }
	}
}
