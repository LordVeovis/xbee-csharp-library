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
using System.Text;
using System.Text.RegularExpressions;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents a 64-bit address (also known as MAC address). 
	/// </summary>
	/// <remarks>The 64-bit address is a unique device address assigned during manufacturing. This 
	/// address is unique to each physical device.</remarks>
	public sealed class XBee64BitAddress : IEquatable<XBee64BitAddress>
	{
		// Constants.
		private const string DEVICE_ID_SEPARATOR = "-";
		private const string DEVICE_ID_MAC_SEPARATOR = "FF";

		private const int HASH_SEED = 23;

		/// <summary>
		/// Pattern for the 64-bit address string.
		/// </summary>
		public static readonly Regex XBEE_64_BIT_ADDRESS_PATTERN = new Regex("(0[xX])?[0-9a-fA-F]{1,16}");

		/// <summary>
		/// 64-bit address reserved for the coordinator (value: 0000000000000000).
		/// </summary>
		public static readonly XBee64BitAddress COORDINATOR_ADDRESS = new XBee64BitAddress("0000");

		/// <summary>
		/// 64-bit broadcast address (value: 000000000000FFFF).
		/// </summary>
		public static readonly XBee64BitAddress BROADCAST_ADDRESS = new XBee64BitAddress("FFFF");

		/// <summary>
		/// 64-bit unknown address (value: 000000000000FFFE).
		/// </summary>
		public static readonly XBee64BitAddress UNKNOWN_ADDRESS = new XBee64BitAddress("FFFE");

		// Variables
		private byte[] address;

		/// <summary>
		/// Initializes a new instance of class <see cref="XBee64BitAddress"/>.
		/// </summary>
		/// <param name="address">The 64-bit address as byte array.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If the Length of <paramref name="address"/> is 
		/// not 8.</exception>
		public XBee64BitAddress(byte[] address)
		{
			if (address == null)
				throw new ArgumentNullException("Address cannot be null.");
			if (address.Length < 1 || address.Length > 8)
				throw new ArgumentOutOfRangeException("Address must have between 1 abd 8 bytes.");

			this.address = new byte[8];
			int diff = this.address.Length - address.Length;
			for (int i = 0; i < diff; i++)
				this.address[i] = 0;
			for (int i = diff; i < this.address.Length; i++)
				this.address[i] = address[i - diff];
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="XBee64BitAddress"/>.
		/// </summary>
		/// <remarks>The string must be the hexadecimal representation of a 64-bit address.</remarks>
		/// <param name="address">A string containing the 64-bit address.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If Length of <paramref name="address"/> is 
		/// lower than 1.</exception>
		/// <exception cref="FormatException">If <paramref name="address"/> contains non-hexadecimal 
		/// characters or if it is longer than 8 bytes.</exception>
		public XBee64BitAddress(string address)
		{
			if (address == null)
				throw new ArgumentNullException("Address cannot be null.");
			if (address.Length < 1)
				throw new ArgumentOutOfRangeException("Address must contain at least 1 character.");
			if (!XBEE_64_BIT_ADDRESS_PATTERN.IsMatch(address))
				throw new FormatException("Address must follow this pattern: (0x)0013A20040XXXXXX.");

			byte[] byteAddress = HexUtils.HexStringToByteArray(address);
			this.address = new byte[8];
			int diff = this.address.Length - byteAddress.Length;
			for (int i = 0; i < diff; i++)
				this.address[i] = 0;
			for (int i = diff; i < this.address.Length; i++)
				this.address[i] = byteAddress[i - diff];
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="XBee64BitAddress"/> with the given bytes being 
		/// <paramref name="b0"/> the more significant byte and <paramref name="b7"/> the less significant byte.
		/// </summary>
		/// <param name="b0">XBee 64-bit address bit 0.</param>
		/// <param name="b1">XBee 64-bit address bit 1.</param>
		/// <param name="b2">XBee 64-bit address bit 2.</param>
		/// <param name="b3">XBee 64-bit address bit 3.</param>
		/// <param name="b4">XBee 64-bit address bit 4.</param>
		/// <param name="b5">XBee 64-bit address bit 5.</param>
		/// <param name="b6">XBee 64-bit address bit 6.</param>
		/// <param name="b7">XBee 64-bit address bit 7.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If <c><paramref name="b0"/> <![CDATA[>]]> 255</c> or if <c><paramref name="b0"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="b1"/> <![CDATA[>]]> 255</c> or if <c><paramref name="b1"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="b2"/> <![CDATA[>]]> 255</c> or if <c><paramref name="b2"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="b3"/> <![CDATA[>]]> 255</c> or if <c><paramref name="b3"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="b4"/> <![CDATA[>]]> 255</c> or if <c><paramref name="b4"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="b5"/> <![CDATA[>]]> 255</c> or if <c><paramref name="b5"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="b6"/> <![CDATA[>]]> 255</c> or if <c><paramref name="b6"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="b7"/> <![CDATA[>]]> 255</c> or if <c><paramref name="b7"/> <![CDATA[<]]> 0</c> 
		/// </exception>
		public XBee64BitAddress(int b0, int b1, int b2, int b3, int b4, int b5, int b6, int b7)
		{
			if (b0 < 0 || b0 > 255)
				throw new ArgumentOutOfRangeException("B0 must be between 0 and 255.");
			if (b1 < 0 || b1 > 255)
				throw new ArgumentOutOfRangeException("B1 must be between 0 and 255.");
			if (b2 < 0 || b2 > 255)
				throw new ArgumentOutOfRangeException("B2 must be between 0 and 255.");
			if (b3 < 0 || b3 > 255)
				throw new ArgumentOutOfRangeException("B3 must be between 0 and 255.");
			if (b4 < 0 || b4 > 255)
				throw new ArgumentOutOfRangeException("B4 must be between 0 and 255.");
			if (b5 < 0 || b5 > 255)
				throw new ArgumentOutOfRangeException("B5 must be between 0 and 255.");
			if (b6 < 0 || b6 > 255)
				throw new ArgumentOutOfRangeException("B6 must be between 0 and 255.");
			if (b7 < 0 || b7 > 255)
				throw new ArgumentOutOfRangeException("B7 must be between 0 and 255.");

			address = new byte[8];
			address[0] = (byte)b0;
			address[1] = (byte)b1;
			address[2] = (byte)b2;
			address[3] = (byte)b3;
			address[4] = (byte)b4;
			address[5] = (byte)b5;
			address[6] = (byte)b6;
			address[7] = (byte)b7;
		}

		/// <summary>
		/// The XBee 64-bit address value as byte array.
		/// </summary>
		public byte[] Value
		{
			get
			{
				return (byte[])address.Clone();
			}
		}

		/// <summary>
		/// Generates the Device ID corresponding to this <see cref="XBee64BitAddress"/> to be used in Device Cloud.
		/// </summary>
		/// <returns>Device ID corresponding to this address.</returns>
		public string GenerateDeviceID()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 4; j++)
					sb.Append(HexUtils.ByteArrayToHexString(new byte[] { 0 }));
				sb.Append(DEVICE_ID_SEPARATOR);
			}
			// Here we should have "00000000-00000000-"
			// Append first three bytes of the MAC Address, discard first 2.
			sb.Append(HexUtils.ByteArrayToHexString(new byte[] { address[2], address[3], address[4] }));
			sb.Append(DEVICE_ID_MAC_SEPARATOR);
			sb.Append(DEVICE_ID_SEPARATOR);
			sb.Append(DEVICE_ID_MAC_SEPARATOR);
			// Here we should have "00000000-00000000-XXXXXXFF-FF"
			// Append second three bytes of the MAC Address.
			sb.Append(HexUtils.ByteArrayToHexString(new byte[] { address[5], address[6], address[7] }));
			return sb.ToString();
		}

		/// <summary>
		/// Returns whether this object is equal to the given one.
		/// </summary>
		/// <param name="obj">The object to compare if it is equal to this one.</param>
		/// <returns><c>true</c> if this object is equal to the given one, <c>false</c> 
		/// otherwise.</returns>
		public override bool Equals(object obj)
		{
			XBee64BitAddress other = obj as XBee64BitAddress;
			return other != null && Equals(other);
		}

		/// <summary>
		/// Returns whether this <see cref="XBee64BitAddress"/> is equal to the given one.
		/// </summary>
		/// <param name="other">The <see cref="XBee64BitAddress"/> to compare if it is equal 
		/// to this one.</param>
		/// <returns><c>true</c> if this <see cref="XBee64BitAddress"/> is equal to the given 
		/// one, <c>false</c> otherwise.</returns>
		public bool Equals(XBee64BitAddress other)
		{
			return other != null && Enumerable.SequenceEqual(other.Value, Value);
		}

		/// <summary>
		/// Returns the Hash code of this object.
		/// </summary>
		/// <returns>The Hash code of this object.</returns>
		public override int GetHashCode()
		{
			int hash = HASH_SEED;

			foreach (byte b in Value)
				hash = hash * (hash + b);

			return hash;
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return HexUtils.ByteArrayToHexString(address);
		}
	}
}