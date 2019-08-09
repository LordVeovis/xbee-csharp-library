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

using System;
using System.Collections.Generic;
using System.Linq;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates the different association indication statuses for the Cellular 
	/// protocol.
	/// </summary>
	public enum CellularAssociationIndicationStatus
	{
		// Enumeration entries.
		SUCCESSFULLY_CONNECTED = 0x00,
		POWERED_UP = 0x20,
		IDENTIFIED = 0x21,
		REGISTERING = 0x22,
		REGISTERED = 0x23,
		SETUP_DEVICE = 0xA0,
		SETUP_USB = 0xC0,
		POWERUP = 0xE0,
		UNKNOWN = 0xFA,
		REBOOTING = 0xFB,
		SHUTTING_DOWN = 0xFC,
		MANUFACTURING_STATE = 0xFD,
		UNEXPECTED_STATE = 0xFE,
		POWERED_DOWN = 0xFF
	}

	public static class CellularAssociationIndicationStatusExtensions
	{
		static IDictionary<CellularAssociationIndicationStatus, string> lookupTable = new Dictionary<CellularAssociationIndicationStatus, string>();

		static CellularAssociationIndicationStatusExtensions()
		{
			lookupTable.Add(CellularAssociationIndicationStatus.SUCCESSFULLY_CONNECTED, "Connected to the Internet.");
			lookupTable.Add(CellularAssociationIndicationStatus.POWERED_UP, "Modem powered up and enumerated.");
			lookupTable.Add(CellularAssociationIndicationStatus.IDENTIFIED, "Modem identified.");
			lookupTable.Add(CellularAssociationIndicationStatus.REGISTERING, "Modem registering.");
			lookupTable.Add(CellularAssociationIndicationStatus.REGISTERED, "Modem registered.");
			lookupTable.Add(CellularAssociationIndicationStatus.SETUP_DEVICE, "Setup modem device.");
			lookupTable.Add(CellularAssociationIndicationStatus.SETUP_USB, "Setup modem USB.");
			lookupTable.Add(CellularAssociationIndicationStatus.POWERUP, "Power up modem.");
			lookupTable.Add(CellularAssociationIndicationStatus.UNKNOWN, "Status unknown.");
			lookupTable.Add(CellularAssociationIndicationStatus.REBOOTING, "Rebooting modem.");
			lookupTable.Add(CellularAssociationIndicationStatus.SHUTTING_DOWN, "Shutting down modem.");
			lookupTable.Add(CellularAssociationIndicationStatus.MANUFACTURING_STATE, "Modem in manufacturing state.");
			lookupTable.Add(CellularAssociationIndicationStatus.UNEXPECTED_STATE, "Modem in unexpected state.");
			lookupTable.Add(CellularAssociationIndicationStatus.POWERED_DOWN, "Modem powered down.");
		}

		/// <summary>
		/// Gets the status ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>Status ID.</returns>
		public static int GetId(this CellularAssociationIndicationStatus source)
		{
			return (int)source;
		}

		/// <summary>
		/// Gets the status description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>Status description.</returns>
		public static string GetDescription(this CellularAssociationIndicationStatus source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the <see cref="CellularAssociationIndicationStatus"/> associated to the given ID.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="id">ID of the <see cref="CellularAssociationIndicationStatus"/> to retrieve.</param>
		/// <returns>The <see cref="CellularAssociationIndicationStatus"/> associated with the given ID.</returns>
		public static CellularAssociationIndicationStatus Get(this CellularAssociationIndicationStatus source, int id)
		{
			var values = Enum.GetValues(typeof(CellularAssociationIndicationStatus));

			if (values.OfType<int>().Contains(id))
				return (CellularAssociationIndicationStatus)id;

			return CellularAssociationIndicationStatus.UNKNOWN;
		}

		/// <summary>
		/// Returns the <see cref="CellularAssociationIndicationStatus"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="CellularAssociationIndicationStatus"/> in string format.</returns>
		public static string ToDisplayString(this CellularAssociationIndicationStatus source)
		{
			return string.Format("{0}: {1}", HexUtils.ByteToHexString((byte)(int)source), source.GetDescription());
		}
	}
}
