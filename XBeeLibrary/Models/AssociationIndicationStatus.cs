using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates the different association indication status.
	/// </summary>
	public enum AssociationIndicationStatus : byte
	{
		SUCCESSFULLY_JOINED = 0x00,
		AS_TIMEOUT = 0x01,
		AS_NO_PANS_FOUND = 0x02,
		AS_ASSOCIATION_NOT_ALLOED = 0x03,
		AS_BEACONS_NOT_SUPPORTED = 0x04,
		AS_ID_DOESNT_MATCH = 0x05,
		AS_CHANNEL_DOESNT_MATCH = 0x06,
		ENERGY_SCAN_TIMEOUT = 0x07,
		COORDINATOR_START_REQUEST_FAILED = 0x08,
		COORDINATOR_INVALID_PARAMETER = 0x09,
		COORDINATOR_REALIGNMENT = 0x0A,
		AR_NOT_SENT = 0x0B,
		AR_TIMED_OUT = 0x0C,
		AR_INVALID_PARAMETER = 0x0D,
		AR_CHANNEL_ACCESS_FAILURE = 0x0E,
		AR_COORDINATOT_ACK_WASNT_RECEIVED = 0x0F,
		AR_COORDINATOT_DIDNT_REPLY = 0x10,
		SYNCHRONIZATION_LOST = 0x12,
		DISSASOCIATED = 0x13,
		NO_PANS_FOUND = 0x21,
		NO_PANS_WITH_ID_FOUND = 0x22,
		NJ_EXPIRED = 0x23,
		NO_JOINABLE_BEACONS_FOUND = 0x24,
		UNEXPECTED_STATE = 0x25,
		JOIN_FAILED = 0x27,
		COORDINATOR_START_FAILED = 0x2A,
		CHECKING_FOR_COORDINATOR = 0x2B,
		NETWORK_LEAVE_FAILED = 0x2C,
		DEVICE_DIDNT_RESPOND = 0xAB,
		UNSECURED_KEY_RECEIVED = 0xAC,
		KEY_NOT_RECEIVED = 0xAD,
		INVALID_SECURITY_KEY = 0xAF,
		SCANNING_NETWORK = 0xff
	}

	public static class AssociationIndicationStatusExtensions
	{
		private static IDictionary<AssociationIndicationStatus, string> lookupTable = new Dictionary<AssociationIndicationStatus, string>();

		static AssociationIndicationStatusExtensions()
		{
			lookupTable.Add(AssociationIndicationStatus.SUCCESSFULLY_JOINED, "Successfully formed or joined a network.");
			lookupTable.Add(AssociationIndicationStatus.AS_TIMEOUT, "Active Scan Timeout.");
			lookupTable.Add(AssociationIndicationStatus.AS_NO_PANS_FOUND, "Active Scan found no PANs.");
			lookupTable.Add(AssociationIndicationStatus.AS_ASSOCIATION_NOT_ALLOED, "Active Scan found PAN, but the CoordinatorAllowAssociation bit is not set.");
			lookupTable.Add(AssociationIndicationStatus.AS_BEACONS_NOT_SUPPORTED, "Active Scan found PAN, but Coordinator and End Device are not configured to support beacons.");
			lookupTable.Add(AssociationIndicationStatus.AS_ID_DOESNT_MATCH, "Active Scan found PAN, but the Coordinator ID parameter does not match the ID parameter of the End Device.");
			lookupTable.Add(AssociationIndicationStatus.AS_CHANNEL_DOESNT_MATCH, "Active Scan found PAN, but the Coordinator CH parameter does not match the CH parameter of the End Device.");
			lookupTable.Add(AssociationIndicationStatus.ENERGY_SCAN_TIMEOUT, "Energy Scan Timeout.");
			lookupTable.Add(AssociationIndicationStatus.COORDINATOR_START_REQUEST_FAILED, "Coordinator start request failed.");
			lookupTable.Add(AssociationIndicationStatus.COORDINATOR_INVALID_PARAMETER, "Coordinator could not start due to invalid parameter.");
			lookupTable.Add(AssociationIndicationStatus.COORDINATOR_REALIGNMENT, "Coordinator Realignment is in progress.");
			lookupTable.Add(AssociationIndicationStatus.AR_NOT_SENT, "Association Request not sent.");
			lookupTable.Add(AssociationIndicationStatus.AR_TIMED_OUT, "Association Request timed out - no reply was received.");
			lookupTable.Add(AssociationIndicationStatus.AR_INVALID_PARAMETER, "Association Request had an Invalid Parameter.");
			lookupTable.Add(AssociationIndicationStatus.AR_CHANNEL_ACCESS_FAILURE, "Association Request Channel Access Failure. Request was not transmitted - CCA failure.");
			lookupTable.Add(AssociationIndicationStatus.AR_COORDINATOT_ACK_WASNT_RECEIVED, "Remote Coordinator did not send an ACK after Association Request was sent.");
			lookupTable.Add(AssociationIndicationStatus.AR_COORDINATOT_DIDNT_REPLY, "Remote Coordinator did not reply to the Association Request, but an ACK was received after sending the request.");
			lookupTable.Add(AssociationIndicationStatus.SYNCHRONIZATION_LOST, "Sync-Loss - Lost synchronization with a Beaconing Coordinator.");
			lookupTable.Add(AssociationIndicationStatus.DISSASOCIATED, "Disassociated - No longer associated to Coordinator.");
			lookupTable.Add(AssociationIndicationStatus.NO_PANS_FOUND, "Scan found no PANs.");
			lookupTable.Add(AssociationIndicationStatus.NO_PANS_WITH_ID_FOUND, "Scan found no valid PANs based on current SC and ID settings.");
			lookupTable.Add(AssociationIndicationStatus.NJ_EXPIRED, "Valid Coordinator or Routers found, but they are not allowing joining (NJ expired).");
			lookupTable.Add(AssociationIndicationStatus.NO_JOINABLE_BEACONS_FOUND, "No joinable beacons were found.");
			lookupTable.Add(AssociationIndicationStatus.UNEXPECTED_STATE, "Unexpected state, node should not be attempting to join at this time.");
			lookupTable.Add(AssociationIndicationStatus.JOIN_FAILED, "Node Joining attempt failed (typically due to incompatible security settings).");
			lookupTable.Add(AssociationIndicationStatus.COORDINATOR_START_FAILED, "Coordinator Start attempt failed.");
			lookupTable.Add(AssociationIndicationStatus.CHECKING_FOR_COORDINATOR, "Checking for an existing coordinator.");
			lookupTable.Add(AssociationIndicationStatus.NETWORK_LEAVE_FAILED, "Attempt to leave the network failed.");
			lookupTable.Add(AssociationIndicationStatus.DEVICE_DIDNT_RESPOND, "Attempted to join a device that did not respond.");
			lookupTable.Add(AssociationIndicationStatus.UNSECURED_KEY_RECEIVED, "Secure join error - network security key received unsecured.");
			lookupTable.Add(AssociationIndicationStatus.KEY_NOT_RECEIVED, "Secure join error - network security key not received.");
			lookupTable.Add(AssociationIndicationStatus.INVALID_SECURITY_KEY, "Secure join error - joining device does not have the right preconfigured link key.");
			lookupTable.Add(AssociationIndicationStatus.SCANNING_NETWORK, "Scanning for a network/Attempting to associate.");
		}

		/// <summary>
		/// Gets the association indication status value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The association indication status value.</returns>
		public static byte GetValue(this AssociationIndicationStatus source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the association indication status description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The association indication status description.</returns>
		public static string GetDescription(this AssociationIndicationStatus source)
		{
			return lookupTable[source];
		}

		public static AssociationIndicationStatus Get(this AssociationIndicationStatus dumb, byte value)
		{
			//var values = Enum.GetValues(typeof(AssociationIndicationStatus));
			//if (values.OfType<byte>().Contains(value))
			return (AssociationIndicationStatus)value;
		}

		public static string ToDisplayString(this AssociationIndicationStatus source)
		{
			var data = lookupTable[source];

			return string.Format("{0}: {1}", HexUtils.ByteToHexString((byte)source), data);
		}
	}
}
