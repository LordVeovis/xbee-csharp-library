using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System.Threading;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.Packet.Raw;
using Kveer.XBeeApi.Packet.Common;
using System.Diagnostics;

namespace Kveer.XBeeApi.Packet
{
	/**
	 * This class reads and parses XBee packets from the input stream returning
	 * a generic {@code XBeePacket} which can be casted later to the corresponding 
	 * high level specific API packet.
	 * 
	 * <p>All the API and API2 logic is already included so all packet reads are 
	 * independent of the XBee operating mode.</p>
	 * 
	 * <p>Two API modes are supported and both can be enabled using the {@code AP} 
	 * (API Enable) command:
	 * 
	 * <ul>
	 * <li><b>API1 - API Without Escapes</b>
	 * <p>The data frame structure is defined as follows:</p>
	 * 
	 * <pre>
	 * {@code 
	 *   Start Delimiter          Length                   Frame Data                   Checksum
	 *       (Byte 1)            (Bytes 2-3)               (Bytes 4-n)                (Byte n + 1)
	 * +----------------+  +-------------------+  +--------------------------- +  +----------------+
	 * |      0x7E      |  |   MSB   |   LSB   |  |   API-specific Structure   |  |     1 Byte     |
	 * +----------------+  +-------------------+  +----------------------------+  +----------------+
	 *                MSB = Most Significant Byte, LSB = Least Significant Byte
	 * }
	 * </pre>
	 * </li>
	 * 
	 * <li><b>API2 - API With Escapes</b>
	 * <p>The data frame structure is defined as follows:</p>
	 * 
	 * <pre>
	 * {@code 
	 *   Start Delimiter          Length                   Frame Data                   Checksum
	 *       (Byte 1)            (Bytes 2-3)               (Bytes 4-n)                (Byte n + 1)
	 * +----------------+  +-------------------+  +--------------------------- +  +----------------+
	 * |      0x7E      |  |   MSB   |   LSB   |  |   API-specific Structure   |  |     1 Byte     |
	 * +----------------+  +-------------------+  +----------------------------+  +----------------+
	 *                     \___________________________________  _________________________________/
	 *                                                         \/
	 *                                             Characters Escaped If Needed
	 *                                             
	 *                MSB = Most Significant Byte, LSB = Least Significant Byte
	 * }
	 * </pre>
	 * 
	 * <p>When sending or receiving an API2 frame, specific data values must be 
	 * escaped (flagged) so they do not interfere with the data frame sequencing. 
	 * To escape an interfering data byte, the byte {@code 0x7D} is inserted before 
	 * the byte to be escaped XOR'd with {@code 0x20}.</p>
	 * 
	 * <p>The data bytes that need to be escaped:</p>
	 * <ul>
	 * <li>{@code 0x7E} - Frame Delimiter ({@link SpecialByte#HEADER_BYTE})</li>
	 * <li>{@code 0x7D} - Escape ({@link SpecialByte#ESCAPE_BYTE})</li>
	 * <li>{@code 0x11} - XON ({@link SpecialByte#XON_BYTE})</li>
	 * <li>{@code 0x13} - XOFF ({@link SpecialByte#XOFF_BYTE})</li>
	 * </ul>
	 * 
	 * </li>
	 * </ul>
	 * 
	 * <p>The <b>Length</b> field has a two-byte value that specifies the number of 
	 * bytes that will be contained in the frame data field. It does not include the 
	 * checksum field.</p>
	 * 
	 * <p>The <b>frame data</b>  forms an API-specific structure as follows:</p>
	 * 
	 * <pre>
	 * {@code 
	 *   Start Delimiter          Length                   Frame Data                   Checksum
	 *       (Byte 1)            (Bytes 2-3)               (Bytes 4-n)                (Byte n + 1)
	 * +----------------+  +-------------------+  +--------------------------- +  +----------------+
	 * |      0x7E      |  |   MSB   |   LSB   |  |   API-specific Structure   |  |     1 Byte     |
	 * +----------------+  +-------------------+  +----------------------------+  +----------------+
	 *                                            /                                                 \
	 *                                           /  API Identifier        Identifier specific data   \
	 *                                           +------------------+  +------------------------------+
	 *                                           |       cmdID      |  |           cmdData            |
	 *                                           +------------------+  +------------------------------+
	 * }
	 * </pre>
	 * 
	 * <p>The {@code cmdID} frame (API-identifier) indicates which API messages 
	 * will be contained in the {@code cmdData} frame (Identifier-specific data).
	 * </p>
	 * 
	 * <p>To test data integrity, a <b>checksum</b> is calculated and verified on 
	 * non-escaped data.</p>
	 * 
	 * @see APIFrameType
	 * @see XBeePacket
	 * @see com.digi.xbee.api.models.OperatingMode
	 */
	public class XBeePacketParser
	{
		/// <summary>
		/// Parses the bytes from the given <paramref name="inputStream"/> depending on the provided <paramref name="mode"/> and returns the API packet.
		/// </summary>
		/// <remarks>The operating mode must be <code>OperationMode.API</code> or <code>OperationMode.API_ESCAPE</code>.</remarks>
		/// <param name="inputStream">Input stream to read bytes from.</param>
		/// <param name="mode">XBee device operating mode.</param>
		/// <returns>Parsed packet from the input stream.</returns>
		/// <exception cref="ArgumentNullException">if <paramref name="inputStream"/> is null.</exception>
		/// <exception cref="ArgumentException">if <paramref name="mode"/> is invalid or if the <paramref name="inputStream"/> cannot be read.</exception>
		/// <exception cref="InvalidPacketException">if there is not enough data in the stream or if there is an error verifying the checksum or if the payload is invalid for the specified frame type.</exception>
		public XBeePacket ParsePacket(Stream inputStream, OperatingMode mode)
		{
			Contract.Requires<ArgumentNullException>(inputStream != null, "Input stream cannot be null.");
			Contract.Requires<ArgumentException>(inputStream.CanRead);
			Contract.Requires<ArgumentException>(mode == OperatingMode.API || mode == OperatingMode.API_ESCAPE, "Operating mode must be API or API Escaped.");

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

		/**
		 * Parses the bytes from the given array depending on the provided operating
		 * mode and returns the API packet.
		 * 
		 * <p>The operating mode must be {@link OperatingMode#API} or 
		 * {@link OperatingMode#API_ESCAPE}.</p>
		 * 
		 * @param packetByteArray Byte array with the complete frame, starting from 
		 *                        the header and ending in the checksum.
		 * @param mode XBee device operating mode.
		 * 
		 * @return Parsed packet from the given byte array.
		 * 
		 * @throws InvalidPacketException if there is not enough data in the array or 
		 *                                if there is an error verifying the checksum or
		 *                                if the payload is invalid for the specified frame type.
		 * @throws ArgumentException if {@code mode != OperatingMode.API } and
		 *                              if {@code mode != OperatingMode.API_ESCAPE}.
		 * @throws ArgumentNullException if {@code packetByteArray == null} or 
		 *                              if {@code mode == null}.
		 * 
		 * @see XBeePacket
		 * @see com.digi.xbee.api.models.OperatingMode#API
		 * @see com.digi.xbee.api.models.OperatingMode#API_ESCAPE
		 */
		public XBeePacket ParsePacket(byte[] packetByteArray, OperatingMode mode)
		{
			Contract.Requires<ArgumentNullException>(packetByteArray != null, "Packet byte array cannot be null.");
			Contract.Requires<ArgumentNullException>(mode != null, "Operating mode cannot be null.");
			Contract.Requires<ArgumentException>(mode == OperatingMode.API || mode == OperatingMode.API_ESCAPE, "Operating mode must be API or API Escaped.");
			Contract.Requires<InvalidOperationException>(packetByteArray.Length >= 4, "Error parsing packet: Incomplete packet.");

			// Check the header of the frame.
			if ((packetByteArray[0] & 0xFF) != SpecialByte.HEADER_BYTE.GetValue())
				throw new InvalidPacketException("Invalid start delimiter (expected 0x"
							+ HexUtils.ByteToHexString((byte)SpecialByte.HEADER_BYTE.GetValue()) + ").");

			return ParsePacket(new MemoryStream(packetByteArray, 1, packetByteArray.Length - 1), mode);
		}

		/**
		 * Parses the given API payload to get the right API packet, depending 
		 * on its API type ({@code payload[0]}).
		 * 
		 * @param payload The payload of the API frame.
		 * 
		 * @return The corresponding API packet or {@code UnknownXBeePacket} if 
		 *         the frame API type is unknown.
		 *         
		 * @throws InvalidPacketException if the payload is invalid for the 
		 *                                specified frame type.
		 * 
		 * @see APIFrameType
		 * @see XBeePacket
		 */
		private XBeePacket ParsePayload(byte[] payload) /*throws InvalidPacketException*/ {
			// Get the API frame type.
			APIFrameType apiType = APIFrameType.GENERIC.Get(payload[0]);

			if (apiType == null)
				// Create unknown packet.
				return UnknownXBeePacket.CreatePacket(payload);

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
				case APIFrameType.AT_COMMAND:
					packet = ATCommandPacket.CreatePacket(payload);
					break;
				case APIFrameType.AT_COMMAND_QUEUE:
					packet = ATCommandQueuePacket.CreatePacket(payload);
					break;
				case APIFrameType.TRANSMIT_REQUEST:
					packet = TransmitPacket.createPacket(payload);
					break;
				case APIFrameType.REMOTE_AT_COMMAND_REQUEST:
					packet = RemoteATCommandPacket.createPacket(payload);
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
					packet = ATCommandResponsePacket.createPacket(payload);
					break;
				case APIFrameType.TX_STATUS:
					packet = TXStatusPacket.createPacket(payload);
					break;
				case APIFrameType.MODEM_STATUS:
					packet = ModemStatusPacket.CreatePacket(payload);
					break;
				case APIFrameType.TRANSMIT_STATUS:
					packet = TransmitStatusPacket.createPacket(payload);
					break;
				case APIFrameType.RECEIVE_PACKET:
					packet = ReceivePacket.createPacket(payload);
					break;
				case APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR:
					packet = IODataSampleRxIndicatorPacket.CreatePacket(payload);
					break;
				case APIFrameType.REMOTE_AT_COMMAND_RESPONSE:
					packet = RemoteATCommandResponsePacket.createPacket(payload);
					break;
				case APIFrameType.GENERIC:
				default:
					packet = GenericXBeePacket.CreatePacket(payload);
					break;
			}
			return packet;
		}

		/**
		 * Reads one byte from the input stream.
		 * 
		 * <p>This operation checks several things like the working mode in order 
		 * to consider escaped bytes.</p>
		 * 
		 * @param inputStream Input stream to read bytes from.
		 * @param mode XBee device working mode.
		 * 
		 * @return The read byte.
		 * 
		 * @throws InvalidPacketException if there is not enough data in the stream or 
		 *                                if there is an error verifying the checksum.
		 * @throws IOException if the first byte cannot be read for any reason other than end of file, or 
		 *                     if the input stream has been closed, or 
		 *                     if some other I/O error occurs.
		 */
		private int ReadByte(Stream inputStream, OperatingMode mode) /*throws InvalidPacketException, IOException*/ {
			Contract.Requires<ArgumentNullException>(inputStream != null, "Input stream cannot be null.");
			Contract.Requires<ArgumentException>(inputStream.CanRead);

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

		/**
		 * Reads the given amount of bytes from the input stream.
		 * 
		 * <p>This operation checks several things like the working mode in order 
		 * to consider escaped bytes.</p>
		 * 
		 * @param inputStream Input stream to read bytes from.
		 * @param mode XBee device working mode.
		 * @param numBytes Number of bytes to read.
		 * 
		 * @return The read byte array.
		 * 
		 * @throws IOException if the first byte cannot be read for any reason other than end of file, or 
		 *                     if the input stream has been closed, or 
		 *                     if some other I/O error occurs.
		 * @throws InvalidPacketException if there is not enough data in the stream or 
		 *                                if there is an error verifying the checksum.
		 */
		private byte[] ReadBytes(Stream inputStream, OperatingMode mode, int numBytes)/*throws IOException, InvalidPacketException*/ {
			Contract.Requires<ArgumentNullException>(inputStream != null, "Input stream cannot be null.");
			Contract.Requires<ArgumentException>(inputStream.CanRead);

			byte[] data = new byte[numBytes];

			for (int i = 0; i < numBytes; i++)
				data[i] = (byte)ReadByte(inputStream, mode);

			return data;
		}

		/**
		 * Reads a byte from the given input stream.
		 * 
		 * @param inputStream The input stream to read the byte.
		 * @param timeout Timeout to wait for a byte in the input stream 
		 *                in milliseconds.
		 * 
		 * @return The read byte or {@code -1} if the timeout expires or the end
		 *         of the stream is reached.
		 * 
		 * @throws IOException if an I/O errors occurs while reading the byte.
		 */
		private int ReadByteFrom(Stream inputStream, int timeout) /*throws IOException*/ {
			Contract.Requires<ArgumentNullException>(inputStream != null, "Input stream cannot be null.");
			Contract.Requires<ArgumentException>(inputStream.CanRead);

			var stopwatch = Stopwatch.StartNew();

			int b = inputStream.ReadByte();
			// Let's try again if the byte is -1.
			while (b == -1 && stopwatch.ElapsedMilliseconds < timeout)
			{
				b = inputStream.ReadByte();
				try
				{
					Thread.Sleep(10);
				}
				catch (ThreadInterruptedException) { }
			}

			stopwatch.Stop();
			return b;
		}
	}
}