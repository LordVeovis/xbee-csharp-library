using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates the different power levels. The power level indicates the output power value of a radio when transmitting data.
	/// </summary>
	public enum PowerLevel
	{
		// Enumeration entries
		LEVEL_LOWEST = 0x00,
		LEVEL_LOW = 0x01,
		LEVEL_MEDIUM = 0x02,
		LEVEL_HIGH = 0x03,
		LEVEL_HIGHEST = 0x04,
		LEVEL_UNKNOWN = 0xFF
	}

	public static class PowerLevelExtensions
	{
		private static IDictionary<PowerLevel, string> lookupTable = new Dictionary<PowerLevel, string>();

		static PowerLevelExtensions()
		{
			lookupTable.Add(PowerLevel.LEVEL_LOWEST, "Lowest");
			lookupTable.Add(PowerLevel.LEVEL_LOW, "Low");
			lookupTable.Add(PowerLevel.LEVEL_MEDIUM, "Medium");
			lookupTable.Add(PowerLevel.LEVEL_HIGH, "High");
			lookupTable.Add(PowerLevel.LEVEL_HIGHEST, "Highest");
			lookupTable.Add(PowerLevel.LEVEL_UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the power level value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The power level value.</returns>
		public static int GetValue(this PowerLevel source)
		{
			return (int)source;
		}

		/// <summary>
		/// Gets the power level description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The power level description.</returns>
		public static string GetDescription(this PowerLevel source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the <see cref="PowerLevel"/> entry associated to the given value.
		/// </summary>
		/// <param name="dumb"></param>
		/// <param name="value">Value of the <see cref="PowerLevel"/> to retrieve.</param>
		/// <returns>The <see cref="PowerLevel"/> entry associated to the given value, <code>PowerLevel.LEVEL.UNKNOWN</code> if the <paramref name="value"/> could not be found in the list.</returns>
		public static PowerLevel Get(this PowerLevel dumb, int value)
		{
			var values = Enum.GetValues(typeof(PowerLevel));

			if (values.OfType<int>().Contains(value))
				return (PowerLevel)value;

			return PowerLevel.LEVEL_UNKNOWN;
		}

		public static string ToDisplayString(this PowerLevel source)
		{
			return string.Format("{0}: {1}", HexUtils.ByteToHexString((byte)source), lookupTable[source]);
		}
	}

}
