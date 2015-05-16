using System;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// This class lists all the possible options that can be set while transmitting a remote AT Command.
	/// </summary>
	[Flags]
	public enum RemoteATCommandOptions : byte
	{
		/// <summary>
		/// No special transmit options.
		/// </summary>
		OPTION_NONE = 0x00,

		/// <summary>
		/// Disables ACK.
		/// </summary>
		OPTION_DISABLE_ACK = 0x01,

		/// <summary>
		/// Applies changes in the remote device.
		/// </summary>
		/// <remarks>If this option is not set, AC command must be sent before changes will take effect.</remarks>
		OPTION_APPLY_CHANGES = 0x02,

		/// <summary>
		/// Uses the extended transmission timeout.
		/// </summary>
		/// <remarks>Setting the extended timeout bit causes the stack to set the extended transmission timeout for the destination address.</remarks>
		OPTION_EXTENDED_TIMEOUT = 0x40
	}
}
