/*
 * Copyright 2019, 2020, Digi International Inc.
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.Common;
using XBeeLibrary.Core.Packet.IP;
using XBeeLibrary.Core.Packet.Raw;
using XBeeLibrary.Core.Packet.Relay;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents a queue of XBee packets used for sequential packet reading.
	/// </summary>
	/// <remarks>The class provides some methods to get specific packet types from different source 
	/// nodes.</remarks>
	/// <seealso cref="XBeePacket"/>
	public class XBeePacketsQueue
	{
		// Constants.
		/// <summary>
		/// Default maximum number of packets to store in the queue.
		/// </summary>
		public const int DEFAULT_MAX_LENGTH = 50;

		// Variables.
		private LinkedList<XBeePacket> packetsList;

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeePacketsQueue"/>.
		/// </summary>
		public XBeePacketsQueue() : this(DEFAULT_MAX_LENGTH) { }

		/// <summary>
		/// Initializes a new instance of class <see cref="XBeePacketsQueue"/> with the specified 
		/// <paramref name="maxLength"/>.
		/// </summary>
		/// <param name="maxLength">Maximum Length of the queue.</param>
		/// <exception cref="ArgumentException">If <paramref name="maxLength"/> <![CDATA[<]]> 1.</exception>
		public XBeePacketsQueue(int maxLength)
		{
			if (maxLength < 1)
				throw new ArgumentException("Queue Length must be greater than 0.");

			MaxSize = maxLength;
			packetsList = new LinkedList<XBeePacket>();
		}

		// Properties.
		/// <summary>
		/// Gets the maximum size of the XBee packets queue.
		/// </summary>
		public int MaxSize { get; private set; }

		/// <summary>
		/// Adds the provided packet to the list of packets. If the queue is full 
		/// the first packet will be discarded to add the given one.
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

		/// <summary>
		/// Returns the current time in milliseconds since 01/01/1970.
		/// </summary>
		/// <returns>The current time in milliseconds since 01/01/1970.</returns>
		private static long CurrentTimeMillis()
		{
			return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
		}

		/// <summary>
		/// Returns the first packet from the queue waiting up to the specified timeout if necessary 
		/// for an XBee packet to become available. Or <c>null</c> if the queue is empty.
		/// </summary>
		/// <param name="timeout">The time in milliseconds to wait for an XBee packet to become 
		/// available. 0 to return immediately.</param>
		/// <returns>The first packet from the queue, <c>null</c> if it is empty.</returns>
		/// <seealso cref="XBeePacket"/>
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

		/// <summary>
		/// Returns the first packet from the queue whose 64-bit source address matches the address 
		/// of the provided remote XBee device.
		/// </summary>
		/// <remarks>The methods waits up to the specified timeout if necessary for an XBee packet to 
		/// become available. <c>null</c> if the queue is empty or there is not any XBee packet sent by the 
		/// provided remote XBee device.</remarks>
		/// <param name="remoteXBeeDevice">The remote XBee device containing the 4-bit address to look 
		/// for in the list of packets.</param>
		/// <param name="timeout">The time in milliseconds to wait for an XBee packet from the specified 
		/// remote XBee device to become available. 0 to return immediately.</param>
		/// <returns>The first XBee packet whose 64-bit address matches the address of the provided remote 
		/// XBee device. <c>null</c> if no packets from the specified XBee device are found in the queue.</returns>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="XBeePacket"/>
		public XBeePacket GetFirstPacketFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstPacketFrom(remoteXBeeDevice, 0);
				// Wait for a timeout or until an XBee packet from remoteXBeeDevice is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstPacketFrom(remoteXBeeDevice, 0);
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

		/// <summary>
		/// Returns the first data packet from the queue waiting up to the specified timeout if necessary 
		/// for an XBee data packet to become available. <c>null</c> if the queue is empty or there is not 
		/// any data packet inside.
		/// </summary>
		/// <param name="timeout">The time in milliseconds to wait for an XBee data packet to become 
		/// available. 0 to return immediately.</param>
		/// <returns>The first data packet from the queue, <c>null</c> if it is empty or no data packets 
		/// are contained in the queue.</returns>
		/// <seealso cref="XBeePacket"/>
		public XBeePacket GetFirstDataPacket(int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstDataPacket(0);
				// Wait for a timeout or until a data XBee packet is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstDataPacket(0);
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

		/// <summary>
		/// Returns the first data packet from the queue whose 64-bit source address matches the address 
		/// of the provided remote XBee device.
		/// </summary>
		/// <remarks>The methods waits up to the specified timeout if necessary for an XBee data packet to 
		/// become available. <c>null</c> if the queue is empty or there is not any XBee data packet sent 
		/// by the provided remote XBee device.</remarks>
		/// <param name="remoteXBeeDevice">The XBee device containing the 64-bit address to look for in 
		/// the list of packets.</param>
		/// <param name="timeout">The time in milliseconds to wait for an XBee data packet from the 
		/// specified remote XBee device to become available. 0 to return immediately.</param>
		/// <returns>The first XBee data packet whose its 64-bit address matches the address of the 
		/// provided remote XBee device. <c>null</c> if no data packets from the specified XBee device are 
		/// found in the queue.</returns>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="XBeePacket"/>
		public XBeePacket GetFirstDataPacketFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstDataPacketFrom(remoteXBeeDevice, 0);
				// Wait for a timeout or until an XBee packet from remoteXBeeDevice is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstDataPacketFrom(remoteXBeeDevice, 0);
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

		/// <summary>
		/// Returns the first explicit data packet from the queue waiting up to the specified timeout if 
		/// necessary for an XBee explicit data packet to become available. <c>null</c> if the queue is 
		/// empty or there is not any explicit data packet inside.
		/// </summary>
		/// <param name="timeout">The time in milliseconds to wait for an XBee explicit data packet to 
		/// become available. 0 to return immediately.</param>
		/// <returns>The first explicit data packet from the queue, <c>null</c> if it is empty or no data 
		/// packets are contained in the queue.</returns>
		/// <seealso cref="XBeePacket"/>
		/// <seealso cref="ExplicitRxIndicatorPacket"/>
		public XBeePacket GetFirstExplicitDataPacket(int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstExplicitDataPacket(0);
				// Wait for a timeout or until an explicit data XBee packet is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstExplicitDataPacket(0);
				}
				return xbeePacket;
			}
			else
			{
				for (int i = 0; i < packetsList.Count; i++)
				{
					XBeePacket xbeePacket = packetsList.ElementAt(i);
					if (IsExplicitDataPacket(xbeePacket))
					{
						packetsList.Remove(xbeePacket);
						return xbeePacket;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the first explicit data packet from the queue whose 64-bit source address matches the 
		/// address of the provided remote XBee device.
		/// </summary>
		/// <remarks>The methods waits up to the specified timeout if necessary for an XBee explicit data 
		/// packet to become available. <c>null</c> if the queue is empty or there is not any XBee explicit 
		/// data packet sent by the provided remote XBee device.</remarks>
		/// <param name="remoteXBeeDevice">The XBee device containing the 64-bit address to look for in the 
		/// list of packets.</param>
		/// <param name="timeout">The time in milliseconds to wait for an XBee explicit data packet from 
		/// the specified remote XBee device to become available. 0 to return immediately.</param>
		/// <returns>The first XBee explicit data packet whose its 64-bit address matches the address of the 
		/// provided remote XBee device. <c>null</c> if no explicit data packets from the specified XBee 
		/// device are found in the queue.</returns>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="XBeePacket"/>
		/// <seealso cref="ExplicitRxIndicatorPacket"/>
		public XBeePacket GetFirstExplicitDataPacketFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstExplicitDataPacketFrom(remoteXBeeDevice, 0);
				// Wait for a timeout or until an XBee explicit data packet 
				// from remoteXBeeDevice is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstExplicitDataPacketFrom(remoteXBeeDevice, 0);
				}
				return xbeePacket;
			}
			else
			{
				for (int i = 0; i < packetsList.Count; i++)
				{
					XBeePacket xbeePacket = packetsList.ElementAt(i);
					if (IsExplicitDataPacket(xbeePacket) && AddressesMatch(xbeePacket, remoteXBeeDevice))
					{
						packetsList.Remove(xbeePacket);
						return xbeePacket;
					}
				}
			}
			return null;
		}
		
		/// <summary>
		/// Returns the first User Data Relay packet from the queue waiting up to the specified timeout if 
		/// necessary for the packet to become available. <c>null</c> if the queue is empty or there is not 
		/// any User Data Relay packet inside.
		/// </summary> 
		/// <param name="timeout">The time in milliseconds to wait for a User Data Relay packet to become 
		/// available. 0 to return immediately.
		/// </param>
		/// <returns>The first User Data Relay packet from the queue, <c>null</c> if it is empty or no data 
		/// packets are contained in the queue.</returns>
		/// <seealso cref="XBeePacket"/>
		/// <seealso cref="UserDataRelayOutputPacket"/>
		public XBeePacket GetFirstUserDataRelayPacket(int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstUserDataRelayPacket(0);
				// Wait for a timeout or until a User Data Relay packet is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstUserDataRelayPacket(0);
				}
				return xbeePacket;
			}
			else
			{
				for (int i = 0; i < packetsList.Count; i++)
				{
					XBeePacket xbeePacket = packetsList.ElementAt(i);
					if (IsUserDataRelayPacket(xbeePacket))
					{
						packetsList.Remove(xbeePacket);
						return xbeePacket;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the first IP data packet from the queue waiting up to the specified timeout if necessary 
		/// for an IP data packet to become available. <c>null</c> if the queue is empty or there is not any 
		/// IP data packet inside.
		/// </summary>
		/// <param name="timeout">The time in milliseconds to wait for an IP data packet to become available. 
		/// 0 to return immediately.</param>
		/// <returns>The first IP data packet from the queue, <c>null</c> if it is empty or no IP packets are 
		/// contained in the queue.</returns>
		/// <seealso cref="XBeePacket"/>
		public XBeePacket GetFirstIPDataPacket(int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstIPDataPacket(0);
				// Wait for a timeout or until a IP data packet is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstIPDataPacket(0);
				}
				return xbeePacket;
			}
			else
			{
				for (int i = 0; i < packetsList.Count; i++)
				{
					XBeePacket xbeePacket = packetsList.ElementAt(i);
					if (IsIPDataPacket(xbeePacket))
					{
						packetsList.Remove(xbeePacket);
						return xbeePacket;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the first IP data packet from the queue whose IP address matches the provided address.
		/// </summary>
		/// <param name="ipAddress">The IP address to look for in the list of packets.</param>
		/// <param name="timeout">The time in milliseconds to wait for an IP data packet to become 
		/// available. 0 to return immediately.</param>
		/// <returns>The first IP packet whose IP address matches the provided IP address. <c>null</c> if 
		/// no IP data packets from the specified IP address are found in the queue.</returns>
		/// <seealso cref="XBeePacket"/>
		/// <seealso cref="IPAddress"/>
		public XBeePacket GetFirstIPDataPacketFrom(IPAddress ipAddress, int timeout)
		{
			if (timeout > 0)
			{
				XBeePacket xbeePacket = GetFirstIPDataPacketFrom(ipAddress, 0);
				// Wait for a timeout or until a IP data packet with the 
				// provided IP address is read.
				long deadLine = CurrentTimeMillis() + timeout;
				while (xbeePacket == null && deadLine > CurrentTimeMillis())
				{
					Sleep(100);
					xbeePacket = GetFirstIPDataPacketFrom(ipAddress, 0);
				}
				return xbeePacket;
			}
			else
			{
				for (int i = 0; i < packetsList.Count; i++)
				{
					XBeePacket xbeePacket = packetsList.ElementAt(i);
					if (IsIPDataPacket(xbeePacket) && IPAddressesMatch(xbeePacket, ipAddress))
					{
						packetsList.Remove(xbeePacket);
						return xbeePacket;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Returns whether or not the source address of the provided XBee packet matches the address 
		/// of the given remote XBee device.
		/// </summary>
		/// <param name="xbeePacket">The XBee packet to compare its address with the remote XBee 
		/// device.</param>
		/// <param name="remoteXBeeDevice">The remote XBee device to compare its address with the XBee 
		/// packet.</param>
		/// <returns><c>true</c> if the source address of the provided packet (if it has) matches the 
		/// address of the remote XBee device.</returns>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="XBeePacket"/>
		private bool AddressesMatch(XBeePacket xbeePacket, RemoteXBeeDevice remoteXBeeDevice)
		{
			if (!(xbeePacket is XBeeAPIPacket))
				return false;
			APIFrameType packetType = ((XBeeAPIPacket)xbeePacket).FrameType;
			switch (packetType)
			{
				case APIFrameType.RECEIVE_PACKET:
					if (remoteXBeeDevice.XBee64BitAddr != null 
						&& ((ReceivePacket)xbeePacket).SourceAddress64.Equals(remoteXBeeDevice.XBee64BitAddr))
						return true;
					if (remoteXBeeDevice.XBee16BitAddr != null
						&& !remoteXBeeDevice.XBee16BitAddr.Equals(XBee16BitAddress.UNKNOWN_ADDRESS)
						&& ((ReceivePacket)xbeePacket).SourceAddress16.Equals(remoteXBeeDevice.XBee16BitAddr))
						return true;
					break;
				case APIFrameType.REMOTE_AT_COMMAND_RESPONSE:
					if (remoteXBeeDevice.XBee64BitAddr != null 
						&& ((RemoteATCommandResponsePacket)xbeePacket).SourceAddress64.Equals(remoteXBeeDevice.XBee64BitAddr))
						return true;
					if (remoteXBeeDevice.XBee16BitAddr != null
						&& !remoteXBeeDevice.XBee16BitAddr.Equals(XBee16BitAddress.UNKNOWN_ADDRESS)
						&& ((RemoteATCommandResponsePacket)xbeePacket).SourceAddress16.Equals(remoteXBeeDevice.XBee16BitAddr))
						return true;
					break;
				case APIFrameType.RX_16:
					if (((RX16Packet)xbeePacket).SourceAddress16.Equals(remoteXBeeDevice.XBee16BitAddr))
						return true;
					break;
				case APIFrameType.RX_64:
					if (((RX64Packet)xbeePacket).SourceAddress64.Equals(remoteXBeeDevice.XBee64BitAddr))
						return true;
					break;
				case APIFrameType.RX_IO_16:
					if (((RX16IOPacket)xbeePacket).SourceAddress16.Equals(remoteXBeeDevice.XBee16BitAddr))
						return true;
					break;
				case APIFrameType.RX_IO_64:
					if (((RX64IOPacket)xbeePacket).SourceAddress64.Equals(remoteXBeeDevice.XBee64BitAddr))
						return true;
					break;
				case APIFrameType.EXPLICIT_RX_INDICATOR:
					if (((ExplicitRxIndicatorPacket)xbeePacket).SourceAddress64.Equals(remoteXBeeDevice.XBee64BitAddr))
						return true;
					break;
				default:
					return false;
			}
			return false;
		}

		/// <summary>
		/// Returns whether or not the IP address of the XBee packet matches the provided one.
		/// </summary>
		/// <param name="xbeePacket">The XBee packet to compare its IP address with the provided 
		/// one.</param>
		/// <param name="ipAddress">The IP address to be compared with the XBee packet's one.</param>
		/// <returns><c>true</c> if the IP address of the XBee packet (if it has) matches the 
		/// provided one. <c>false</c> otherwise.</returns>
		private bool IPAddressesMatch(XBeePacket xbeePacket, IPAddress ipAddress)
		{
			if (xbeePacket == null || ipAddress == null || !(xbeePacket is XBeeAPIPacket))
				return false;
			APIFrameType packetType = (xbeePacket as XBeeAPIPacket).FrameType;
			switch (packetType)
			{
				case APIFrameType.RX_IPV4:
					if ((xbeePacket as RXIPv4Packet).SourceAddress.Equals(ipAddress))
						return true;
					break;
				default:
					return false;
			}
			return false;
		}

		/// <summary>
		/// Returns whether or not the given XBee packet is a data packet.
		/// </summary>
		/// <param name="xbeePacket">The XBee packet to check if is data packet.</param>
		/// <returns><c>true</c> if the XBee packet is a data packet, <c>false</c> otherwise.</returns>
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
		/// Returns whether or not the given XBee packet is an explicit data packet.
		/// </summary>
		/// <param name="xbeePacket">The XBee packet to check if is an explicit data packet.</param>
		/// <returns><c>true</c> if the XBee packet is an explicit data packet, <c>false</c> 
		/// otherwise.</returns>
		/// <seealso cref="XBeePacket"/>
		/// <seealso cref="ExplicitRxIndicatorPacket"/>
		private bool IsExplicitDataPacket(XBeePacket xbeePacket)
		{
			if (!(xbeePacket is XBeeAPIPacket))
				return false;
			APIFrameType packetType = ((XBeeAPIPacket)xbeePacket).FrameType;
			return packetType == APIFrameType.EXPLICIT_RX_INDICATOR;
		}

		/// <summary>
		/// Returns whether the given XBee packet is a User Data Relay packet or not.
		/// </summary>
		/// <param name="xbeePacket">The XBee packet to check if is a User Data Relay packet.</param>
		/// <returns><c>true</c> if the XBee packet is an User Data Relay packet, <c>false</c> 
		/// otherwise.</returns>
		/// <seealso cref="XBeePacket"/>
		/// <seealso cref="UserDataRelayOutputPacket"/>
		private bool IsUserDataRelayPacket(XBeePacket xbeePacket)
		{
			if (!(xbeePacket is XBeeAPIPacket))
				return false;
			APIFrameType packetType = ((XBeeAPIPacket)xbeePacket).FrameType;
			return packetType == APIFrameType.USER_DATA_RELAY_OUTPUT;
		}
		
		/// <summary>
		/// Returns whether or not the given XBee packet is an IP data packet.
		/// </summary>
		/// <param name="xbeePacket">The XBee packet to check if is an IP data packet.</param>
		/// <returns><c>true</c> if the XBee packet is an IP data packet, <c>false</c> 
		/// otherwise.</returns>
		private bool IsIPDataPacket(XBeePacket xbeePacket)
		{
			if (!(xbeePacket is XBeeAPIPacket))
				return false;
			APIFrameType packetType = ((XBeeAPIPacket)xbeePacket).FrameType;
			return packetType == APIFrameType.RX_IPV4;
		}

		/// <summary>
		/// Sleeps the thread for the given number of milliseconds.
		/// </summary>
		/// <param name="milliseconds">The number of milliseconds that the thread should be 
		/// sleeping.</param>
		private void Sleep(int milliseconds)
		{
			Task.Delay(milliseconds).Wait();
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