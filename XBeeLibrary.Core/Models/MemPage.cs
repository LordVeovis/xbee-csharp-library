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
	internal class MemPage
	{
		// Constants.
		private static readonly byte EMPTY_BYTE = 0xFF;

		public MemPage(byte[] pageBytes, int index)
		{
			PageBytes = pageBytes;
			Index = index;
			IsEmptyPage = true;

			// Check if page is empty.
			foreach (byte dataByte in pageBytes)
			{
				if (dataByte != EMPTY_BYTE)
				{
					IsEmptyPage = false;
					break;
				}
			}

			PageIndex = new byte[] { (byte)(index & 0xFF) };
			PageVerification = new byte[1];
			Checksum checksum = new Checksum();
			checksum.Update(pageBytes);
			PageVerification[0] = (byte)((checksum.Crc - pageBytes.Length) & 0xFF);
		}

		/// <summary>
		/// The page index.
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// The page index in byte array format.
		/// </summary>
		public byte[] PageIndex { get; private set; }

		/// <summary>
		/// The byte array of the page.
		/// </summary>
		public byte[] PageBytes { get; private set; }

		/// <summary>
		/// The verification byte(s) of the page.
		/// </summary>
		public byte[] PageVerification { get; private set; }

		/// <summary>
		/// The page address.
		/// </summary>
		public int PageAddress => Index * PageBytes.Length;

		/// <summary>
		/// Whether the page is empty or not.
		/// </summary>
		public bool IsEmptyPage { get; private set; }
	}
}
