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

using System;
using System.Collections.Generic;
using System.IO;

namespace XBeeLibrary.Core.Models
{
	internal class FirmwareFile
	{
		// Constants.
		private static readonly object LOCK = new object();

		// Variables.
		private readonly Stream firmwareBinaryStream;
		private readonly int pageSize;

		public FirmwareFile(Stream firmwareBinaryStream, int pageSize)
		{
			this.firmwareBinaryStream = firmwareBinaryStream;
			this.pageSize = pageSize;
		}

		/// <summary>
		/// Returns the list of memory pages corresponding to the firmware's binary stream.
		/// </summary>
		/// <returns>The list of memory pages.</returns>
		public List<MemPage> GetMemPages()
		{
			lock (LOCK)
			{
				List<MemPage> memPages = new List<MemPage>();
				byte[] readBuffer = new byte[pageSize];
				int pageIndex = 0;
				int bytesRead = 0;
				long totalBytesRead = 0;
				using (var reader = new BufferedStream(firmwareBinaryStream))
				{
					while ((bytesRead = reader.Read(readBuffer, 0, pageSize)) > 0)
					{
						byte[] pageBytes = new byte[pageSize];
						for (int i = 0; i < pageSize; i++)
							pageBytes[i] = 0xFF;
						totalBytesRead += bytesRead;
						Array.Copy(readBuffer, 0, pageBytes, 0, bytesRead);
						memPages.Add(new MemPage(pageBytes, pageIndex));
						pageIndex += 1;
					}
				}

				return memPages;
			}
		}
	}
}
