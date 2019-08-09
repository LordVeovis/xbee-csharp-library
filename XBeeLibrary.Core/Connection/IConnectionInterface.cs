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

namespace XBeeLibrary.Core.Connection
{
	/// <summary>
	/// This interface represents a protocol independent connection with an XBee device.
	/// </summary>
	/// <remarks>As an important point, the class implementing this interface must always notify 
	/// whenever new data is available to read. Not doing this will make the <see cref="DataReader"/> 
	/// class to wait forever for new data.</remarks>
	public interface IConnectionInterface
	{
		/// <summary>
		/// Attempts to open the connection interface.
		/// </summary>
		/// <seealso cref="Close"/>
		/// <seealso cref="IsOpen"/>
		void Open();

		/// <summary>
		/// Attempts to close the connection interface.
		/// </summary>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Open"/>
		void Close();

		/// <summary>
		/// Returns whether the connection interface is open or not.
		/// </summary>
		bool IsOpen { get; }

		/// <summary>
		/// Returns the connection interface stream to read and write data.
		/// </summary>
		/// <seealso cref="DataStream"/>
		DataStream Stream { get; }

		/// <summary>
		/// Writes the given data in the connection interface.
		/// </summary>
		/// <param name="data">The data to be written in the connection interface.</param>
		/// <seealso cref="WriteData(byte[], int, int)"/>
		void WriteData(byte[] data);

		/// <summary>
		/// Writes the given data in the connection interface.
		/// </summary>
		/// <param name="data">The data to be written in the connection interface.</param>
		/// <param name="offset">The start offset in the data to write.</param>
		/// <param name="length">The number of bytes to write.</param>
		/// <seealso cref="WriteData(byte[])"/>
		void WriteData(byte[] data, int offset, int length);

		/// <summary>
		/// Reads data from the connection interface and stores it in the provided byte array 
		/// returning the number of read bytes.
		/// </summary>
		/// <param name="data">The byte array to store the read data.</param>
		/// <returns>The number of bytes read.</returns>
		/// <seealso cref="ReadData(byte[], int, int)"/>
		int ReadData(byte[] data);

		/// <summary>
		/// Reads the given number of bytes at the given offset from the connection interface and 
		/// stores it in the provided byte array returning the number of read bytes.
		/// </summary>
		/// <param name="data">The byte array to store the read data.</param>
		/// <param name="offset">The start offset in data array at which the data is written.</param>
		/// <param name="length">Maximum number of bytes to read.</param>
		/// <returns>The number of bytes read.</returns>
		/// <seealso cref="ReadData(byte[])"/>
		int ReadData(byte[] data, int offset, int length);

		/// <summary>
		/// Returns the connection type of this XBee interface.
		/// </summary>
		/// <returns>The connection type of this XBee interface.</returns>
		/// <seealso cref="ConnectionType"/>
		ConnectionType GetConnectionType();

		/// <summary>
		/// Sets the keys used for encryption and decryption.
		/// </summary>
		/// <param name="key">Encryption key.</param>
		/// <param name="txNonce">Transmission nonce.</param>
		/// <param name="rxNonce">Reception nonce.</param>
		void SetEncryptionKeys(byte[] key, byte[] txNonce, byte[] rxNonce);
	}
}