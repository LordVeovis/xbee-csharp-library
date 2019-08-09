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
using System.Text.RegularExpressions;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents a 16-bit network address.
	/// </summary>
	/// <remarks>This address is only applicable for:
	/// <list type="bullet">
	/// <item><description>802.15.4</description></item>
	/// <item><description>ZigBee</description></item>
	/// <item><description>ZNet 2.5</description></item>
	/// <item><description>XTend (Legacy)</description></item></list>
	/// 
	/// Each device has its own 16-bit address which is unique in the network. It is automatically 
	/// assigned when the radio joins the network for ZigBee and ZNet 2.5, and manually configured in 
	/// 802.15.4 radios. DigiMesh and Point-to-Multipoint protocols don't support 16-bit addressing.</remarks>
	public sealed class XBee16BitAddress : IEquatable<XBee16BitAddress>
	{
		// Constants.
		/// <summary>
		/// Pattern for the 16-bit address string.
		/// </summary>
		public static readonly Regex XBEE_16_BIT_ADDRESS_PATTERN = new Regex("(0[xX])?[0-9a-fA-F]{1,4}");

		/// <summary>
		/// 16-bit address reserved for the coordinator (value: 0000).
		/// </summary>
		public static readonly XBee16BitAddress COORDINATOR_ADDRESS = new XBee16BitAddress("0000");

		/// <summary>
		/// 16-bit broadcast address (value: FFFF).
		/// </summary>
		public static readonly XBee16BitAddress BROADCAST_ADDRESS = new XBee16BitAddress("FFFF");

		/// <summary>
		/// 16-bit unknown address (value: FFFE).
		/// </summary>
		public static readonly XBee16BitAddress UNKNOWN_ADDRESS = new XBee16BitAddress("FFFE");

		private const int HASH_SEED = 23;

		// Variables
		private readonly byte[] address;

		/// <summary>
		/// Initializes a new instance of class <see cref="XBee16BitAddress"/>.
		/// </summary>
		/// <param name="hsb">High significant byte of the address.</param>
		/// <param name="lsb">Low significant byte of the address.</param>
		/// <exception cref="ArgumentOutOfRangeException">If <c><paramref name="hsb"/> <![CDATA[>]]> 255</c> 
		/// or if <c><paramref name="hsb"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="lsb"/> <![CDATA[>]]> 255</c> 
		/// or if <c><paramref name="lsb"/> <![CDATA[<]]> 0</c>.</exception>
		public XBee16BitAddress(byte hsb, byte lsb)
		{
			if (hsb < 0 || hsb > 255)
				throw new ArgumentOutOfRangeException("HSB must be between 0 and 255.");
			if (lsb < 0 || lsb > 255)
				throw new ArgumentOutOfRangeException("LSB must be between 0 and 255.");

			address = new byte[2];
			address[0] = hsb;
			address[1] = lsb;
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="XBee16BitAddress"/>.
		/// </summary>
		/// <param name="address">The 16-bit address as byte array.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If Length of <c><paramref name="address"/> != 2</c>.</exception>
		public XBee16BitAddress(byte[] address)
		{
			if (address == null)
				throw new ArgumentNullException("Address cannot be null.");
			if (address.Length < 1 || address.Length > 2)
				throw new ArgumentOutOfRangeException("Address must have between 1 and 2 bytes.");

			// Check array size.
			this.address = new byte[2];
			int diff = this.address.Length - address.Length;
			for (int i = 0; i < diff; i++)
				this.address[i] = 0;
			for (int i = diff; i < this.address.Length; i++)
				this.address[i] = address[i - diff];
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="XBee16BitAddress"/>.
		/// </summary>
		/// <remarks>The string must be the hexadecimal representation of a 16-bit address.</remarks>
		/// <param name="address">A string containing the 16-bit address.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If Length of <paramref name="address"/> is 
		/// lower than 1.</exception>
		/// <exception cref="FormatException">If <paramref name="address"/> contains non-hexadecimal 
		/// characters or if it is longer than 2 bytes.</exception>
		public XBee16BitAddress(string address)
		{
			if (address == null)
				throw new ArgumentNullException("Address cannot be null.");
			if (address.Length < 1)
				throw new ArgumentOutOfRangeException("Address must contain at least 1 character.");
			if (!XBEE_16_BIT_ADDRESS_PATTERN.IsMatch(address))
				throw new FormatException("Address must follow this pattern: (0x)XXXX.");

			// Convert the string into a byte array.
			byte[] byteAddress = HexUtils.HexStringToByteArray(address);
			// Check array size.
			this.address = new byte[2];
			int diff = this.address.Length - byteAddress.Length;
			for (int i = 0; i < diff; i++)
				this.address[i] = 0;
			for (int i = diff; i < this.address.Length; i++)
				this.address[i] = byteAddress[i - diff];
		}

		/// <summary>
		/// Gets the address high significant byte.
		/// </summary>
		public int Hsb
		{
			get
			{
				return address[0];
			}
		}

		/// <summary>
		/// Gets the address low significant byte.
		/// </summary>
		public int Lsb
		{
			get
			{
				return address[1];
			}
		}

		/// <summary>
		/// Gets the 16-bit address value in byte array format.
		/// </summary>
		public byte[] Value
		{
			get
			{
				return (byte[])address.Clone();
			}
		}

		/// <summary>
		/// Returns whether this object is equal to the given one.
		/// </summary>
		/// <param name="obj">The object to compare if it is equal to this one.</param>
		/// <returns><c>true</c> if this object is equal to the given one, <c>false</c> 
		/// otherwise.</returns>
		public override bool Equals(object obj)
		{
			XBee16BitAddress other = obj as XBee16BitAddress;
			return other != null && Equals(other);
		}

		/// <summary>
		/// Returns whether this <see cref="XBee16BitAddress"/> is equal to the given one.
		/// </summary>
		/// <param name="other">The <see cref="XBee16BitAddress"/> to compare if it is equal 
		/// to this one.</param>
		/// <returns><c>true</c> if this <see cref="XBee16BitAddress"/> is equal to the given 
		/// one, <c>false</c> otherwise.</returns>
		public bool Equals(XBee16BitAddress other)
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
