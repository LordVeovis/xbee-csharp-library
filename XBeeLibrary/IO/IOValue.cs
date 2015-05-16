using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.IO
{
	/// <summary>
	/// Enumerates the possible values of a <see cref="IOLine"/> configured as digital I/O.
	/// </summary>
	/// <seealso cref="IOLine"/>
	public enum IOValue
	{
		// Enumeration types.
		UNKNOWN = 0,
		LOW = 4,
		HIGH = 5

	}

	public static class IOValueExtensions
	{
		private static IDictionary<IOValue, string> lookupTable = new Dictionary<IOValue, string>();

		static IOValueExtensions()
		{
			lookupTable.Add(IOValue.LOW, "Low");
			lookupTable.Add(IOValue.HIGH, "High");
		}

		/// <summary>
		/// Gets the ID of the IO value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int GetID(this IOValue value)
		{
			return (int)value;
		}

		/// <summary>
		/// Gets the name of the IO value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetName(this IOValue value)
		{
			return value.ToString();
		}

		public static IOValue GetIOValue(this IOValue dumb, int valueID)
		{
			if (Enum.GetValues(typeof(IOValue)).OfType<int>().Contains(valueID))
				return (IOValue)valueID;

			return IOValue.UNKNOWN;
		}
	}
}