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
using System.Linq;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates all the special bytes of the XBee protocol that must be escaped when working on 
	/// API 2 mode.
	/// </summary>
	public enum SpecialByte : byte
	{
		// Enumeration entries.
		ESCAPE_BYTE = 0x7D,
		HEADER_BYTE = 0x7E,
		XON_BYTE = 0x11,
		XOFF_BYTE = 0x13
	}

	public static class SpecialByteExtensions
	{
		/// <summary>
		/// Gest the special byte value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The special byte value.</returns>
		public static byte GetValue(this SpecialByte source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the <see cref="SpecialByte"/> entry associated with the given value.
		/// </summary>
		/// <param name="dumb"></param>
		/// <param name="value">Value of the <see cref="SpecialByte"/> to retrieve.</param>
		/// <returns><see cref="SpecialByte"/> associated to the given value, <c>0</c> if it does not 
		/// exist in the list.</returns>
		public static SpecialByte Get(this SpecialByte dumb, byte value)
		{
			var values = Enum.GetValues(typeof(SpecialByte)).OfType<SpecialByte>();

			if (values.Cast<byte>().Contains(value))
				return (SpecialByte)value;

			return 0;
		}

		/// <summary>
		/// Escapes the byte by performing a XOR operation with <code>0x20</code> value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>Escaped byte value.</returns>
		public static byte EscapeByte(this SpecialByte source)
		{
			return (byte)(((byte)source) ^ 0x20);
		}

		/// <summary>
		/// Checks whether the given byte is special or not.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="byteToCheck">Byte to check.</param>
		/// <returns><c>true</c> if given byte is special, <c>false</c> otherwise.</returns>
		public static bool IsSpecialByte(this SpecialByte source, byte byteToCheck)
		{
			return source.Get(byteToCheck) != 0;
		}
	}
}
