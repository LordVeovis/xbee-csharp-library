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
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates the different power levels. The power level indicates the output power value of a 
	/// radio when transmitting data.
	/// </summary>
	public enum PowerLevel : byte
	{
		// Enumeration entries.
		LEVEL_LOWEST = 0x00,
		LEVEL_LOW = 0x01,
		LEVEL_MEDIUM = 0x02,
		LEVEL_HIGH = 0x03,
		LEVEL_HIGHEST = 0x04,
		LEVEL_UNKNOWN = 0xFF
	}

	public static class PowerLevelExtensions
	{
		private static IDictionary<PowerLevel, string> lookupTable = new Dictionary<PowerLevel, string>();

		static PowerLevelExtensions()
		{
			lookupTable.Add(PowerLevel.LEVEL_LOWEST, "Lowest");
			lookupTable.Add(PowerLevel.LEVEL_LOW, "Low");
			lookupTable.Add(PowerLevel.LEVEL_MEDIUM, "Medium");
			lookupTable.Add(PowerLevel.LEVEL_HIGH, "High");
			lookupTable.Add(PowerLevel.LEVEL_HIGHEST, "Highest");
			lookupTable.Add(PowerLevel.LEVEL_UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the power level value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The power level value.</returns>
		public static byte GetValue(this PowerLevel source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the power level description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The power level description.</returns>
		public static string GetDescription(this PowerLevel source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the <see cref="PowerLevel"/> entry associated to the given value.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">Value of the <see cref="PowerLevel"/> to retrieve.</param>
		/// <returns>The <see cref="PowerLevel"/> entry associated to the given value, 
		/// <see cref="PowerLevel.LEVEL_UNKNOWN"/> if the <paramref name="value"/> could not be 
		/// found in the list.</returns>
		public static PowerLevel Get(this PowerLevel source, byte value)
		{
			var values = Enum.GetValues(typeof(PowerLevel)).OfType<PowerLevel>();

			if (values.Cast<byte>().Contains(value))
				return (PowerLevel)value;

			return PowerLevel.LEVEL_UNKNOWN;
		}

		/// <summary>
		/// Returns the <see cref="PowerLevel"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="PowerLevel"/> in string format.</returns>
		public static string ToDisplayString(this PowerLevel source)
		{
			return string.Format("{0}: {1}", HexUtils.ByteToHexString((byte)source), lookupTable[source]);
		}
	}
}
