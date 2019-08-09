/*
 * Copyright 2019, Digi International Inc.
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

using System.Collections.Generic;
using XBeeLibrary.Core.Models;

namespace XBeeLibrary.Core.Tools
{
	/// <summary>
	/// This class compares and sorts the different XBee categories, returning the 
	/// firmware actions category in first place.
	/// </summary>
	public class XBeeCategoriesSorter : IComparer<XBeeCategory>
	{
		// Constants.
		public const string SPECIAL_CATEGORY_NAME = "Firmware actions";

		/// <summary>
		/// Compares the given categories. Actually the method just checks if the first category 
		/// corresponds to the <see cref="SPECIAL_CATEGORY_NAME"/> to return it in first place.
		/// </summary>
		/// <param name="arg0">First category to compare.</param>
		/// <param name="arg1">Second category to compare (not used).</param>
		/// <returns><c>-1</c> if the first category corresponds to the
		/// <see cref="SPECIAL_CATEGORY_NAME"/>, <c>0</c> otherwise.</returns>
		public int Compare(XBeeCategory arg0, XBeeCategory arg1)
		{
			if (arg0.Name.Equals(SPECIAL_CATEGORY_NAME))
				return -1;

			return 0;
		}
	}
}
