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
using System.Collections.Generic;
using System.Linq;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates the different IP protocols.
	/// </summary>
	public enum IPProtocol
	{
		// Enumeration entries.
		UDP = 0,
		TCP = 1,
		TCP_SSL = 4,
		UNKNOWN = 99
	}

	public static class IPProtocolExtensions
	{
		static IDictionary<IPProtocol, string> lookupTable = new Dictionary<IPProtocol, string>();

		static IPProtocolExtensions()
		{
			lookupTable.Add(IPProtocol.UDP, "UDP");
			lookupTable.Add(IPProtocol.TCP, "TCP");
			lookupTable.Add(IPProtocol.TCP_SSL, "TCP SSL");
			lookupTable.Add(IPProtocol.UNKNOWN, "UNKNOWN");
		}

		/// <summary>
		/// Gets the IP protocol ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The IP protocol ID.</returns>
		public static int GetID(this IPProtocol source)
		{
			return (int)source;
		}

		/// <summary>
		/// Gets the IP protocol name.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The IP protocol name.</returns>
		public static string GetName(this IPProtocol source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the IP protocol for the given ID.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="id">ID value to retrieve <see cref="IPProtocol"/>.</param>
		/// <returns>The IP protocol for the given ID., <see cref="IPProtocol.UNKNOWN"/> 
		/// if the <paramref name="id"/> could not be found in the list.</returns>
		public static IPProtocol Get(this IPProtocol source, int id)
		{
			var values = Enum.GetValues(typeof(IPProtocol));

			if (values.OfType<int>().Contains(id))
				return (IPProtocol)id;

			return IPProtocol.UNKNOWN;
		}

		/// <summary>
		/// Returns the <see cref="IPProtocol"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="IPProtocol"/> in string format.</returns>
		public static string ToDisplayString(this IPProtocol source)
		{
			return lookupTable[source];
		}
	}
}
