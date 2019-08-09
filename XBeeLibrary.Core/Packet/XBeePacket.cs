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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet
{
	/// <summary>
	/// This abstract class represents the basic structure of an XBee packet.
	/// </summary>
	/// <remarks>Derived classes should implement their own payload generation depending on their type.
	/// 
	/// Generic actions like checksum compute or packet Length calculation is performed here.</remarks>
	public abstract class XBeePacket
	{
		// Constants.
		private static readonly int HASH_SEED = 23;

		private XBeeChecksum checksum;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeePacket"/> object.
		/// </summary>
		protected XBeePacket()
		{
			checksum = new XBeeChecksum();
		}

		// Properties.
		/// <summary>
		/// The packet Length.
		/// </summary>
		public int PacketLength
		{
			get
			{
				byte[] packetData = GetPacketData();

				if (packetData == null)
					return 0;
				return packetData.Length;
			}
		}

		/// <summary>
		/// The packet checksum.
		/// </summary>
		/// <remarks>To calculate: Not including frame delimiters and Length, add all bytes keeping only 
		/// the lowest 8 bits of the result and subtract the result from <c>0xFF</c>.</remarks>
		public int Checksum
		{
			get
			{
				checksum.Reset();
				byte[] packetData = GetPacketData();
				if (packetData != null)
					checksum.Add(packetData);
				return checksum.Generate() & 0xFF;
			}
		}

		/// <summary>
		/// The dictionary with the XBee packet parameters and their values.
		/// </summary>
		public LinkedDictionary<string, string> Parameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>
				{
					new KeyValuePair<string, string>("Start delimiter", HexUtils.IntegerToHexString(SpecialByte.HEADER_BYTE.GetValue(), 1)),
					new KeyValuePair<string, string>("Length", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(PacketLength, 2)) + " (" + PacketLength + ")")
				};

				foreach (var kvp in PacketParameters)
					parameters.Add(kvp);

				parameters.Add(new KeyValuePair<string, string>("Checksum", ToString().Substring(ToString().Length - 2)));
				return parameters;
			}
		}

		/// <summary>
		/// A sorted dictionary with the XBee packet parameters and their values.
		/// </summary>
		protected abstract LinkedDictionary<string, string> PacketParameters { get; }

		/// <summary>
		/// Generates the XBee packet byte array. 
		/// </summary>
		/// <remarks>Use only while working in <see cref="OperatingMode.API"/> mode. If working in 
		/// <see cref="OperatingMode.API_ESCAPE"/> mode, use<see cref="GenerateByteArrayEscaped"/>.</remarks>
		/// <returns>The XBee packet byte array.</returns>
		/// <seealso cref="GenerateByteArrayEscaped"/>
		public byte[] GenerateByteArray()
		{
			checksum.Reset();
			byte[] packetData = GetPacketData();

			using (var os = new MemoryStream())
			{
				os.WriteByte(SpecialByte.HEADER_BYTE.GetValue());
				if (packetData != null)
				{
					byte[] Length = ByteUtils.ShortToByteArray((short)packetData.Length);
					var msb = Length[0];
					var lsb = Length[1];
					os.WriteByte(msb);
					os.WriteByte(lsb);
					for (int i = 0; i < packetData.Length; i++)
					{
						checksum.Add(packetData[i]);
						os.WriteByte(packetData[i]);
					}
				}
				else
				{
					os.WriteByte(0);
					os.WriteByte(0);
				}
				os.WriteByte((byte)(checksum.Generate() & 0xFF));
				return os.ToArray();
			}
		}

		/// <summary>
		/// Generates the XBee packet byte array escaping the special bytes.
		/// </summary>
		/// <remarks>Use only while working in <see cref="OperatingMode.API_ESCAPE"/> mode. If working in 
		/// <see cref="OperatingMode.API"/> mode, use <see cref="GenerateByteArray"/>.</remarks>
		/// <returns>he XBee packet byte array with escaped characters.</returns>
		/// <seealso cref="GenerateByteArray"/>
		public byte[] GenerateByteArrayEscaped()
		{
			byte[] unescapedArray = GenerateByteArray();
			using (var os = new MemoryStream())
			{
				// Write header byte and do not escape it.
				os.WriteByte(SpecialByte.HEADER_BYTE.GetValue());
				for (int i = 1; i < unescapedArray.Length; i++)
				{
					// Start at 1 to avoid escaping header byte.
					if (SpecialByte.ESCAPE_BYTE.IsSpecialByte(unescapedArray[i]))
					{
						os.WriteByte(SpecialByte.ESCAPE_BYTE.GetValue());
						SpecialByte specialByte = SpecialByte.ESCAPE_BYTE.Get(unescapedArray[i]);
						os.WriteByte(specialByte.EscapeByte());
					}
					else
						os.WriteByte(unescapedArray[i]);
				}
				return os.ToArray();
			}
		}

		/// <summary>
		/// Returns the packet data.
		/// </summary>
		/// <returns>The packet data.</returns>
		public abstract byte[] GetPacketData();

		/// <summary>
		/// Returns the string representation of this packet.
		/// </summary>
		/// <returns>The string representation of this packet.</returns>
		public override string ToString()
		{
			return HexUtils.ByteArrayToHexString(GenerateByteArray());
		}

		/// <summary>
		/// Gets a pretty string representation of the packet.
		/// </summary>
		/// <returns>Pretty String representation of the packet.</returns>
		public string ToPrettyString()
		{
			var value = new StringBuilder("Packet: " + ToString() + "\n");

			foreach (var kvp in Parameters)
				value.AppendFormat("{0}: {1}\n", kvp.Key, kvp.Value);
			return value.ToString();
		}

		/// <summary>
		/// Parses the given hexadecimal string and returns an XBee packet.
		/// </summary>
		/// <remarks>The string can contain white spaces.</remarks>
		/// <param name="packet">The hexadecimal string to parse.</param>
		/// <param name="mode">The operating mode to parse the packet (<see cref="OperatingMode.API"/> or 
		/// <see cref="OperatingMode.API_ESCAPE"/>).</param>
		/// <returns>The generated XBee Packet.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packet"/> == null</c>.</exception>
		/// <seealso cref="OperatingMode.API"/>
		/// <seealso cref="OperatingMode.API_ESCAPE"/>
		public static XBeePacket ParsePacket(string packet, OperatingMode mode)
		{
			if (packet == null)
				throw new ArgumentNullException("Packet cannot be null.");

			return ParsePacket(HexUtils.HexStringToByteArray(packet.Trim().Replace(" ", "")), mode);
		}

		/// <summary>
		/// Parses the given byte array and returns an XBee packet.
		/// </summary>
		/// <remarks>The string can contain white spaces.</remarks>
		/// <param name="packet">The byte array to parse.</param>
		/// <param name="mode">The operating mode to parse the packet (<see cref="OperatingMode.API"/> or 
		/// <see cref="OperatingMode.API_ESCAPE"/>).</param>
		/// <returns>The generated XBee Packet.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="mode"/> != <see cref="OperatingMode.API"/></c>
		/// and if <c><paramref name="mode"/> != <see cref="OperatingMode.API_ESCAPE"/></c>
		/// or if the length of <paramref name="packet"/> is 0
		/// or if the <paramref name="packet"/> does not have the correct start delimiter.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packet"/> == null</c>.</exception>
		/// <seealso cref="OperatingMode.API"/>
		/// <seealso cref="OperatingMode.API_ESCAPE"/>
		public static XBeePacket ParsePacket(byte[] packet, OperatingMode mode)
		{
			if (packet == null)
				throw new ArgumentNullException("Packet byte array cannot be null.");
			if (mode != OperatingMode.API && mode != OperatingMode.API_ESCAPE)
				throw new ArgumentException("Operating mode must be API or API Escaped.");
			if (packet.Length == 0)
				throw new ArgumentException("Packet Length should be greater than 0.");
			if (packet.Length != 1 && packet[0] != (byte)SpecialByte.HEADER_BYTE)
				throw new ArgumentException("Invalid start delimiter.");

			XBeePacketParser parser = new XBeePacketParser();
			XBeePacket xbeePacket = parser.ParsePacket(new MemoryStream(packet, 1, packet.Length - 1), mode);
			return xbeePacket;
		}

		/// <summary>
		/// Returns whether this object is equal to the given one.
		/// </summary>
		/// <param name="obj">The object to compare if it is equal to this one.</param>
		/// <returns><c>true</c> if this object is equal to the given one, <c>false</c> 
		/// otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is XBeePacket))
				return false;
			XBeePacket packet = (XBeePacket)obj;

			return packet.GenerateByteArray().SequenceEqual(GenerateByteArray());
		}

		/// <summary>
		/// Returns the Hash code of this object.
		/// </summary>
		/// <returns>The Hash code of this object.</returns>
		public override int GetHashCode()
		{
			int hash = HASH_SEED;

			byte[] array = GenerateByteArray();
			foreach (byte b in array)
				hash = 31 * (hash + b);

			return hash;
		}
	}
}