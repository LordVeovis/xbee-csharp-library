using System;
using System.Diagnostics.Contracts;
namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// This class represents the hardware version number of an XBee device.
	/// </summary>
	public class HardwareVersion : IEquatable<HardwareVersion>
	{
		// Constants.
		private const int HASH_SEED = 23;

		/// <summary>
		/// Gets the Hardware version numeric value.
		/// </summary>
		public int Value { get; private set; }

		/// <summary>
		/// Gets the Hardware version description.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Initializes a new instance of <see cref="HardwareVersion"/>.
		/// </summary>
		/// <param name="value">The hardware version numeric value.</param>
		/// <param name="description">The hardware version description.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="description"/> is null.</exception>
		/// <exception cref="ArgumentException">if <paramref name="value"/> &lt; 0 or Length of <paramref name="description"/> is lower than 1.</exception>
		private HardwareVersion(int value, string description)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(description), "Description cannot be null or empty.");
			Contract.Requires<ArgumentException>(value >= 0, "Value cannot be less than 0.");

			this.Value = value;
			this.Description = description;
		}

		/// <summary>
		/// Gets the <paramref name="HardwareVersion"/> object associated to the given numeric value.
		/// </summary>
		/// <param name="value">Numeric value of the <see cref="HardwareVersion"/> retrieve.</param>
		/// <returns>The <see cref="HardwareVersion"/> associated to the specified <paramref name="value"/>, null if there is not any <see cref="HardwareVersion"/> with the <paramref name="value"/>.</returns>
		public static HardwareVersion Get(int value)
		{
			var hvEnum = HardwareVersionEnum.ABANDONATED.Get(value);

			if (hvEnum == null)
				return new HardwareVersion(value, "Unknown");

			return new HardwareVersion(hvEnum.GetValue(), hvEnum.GetDescription());
		}

		/// <summary>
		/// Gets the <see cref="HardwareVersion"/> object associated to the specified numeric <paramref name="value"/> and <paramref name="description"/>.
		/// </summary>
		/// <param name="value">Numeric value of the <see cref="HardwareVersion"/> retrieve.</param>
		/// <param name="description">Description of the <see cref="HardwareVersion"/> retrieve.</param>
		/// <returns>The <see cref="HardwareVersion"/> associated to the given value and description</returns>
		/// <exception cref="ArgumentNullException">if <paramref name="description"/> is null.</exception>
		/// <exception cref="ArgumentException">if <paramref name="value"/> &lt; 0 or Length of <paramref name="description"/> is lower than 1.</exception>
		public static HardwareVersion Get(int value, string description)
		{
			return new HardwareVersion(value, description);
		}

		public override bool Equals(object obj)
		{
			var other = obj as HardwareVersion;

			return other != null && Equals(other);
		}

		public bool Equals(HardwareVersion other)
		{
			return other != null
				&& other.Value == Value
				&& other.Description == Description;
		}

		public override int GetHashCode()
		{
			int hash = HASH_SEED * (HASH_SEED + Value);

			return hash;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}