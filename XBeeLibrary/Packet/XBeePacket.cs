using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Kveer.XBeeApi.Packet
{
	/// <summary>
	/// This abstract class represents the basic structure of an XBee packet.
	/// 
	/// Derived classes should implement their own payload generation depending on their type.
	/// 
	/// Generic actions like checksum compute or packet Length calculation is performed here.
	/// </summary>
	public abstract class XBeePacket
	{
		private XBeeChecksum checksum;

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeePacket"/>.
		/// </summary>
		protected XBeePacket()
		{
			checksum = new XBeeChecksum();
		}

		/// <summary>
		/// Generates the XBee packet byte array. 
		/// </summary>
		/// <remarks>Use only while working in API mode 1. If API mode is 2, use<see cref="GenerateByteArrayEscaped"/>.</remarks>
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
		/// <remarks>Use only while working in API mode 2. If API mode is 1 use <see cref="GenerateByteArray"/>.</remarks>
		/// <returns>he XBee packet byte array with escaped characters.</returns>
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
		/// Gets the packet Length.
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
		/// Gets the packet checksum.
		/// </summary>
		/// <remarks>To calculate: Not including frame delimiters and Length, add all bytes keeping only the lowest 8 bits of the result and subtract the result from <code>0xff</code>.</remarks>
		public int Checksum
		{
			get
			{
				checksum.Reset();
				byte[] packetData = GetPacketData();
				if (packetData != null)
					checksum.Add(packetData);
				return (byte)checksum.Generate() & 0xFF;
			}
		}

		/// <summary>
		/// Gets a dictionary with the XBee packet parameters and their values.
		/// </summary>
		public LinkedDictionary<string, string> Parameters
		{
			get
			{
				var parameters = new LinkedDictionary<string, string>();
				parameters.Add(new KeyValuePair<string, string>("Start delimiter", HexUtils.IntegerToHexString(SpecialByte.HEADER_BYTE.GetValue(), 1)));
				parameters.Add(new KeyValuePair<string, string>("Length", HexUtils.PrettyHexString(HexUtils.IntegerToHexString(PacketLength, 2)) + " (" + PacketLength + ")"));

				foreach (var kvp in PacketParameters)
					parameters.Add(kvp);

				parameters.Add(new KeyValuePair<string, string>("Checksum", ToString().Substring(ToString().Length - 2)));
				return parameters;
			}
		}

		/// <summary>
		/// Gets a sorted dictionary with the XBee packet parameters and their values.
		/// </summary>
		protected abstract LinkedDictionary<string, string> PacketParameters { get; }

		public override string ToString()
		{
			return HexUtils.ByteArrayToHexString(GenerateByteArray());
		}

		/// <summary>
		/// Gets a pretty string representing the packet.
		/// </summary>
		/// <returns>Pretty String representing the packet.</returns>
		public string ToPrettyString()
		{
			var value = new StringBuilder("Packet: " + ToString() + "\n");

			foreach (var kvp in Parameters)
				value.AppendFormat("{0}: {1}\n", kvp.Key, kvp.Value);
			return value.ToString();
		}

		/**
		 * Parses the given hexadecimal string and returns a Generic XBee packet. 
		 * 
		 * <p>The string can contain white spaces.</p>
		 * 
		 * @param packet The hexadecimal string to parse.
		 * @param mode The operating mode to parse the packet (API 1 or API 2).
		 * 
		 * @return The generated Generic XBee Packet.
		 * 
		 * @throws ArgumentException if {@code mode != OperatingMode.API } and
		 *                                  if {@code mode != OperatingMode.API_ESCAPE}.
		 * @throws InvalidPacketException if the given string does not represent a 
		 *                                valid frame: invalid checksum, Length, 
		 *                                start delimiter, etc.
		 * @throws ArgumentNullException if {@code packet == null}.
		 * 
		 * @see com.digi.xbee.api.models.OperatingMode#API
		 * @see com.digi.xbee.api.models.OperatingMode#API_ESCAPE
		 */
		public static XBeePacket ParsePacket(String packet, OperatingMode mode) /*throws InvalidPacketException*/ {
			Contract.Requires<ArgumentNullException>(packet != null, "Packet cannot be null.");

			return ParsePacket(HexUtils.HexStringToByteArray(packet.Trim().Replace(" ", "")), mode);
		}

		/**
		 * Parses the given byte array and returns a Generic XBee packet.
		 * 
		 * @param packet The byte array to parse.
		 * @param mode The operating mode to parse the packet (API 1 or API 2).
		 * 
		 * @return The generated Generic XBee Packet.
		 * 
		 * @throws ArgumentException if {@code mode != OperatingMode.API } and
		 *                                  if {@code mode != OperatingMode.API_ESCAPE} 
		 *                                  or if {@code packet.Length == 0}.
		 * @throws InvalidPacketException if the given byte array does not represent 
		 *                                a valid frame: invalid checksum, Length, 
		 *                                start delimiter, etc.
		 * @throws ArgumentNullException if {@code packet == null}.
		 * 
		 * @see com.digi.xbee.api.models.OperatingMode#API
		 * @see com.digi.xbee.api.models.OperatingMode#API_ESCAPE
		 */
		public static XBeePacket ParsePacket(byte[] packet, OperatingMode mode)
		{
			Contract.Requires<ArgumentNullException>(packet != null, "Packet byte array cannot be null.");
			Contract.Requires<ArgumentException>(mode == OperatingMode.API || mode == OperatingMode.API_ESCAPE, "Operating mode must be API or API Escaped.");
			Contract.Requires<ArgumentException>(packet.Length != 0, "Packet Length should be greater than 0.");
			Contract.Requires<ArgumentException>(packet.Length == 1 || packet[0] == (byte)SpecialByte.HEADER_BYTE, "Invalid start delimiter.");

			XBeePacketParser parser = new XBeePacketParser();
			XBeePacket xbeePacket = parser.ParsePacket(new MemoryStream(packet, 1, packet.Length - 1), mode);
			return xbeePacket;
		}
	}
}