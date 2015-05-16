using System.Collections.Generic;
namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates the different working modes of the XBee device. The operating mode establishes the way a user communicates with an XBee device through its serial interface.
	/// </summary>
	public enum OperatingMode
	{

		// Enumeration types
		AT = 0,
		API = 1,
		API_ESCAPE = 2,
		UNKNOWN = 3
	}

	public static class OperatingModeExtensions
	{
		static IDictionary<OperatingMode, string> lookupTable = new Dictionary<OperatingMode, string>();

		static OperatingModeExtensions()
		{
			lookupTable.Add(OperatingMode.AT, "AT mode");
			lookupTable.Add(OperatingMode.API, "API mode");
			lookupTable.Add(OperatingMode.API_ESCAPE, "API mode with escaped characters");
			lookupTable.Add(OperatingMode.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the operating mode ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>Operating mode ID.</returns>
		public static int GetID(this OperatingMode source)
		{
			return (int)source;
		}

		/// <summary>
		/// Gets the operating mode name.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>Operating mode name.</returns>
		public static string GetName(this OperatingMode source)
		{
			return lookupTable[source];
		}

		public static string ToDisplayString(this OperatingMode source)
		{
			return lookupTable[source];
		}
	}
}
