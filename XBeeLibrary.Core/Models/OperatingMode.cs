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

using System.Collections.Generic;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates the different working modes of the XBee device. The operating mode establishes the 
	/// way a user communicates with an XBee device through its serial interface.
	/// </summary>
	public enum OperatingMode
	{
		// Enumeration entries.
		AT = 0,
		API = 1,
		API_ESCAPE = 2,
		UNKNOWN = 3
	}

	public static class OperatingModeExtensions
	{
		static IDictionary<OperatingMode, string> lookupTable = new Dictionary<OperatingMode, string>();

		static OperatingModeExtensions()
		{
			lookupTable.Add(OperatingMode.AT, "AT mode");
			lookupTable.Add(OperatingMode.API, "API mode");
			lookupTable.Add(OperatingMode.API_ESCAPE, "API mode with escaped characters");
			lookupTable.Add(OperatingMode.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the operating mode ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The operating mode ID.</returns>
		public static int GetID(this OperatingMode source)
		{
			return (int)source;
		}

		/// <summary>
		/// Gets the operating mode name.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The operating mode name.</returns>
		public static string GetName(this OperatingMode source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Returns the <see cref="OperatingMode"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="OperatingMode"/> in string format.</returns>
		public static string ToDisplayString(this OperatingMode source)
		{
			return lookupTable[source];
		}
	}
}
