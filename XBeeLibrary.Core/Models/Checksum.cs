/*
 * Copyright 2023, Digi International Inc.
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
	internal class Checksum
	{
		private int value = 0;

		public Checksum()
		{
			Crc = 0x00;
		}

		/// <summary>
		/// The CRC.
		/// </summary>
		public byte Crc { get; private set; }

		/// <summary>
		/// Resets the CRC.
		/// </summary>
		public void ResetCrc()
		{
			value = 0;
			Crc = 0xFF;
		}

		/// <summary>
		/// Updates the checksum with the given byte array.
		/// </summary>
		/// <param name="args">Byte array to update.</param>
		public void Update(byte[] args)
		{
			if (args == null)
				return;
			for (int i = 0; i < args.Length; i++)
				value += args[i];
			value &= 0xFF;
			Crc = (byte)(~value & 0xFF);
		}
	}
}
