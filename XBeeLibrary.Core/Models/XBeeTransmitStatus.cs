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
using System.Linq;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates the different transmit status. Transmit status field is part of the 
	/// <see cref="Packet.Common.TransmitStatusPacket"/> indicating the status of the transmission.
	/// </summary>
	public enum XBeeTransmitStatus : byte
	{
		// Enumeration entries.
		SUCCESS = 0x00,
		NO_ACK = 0x01,
		CCA_FAILURE = 0x02,
		PURGED = 0x03,
		WIFI_PHYSICAL_ERROR = 0x04,
		INVALID_DESTINATION = 0x15,
		NO_BUFFERS = 0x18,
		NETWORK_ACK_FAILURE = 0x21,
		NOT_JOINED_NETWORK = 0x22,
		SELF_ADDRESSED = 0x23,
		ADDRESS_NOT_FOUND = 0x24,
		ROUTE_NOT_FOUND = 0x25,
		BROADCAST_FAILED = 0x26,
		INVALID_BINDING_TABLE_INDEX = 0x2B,
		INVALID_ENDPOINT = 0x2C,
		BROADCAST_ERROR_APS = 0x2D,
		BROADCAST_ERROR_APS_EE0 = 0x2E,
		SOFTWARE_ERROR = 0x31,
		RESOURCE_ERROR = 0x32,
		PAYLOAD_TOO_LARGE = 0x74,
		INDIRECT_MESSAGE_UNREQUESTED = 0x75,
		SOCKET_CREATION_FAILED = 0x76,
		IP_PORT_NOT_EXIST = 0x77,
		INVALID_UDP_PORT = 0x78,
		INVALID_TCP_PORT = 0x79,
		INVALID_HOST = 0x7A,
		INVALID_DATA_MODE = 0x7B,
		INVALID_INTERFACE = 0x7C,
		NOT_ACCEPT_FRAMES = 0x7D,
		CONNECTION_REFUSED = 0x80,
		CONNECTION_LOST = 0x81,
		NO_SERVER = 0x82,
		SOCKET_CLOSED = 0x83,
		UNKNOWN_SERVER = 0x84,
		UNKNOWN_ERROR = 0x85,
		KEY_NOT_AUTHORIZED = 0xBB,
		UNKNOWN = 0xFF
	}

	public static class XBeeTransmitStatusExtensions
	{
		static IDictionary<XBeeTransmitStatus, string> lookupTable = new Dictionary<XBeeTransmitStatus, string>();

		static XBeeTransmitStatusExtensions()
		{
			lookupTable.Add(XBeeTransmitStatus.SUCCESS, "Success");
			lookupTable.Add(XBeeTransmitStatus.NO_ACK, "No acknowledgement received");
			lookupTable.Add(XBeeTransmitStatus.CCA_FAILURE, "CCA failure");
			lookupTable.Add(XBeeTransmitStatus.PURGED, "Transmission purged, it was attempted before stack lookupTable.Add(XBeeTransmitStatus.was up");
			lookupTable.Add(XBeeTransmitStatus.WIFI_PHYSICAL_ERROR, "Physical error occurred on the interface with the WiFi transceiver");
			lookupTable.Add(XBeeTransmitStatus.INVALID_DESTINATION, "Invalid destination endpoint");
			lookupTable.Add(XBeeTransmitStatus.NO_BUFFERS, "No buffers");
			lookupTable.Add(XBeeTransmitStatus.NETWORK_ACK_FAILURE, "Network ACK Failure");
			lookupTable.Add(XBeeTransmitStatus.NOT_JOINED_NETWORK, "Not joined to network");
			lookupTable.Add(XBeeTransmitStatus.SELF_ADDRESSED, "Self-addressed");
			lookupTable.Add(XBeeTransmitStatus.ADDRESS_NOT_FOUND, "Address not found");
			lookupTable.Add(XBeeTransmitStatus.ROUTE_NOT_FOUND, "Route not found");
			lookupTable.Add(XBeeTransmitStatus.BROADCAST_FAILED, "Broadcast source failed to hear a neighbor relay the message");
			lookupTable.Add(XBeeTransmitStatus.INVALID_BINDING_TABLE_INDEX, "Invalid binding table index");
			lookupTable.Add(XBeeTransmitStatus.INVALID_ENDPOINT, "Invalid endpoint");
			lookupTable.Add(XBeeTransmitStatus.BROADCAST_ERROR_APS, "Attempted broadcast with APS transmission");
			lookupTable.Add(XBeeTransmitStatus.BROADCAST_ERROR_APS_EE0, "Attempted broadcast with APS transmission, but EE=0");
			lookupTable.Add(XBeeTransmitStatus.SOFTWARE_ERROR, "A software error occurred");
			lookupTable.Add(XBeeTransmitStatus.RESOURCE_ERROR, "Resource error lack of free buffers, timers, etc.");
			lookupTable.Add(XBeeTransmitStatus.PAYLOAD_TOO_LARGE, "Data payload too large");
			lookupTable.Add(XBeeTransmitStatus.INDIRECT_MESSAGE_UNREQUESTED, "Indirect message unrequested");
			lookupTable.Add(XBeeTransmitStatus.SOCKET_CREATION_FAILED, "Attempt to create a client socket failed");
			lookupTable.Add(XBeeTransmitStatus.IP_PORT_NOT_EXIST, "TCP connection to given IP address and port doesn't exist. Source port is non-zero so that a new connection is not attempted");
			lookupTable.Add(XBeeTransmitStatus.INVALID_UDP_PORT, "Invalid UDP port");
			lookupTable.Add(XBeeTransmitStatus.INVALID_TCP_PORT, "Invalid TCP port");
			lookupTable.Add(XBeeTransmitStatus.INVALID_HOST, "Invalid host");
			lookupTable.Add(XBeeTransmitStatus.INVALID_DATA_MODE, "Invalid data mode");
			lookupTable.Add(XBeeTransmitStatus.INVALID_INTERFACE, "Invalid interface");
			lookupTable.Add(XBeeTransmitStatus.NOT_ACCEPT_FRAMES, "Interface not accepting frames");
			lookupTable.Add(XBeeTransmitStatus.CONNECTION_REFUSED, "Connection refused");
			lookupTable.Add(XBeeTransmitStatus.CONNECTION_LOST, "Connection lost");
			lookupTable.Add(XBeeTransmitStatus.NO_SERVER, "No server");
			lookupTable.Add(XBeeTransmitStatus.SOCKET_CLOSED, "Socket closed");
			lookupTable.Add(XBeeTransmitStatus.UNKNOWN_SERVER, "Unknown server");
			lookupTable.Add(XBeeTransmitStatus.UNKNOWN_ERROR, "Unknown error");
			lookupTable.Add(XBeeTransmitStatus.KEY_NOT_AUTHORIZED, "Key not authorized");
			lookupTable.Add(XBeeTransmitStatus.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the XBee transmit status ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>XBee transmit status ID.</returns>
		public static byte GetId(this XBeeTransmitStatus source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the XBee transmit status description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>XBee transmit status description.</returns>
		public static string GetDescription(this XBeeTransmitStatus source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Returns the <see cref="XBeeTransmitStatus"/> associated to the given ID.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="id">The ID of the <see cref="XBeeTransmitStatus"/> to retrieve.</param>
		/// <returns>The <see cref="XBeeTransmitStatus"/> associated to the given ID,
		/// <see cref="XBeeTransmitStatus.UNKNOWN"/> if it does not exist.</returns>
		public static XBeeTransmitStatus Get(this XBeeTransmitStatus source, byte id)
		{
			var values = Enum.GetValues(typeof(XBeeTransmitStatus)).OfType<XBeeTransmitStatus>();

			if (values.Cast<byte>().Contains(id))
				return (XBeeTransmitStatus)id;

			return XBeeTransmitStatus.UNKNOWN;
		}

		/// <summary>
		/// Returns the <see cref="XBeeTransmitStatus"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="XBeeTransmitStatus"/> in string format.</returns>
		public static string ToDisplayString(this XBeeTransmitStatus source)
		{
			if (source != XBeeTransmitStatus.SUCCESS)
				return "Error: " + lookupTable[source];
			else
				return lookupTable[source];
		}
	}
}
