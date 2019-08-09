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
	/// This enumeration lists all the available phases of the SRP authentication.
	/// </summary>
	public enum SrpPhase : byte
	{
		// Enumeration entries.
		PHASE_1 = 0x01,
		PHASE_2 = 0x02,
		PHASE_3 = 0x03,
		PHASE_4 = 0x04,
		UNKNOWN = 0xFF
	}

	public static class SrpPhaseExtensions
	{
		static IDictionary<SrpPhase, string> lookupTable = new Dictionary<SrpPhase, string>();

		static SrpPhaseExtensions()
		{
			lookupTable.Add(SrpPhase.PHASE_1, "Phase 1");
			lookupTable.Add(SrpPhase.PHASE_2, "Phase 2");
			lookupTable.Add(SrpPhase.PHASE_3, "Phase 3");
			lookupTable.Add(SrpPhase.PHASE_4, "Phase 4");
			lookupTable.Add(SrpPhase.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the <see cref="SrpPhase"/> associated with the specified ID <paramref name="value"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">ID value to retrieve <see cref="SrpPhase"/>.</param>
		/// <returns>The <see cref="SrpPhase"/> for the specified ID <paramref name="value"/>, 
		/// <see cref="SrpPhase.UNKNOWN"/> if it does not exist.</returns>
		public static SrpPhase Get(this SrpPhase source, byte value)
		{
			var values = Enum.GetValues(typeof(SrpPhase)).OfType<SrpPhase>();

			if (values.Cast<byte>().Contains(value))
				return (SrpPhase)value;

			return SrpPhase.UNKNOWN;
		}

		/// <summary>
		/// Gets the SRP phase value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The SRP phase value.</returns>
		public static byte GetValue(this SrpPhase source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the SRP phase name.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The SRP phase name.</returns>
		public static string GetName(this SrpPhase source)
		{
			return lookupTable.ContainsKey(source) ? lookupTable[source] : source.ToString();
		}

		/// <summary>
		/// Returns the <see cref="SrpPhase"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="SrpPhase"/> in string format.</returns>
		public static string ToDisplayString(this SrpPhase source)
		{
			return string.Format("({0}) {1}", HexUtils.ByteArrayToHexString(ByteUtils.IntToByteArray((byte)source)), GetName(source));
		}
	}
}