using System;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates all the possible options that have been set while receiving an XBee packet.
	/// </summary>
	[Flags]
	public enum XBeeReceiveOptions
	{
		/// <summary>
		/// No special receive options.
		/// </summary>
		NONE = 0x00,

		/// <summary>
		/// Packet was acknowledged.
		/// </summary>
		/// <remarks>Not valid for Wi-Fi protocol</remarks>
		PACKET_ACKNOWLEDGED = 0x01,

		/// <summary>
		/// Packet was a broadcast packet.
		/// </summary>
		/// <remarks>Not valid for Wi-Fi protocol</remarks>
		BROADCAST_PACKET = 0x02,

		/// <summary>
		/// Packet encrypted with APS encryption.
		/// </summary>
		/// <remarks>Only valid for ZigBee XBee protocol.</remarks>
		APS_ENCRYPTED = 0x20,

		/// <summary>
		/// Packet was sent from an end device, if known.
		/// </summary>
		/// <remarks>Only valid for ZigBee XBee protocol.</remarks>
		SENT_FROM_END_DEVICE = 0x40,
	}
}