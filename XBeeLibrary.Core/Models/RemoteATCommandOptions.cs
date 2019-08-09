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

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class lists all the possible options that can be set while transmitting a remote AT Command.
	/// </summary>
	[Flags]
	public enum RemoteATCommandOptions : byte
	{
		/// <summary>
		/// No special transmit options.
		/// </summary>
		OPTION_NONE = 0x00,

		/// <summary>
		/// Disables ACK.
		/// </summary>
		OPTION_DISABLE_ACK = 0x01,

		/// <summary>
		/// Applies changes in the remote device.
		/// </summary>
		/// <remarks>If this option is not set, AC command must be sent before changes will take effect.</remarks>
		OPTION_APPLY_CHANGES = 0x02,

		/// <summary>
		/// Uses the extended transmission timeout.
		/// </summary>
		/// <remarks>Setting the extended timeout bit causes the stack to set the extended transmission timeout 
		/// for the destination address.</remarks>
		OPTION_EXTENDED_TIMEOUT = 0x40
	}
}
