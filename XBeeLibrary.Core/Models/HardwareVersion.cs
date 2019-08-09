/*
 * Copyright 2019, Digi International Inc.
 * Copyright 2014, 2015, Sébastien Rault.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents the hardware version number of an XBee device.
	/// </summary>
	public class HardwareVersion : IEquatable<HardwareVersion>
	{
		// Constants.
		private const int HASH_SEED = 23;

		/// <summary>
		/// Initializes a new instance of <see cref="HardwareVersion"/>.
		/// </summary>
		/// <param name="value">The hardware version numeric value.</param>
		/// <param name="description">The hardware version description.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="description"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If <c><paramref name="value"/> <![CDATA[<]]> 0 </c> 
		/// or if Length of <paramref name="description"/> is lower than 1.</exception>
		private HardwareVersion(int value, string description)
		{
			if (string.IsNullOrEmpty(description))
				throw new ArgumentNullException("Description cannot be null or empty.");
			if (value < 0)
				throw new ArgumentException("Value cannot be less than 0.");

			Value = value;
			Description = description;
		}

		// Properties.
		/// <summary>
		/// The Hardware version numeric value.
		/// </summary>
		public int Value { get; private set; }

		/// <summary>
		/// The Hardware version description.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Gets the <see cref="HardwareVersion"/> object associated to the given numeric value.
		/// </summary>
		/// <param name="value">Numeric value of the <see cref="HardwareVersion"/> retrieve.</param>
		/// <returns>The <see cref="HardwareVersion"/> associated to the specified <paramref name="value"/>, 
		/// <c>null</c> if there is not any <see cref="HardwareVersion"/> with the <paramref name="value"/>.</returns>
		public static HardwareVersion Get(int value)
		{
			var hvEnum = HardwareVersionEnum.ABANDONATED.Get(value);
			return new HardwareVersion(hvEnum.GetValue(), hvEnum.GetDescription());
		}

		/// <summary>
		/// Gets the <see cref="HardwareVersion"/> object associated to the specified numeric 
		/// <paramref name="value"/> and <paramref name="description"/>.
		/// </summary>
		/// <param name="value">Numeric value of the <see cref="HardwareVersion"/> retrieve.</param>
		/// <param name="description">Description of the <see cref="HardwareVersion"/> retrieve.</param>
		/// <returns>The <see cref="HardwareVersion"/> associated to the given value and description</returns>
		/// <exception cref="ArgumentNullException">if <paramref name="description"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">if <paramref name="value"/> &lt; 0 or Length of 
		/// <paramref name="description"/> is lower than 1.</exception>
		public static HardwareVersion Get(int value, string description)
		{
			return new HardwareVersion(value, description);
		}

		/// <summary>
		/// Returns whether this object is equal to the given one.
		/// </summary>
		/// <param name="obj">Object to compare if it is equal to the caller one.</param>
		/// <returns><c>true</c> if the caller/this object is equal to <paramref name="obj"/>, 
		/// <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			HardwareVersion other = obj as HardwareVersion;
			return other != null && Equals(other);
		}

		/// <summary>
		/// Returns whether this <see cref="HardwareVersion"/> is equal to the given one.
		/// </summary>
		/// <param name="other"><see cref="HardwareVersion"/> to compare if it is equal to 
		/// the caller one.</param>
		/// <returns><c>true</c> if the caller/this object is equal to <paramref name="other"/>, 
		/// <c>false</c> otherwise.</returns>
		public bool Equals(HardwareVersion other)
		{
			return other != null
				&& other.Value == Value
				&& other.Description == Description;
		}

		/// <summary>
		/// Returns the Hash code of this object.
		/// </summary>
		/// <returns>The Hash code of this object.</returns>
		public override int GetHashCode()
		{
			int hash = HASH_SEED * (HASH_SEED + Value);
			return hash;
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return Value.ToString();
		}
	}
}