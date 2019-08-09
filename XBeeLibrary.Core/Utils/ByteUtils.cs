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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeLibrary.Core.Utils
{
	/// <summary>
	/// Utility class containing methods to work with bytes and byte arrays and several data type conversions.
	/// </summary>
	public static class ByteUtils
	{
		/// <summary>
		/// Reads the given amount of bytes from the given byte array input stream.
		/// </summary>
		/// <param name="numBytes">Number of bytes to read.</param>
		/// <param name="inputStream">Byte array input stream to read bytes from.</param>
		/// <returns>An array with the read bytes.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="numBytes"/> is negative.</exception>
		/// <exception cref="ArgumentException">If <paramref name="inputStream"/> cannot be read.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="inputStream"/> is <c>null</c>.</exception>
		public static async Task<byte[]> ReadBytes(int numBytes, Stream inputStream)
		{
			if (inputStream == null)
				throw new ArgumentNullException("Input stream cannot be null.");
			if (numBytes < 0)
				throw new ArgumentOutOfRangeException("Number of bytes to read must be greater than 0.");
			if (!inputStream.CanRead)
				throw new ArgumentException("Could not read from the input stream.");

			byte[] data = new byte[numBytes];
			int len = await inputStream.ReadAsync(data, 0, numBytes);
			if (len < numBytes)
			{
				byte[] d = new byte[len];
				Array.Copy(data, 0, d, 0, len);
				return d;
			}

			return data;
		}

		/// <summary>
		/// Reads a null-terminated string from the given byte array input stream.
		/// </summary>
		/// <param name="inputStream">Byte array input stream to read string from.</param>
		/// <returns>The read string from the given <paramref name="inputStream"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="inputStream"/> cannot be read.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="inputStream"/> is <c>null</c>.</exception>
		public static string ReadString(Stream inputStream)
		{
			if (inputStream == null)
				throw new ArgumentNullException("Input stream cannot be null.");
			if (!inputStream.CanRead)
				throw new ArgumentException("Could not read from the input stream.");

			StringBuilder sb = new StringBuilder();
			int readByte;
			while ((readByte = inputStream.ReadByte()) != -1 && readByte != 0)
				sb.Append((char)(byte)readByte);

			return sb.ToString();
		}

		/// <summary>
		/// Converts the given long value into a byte array.
		/// </summary>
		/// <param name="value">Long value to convert to byte array.</param>
		/// <returns>Byte array of the given long value (8 bytes length).</returns>
		/// <seealso cref="ByteArrayToLong"/>
		public static byte[] LongToByteArray(long value)
		{
			return new byte[] {
				(byte)((value >> 56) & 0xFF),
				(byte)((value >> 48) & 0xFF),
				(byte)((value >> 40) & 0xFF),
				(byte)((value >> 32) & 0xFF),
				(byte)((value >> 24) & 0xFF),
				(byte)((value >> 16) & 0xFF),
				(byte)((value >> 8) & 0xFF),
				(byte)(value & 0xFF)
			};
		}

		/// <summary>
		/// Converts the given byte array (8 bytes length max) into a long.
		/// </summary>
		/// <param name="byteArray">Byte array to convert to long (8 bytes length max).</param>
		/// <returns>Converted long value.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="byteArray"/> is <c>null</c>.</exception>
		/// <seealso cref="LongToByteArray"/>
		public static long ByteArrayToLong(byte[] byteArray)
		{
			if (byteArray == null)
				throw new ArgumentNullException("Byte array cannot be null.");

			byte[] values = byteArray;
			if (byteArray.Length < 8)
			{
				values = new byte[8];
				int diff = 8 - byteArray.Length;
				for (int i = 0; i < diff; i++)
					values[i] = 0;
				for (int i = diff; i < 8; i++)
					values[i] = byteArray[i - diff];
			}
			return ((long)values[0] << 56)
					+ ((long)(values[1] & 0xFF) << 48)
					+ ((long)(values[2] & 0xFF) << 40)
					+ ((long)(values[3] & 0xFF) << 32)
					+ ((long)(values[4] & 0xFF) << 24)
					+ ((values[5] & 0xFF) << 16)
					+ ((values[6] & 0xFF) << 8)
					+ (values[7] & 0xFF);
		}

		/// <summary>
		/// Converts the given integer value into a byte array.
		/// </summary>
		/// <param name="value">Integer value to convert to byte array.</param>
		/// <returns>Byte array of the given integer (4 bytes length).</returns>
		/// <seealso cref="ByteArrayToInt"/>
		public static byte[] IntToByteArray(int value)
		{
			return new byte[] {
				(byte)((value >> 24) & 0xFF),
				(byte)((value >> 16) & 0xFF),
				(byte)((value >> 8) & 0xFF),
				(byte)(value & 0xFF)
			};
		}

		/// <summary>
		/// Converts the given byte array (4 bytes length max) into an integer.
		/// </summary>
		/// <param name="byteArray">Byte array to convert to integer (4 bytes length max).</param>
		/// <returns>Converted integer value.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="byteArray"/> is <c>null</c>.</exception>
		/// <seealso cref="IntToByteArray"/>
		public static int ByteArrayToInt(byte[] byteArray)
		{
			if (byteArray == null)
				throw new ArgumentNullException("Byte array cannot be null.");

			byte[] values = byteArray;
			if (byteArray.Length < 4)
			{
				values = new byte[4];
				int diff = 4 - byteArray.Length;
				for (int i = 0; i < diff; i++)
					values[i] = 0;
				for (int i = diff; i < 4; i++)
					values[i] = byteArray[i - diff];
			}
			return ((values[0] & 0xFF) << 24)
					| ((values[1] & 0xFF) << 16)
					| ((values[2] & 0xFF) << 8)
					| (values[3] & 0xFF);
		}

		/// <summary>
		/// Converts the given short value into a byte array.
		/// </summary>
		/// <param name="value">Short value to convert to byte array.</param>
		/// <returns>Byte array of the given short (2 bytes length).</returns>
		/// <seealso cref="ByteArrayToShort"/>
		public static byte[] ShortToByteArray(short value)
		{
			byte[] b = new byte[2];
			b[0] = (byte)((value >> 8) & 0xFF);
			b[1] = (byte)(value & 0xFF);
			return b;
		}

		/// <summary>
		/// Converts the given byte array (2 bytes length max) to short.
		/// </summary>
		/// <param name="byteArray">Byte array to convert to short (2 bytes length max).</param>
		/// <returns>Converted short value.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="byteArray"/> is <c>null</c>.</exception>
		/// <seealso cref="ShortToByteArray"/>
		public static short ByteArrayToShort(byte[] byteArray)
		{
			if (byteArray == null)
				throw new ArgumentNullException("Byte array cannot be null.");

			return (short)(((byteArray[0] << 8) & 0xFF00)
							| byteArray[1] & 0x00FF);
		}

		/// <summary>
		/// Converts the given string into a byte array.
		/// </summary>
		/// <param name="value">String to convert to byte array.</param>
		/// <returns>Byte array of the given string.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
		/// <seealso cref="ByteArrayToString"/>
		public static byte[] StringToByteArray(string value)
		{
			if (value == null)
				throw new ArgumentNullException("Value cannot be null.");

			return Encoding.UTF8.GetBytes(value);
		}

		/// <summary>
		/// Converts the given byte array into a string.
		/// </summary>
		/// <param name="value">Byte array to convert to string.</param>
		/// <returns>Converted string.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
		public static string ByteArrayToString(byte[] value)
		{
			if (value == null)
				throw new ArgumentNullException("Byte array cannot be null.");

			return Encoding.UTF8.GetString(value, 0, value.Length);
		}

		/// <summary>
		/// Converts the given byte into an integer.
		/// </summary>
		/// <param name="b">Byte to convert to integer.</param>
		/// <returns>Converted byte into integer.</returns>
		public static int ByteToInt(byte b)
		{
			return b & 0xFF;
		}

		/// <summary>
		/// Returns whether the specified bit of the given integer is set to 1 or not.
		/// </summary>
		/// <param name="containerInteger">Integer to check the given bit position enablement state.</param>
		/// <param name="bitPosition">Position of the bit to check its enablement state.</param>
		/// <returns><c>true</c> if the given bit position is set to 1 in the <paramref name="containerInteger"/>, 
		/// <c>false</c> otherwise.</returns>
		public static bool IsBitEnabled(int containerInteger, int bitPosition)
		{
			return (((containerInteger & 0xFFFFFFFF) >> bitPosition) & 0x01) == 0x01;
		}

		/// <summary>
		/// Reads an integer value from the given byte using the given bit offset and the given bit size.
		/// </summary>
		/// <param name="containerByte">Byte to read the integer from.</param>
		/// <param name="bitOffset">Offset inside the byte to start reading integer value.</param>
		/// <param name="bitLength">Size in bits of the integer value to read.</param>
		/// <returns>The integer read value.</returns>
		public static int ReadIntegerFromByte(byte containerByte, int bitOffset, int bitLength)
		{
			int readInteger = 0;
			for (int i = 0; i < bitLength; i++)
			{
				if (IsBitEnabled(containerByte, bitOffset + i))
					readInteger = readInteger | (int)Math.Pow(2, i);
			}
			return readInteger;
		}

		/// <summary>
		/// Reads a boolean value from the given byte at the given bit position.
		/// </summary>
		/// <param name="containerByte">Byte to read boolean value from.</param>
		/// <param name="bitOffset">Offset inside the byte to read the boolean value.</param>
		/// <returns>The read boolean value.</returns>
		public static bool ReadBooleanFromByte(byte containerByte, int bitOffset)
		{
			return IsBitEnabled(containerByte, bitOffset);
		}

		/// <summary>
		/// Reads from the given byte array input stream until a CR character is found or the end of stream 
		/// is reached. Read bytes are returned.
		/// </summary>
		/// <param name="inputStream">Byte array input stream to read from.</param>
		/// <returns>An array with the read bytes.</returns>
		/// <seealso cref="ReadBytes"/>
		/// <seealso cref="ReadString"/>
		/// <exception cref="ArgumentException">If <paramref name="inputStream"/> cannot be read.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="inputStream"/> is <c>null</c>.</exception>
		public static byte[] ReadUntilCR(Stream inputStream)
		{
			if (inputStream == null)
				throw new ArgumentNullException("Input stream cannot be null.");
			if (!inputStream.CanRead)
				throw new ArgumentException("Could not read from the input stream.");

			using (var outputStream = new MemoryStream())
			{
				int readByte;
				while (((readByte = inputStream.ReadByte()) != -1) && readByte != 0xd)
					outputStream.WriteByte((byte)readByte);

				return outputStream.ToArray();
			}
		}

		/// <summary>
		/// Generates a new byte array of the given size using the given data and filling with ASCII 
		/// zeros (0x48) the remaining space.
		/// </summary>
		/// <param name="data">Data to use in the new array.</param>
		/// <param name="finalSize">Final size of the array.</param>
		/// <returns>Final byte array of the given size containing the given data and replacing with 
		/// zeros the remaining space.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="data"/> is <c>null</c>.</exception>
		public static byte[] NewByteArray(byte[] data, int finalSize)
		{
			if (data == null)
				throw new ArgumentNullException("Data cannot be null.");

			byte[] filledArray = new byte[finalSize];
			int diff = finalSize - data.Length;
			if (diff >= 0)
			{
				for (int i = 0; i < diff; i++)
					filledArray[i] = (byte)'0';
				Array.Copy(data, 0, filledArray, diff, data.Length);
			}
			else
				Array.Copy(data, 0, filledArray, 0, finalSize);
			return filledArray;
		}

		/// <summary>
		/// Swaps the given byte array order.
		/// </summary>
		/// <param name="source">Byte array to swap.</param>
		/// <returns>The swapped byte array.</returns>
		public static byte[] SwapByteArray(byte[] source)
		{
			return source.Reverse().ToArray();
		}
	}
}