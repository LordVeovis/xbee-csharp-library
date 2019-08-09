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

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates several AT commands used to parse AT command packets. The list of AT Command alias 
	/// listed here represents those AT commands whose values should be parsed as strings.
	/// </summary>
	public enum ATStringCommands
	{
		// Enumeration entries.
		NI,
		KY,
		NK,
		ZU,
		ZV,
		CC
	}

	public static class ATStringCommandsStructExtensions
	{
		/// <summary>
		/// Gets the AT Command alias.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The AT Command alias.</returns>
		public static string GetCommand(this ATStringCommands source)
		{
			return source.ToString();
		}
	}
}