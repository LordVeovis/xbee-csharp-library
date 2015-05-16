namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// This enumeration lists all the possible options that can be set while transmitting an XBee data packet.
	/// </summary>
	public enum XBeeTransmitOptions
	{

		/// <summary>
		/// No special transmit options.
		/// </summary>
		NONE = 0x00,

		/// <summary>
		/// Disables acknowledgments on all unicasts.
		/// </summary>
		/// <remarks>Only valid for DigiMesh, 802.15.4 and Point-to-multipoint protocols.</remarks>
		DISABLE_ACK = 0x01,

		/// <summary>
		/// Disables the retries and router repair in the frame.
		/// </summary>
		/// <remarks>Only valid for ZigBee protocol.</remarks>
		DISABLE_RETRIES_AND_REPAIR = 0x01,

		/// <summary>
		/// Doesn't attempt Route Discovery.
		/// Disables Route Discovery on all DigiMesh unicasts.
		/// </summary>
		/// <remarks>Only valid for DigiMesh protocol.</remarks>
		DONT_ATTEMPT_RD = 0x02,

		/// <summary>
		/// Sends packet with broadcast <code>PAN ID</code>. Packet will be sent to all devices in the same channel ignoring the <code>PAN ID</code>.
		/// It cannot be combined with other options.
		/// </summary>
		/// <remarks><Only valid for 802.15.4 XBee protocol./remarks>
		USE_BROADCAST_PAN_ID = 0x04,

		/// <summary>
		/// Enables unicast NACK messages.
		/// NACK message is enabled on the packet.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 protocol.</remarks>
		ENABLE_UNICAST_NACK = 0x04,

		/// <summary>
		/// Enables unicast trace route messages.
		/// Trace route is enabled on the packets.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 protocol.</remarks>
		ENABLE_UNICAST_TRACE_ROUTE = 0x04,

		/// <summary>
		/// Enables APS encryption, only if <code>EE=1</code>.
		/// Enabling APS encryption decreases the maximum number of RF payload bytes by 4 (below the value reported by <code>NP</code>).
		/// </summary>
		/// <remarks>Only valid for ZigBee XBee protocol.</remarks>
		ENABLE_APS_ENCRYPTION = 0x20,

		/// <summary>
		/// Uses the extended transmission timeout.
		/// Setting the extended timeout bit causes the stack to set the extended transmission timeout for the destination address.
		/// </summary>
		/// <remarks>Only valid for ZigBee XBee protocol.</remarks>
		USE_EXTENDED_TIMEOUT = 0x40,

		/// <summary>
		/// Transmission is performed using point-to-Multipoint mode.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
		POINT_MULTIPOINT_MODE = 0x40,

		/// <summary>
		/// Transmission is performed using repeater mode.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
		REPEATER_MODE = 0x80,

		/// <summary>
		/// Transmission is performed using DigiMesh mode.
		/// </summary>
		/// <remarks>Only valid for DigiMesh 868/900 and Point-to-Multipoint 868/900 protocols.</remarks>
		DIGIMESH_MODE = 0xC0
	}
}