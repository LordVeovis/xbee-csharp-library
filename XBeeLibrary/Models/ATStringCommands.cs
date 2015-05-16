using Kveer.XBeeApi.Models;
using System.Collections.Generic;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates several AT commands used to parse AT command packets. The list of AT Command alias listed here represents those AT commands whose values should be parsed as strings.
	/// </summary>
	public enum ATStringCommands
	{
		NI,
		KY,
		NK,
		ZU,
		ZV,
		CC
	}

	public static class ATStringCommandsStructExtensions
	{
		/// <summary>
		/// Gets the AT Command alias.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The AT Command alias.</returns>
		public static string GetCommand(this ATStringCommands source)
		{
			return source.ToString();
		}
	}
}