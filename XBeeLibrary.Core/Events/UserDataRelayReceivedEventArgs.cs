﻿/*
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
	/// Provides the contents of the User Data Relay received event.
	/// </summary>
	public class UserDataRelayReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Instantiates a <see cref="UserDataRelayReceivedEventArgs"/> object with the provided 
		/// parameters.
		/// </summary>
		/// <param name="userDataRelayMessage">The User Data Relay message received.</param>
		/// <seealso cref="Models.UserDataRelayMessage"/>
		public UserDataRelayReceivedEventArgs(UserDataRelayMessage userDataRelayMessage)
		{
			UserDataRelayMessage = userDataRelayMessage;
		}

		// Properties.
		/// <summary>
		/// The User Data Relay message received.
		/// </summary>
		/// <seealso cref="Models.UserDataRelayMessage"/>
		public UserDataRelayMessage UserDataRelayMessage { get; private set; }
	}
}