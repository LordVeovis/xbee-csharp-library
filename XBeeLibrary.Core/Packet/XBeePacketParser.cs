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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet.Bluetooth;
using XBeeLibrary.Core.Packet.Cellular;
using XBeeLibrary.Core.Packet.Common;
using XBeeLibrary.Core.Packet.IP;
using XBeeLibrary.Core.Packet.Raw;
using XBeeLibrary.Core.Packet.Relay;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Packet
{
	/// <summary>
	/// This class reads and parses XBee packets from the input stream returning an <see cref="XBeePacket"/> 
	/// which can be casted later to the corresponding high level specific API packet.
	/// </summary>
	/// <remarks>All the API and API2 logic is already included so all packet reads are independent 
	/// of the XBee operating mode.
	/// 
	/// Two API modes are supported and both can be enabled using the <c>AP</c> (API Enable) command:
	/// <list type="bullet">
	/// <item>
	/// <term>API1 - API Without Escapes</term>
	/// <description>The data frame structure is defined as follows:
	/// <c>
	///    Start Delimiter         Length                  Frame Data                  Checksum
	///       (Byte 1)          (Bytes 2-3)               (Bytes 4-n)                (Byte n + 1)
	///  +----------------+  +-------------------+  +--------------------------- +  +----------------+
	///  |      0x7E      |  |   MSB   |   LSB   |  |   API-specific Structure   |  |     1 Byte     |
	///  +----------------+  +-------------------+  +----------------------------+  +----------------+
	///                MSB = Most Significant Byte, LSB = Least Significant Byte
	/// </c>
	/// </description>
	/// </item>
	/// <item>
	/// <term>API2 - API With Escapes</term>
	/// <description>
	/// The data frame structure is defined as follows:
	/// <c>
	///    Start Delimiter         Length                  Frame Data                  Checksum
	///       (Byte 1)          (Bytes 2-3)               (Bytes 4-n)                (Byte n + 1)
	///  +----------------+  +-------------------+  +--------------------------- +  +----------------+
	///  |      0x7E      |  |   MSB   |   LSB   |  |   API-specific Structure   |  |     1 Byte     |
	///  +----------------+  +-------------------+  +----------------------------+  +----------------+
	///                       \____________________________________________________________________/
	///                                                          \/
	///                                             Characters Escaped If Needed
	///                MSB = Most Significant Byte, LSB = Least Significant Byte
	/// </c>
	/// 
	/// When sending or receiving an API2 frame, specific data values must be escaped (flagged) so they 
	/// do not interfere with the data frame sequencing.
	/// 
	/// To escape an interfering data byte, the byte <c>0x7D</c> is inserted before the byte to be escaped XOR'd 
	/// with <c>0x20</c>.
	/// 
	/// The data bytes that need to be escaped:
	/// <list type="bullet">
	/// <item><description><c>0x7E</c> - Frame Delimiter (<see cref="SpecialByte.HEADER_BYTE"/>)</description></item>
	/// <item><description><c>0x7D</c> - Escape (<see cref="SpecialByte.ESCAPE_BYTE"/>)</description></item>
	/// <item><description><c>0x11</c> - XON (<see cref="SpecialByte.XON_BYTE"/>)</description></item>
	/// <item><description><c>0x13</c> - XOFF (<see cref="SpecialByte.XOFF_BYTE"/>)</description></item>
	/// </list>
	/// </description>
	/// </item>
	/// </list>
	/// 
	/// The Length field has a two-byte value that specifies the number of bytes that will be contained 
	/// in the frame data field. It does not include the checksum field.
	/// 
	/// The frame data forms an API-specific structure as follows:
	/// <c>
	///    Start Delimiter          Length                  Frame Data                  Checksum
	///      (Byte 1)            (Bytes 2-3)               (Bytes 4-n)                (Byte n + 1)
	///  +----------------+  +-------------------+  +---------------------------+  +----------------+
	///  |      0x7E      |  |   MSB   |   LSB   |  | API - specific Structure  |  |     1 Byte     |
	///  +----------------+  +-------------------+  +---------------------------+  +----------------+
	///                                            /                                                 \
	///                                           /   API Identifier       Identifier specific data   \
	///                                           +------------------+  +------------------------------+
	///                                           |      cmdID       |  |           cmdData            |
	///                                           +------------------+  +------------------------------+
	/// </c>
	/// 
	/// The <c>cmdID</c> frame (API-identifier) indicates which API messages will be contained in 
	/// the <c>cmdData</c> frame (Identifier-specific data).
	/// 
	/// To test data integrity, a <b>checksum</b> is calculated and verified on non-escaped data.</remarks>
	/// <seealso cref="APIFrameType"/>
	/// <seealso cref="XBeePacket"/>
	/// <seealso cref="OperatingMode"/>
	public class XBeePacketParser
	{
		/// <summary>
		/// Parses the bytes from the given <paramref name="inputStream"/> depending on the provided 
		/// <paramref name="mode"/> and returns the API packet.
		/// </summary>
		/// <remarks>The operating mode must be <see cref="OperatingMode.API"/> or 
		/// <see cref="OperatingMode.API_ESCAPE"/>.</remarks>
		/// <param name="inputStream">Input stream to read bytes from.</param>
		/// <param name="mode">XBee device operating mode.</param>
		/// <returns>Parsed packet from the input stream.</returns>
		/// <exception cref="ArgumentException">If <paramref name="mode"/> is invalid 
		/// or if the <paramref name="inputStream"/> cannot be read.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="inputStream"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidPacketException">If there is not enough data in the stream or if there 
		/// is an error verifying the checksum or if the payload is invalid for the specified frame type.</exception>
		public XBeePacket ParsePacket(Stream inputStream, OperatingMode mode)
		{
			if (inputStream == null)
				throw new ArgumentNullException("Input stream cannot be null.");
			if (!inputStream.CanRead)
				throw new ArgumentException("Could not read from the input stream.");
			if (mode != OperatingMode.API && mode != OperatingMode.API_ESCAPE)
				throw new ArgumentException("Operating mode must be API or API Escaped.");

			try
			{
				// Read packet size.
				int hSize = ReadByte(inputStream, mode);
				int lSize = ReadByte(inputStream, mode);
				int Length = hSize << 8 | lSize;

				// Read the payload.
				byte[] payload = ReadBytes(inputStream, mode, Length);

				// Calculate the expected checksum.
				XBeeChecksum checksum = new XBeeChecksum();
				checksum.Add(payload);
				byte expectedChecksum = (byte)(checksum.Generate() & 0xFF);

				// Read checksum from the input stream.
				byte readChecksum = (byte)(ReadByte(inputStream, mode) & 0xFF);

				// Verify the checksum of the read bytes.
				if (readChecksum != expectedChecksum)
					throw new InvalidPacketException("Invalid checksum (expected 0x"
								+ HexUtils.ByteToHexString(expectedChecksum) + ").");

				return ParsePayload(payload);

			}
			catch (IOException e)
			{
				throw new InvalidPacketException("Error parsing packet: " + e.Message, e);
			}
		}

		/// <summary>
		/// Parses the bytes from the given array depending on the provided operating mode and 
		/// returns the API packet.
		/// </summary>
		/// <remarks>The operating mode must be <see cref="OperatingMode.API"/> or 
		/// <see cref="OperatingMode.API_ESCAPE"/></remarks>
		/// <param name="packetByteArray">Byte array with the complete frame, starting from the 
		/// header and ending in the checksum.</param>
		/// <param name="mode">XBee device operating mode.</param>
		/// <returns>Parsed packet from the given byte array.</returns>
		/// <exception cref="InvalidPacketException">If there is not enough data in the array 
		/// or if there is an error verifying the checksum 
		/// or if the payload is invalid for the specified frame type.</exception>
		/// <exception cref="ArgumentException">If <c><paramref name="mode"/> != <see cref="OperatingMode.API"/></c>
		/// and if <c><paramref name="mode"/> != <see cref="OperatingMode.API_ESCAPE"/></c></exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packetByteArray"/> == null</c>.</exception>
		/// <seealso cref="XBeePacket"/>
		/// <seealso cref="OperatingMode.API"/>
		/// <seealso cref="OperatingMode.API_ESCAPE"/>
		public XBeePacket ParsePacket(byte[] packetByteArray, OperatingMode mode)
		{
			if (packetByteArray == null)
				throw new ArgumentNullException("Packet byte array cannot be null.");
			if (mode != OperatingMode.API && mode != OperatingMode.API_ESCAPE)
				throw new ArgumentException("Operating mode must be API or API Escaped.");
			if (packetByteArray.Length < 4)
				throw new InvalidPacketException("Error parsing packet: Incomplete packet.");

			// Check the header of the frame.
			if ((packetByteArray[0] & 0xFF) != SpecialByte.HEADER_BYTE.GetValue())
				throw new InvalidPacketException("Invalid start delimiter (expected 0x"
							+ HexUtils.ByteToHexString(SpecialByte.HEADER_BYTE.GetValue()) + ").");

			return ParsePacket(new MemoryStream(packetByteArray, 1, packetByteArray.Length - 1), mode);
		}

		/// <summary>
		/// Parses the given API payload to get the right API packet, depending on its API 
		/// type (<c><paramref name="payload"/>[0]</c>).
		/// </summary>
		/// <param name="payload">The payload of the API frame.</param>
		/// <returns>The corresponding API packet or <see cref="UnknownXBeePacket"/> if the frame API 
		/// type is unknown.</returns>
		/// <exception cref="InvalidPacketException">If the payload is invalid for the specified 
		/// frame type.</exception>
		/// <seealso cref="APIFrameType"/>
		/// <seealso cref="XBeePacket"/>
		private XBeePacket ParsePayload(byte[] payload)
		{
			// Get the API frame type.
			APIFrameType apiType = APIFrameType.UNKNOWN.Get(payload[0]);

			// Parse API payload depending on API ID.
			XBeePacket packet = null;
			switch (apiType)
			{
				case APIFrameType.TX_64:
					packet = TX64Packet.CreatePacket(payload);
					break;
				case APIFrameType.TX_16:
					packet = TX16Packet.CreatePacket(payload);
					break;
				case APIFrameType.BLE_UNLOCK:
					packet = BluetoothUnlockPacket.CreatePacket(payload);
					break;
				case APIFrameType.USER_DATA_RELAY:
					packet = UserDataRelayPacket.CreatePacket(payload);
					break;
				case APIFrameType.AT_COMMAND:
					packet = ATCommandPacket.CreatePacket(payload);
					break;
				case APIFrameType.AT_COMMAND_QUEUE:
					packet = ATCommandQueuePacket.CreatePacket(payload);
					break;
				case APIFrameType.TRANSMIT_REQUEST:
					packet = TransmitPacket.CreatePacket(payload);
					break;
				case APIFrameType.EXPLICIT_ADDRESSING_COMMAND_FRAME:
					packet = ExplicitAddressingPacket.CreatePacket(payload);
					break;
				case APIFrameType.REMOTE_AT_COMMAND_REQUEST:
					packet = RemoteATCommandPacket.CreatePacket(payload);
					break;
				case APIFrameType.TX_SMS:
					packet = TXSMSPacket.CreatePacket(payload);
					break;
				case APIFrameType.TX_IPV4:
					packet = TXIPv4Packet.CreatePacket(payload);
					break;
				case APIFrameType.TX_REQUEST_TLS_PROFILE:
					packet = TXTLSProfilePacket.CreatePacket(payload);
					break;
				case APIFrameType.RX_64:
					packet = RX64Packet.CreatePacket(payload);
					break;
				case APIFrameType.RX_16:
					packet = RX16Packet.CreatePacket(payload);
					break;
				case APIFrameType.RX_IO_64:
					packet = RX64IOPacket.CreatePacket(payload);
					break;
				case APIFrameType.RX_IO_16:
					packet = RX16IOPacket.CreatePacket(payload);
					break;
				case APIFrameType.AT_COMMAND_RESPONSE:
					packet = ATCommandResponsePacket.CreatePacket(payload);
					break;
				case APIFrameType.TX_STATUS:
					packet = TXStatusPacket.CreatePacket(payload);
					break;
				case APIFrameType.MODEM_STATUS:
					packet = ModemStatusPacket.CreatePacket(payload);
					break;
				case APIFrameType.TRANSMIT_STATUS:
					packet = TransmitStatusPacket.CreatePacket(payload);
					break;
				case APIFrameType.RECEIVE_PACKET:
					packet = ReceivePacket.CreatePacket(payload);
					break;
				case APIFrameType.EXPLICIT_RX_INDICATOR:
					packet = ExplicitRxIndicatorPacket.CreatePacket(payload);
					break;
				case APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR:
					packet = IODataSampleRxIndicatorPacket.CreatePacket(payload);
					break;
				case APIFrameType.REMOTE_AT_COMMAND_RESPONSE:
					packet = RemoteATCommandResponsePacket.CreatePacket(payload);
					break;
				case APIFrameType.RX_SMS:
					packet = RXSMSPacket.CreatePacket(payload);
					break;
				case APIFrameType.BLE_UNLOCK_RESPONSE:
					packet = BluetoothUnlockResponsePacket.CreatePacket(payload);
					break;
				case APIFrameType.USER_DATA_RELAY_OUTPUT:
					packet = UserDataRelayOutputPacket.CreatePacket(payload);
					break;
				case APIFrameType.RX_IPV4:
					packet = RXIPv4Packet.CreatePacket(payload);
					break;
				case APIFrameType.UNKNOWN:
				default:
					packet = UnknownXBeePacket.CreatePacket(payload);
					break;
			}
			return packet;
		}

		/// <summary>
		/// Reads one byte from the input stream.
		/// </summary>
		/// <remarks>This operation checks several things like the working mode in order to consider 
		/// escaped bytes.</remarks>
		/// <param name="inputStream">The input stream to read bytes from.</param>
		/// <param name="mode">The XBee device working mode.</param>
		/// <returns>The read byte.</returns>
		/// <exception cref="InvalidPacketException">If there is not enough data in the stream 
		/// or if there is an error verifying the checksum.</exception>
		/// <exception cref="ArgumentException">If input stream cannot be read.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="inputStream"/> is <c>null</c>.</exception>
		private int ReadByte(Stream inputStream, OperatingMode mode)
		{
			if (inputStream == null)
				throw new ArgumentNullException("Input stream cannot be null.");
			if (!inputStream.CanRead)
				throw new ArgumentException("Could not read from the input stream.");

			int timeout = 300;

			int b = ReadByteFrom(inputStream, timeout);

			if (b == -1)
				throw new InvalidPacketException("Error parsing packet: Incomplete packet.");

			/* Process the byte for API1. */

			if (mode == OperatingMode.API)
				return b;

			/* Process the byte for API2. */

			// Check if the byte is special.
			if (!SpecialByte.ESCAPE_BYTE.IsSpecialByte((byte)b))
				return b;

			// Check if the byte is ESCAPE.
			if (b == SpecialByte.ESCAPE_BYTE.GetValue())
			{
				// Read next byte and escape it.
				b = ReadByteFrom(inputStream, timeout);

				if (b == -1)
					throw new InvalidPacketException("Error parsing packet: Incomplete packet.");

				b ^= 0x20;
			}
			else
				// If the byte is not a escape there is a special byte not escaped.
				throw new InvalidPacketException("Special byte not escaped: 0x" + HexUtils.ByteToHexString((byte)(b & 0xFF)) + ".");

			return b;
		}

		/// <summary>
		/// Reads the given amount of bytes from the input stream.
		/// </summary>
		/// <remarks>This operation checks several things like the working mode in order to consider 
		/// escaped bytes.</remarks>
		/// <param name="inputStream">The Input stream to read bytes from.</param>
		/// <param name="mode">The XBee device working mode.</param>
		/// <param name="numBytes">The number of bytes to read.</param>
		/// <returns>The read byte array.</returns>
		/// <exception cref="ArgumentException">If input stream cannot be read.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="inputStream"/> is <c>null</c>.</exception>
		private byte[] ReadBytes(Stream inputStream, OperatingMode mode, int numBytes)
		{
			if (inputStream == null)
				throw new ArgumentNullException("Input stream cannot be null.");
			if (!inputStream.CanRead)
				throw new ArgumentException("Could not read from the input stream.");

			byte[] data = new byte[numBytes];

			for (int i = 0; i < numBytes; i++)
				data[i] = (byte)ReadByte(inputStream, mode);

			return data;
		}

		/// <summary>
		/// Reads a byte from the given input stream.
		/// </summary>
		/// <param name="inputStream">The input stream to read the byte.</param>
		/// <param name="timeout">The timeout to wait for a byte in the input stream in milliseconds.</param>
		/// <returns>The read byte or <c>-1</c> if the timeout expires or the end of the stream 
		/// is reached.</returns>
		/// <exception cref="ArgumentException">If input stream cannot be read.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="inputStream"/> is <c>null</c>.</exception>
		private int ReadByteFrom(Stream inputStream, int timeout)
		{
			if (inputStream == null)
				throw new ArgumentNullException("Input stream cannot be null.");
			if (!inputStream.CanRead)
				throw new ArgumentException("Could not read from the input stream.");

			var stopwatch = Stopwatch.StartNew();

			int b = inputStream.ReadByte();
			// Let's try again if the byte is -1.
			while (b == -1 && stopwatch.ElapsedMilliseconds < timeout)
			{
				b = inputStream.ReadByte();
				Task.Delay(10).Wait();
			}

			stopwatch.Stop();
			return b;
		}
	}
}