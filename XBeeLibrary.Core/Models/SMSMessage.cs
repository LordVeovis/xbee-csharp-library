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
using System.Text.RegularExpressions;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an SMS message containing the phone number that sent the message and 
	/// the content (data) of the message.
	/// </summary>
	/// <remarks> This class is used within the library to read SMS messages sent to Cellular devices.</remarks>
	public class SMSMessage
	{
		// Constants.
		private const string PHONE_NUMBER_PATTERN = "^\\+?\\d+$";

		/// <summary>
		/// Class constructor. Instantiates a new object of type <see cref="SMSMessage"/> with the given 
		/// parameters.
		/// </summary>
		/// <param name="phoneNumber">The phone number that sent the message.</param>
		/// <param name="data">String containing the message text.</param>
		/// <exception cref="ArgumentException">If <paramref name="phoneNumber"/> is invalid.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="phoneNumber"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		public SMSMessage(string phoneNumber, string data)
		{
			if (PhoneNumber == null)
				throw new ArgumentNullException("Phone number cannot be null.");
			if (Data == null)
				throw new ArgumentNullException("Data cannot be null.");
			if (!Regex.IsMatch(PhoneNumber, PHONE_NUMBER_PATTERN))
				throw new ArgumentException("Invalid phone number.");

			PhoneNumber = phoneNumber;
			Data = data;
		}

		// Properties.
		/// <summary>
		/// The phone number that sent the message.
		/// </summary>
		public string PhoneNumber { get; private set; }

		/// <summary>
		/// A string containing the data of the message.
		/// </summary>
		public string Data { get; private set; }
	}
}
