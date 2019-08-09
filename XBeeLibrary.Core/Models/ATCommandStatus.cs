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
using System.Collections.Generic;
using System.Linq;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates all the possible states of an AT Command after executing it.
	/// </summary>
	/// <remarks>The <see cref="ATCommandResponse"/> objects will contain an entry of this enumerator 
	/// indicating the status of the AT Command that was executed.</remarks>
	/// <seealso cref="ATCommandResponse"/>
	public enum ATCommandStatus : byte
	{
		// Enumeration entries.
		OK = 0,
		ERROR = 1,
		INVALID_COMMAND = 2,
		INVALID_PARAMETER = 3,
		TX_FAILURE = 4,
		UNKNOWN = 255
	}

	public static class ATCommandStatusExtensions
	{
		private static IDictionary<ATCommandStatus, string> lookupTable = new Dictionary<ATCommandStatus, string>();

		static ATCommandStatusExtensions()
		{
			lookupTable.Add(ATCommandStatus.OK, "Status OK");
			lookupTable.Add(ATCommandStatus.ERROR, "Status Error");
			lookupTable.Add(ATCommandStatus.INVALID_COMMAND, "Invalid command");
			lookupTable.Add(ATCommandStatus.INVALID_PARAMETER, "Invalid parameter");
			lookupTable.Add(ATCommandStatus.TX_FAILURE, "TX failure");
			lookupTable.Add(ATCommandStatus.UNKNOWN, "Unknown status");
		}

		/// <summary>
		/// Gets the AT Command Status ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The AT Command Status ID.</returns>
		public static byte GetId(this ATCommandStatus source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the AT Command Status description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The AT Command Status description.</returns>
		public static string GetDescription(this ATCommandStatus source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gest the <see cref="ATCommandStatus"/> associated to the specified <paramref name="id"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="id">Id to retrieve the corresponding <paramref name="source"/>.</param>
		/// <returns>The <see cref="ATCommandStatus"/> associated to the specified <paramref name="id"/>.</returns>
		public static ATCommandStatus Get(this ATCommandStatus source, byte id)
		{
			var values = Enum.GetValues(typeof(ATCommandStatus)).OfType<ATCommandStatus>();

			if (values.Cast<byte>().Contains(id))
				return (ATCommandStatus)id;

			return ATCommandStatus.UNKNOWN;
		}

		/// <summary>
		/// Returns the <see cref="ATCommandStatus"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="ATCommandStatus"/> in string format.</returns>
		public static string ToDisplayString(this ATCommandStatus source)
		{
			return lookupTable[source];
		}
	}
}