using Kveer.XBeeApi.Utils;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// This class represents a 16-bit network address.
	/// 
	/// This address is only applicable for:
	/// 802.15.4
	/// ZigBee
	/// ZNet 2.5
	/// XTend (Legacy)
	/// 
	/// Each device has its own 16-bit address which is unique in the network. It is automatically assigned when the radio joins the network for ZigBee and ZNet 2.5, and manually configured in 802.15.4 radios.
	/// </summary>
	/// <remarks>DigiMesh and Point-to-Multipoint protocols don't support 16-bit addressing.</remarks>
	public sealed class XBee16BitAddress
	{
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

		/// <summary>
		/// Pattern for the 16-bit address string.
		/// </summary>
		private static readonly Regex XBEE_16_BIT_ADDRESS_PATTERN = new Regex("(0[xX])?[0-9a-fA-F]{1,4}");

		private const int HASH_SEED = 23;

		// Variables
		private readonly byte[] address;

		/// <summary>
		/// Initializes a new instance of class <see cref="XBee16BitAddress"/>.
		/// </summary>
		/// <param name="hsb">High significant byte of the address.</param>
		/// <param name="lsb">Low significant byte of the address.</param>
		/// <exception cref="ArgumentOutOfRangeException">if <paramref name="hsb"/> &gt; 255 or <paramref name="hsb"/> &lt; 0 or <paramref name="lsb"/> &gt; 255 or <paramref name="lsb"/> &lt; 0.</exception>
		public XBee16BitAddress(int hsb, int lsb)
		{
			Contract.Requires<ArgumentOutOfRangeException>(hsb >= 0 && hsb <= 255, "HSB must be between 0 and 255.");
			Contract.Requires<ArgumentOutOfRangeException>(lsb >= 0 && lsb <= 255, "LSB must be between 0 and 255.");

			address = new byte[2];
			address[0] = (byte)hsb;
			address[1] = (byte)lsb;
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="XBee16BitAddress"/>.
		/// </summary>
		/// <param name="address">The 16-bit address as byte array.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="address"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">if Length of <paramref name="address"/> is not 2.</exception>
		public XBee16BitAddress(byte[] address)
		{
			Contract.Requires<ArgumentNullException>(address != null, "Address cannot be null.");
			Contract.Requires<ArgumentOutOfRangeException>(address.Length >= 1, "Address must contain at least 1 byte.");
			Contract.Requires<ArgumentOutOfRangeException>(address.Length <= 2, "Address cannot contain more than 2 bytes.");

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
		/// <exception cref="ArgumentNullException">if <paramref name="address"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">if Length of <paramref name="address"/> is lower than 1 or does contains non-hexadecimal characters and is longer than 8 bytes.</exception>
		public XBee16BitAddress(string address)
		{
			Contract.Requires<ArgumentNullException>(address != null, "Address cannot be null.");
			Contract.Requires<ArgumentOutOfRangeException>(address.Length < 1, "Address must contain at least 1 character.");
			Contract.Requires<FormatException>(XBEE_16_BIT_ADDRESS_PATTERN.IsMatch(address), "Address must follow this pattern: (0x)XXXX.");

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

		public override bool Equals(object obj)
		{
			if (!(obj is XBee16BitAddress))
				return false;

			XBee16BitAddress addr = (XBee16BitAddress)obj;

			return Enumerable.SequenceEqual(addr.Value, Value);
		}

		public override int GetHashCode()
		{
			int hash = HASH_SEED;
			foreach (byte b in Value)
				hash = hash * (hash + b);
			return hash;
		}

		public override string ToString()
		{
			return HexUtils.ByteArrayToHexString(address);
		}
	}
}
