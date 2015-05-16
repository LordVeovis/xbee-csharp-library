using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates the different modem status events. This enumeration list is intended to be used within the <see cref="ModemStatusPacket"/> packet.
	/// </summary>
	public enum ModemStatusEvent
	{

		STATUS_HARDWARE_RESET = 0,
		STATUS_WATCHDOG_TIMER_RESET = 1,
		STATUS_JOINED_NETWORK = 2,
		STATUS_DISASSOCIATED = 3,
		STATUS_ERROR_SYNCHRONIZATION_LOST = 4,
		STATUS_COORDINATOR_REALIGNMENT = 5,
		STATUS_COORDINATOR_STARTED = 6,
		STATUS_NETWORK_SECURITY_KEY_UPDATED = 7,
		STATUS_NETWORK_WOKE_UP = 0xb,
		STATUS_NETWORK_WENT_TO_SLEEP = 0xc,
		STATUS_VOLTAGE_SUPPLY_LIMIT_EXCEEDED = 0xd,
		STATUS_MODEM_CONFIG_CHANGED_WHILE_JOINING = 0x11,
		STATUS_ERROR_STACK = 0x80,
		STATUS_ERROR_AP_NOT_CONNECTED = 0x82,
		STATUS_ERROR_AP_NOT_FOUND = 0x83,
		STATUS_ERROR_PSK_NOT_CONFIGURED = 0x84,
		STATUS_ERROR_SSID_NOT_FOUND = 0x87,
		STATUS_ERROR_FAILED_JOIN_SECURITY = 0x88,
		STATUS_ERROR_INVALID_CHANNEL = 0x8a,
		STATUS_ERROR_FAILED_JOIN_AP = 0x8e,
		STATUS_UNKNOWN = 0xff
	}

	public static class ModemStatusEventStructExtensions
	{
		static IDictionary<ModemStatusEvent, string> lookupTable = new Dictionary<ModemStatusEvent, string>();

		static ModemStatusEventStructExtensions()
		{
			lookupTable.Add(ModemStatusEvent.STATUS_HARDWARE_RESET, "Device was reset");
			lookupTable.Add(ModemStatusEvent.STATUS_WATCHDOG_TIMER_RESET, "Watchdog timer was reset");
			lookupTable.Add(ModemStatusEvent.STATUS_JOINED_NETWORK, "Device joined to network");
			lookupTable.Add(ModemStatusEvent.STATUS_DISASSOCIATED, "Device disassociated");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_SYNCHRONIZATION_LOST, "Configuration error/synchronization lost");
			lookupTable.Add(ModemStatusEvent.STATUS_COORDINATOR_REALIGNMENT, "Coordinator realignment");
			lookupTable.Add(ModemStatusEvent.STATUS_COORDINATOR_STARTED, "The coordinator started");
			lookupTable.Add(ModemStatusEvent.STATUS_NETWORK_SECURITY_KEY_UPDATED, "Network security key was updated");
			lookupTable.Add(ModemStatusEvent.STATUS_NETWORK_WOKE_UP, "Network Woke Up");
			lookupTable.Add(ModemStatusEvent.STATUS_NETWORK_WENT_TO_SLEEP, "Network Went To Sleep");
			lookupTable.Add(ModemStatusEvent.STATUS_VOLTAGE_SUPPLY_LIMIT_EXCEEDED, "Voltage supply limit exceeded");
			lookupTable.Add(ModemStatusEvent.STATUS_MODEM_CONFIG_CHANGED_WHILE_JOINING, " Modem configuration changed while joining");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_STACK, "Stack error");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_AP_NOT_CONNECTED, "Send/join command issued without connecting from AP");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_AP_NOT_FOUND, "Access point not found");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_PSK_NOT_CONFIGURED, "PSK not configured");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_SSID_NOT_FOUND, "SSID not found");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_FAILED_JOIN_SECURITY, "Failed to join with security enabled");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_INVALID_CHANNEL, "Invalid channel");
			lookupTable.Add(ModemStatusEvent.STATUS_ERROR_FAILED_JOIN_AP, "Failed to join access point");
			lookupTable.Add(ModemStatusEvent.STATUS_UNKNOWN, "UNKNOWN");
		}

		public static int GetId(this ModemStatusEvent source)
		{
			return (int)source;
		}

		/// <summary>
		/// Gets the modem status description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>Modem status description.</returns>
		public static string GetDescription(this ModemStatusEvent source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the <see cref="ModemStatusEvent"/> associated to the given ID.
		/// </summary>
		/// <param name="id">ID of the <see cref="ModemStatusEvent"/> to retrieve.</param>
		/// <returns>The <see cref="ModemStatusEvent"/> associated with the given ID.</returns>
		public static ModemStatusEvent Get(int id)
		{
			var values = Enum.GetValues(typeof(ModemStatusEvent));

			if (values.OfType<int>().Contains(id))
				return (ModemStatusEvent)id;

			return ModemStatusEvent.STATUS_UNKNOWN;
		}

		public static string ToDisplayString(this ModemStatusEvent source)
		{
			return string.Format("{0}: {1}", HexUtils.ByteToHexString((byte)(int)source), source.GetDescription());
		}
	}
}
