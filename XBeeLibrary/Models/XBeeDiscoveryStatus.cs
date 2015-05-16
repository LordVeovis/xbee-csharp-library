using System;
using System.Collections.Generic;
using System.Linq;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates all the possible states of the discovery. Discovery status field is part of the <see cref="TransmitStatusPacket"/> indicating the status of the discovery when a packet is sent.
	/// </summary>
	/// <seealso cref="TransmitStatusPacket"/>
	public enum XBeeDiscoveryStatus : byte
	{
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
		/// <param name="id">ID of the <see cref="XBeeDiscoveryStatus"/> to retrieve.</param>
		/// <returns>The <see cref="XBeeDiscoveryStatus"/> associated with the given ID.</returns>
		public static XBeeDiscoveryStatus Get(this XBeeDiscoveryStatus dumb, byte id)
		{
			var values = Enum.GetValues(typeof(XBeeDiscoveryStatus)).OfType<byte>();

			if (values.Cast<byte>().Contains(id))
				return (XBeeDiscoveryStatus)id;

			return XBeeDiscoveryStatus.DISCOVERY_STATUS_UNKNOWN;
		}

		public static string ToDisplayString(this XBeeDiscoveryStatus source)
		{
			return lookupTable[source];
		}
	}
}
