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
	/// Enumerates all the possible states of the discovery. Discovery status field is part of the 
	/// <see cref="Packet.Common.TransmitStatusPacket"/> indicating the status of the discovery when a 
	/// packet is sent.
	/// </summary>
	/// <seealso cref="Packet.Common.TransmitStatusPacket"/>
	public enum XBeeDiscoveryStatus : byte
	{
		// Enumeration entries.
		DISCOVERY_STATUS_NO_DISCOVERY_OVERHEAD = 0x00,
		DISCOVERY_STATUS_ADDRESS_DISCOVERY = 0x01,
		DISCOVERY_STATUS_ROUTE_DISCOVERY = 0x02,
		DISCOVERY_STATUS_ADDRESS_AND_ROUTE = 0x03,
		DISCOVERY_STATUS_EXTENDED_TIMEOUT_DISCOVERY = 0x40,
		DISCOVERY_STATUS_UNKNOWN = 0xFF,
	}

	public static class XBeeDiscoveryStatusExtensions
	{
		static IDictionary<XBeeDiscoveryStatus, string> lookupTable = new Dictionary<XBeeDiscoveryStatus, string>();

		static XBeeDiscoveryStatusExtensions()
		{
			lookupTable.Add(XBeeDiscoveryStatus.DISCOVERY_STATUS_NO_DISCOVERY_OVERHEAD, "No discovery overhead");
			lookupTable.Add(XBeeDiscoveryStatus.DISCOVERY_STATUS_ADDRESS_DISCOVERY, "Address discovery");
			lookupTable.Add(XBeeDiscoveryStatus.DISCOVERY_STATUS_ROUTE_DISCOVERY, "Route discovery");
			lookupTable.Add(XBeeDiscoveryStatus.DISCOVERY_STATUS_ADDRESS_AND_ROUTE, "Address and route");
			lookupTable.Add(XBeeDiscoveryStatus.DISCOVERY_STATUS_EXTENDED_TIMEOUT_DISCOVERY, "Extended timeout discovery");
			lookupTable.Add(XBeeDiscoveryStatus.DISCOVERY_STATUS_UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the discovery status ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The discovery status ID.</returns>
		public static byte GetId(this XBeeDiscoveryStatus source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the discovery status description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>Discovery status description.</returns>
		public static string GetDescription(this XBeeDiscoveryStatus source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gest the <see cref="XBeeDiscoveryStatus"/> associated to the given ID.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="id">ID of the <see cref="XBeeDiscoveryStatus"/> to retrieve.</param>
		/// <returns>The <see cref="XBeeDiscoveryStatus"/> associated with the given ID, 
		/// <see cref="XBeeDiscoveryStatus.DISCOVERY_STATUS_UNKNOWN"/> if it does not exist.</returns>
		public static XBeeDiscoveryStatus Get(this XBeeDiscoveryStatus source, byte id)
		{
			var values = Enum.GetValues(typeof(XBeeDiscoveryStatus)).OfType<byte>();

			if (values.Cast<byte>().Contains(id))
				return (XBeeDiscoveryStatus)id;

			return XBeeDiscoveryStatus.DISCOVERY_STATUS_UNKNOWN;
		}

		/// <summary>
		/// Returns the <see cref="XBeeDiscoveryStatus"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="XBeeDiscoveryStatus"/> in string format.</returns>
		public static string ToDisplayString(this XBeeDiscoveryStatus source)
		{
			return lookupTable[source];
		}
	}
}
