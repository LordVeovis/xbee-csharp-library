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

using System.Collections.Generic;
using System.Linq;

namespace XBeeLibrary.Core.IO
{
	/// <summary>
	/// Enumerates the different IO lines that can be found in the XBee devices. 
	/// </summary>
	/// <remarks>Depending on the hardware and firmware of the device, the number of lines that can 
	/// be used as well as their functionality may vary. Refer to the product manual to learn more 
	/// about the IO lines of your XBee device.</remarks>
	public enum IOLine
	{
		UNKNOWN = 0xff,
		DIO0_AD0 = 0,
		DIO0_AD1 = 1,
		DIO0_AD2 = 2,
		DIO0_AD3 = 3,
		DIO0_AD4 = 4,
		DIO0_AD5 = 5,
		DIO6 = 6,
		DIO7 = 7,
		DIO8 = 8,
		DIO9 = 9,
		DIO10_PWM0 = 10,
		DIO11_PWM1 = 11,
		DIO12 = 12,
		DIO13 = 13,
		DIO14 = 14,
		DIO15 = 15,
		DIO16 = 16,
		DIO17 = 17,
		DIO18 = 18,
		DIO19 = 19
	}

	public static class IOLineExtensions
	{
		private static IDictionary<IOLine, IOLineStruct> lookup = new Dictionary<IOLine, IOLineStruct>();

		static IOLineExtensions()
		{
			lookup.Add(IOLine.DIO0_AD0, new IOLineStruct("DIO0/AD0", 0, "D0", null));
			lookup.Add(IOLine.DIO0_AD1, new IOLineStruct("DIO1/AD1", 1, "D1", null));
			lookup.Add(IOLine.DIO0_AD2, new IOLineStruct("DIO2/AD2", 2, "D2", null));
			lookup.Add(IOLine.DIO0_AD3, new IOLineStruct("DIO3/AD3", 3, "D3", null));
			lookup.Add(IOLine.DIO0_AD4, new IOLineStruct("DIO4/AD4", 4, "D4", null));
			lookup.Add(IOLine.DIO0_AD5, new IOLineStruct("DIO5/AD5", 5, "D5", null));
			lookup.Add(IOLine.DIO6, new IOLineStruct("DIO6", 6, "D6", null));
			lookup.Add(IOLine.DIO7, new IOLineStruct("DIO7", 7, "D7", null));
			lookup.Add(IOLine.DIO8, new IOLineStruct("DIO8", 8, "D8", null));
			lookup.Add(IOLine.DIO9, new IOLineStruct("DIO9", 9, "D9", null));
			lookup.Add(IOLine.DIO10_PWM0, new IOLineStruct("DIO10/PWM0", 10, "P0", "M0"));
			lookup.Add(IOLine.DIO11_PWM1, new IOLineStruct("DIO11/PWM1", 11, "P1", "M1"));
			lookup.Add(IOLine.DIO12, new IOLineStruct("DIO12", 12, "P2", null));
			lookup.Add(IOLine.DIO13, new IOLineStruct("DIO13", 13, "P3", null));
			lookup.Add(IOLine.DIO14, new IOLineStruct("DIO14", 14, "P4", null));
			lookup.Add(IOLine.DIO15, new IOLineStruct("DIO15", 15, "P5", null));
			lookup.Add(IOLine.DIO16, new IOLineStruct("DIO16", 16, "P6", null));
			lookup.Add(IOLine.DIO17, new IOLineStruct("DIO17", 17, "P7", null));
			lookup.Add(IOLine.DIO18, new IOLineStruct("DIO18", 18, "P8", null));
			lookup.Add(IOLine.DIO19, new IOLineStruct("DIO19", 19, "P9", null));
		}

		/// <summary>
		/// Gets the name of the IO line.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The name of the IO line.</returns>
		public static string GetName(this IOLine source)
		{
			return lookup[source].Name;
		}

		/// <summary>
		/// Gets the configuration AT command associated to the IO line.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The configuration AT command associated to the IO line.</returns>
		public static string GetConfigurationATCommand(this IOLine source)
		{
			return lookup[source].ATCommand;
		}

		/// <summary>
		/// Indicates whether the IO line has PWM capability.
		/// </summary>
		/// <param name="source"></param>
		/// <returns><c>true</c> if the IO line has PWM capability, <c>false</c> otherwise.</returns>
		public static bool HasPWMCapability(this IOLine source)
		{
			return lookup[source].HasPWMCapability;
		}

		/// <summary>
		/// Gets the PWM AT command associated to the IO line.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The PWM AT command associated to the IO line.</returns>
		public static string GetPWMDutyCycleATCommand(this IOLine source)
		{
			return lookup[source].ATPWMCommand;
		}

		/// <summary>
		/// Gets the <see cref="IOLine"/> associated to the given index.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">The index corresponding to the <see cref="IOLine"/> to retrieve.</param>
		/// <returns></returns>
		public static IOLine GetDIO(this IOLine source, int index)
		{
			return lookup.Where(l => l.Value.Index == index).Select(l => l.Key).FirstOrDefault();
		}
	}

	struct IOLineStruct
	{
		public IOLineStruct(string name, int index, string atCommand, string atPwmCommand)
			: this()
		{
			Name = name;
			Index = index;
			ATCommand = atCommand;
			ATPWMCommand = atPwmCommand;
		}

		// Properties.
		/// <summary>
		/// The name of the IO line.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The configuration AT command associated to the IO line.
		/// </summary>
		public string ATCommand { get; private set; }

		/// <summary>
		/// The PWM AT command associated to the IO line.
		/// </summary>
		public string ATPWMCommand { get; private set; }

		/// <summary>
		/// The index of the IO line.
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// Indicates whether the IO line has PWM capability.
		/// </summary>
		public bool HasPWMCapability
		{
			get { return ATPWMCommand != null; }
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
