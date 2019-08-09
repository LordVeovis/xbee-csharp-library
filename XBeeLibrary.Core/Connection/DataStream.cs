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
using System.IO;

namespace XBeeLibrary.Core.Connection
{
	public class DataStream : Stream
	{
		// Constants.
		private static readonly int MAX_CAPACITY = 512 * 1024; // 512 Kb

		// Variables.
		private readonly MemoryStream innerStream;
		private long readPosition;
		private long writePosition;

		public DataStream()
		{
			innerStream = new MemoryStream();
		}

		public DataStream(byte[] byteArray)
		{
			innerStream = new MemoryStream(byteArray);
			writePosition = byteArray.Length;
		}

		// Properties.
		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return false; } }

		public override bool CanWrite { get { return true; } }

		public override void Flush()
		{
			lock (innerStream)
			{
				innerStream.Flush();
			}
		}

		public override long Length
		{
			get
			{
				lock (innerStream)
				{
					return innerStream.Length;
				}
			}
		}

		public int Available
		{
			get
			{
				return (int) (writePosition - readPosition);
			}
		}

		public override long Position
		{
			get { return innerStream.Position; }
			set { throw new NotSupportedException(); }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			lock (innerStream)
			{
				innerStream.Position = readPosition;
				int read = innerStream.Read(buffer, offset, count);
				readPosition = innerStream.Position;

				return read;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			lock (innerStream)
			{
				// If the stream length has reached the max capacity and all data has been read,
				// reset the write and read positions.
				if (writePosition > MAX_CAPACITY && writePosition == readPosition)
				{
					writePosition = 0;
					readPosition = 0;
				}

				innerStream.Position = writePosition;
				innerStream.Write(buffer, offset, count);
				writePosition = innerStream.Position;
			}
		}
	}
}
