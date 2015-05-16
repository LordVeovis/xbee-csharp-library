using System;
using System.Linq;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// Enumerates all the special bytes of the XBee protocol that must be escaped when working on API 2 mode.
	/// </summary>
	public enum SpecialByte : byte
	{

		// Enumeration elements
		ESCAPE_BYTE = 0x7D,
		HEADER_BYTE = 0x7E,
		XON_BYTE = 0x11,
		XOFF_BYTE = 0x13
	}

	public static class SpecialByteExtensions
	{
		/// <summary>
		/// Gest the special byte value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The special byte value.</returns>
		public static byte GetValue(this SpecialByte source)
		{
			return (byte)source;
		}

		/// <summary>
		/// Gets the <see cref="SpecialByte"/> entry associated with the given value.
		/// </summary>
		/// <param name="dumb"></param>
		/// <param name="value">Value of the <see cref="SpecialByte"/> to retrieve.</param>
		/// <returns><see cref="SpecialByte"/> associated to the given value, null if it does not exist in the list.</returns>
		public static SpecialByte Get(this SpecialByte dumb, byte value)
		{
			var values = Enum.GetValues(typeof(SpecialByte));

			if (values.OfType<byte>().Contains(value))
				return (SpecialByte)value;

			return 0;
		}

		/// <summary>
		/// Escapes the byte by performing a XOR operation with <code>0x20</code> value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>Escaped byte value.</returns>
		public static byte EscapeByte(this SpecialByte source)
		{
			return (byte)(((byte)source) ^ 0x20);
		}

		/// <summary>
		/// Checks whether the given byte is special or not.
		/// </summary>
		/// <param name="dumb"></param>
		/// <param name="byteToCheck">Byte to check.</param>
		/// <returns>true if given byte is special, false otherwise.</returns>
		public static bool IsSpecialByte(this SpecialByte dumb, byte byteToCheck)
		{
			return dumb.Get(byteToCheck) != null;
		}
	}
}
