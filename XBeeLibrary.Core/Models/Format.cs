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
	/// Enumerates the different field formats of XBee settings.
	/// </summary>
	public enum Format
	{
		// Enumeration entries.
		HEX,
		ASCII,
		IPV4,
		IPV6,
		PHONE,
		NOFORMAT,
		UNKNOWN
	}

	public static class FormatExtensions
	{
		// Variables.
		static IDictionary<Format, string> lookupTable = new Dictionary<Format, string>();

		static FormatExtensions()
		{
			lookupTable.Add(Format.HEX, "hexadecimal");
			lookupTable.Add(Format.ASCII, "ASCII");
			lookupTable.Add(Format.IPV4, "IPv4");
			lookupTable.Add(Format.IPV6, "IPv6");
			lookupTable.Add(Format.PHONE, "phone");
			lookupTable.Add(Format.NOFORMAT, "No format");
			lookupTable.Add(Format.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Retrieves the Format identifier.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The format id.</returns>
		public static Format GetIdentifier(this Format source)
		{
			return source;
		}

		/// <summary>
		/// Retrieves the Format description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The format description.</returns>
		public static string GetDescription(this Format source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Retrieves the <see cref="Format"/> for the given <paramref name="identifier"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="identifier">ID value to retrieve <see cref="Format"/>.</param>
		/// <returns>The <see cref="Format"/> associated with the given <paramref name="identifier"/>,
		/// <see cref="Format.NOFORMAT"/> if it does not exist.</returns>
		public static Format Get(this Format source, string identifier)
		{
			var values = Enum.GetValues(typeof(Format)).OfType<Format>();

			foreach (var value in values)
			{
				if (value.ToString().Equals(identifier))
					return value;
			}

			return Format.NOFORMAT;
		}

		/// <summary>
		/// Returns the <see cref="Format"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="Format"/> in string format.</returns>
		public static string ToDisplayString(this Format source)
		{
			return GetDescription(source);
		}

		/// <summary>
		/// Returns all the possible <see cref="Format"/> values in regex format (separated by '|').
		/// </summary>
		/// <returns>All the possible values in regex format.</returns>
		public static string GetAllValues()
		{
			string values = "";
			var formats = Enum.GetValues(typeof(Format)).OfType<Format>();
			foreach (Format format in formats)
			{
				values += format.ToDisplayString();
				values += "|";
			}
			// Remove last character added.
			return values.Remove(values.Length - 1);
		}
	}
}
