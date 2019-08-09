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

namespace XBeeLibrary.Core.Packet
{
	/// <summary>
	/// This class stores, computes and verifies the checksum of the API packets.
	/// </summary>
	/// <remarks>To test data integrity, a checksum is calculated and verified on non-escaped API data. 
	/// 
	/// To calculate
	/// 
	/// Not including frame delimiters and Length, add all bytes keeping only the lowest 8 bits of the 
	/// result and subtract the result from <c>0xFF</c>.
	/// 
	/// To verify
	/// 
	/// Add all bytes (include checksum, but not the delimiter and Length). If the checksum is correct, 
	/// the sum will equal <c>0xFF</c>.</remarks>
	public class XBeeChecksum
	{
		// Variables.
		private int value = 0;

		/// <summary>
		/// Adds the given byte to the checksum.
		/// </summary>
		/// <param name="value">Byte to add.</param>
		public void Add(int value)
		{
			this.value += value;
		}

		/// <summary>
		/// Adds the given data to the checksum.
		/// </summary>
		/// <param name="data">Byte array to add.</param>
		public void Add(byte[] data)
		{
			if (data == null)
				return;
			for (int i = 0; i < data.Length; i++)
				Add(data[i]);
		}

		/// <summary>
		/// Resets the checksum.
		/// </summary>
		public void Reset()
		{
			value = 0;
		}

		/// <summary>
		/// Generates the checksum byte for the API packet.
		/// </summary>
		/// <returns>Checksum byte.</returns>
		public byte Generate()
		{
			value = value & 0xFF;
			return (byte)(0xFF - value);
		}

		/// <summary>
		/// Validates the checksum.
		/// </summary>
		/// <returns><c>true</c> if checksum is valid, <c>false</c> otherwise.</returns>
		public bool Validate()
		{
			value = value & 0xFF;
			return value == 0xFF;
		}
	}
}