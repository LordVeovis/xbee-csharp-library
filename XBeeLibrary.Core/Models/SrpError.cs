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
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This enumeration lists all the available errors of the SRP authentication.
	/// </summary>
	public enum SrpError : byte
	{
		// Enumeration entries.
		UNABLE_OFFER_B = 0x80,
		INCORRECT_PAYLOAD_LENGTH = 0x81,
		BAD_PROOF_KEY = 0x82,
		RESOURCE_ALLOCATION_ERROR = 0x83,
		NOT_CORRECT_SEQUENCE = 0x84,
		UNKNOWN = 0xFF
	}

	public static class SrpErrorExtensions
	{
		static IDictionary<SrpError, string> lookupTable = new Dictionary<SrpError, string>();

		static SrpErrorExtensions()
		{
			lookupTable.Add(SrpError.UNABLE_OFFER_B, "Unable to offer B (cryptographic error with content, usually due to A mod N == 0)");
			lookupTable.Add(SrpError.INCORRECT_PAYLOAD_LENGTH, "Incorrect payload length");
			lookupTable.Add(SrpError.BAD_PROOF_KEY, "Bad proof of key");
			lookupTable.Add(SrpError.RESOURCE_ALLOCATION_ERROR, "Resource allocation error");
			lookupTable.Add(SrpError.NOT_CORRECT_SEQUENCE, "Request contained step not in correct sequence");
			lookupTable.Add(SrpError.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the <see cref="SrpError"/> associated with the specified ID <paramref name="value"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">ID value to retrieve <see cref="SrpError"/>.</param>
		/// <returns>The <see cref="SrpError"/> for the specified ID <paramref name="value"/>, 
		/// <see cref="SrpError.UNKNOWN"/> if it does not exist.</returns>
		public static SrpError Get(this SrpError source, byte value)
		{
			var values = Enum.GetValues(typeof(SrpError)).OfType<SrpError>();

			if (values.Cast<byte>().Contains(value))
				return (SrpError)value;

			return SrpError.UNKNOWN;
		}

		/// <summary>
		/// Gets the SRP error value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The SRP error value.</returns>
		public static byte GetValue(this SrpError source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the SRP error name.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The SRP error name.</returns>
		public static string GetName(this SrpError source)
		{
			return lookupTable.ContainsKey(source) ? lookupTable[source] : source.ToString();
		}

		/// <summary>
		/// Returns the <see cref="SrpError"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="SrpError"/> in string format.</returns>
		public static string ToDisplayString(this SrpError source)
		{
			return string.Format("({0}) {1}", HexUtils.ByteArrayToHexString(ByteUtils.IntToByteArray((byte)source)), GetName(source));
		}
	}
}