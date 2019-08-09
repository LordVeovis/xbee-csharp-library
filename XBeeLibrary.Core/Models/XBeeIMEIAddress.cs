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
using System.Text;
using System.Text.RegularExpressions;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an IMEI address used by cellular devices.
	/// </summary>
	/// <remarks>This address is only applicable for:
	/// <list type="bullet">
	/// <item><description>Cellular</description></item>
	/// </list></remarks>
	public class XBeeIMEIAddress
	{
		// Constants.
		private const string ERROR_IMEI_NULL = "IMEI address cannot be null.";
		private const string ERROR_IMEI_TOO_LONG = "IMEI address cannot be longer than 8 bytes.";
		private const string ERROR_IMEI_INVALID = "Invalid IMEI address.";

		private const int HASH_SEED = 23;

		private const string IMEI_PATTERN = "^\\d{0,15}$";
	
		// Variables.
		private byte[] address;

		/// <summary>
		/// Class constructor. Instantiates a new object of type <see cref="XBeeIMEIAddress"/> with the 
		/// given parameter.
		/// </summary>
		/// <param name="address">The IMEI address as byte array.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <c><paramref name="address"/> <![CDATA[>]]> 8</c>.</exception>
		public XBeeIMEIAddress(byte[] address)
		{
			if (address == null)
				throw new ArgumentNullException(ERROR_IMEI_NULL);
			if (address.Length > 8)
				throw new ArgumentException(ERROR_IMEI_TOO_LONG);

			GenerateByteAddress(address);
		}

		/// <summary>
		/// Class constructor. Instantiates a new object of type <see cref="XBeeIMEIAddress"/> with the 
		/// given parameter.
		/// </summary>
		/// <param name="address">The IMEI address as string.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <paramref name="address"/> does not match the IMEI 
		/// address pattern.</exception>
		public XBeeIMEIAddress(string address)
		{
			if (address == null)
				throw new ArgumentNullException(ERROR_IMEI_NULL);

			if (!Regex.IsMatch(IMEI_PATTERN, address))
				throw new ArgumentException(ERROR_IMEI_INVALID);

			byte[] byteAddress = HexUtils.HexStringToByteArray(address);

			GenerateByteAddress(byteAddress);
		}

		// Properties.
		/// <summary>
		/// Gets the IMEI address value.
		/// </summary>
		public string Value => HexUtils.ByteArrayToHexString(address).Substring(1);

		/// <summary>
		/// Returns whether this object is equal to the given one.
		/// </summary>
		/// <param name="obj">The object to compare if it is equal to this one.</param>
		/// <returns><c>true</c> if this object is equal to the given one, <c>false</c> 
		/// otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (!(obj is XBeeIMEIAddress))
				return false;
			XBeeIMEIAddress addr = obj as XBeeIMEIAddress;
			return addr.Value.Equals(Value);
		}

		/// <summary>
		/// Returns the Hash code of this object.
		/// </summary>
		/// <returns>The Hash code of this object.</returns>
		public override int GetHashCode()
		{
			int hash = HASH_SEED;
			foreach (byte b in Encoding.UTF8.GetBytes(Value))
				hash = hash * (hash + b);
			return hash;
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return Value;
		}

		/// <summary>
		/// Generates and saves the IMEI byte address based on the given byte array.
		/// </summary>
		/// <param name="byteAddress">The byte array used to generate the final IMEI byte address.</param>
		private void GenerateByteAddress(byte[] byteAddress)
		{
			address = new byte[8];

			int diff = 8 - byteAddress.Length;
			for (int i = 0; i < diff; i++)
				address[i] = 0;
			for (int i = diff; i < 8; i++)
				address[i] = byteAddress[i - diff];
		}
	}
}
