using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Linq;
using Kveer.XBeeApi.Packet;
using Kveer.XBeeApi.Packet.Raw;
using Kveer.XBeeApi.Packet.Common;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// This class represents a queue of XBee packets used for sequential packets reading within the XBee Java API.
	/// </summary>
	/// <remarks>The class provides some methods to get specific packet types from different source nodes.</remarks>
	/// <seealso cref="XBeePacket"/>
	public class XBeePacketsQueue
	{
		/// <summary>
		/// Default maximum number of packets to store in the queue.
		/// </summary>
		public const int DEFAULT_MAX_LENGTH = 50;

		/// <summary>
		/// Gets the maximum size of the XBee packets queue.
		/// </summary>
		public int MaxSize { get; private set; }

		private LinkedList<XBeePacket> packetsList;

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeePacketsQueue"/>.
		/// </summary>
		public XBeePacketsQueue()
			: this(DEFAULT_MAX_LENGTH)
		{
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeePacketsQueue"/> with the specified <paramref name="maxLength"/>.
		/// </summary>
		/// <param name="maxLength">Maximum Length of the queue.</param>
		/// <exception cref="ArgumentException">if <paramref name="maxLength"/> &lte; 1.</exception>
		public XBeePacketsQueue(int maxLength)
		{
			Contract.Requires<ArgumentException>(maxLength > 0, "Queue Length must be greater than 0.");

			this.MaxSize = maxLength;
			packetsList = new LinkedList<XBeePacket>();
		}

		/// <summary>
		/// Adds the provided packet to the list of packets. If the queue is full the first packet will be discarded to add the given one.
		/// </summary>
		/// <param name="xbeePacket">The XBee packet to be added to the list.</param>
		/// <seealso cref="XBeePacket"/>
		public void AddPacket(XBeePacket xbeePacket)
		{
			if (packetsList.Count == MaxSize)
				packetsList.RemoveFirst();

			packetsList.AddLast(xbeePacket);
		}

		/// <summary>
		/// Clears the list of packets.
		/// </summary>
		public void ClearQueue()
		{
			packetsList.Clear();
		}

		private static long CurrentTimeMillis()
		{
			return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
		}

		/**
		 * Returns the first packet from the queue waiting up to the specified 
		 * timeout if  necessary for an XBee packet to become available. 
		 * {@code null }if the queue is empty.
		 * 
		 * @param timeout The time in milliseconds to wait for an XBee packet to 
		 *                become available. 0 to return immediately.
		 * @return The first packet from the queue, {@code null} if it is empty.
		 * 
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		public XBeePacket GetFirstPacket(int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstPacket(0);
				// Wait for a timeout or until an XBee packet is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstPacket(0);
				}
				return xbeePacket;
			}
			else if (packetsList.Count > 0)
			{
				var first = packetsList.First.Value;
				packetsList.RemoveFirst();

				return first;
			}
			return null;
		}

		/**
		 * Returns the first packet from the queue whose 64-bit source address 
		 * matches the address of the provided remote XBee device.
		 * 
		 * <p>The methods waits up to the specified timeout if necessary for an 
		 * XBee packet to become available. Null if the queue is empty or there is 
		 * not any XBee packet sent by the provided remote XBee device.</p>
		 * 
		 * @param remoteXBeeDevice The remote XBee device containing the 64-bit 
		 *                         address to look for in the list of packets.
		 * @param timeout The time in milliseconds to wait for an XBee packet from 
		 *                the specified remote XBee device to become available. 
		 *                0 to return immediately.
		 * 
		 * @return The first XBee packet whose 64-bit address matches the address 
		 *         of the provided remote XBee device. {@code null} if no packets 
		 *         from the specified XBee device are found in the queue.
		 * 
		 * @see com.digi.xbee.api.RemoteXBeeDevice
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		public XBeePacket getFirstPacketFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = getFirstPacketFrom(remoteXBeeDevice, 0);
				// Wait for a timeout or until an XBee packet from remoteXBeeDevice is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = getFirstPacketFrom(remoteXBeeDevice, 0);
				}
				return xbeePacket;
			}
			else
			{
				for (int i = 0; i < packetsList.Count; i++)
				{
					XBeePacket xbeePacket = packetsList.ElementAt(i);
					if (AddressesMatch(xbeePacket, remoteXBeeDevice))
					{
						packetsList.Remove(xbeePacket);
						return xbeePacket;
					}
				}
			}
			return null;
		}

		/**
		 * Returns the first data packet from the queue waiting up to the 
		 * specified timeout if necessary for an XBee data packet to become 
		 * available. {@code null} if the queue is empty or there is not any data 
		 * packet inside.
		 * 
		 * @param timeout The time in milliseconds to wait for an XBee data packet 
		 *                to become available. 0 to return immediately.
		 * 
		 * @return The first data packet from the queue, {@code null} if it is 
		 *         empty or no data packets are contained in the queue.
		 * 
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		public XBeePacket getFirstDataPacket(int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = getFirstDataPacket(0);
				// Wait for a timeout or until a data XBee packet is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = getFirstDataPacket(0);
				}
				return xbeePacket;
			}
			else
			{
				for (int i = 0; i < packetsList.Count; i++)
				{
					XBeePacket xbeePacket = packetsList.ElementAt(i);
					if (IsDataPacket(xbeePacket))
					{
						packetsList.Remove(xbeePacket);
						return xbeePacket;
					}
				}
			}
			return null;
		}

		/**
		 * Returns the first data packet from the queue whose 64-bit source 
		 * address matches the address of the provided remote XBee device.
		 * 
		 * <p>The methods waits up to the specified timeout if necessary for an 
		 * XBee data packet to become available. {@code null} if the queue is 
		 * empty or there is not any XBee data packet sent by the provided remote 
		 * XBee device.</p>
		 * 
		 * @param remoteXBeeDevice The XBee device containing the 64-bit address 
		 *                         to look for in the list of packets.
		 * @param timeout The time in milliseconds to wait for an XBee data packet 
		 *                from the specified remote XBee device to become 
		 *                available. 0 to return immediately.
		 * 
		 * @return The first XBee data packet whose its 64-bit address matches the 
		 *         address of the provided remote XBee device. {@code null} if no 
		 *         data packets from the specified XBee device are found in the 
		 *         queue.
		 * 
		 * @see com.digi.xbee.api.RemoteXBeeDevice
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		public XBeePacket getFirstDataPacketFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = getFirstDataPacketFrom(remoteXBeeDevice, 0);
				// Wait for a timeout or until an XBee packet from remoteXBeeDevice is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = getFirstDataPacketFrom(remoteXBeeDevice, 0);
				}
				return xbeePacket;
			}
			else
			{
				for (int i = 0; i < packetsList.Count; i++)
				{
					XBeePacket xbeePacket = packetsList.ElementAt(i);
					if (IsDataPacket(xbeePacket) && AddressesMatch(xbeePacket, remoteXBeeDevice))
					{
						packetsList.Remove(xbeePacket);
						return xbeePacket;
					}
				}
			}
			return null;
		}

		/**
		 * Returns whether or not the source address of the provided XBee packet 
		 * matches the address of the given remote XBee device.
		 * 
		 * @param xbeePacket The XBee packet to compare its address with the 
		 *                   remote XBee device.
		 * @param remoteXBeeDevice The remote XBee device to compare its address 
		 *                         with the XBee packet.
		 * 
		 * @return {@code true} if the source address of the provided packet (if 
		 *         it has) matches the address of the remote XBee device.
		 * 
		 * @see com.digi.xbee.api.RemoteXBeeDevice
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		private bool AddressesMatch(XBeePacket xbeePacket, RemoteXBeeDevice remoteXBeeDevice)
		{
			if (!(xbeePacket is XBeeAPIPacket))
				return false;
			APIFrameType packetType = ((XBeeAPIPacket)xbeePacket).FrameType;
			switch (packetType)
			{
				case APIFrameType.RECEIVE_PACKET:
					if (remoteXBeeDevice.Get64BitAddress() != null && ((ReceivePacket)xbeePacket).get64bitSourceAddress().Equals(remoteXBeeDevice.Get64BitAddress()))
						return true;
					if (remoteXBeeDevice.Get16BitAddress() != null && ((ReceivePacket)xbeePacket).get16bitSourceAddress().Equals(remoteXBeeDevice.Get16BitAddress()))
						return true;
					break;
				case APIFrameType.REMOTE_AT_COMMAND_RESPONSE:
					if (remoteXBeeDevice.Get64BitAddress() != null && ((RemoteATCommandResponsePacket)xbeePacket).get64bitSourceAddress().Equals(remoteXBeeDevice.Get64BitAddress()))
						return true;
					if (remoteXBeeDevice.Get16BitAddress() != null && ((RemoteATCommandResponsePacket)xbeePacket).get16bitSourceAddress().Equals(remoteXBeeDevice.Get16BitAddress()))
						return true;
					break;
				case APIFrameType.RX_16:
					if (((RX16Packet)xbeePacket).Get16bitSourceAddress().Equals(remoteXBeeDevice.Get16BitAddress()))
						return true;
					break;
				case APIFrameType.RX_64:
					if (((RX64Packet)xbeePacket).SourceAddress64.Equals(remoteXBeeDevice.Get64BitAddress()))
						return true;
					break;
				case APIFrameType.RX_IO_16:
					if (((RX16IOPacket)xbeePacket).get16bitSourceAddress().Equals(remoteXBeeDevice.Get16BitAddress()))
						return true;
					break;
				case APIFrameType.RX_IO_64:
					if (((RX64IOPacket)xbeePacket).SourceAddress64.Equals(remoteXBeeDevice.Get64BitAddress()))
						return true;
					break;
				default:
					return false;
			}
			return false;
		}

		/**
		 * Returns whether or not the given XBee packet is a data packet.
		 * 
		 * @param xbeePacket The XBee packet to check if is data packet.
		 * 
		 * @return {@code true} if the XBee packet is a data packet, {@code false} 
		 *         otherwise.
		 * 
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		private bool IsDataPacket(XBeePacket xbeePacket)
		{
			if (!(xbeePacket is XBeeAPIPacket))
				return false;
			APIFrameType packetType = ((XBeeAPIPacket)xbeePacket).FrameType;
			switch (packetType)
			{
				case APIFrameType.RECEIVE_PACKET:
				case APIFrameType.RX_16:
				case APIFrameType.RX_64:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Sleeps the thread for the given number of milliseconds.
		/// </summary>
		/// <param name="milliseconds">The number of milliseconds that the thread should be sleeping.</param>
		private void Sleep(int milliseconds)
		{
			try
			{
				Thread.Sleep(milliseconds);
			}
			catch (ThreadInterruptedException ) { }
		}

		/// <summary>
		/// Gets the current size of the XBee packets queue.
		/// </summary>
		/// <returns>The current size of the XBee packets queue.</returns>
		public int CurrentSize
		{
			get
			{
				return packetsList.Count;
			}
		}
	}
}