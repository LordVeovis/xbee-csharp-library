using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates the different transmit status. Transmit status field is part of the <see cref="TransmitStatusPacket"/> indicating the status of the transmission.
	/// </summary>
	public enum XBeeTransmitStatus : byte
	{
		SUCCESS = 0x00,
		NO_ACK = 0x01,
		CCA_FAILURE = 0x02,
		PURGED = 0x03,
		WIFI_PHYSICAL_ERROR = 0x04,
		INVALID_DESTINATION = 0x15,
		NETWORK_ACK_FAILURE = 0x21,
		NOT_JOINED_NETWORK = 0x22,
		SELF_ADDRESSED = 0x23,
		ADDRESS_NOT_FOUND = 0x24,
		ROUTE_NOT_FOUND = 0x25,
		BROADCAST_FAILED = 0x26,
		INVALID_BINDING_TABLE_INDEX = 0x2B,
		RESOURCE_ERROR = 0x2C,
		BROADCAST_ERROR_APS = 0x2D,
		BROADCAST_ERROR_APS_EE0 = 0x2E,
		RESOURCE_ERROR_BIS = 0x32,
		PAYLOAD_TOO_LARGE = 0x74,
		SOCKET_CREATION_FAILED = 0x76,
		INDIRECT_MESSAGE_UNREUESTED = 0x75,
		UNKNOWN = 0xff
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
			lookupTable.Add(XBeeTransmitStatus.NETWORK_ACK_FAILURE, "Network ACK Failure");
			lookupTable.Add(XBeeTransmitStatus.NOT_JOINED_NETWORK, "Not joined to network");
			lookupTable.Add(XBeeTransmitStatus.SELF_ADDRESSED, "Self-addressed");
			lookupTable.Add(XBeeTransmitStatus.ADDRESS_NOT_FOUND, "Address not found");
			lookupTable.Add(XBeeTransmitStatus.ROUTE_NOT_FOUND, "Route not found");
			lookupTable.Add(XBeeTransmitStatus.BROADCAST_FAILED, "Broadcast source failed to hear a neighbor relay the message");
			lookupTable.Add(XBeeTransmitStatus.INVALID_BINDING_TABLE_INDEX, "Invalid binding table index");
			lookupTable.Add(XBeeTransmitStatus.RESOURCE_ERROR, "Resource error lack of free buffers, timers, etc.");
			lookupTable.Add(XBeeTransmitStatus.BROADCAST_ERROR_APS, "Attempted broadcast with APS transmission");
			lookupTable.Add(XBeeTransmitStatus.BROADCAST_ERROR_APS_EE0, "Attempted broadcast with APS transmission, but EE=0");
			lookupTable.Add(XBeeTransmitStatus.RESOURCE_ERROR_BIS, "Resource error lack of free buffers, timers, etc.");
			lookupTable.Add(XBeeTransmitStatus.PAYLOAD_TOO_LARGE, "Data payload too large");
			lookupTable.Add(XBeeTransmitStatus.SOCKET_CREATION_FAILED, "Attempt to create a client socket failed");
			lookupTable.Add(XBeeTransmitStatus.INDIRECT_MESSAGE_UNREUESTED, "Indirect message unrequested");
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

		/**
		 * Returns the {@code XBeeTransmitStatus} associated to the given ID.
		 * 
		 * @param id ID of the {@code XBeeTransmitStatus} to retrieve.
		 * 
		 * @return The {@code XBeeTransmitStatus} associated to the given ID.
		 */
		public static XBeeTransmitStatus Get(this XBeeTransmitStatus dumb, byte id)
		{
			return (XBeeTransmitStatus)id;
		}

		public static string ToDisplayString(this XBeeTransmitStatus source)
		{
			if (source != XBeeTransmitStatus.SUCCESS)
				return "Error: " + lookupTable[source];
			else
				return lookupTable[source];
		}
	}
}
