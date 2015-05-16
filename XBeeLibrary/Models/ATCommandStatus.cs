using System;
using System.Collections.Generic;
using System.Linq;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates all the possible states of an AT Command after executing it.
	/// <see cref="ATCommandResponse"/> objects will contain an entry of this enumerator indicating the status of the AT Command that was executed.
	/// </summary>
	/// <seealso cref="ATCommandResponse"/>
	public enum ATCommandStatus : byte
	{
		OK = 0,
		ERROR = 1,
		INVALID_COMMAND = 2,
		INVALID_PARAMETER = 3,
		TX_FAILURE = 4,
		UNKNOWN = 255
	}

	public static class ATCommandStatusExtensions
	{
		private static IDictionary<ATCommandStatus, string> lookupTable = new Dictionary<ATCommandStatus, string>();

		static ATCommandStatusExtensions()
		{
			lookupTable.Add(ATCommandStatus.OK, "Status OK");
			lookupTable.Add(ATCommandStatus.ERROR, "Status Error");
			lookupTable.Add(ATCommandStatus.INVALID_COMMAND, "Invalid command");
			lookupTable.Add(ATCommandStatus.INVALID_PARAMETER, "Invalid parameter");
			lookupTable.Add(ATCommandStatus.TX_FAILURE, "TX failure");
			lookupTable.Add(ATCommandStatus.UNKNOWN, "Unknown status");
		}

		/// <summary>
		/// Gets the AT Command Status ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The AT Command Status ID.</returns>
		public static byte GetId(this ATCommandStatus source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the AT Command Status description
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The AT Command Status description.</returns>
		public static string GetDescription(this ATCommandStatus source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gest the <see cref="ATCommandStatus"/> associated to the specified <paramref name="id"/>.
		/// </summary>
		/// <param name="dumb"></param>
		/// <param name="id">Id to retrieve the corresponding <paramref name="ATCommandStatus"/>.</param>
		/// <returns>The <see cref="ATCommandStatus"/> associated to the specified <paramref name="id"/>.</returns>
		public static ATCommandStatus Get(this ATCommandStatus dumb, byte id)
		{
			var values = Enum.GetValues(typeof(ATCommandStatus)).OfType<ATCommandStatus>();

			if (values.Cast<byte>().Contains(id))
				return (ATCommandStatus)id;

			return ATCommandStatus.UNKNOWN;
		}


		public static string ToDisplayString(this ATCommandStatus source)
		{
			return lookupTable[source];
		}
	}
}