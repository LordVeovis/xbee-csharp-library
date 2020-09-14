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

using Plugin.BLE.Abstractions.Contracts;
using System;
using XBeeLibrary.Core.Connection;
using XBeeLibrary.Xamarin.Connection.Bluetooth;

namespace XBeeLibrary.Xamarin
{
	/// <summary>
	/// Helper class used to create a bluetooth connection interface.
	/// </summary>
	public class XBee
	{
		/// <summary>
		/// Retrieves a bluetooth connection interface for the provided device.
		/// </summary>
		/// <param name="device">Bluetooth device to connect to.</param>
		/// <returns>The connection interface of the Bluetooth device.</returns>
		/// <seealso cref="IConnectionInterface"/>
		/// <seealso cref="IDevice"/>
		public static IConnectionInterface CreateConnectionInterface(IDevice device)
		{
			IConnectionInterface connectionInterface = new BluetoothInterface(device);
			return connectionInterface;
		}

		/// <summary>
		/// Retrieves a bluetooth connection interface for the device with the provided GUID.
		/// </summary>
		/// <param name="deviceAddress">The address or GUID of the Bluetooth device. It must follow the
		/// format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c> for the address or
		/// <c>01234567-0123-0123-0123-0123456789AB</c> for the GUID.</param>
		/// <returns>The connection interface of the Bluetooth device.</returns>
		/// <exception cref="ArgumentException">If <paramref name="deviceAddress"/> does not follow
		/// the format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c> or
		/// <c>01234567-0123-0123-0123-0123456789AB</c>.</exception>
		/// <seealso cref="IConnectionInterface"/>
		public static IConnectionInterface CreateConnectionInterface(string deviceAddress)
		{
			IConnectionInterface connectionInterface = new BluetoothInterface(deviceAddress);
			return connectionInterface;
		}

		/// <summary>
		/// Retrieves a bluetooth connection interface for the device with the provided GUID.
		/// </summary>
		/// <param name="deviceGuid">The Bluetooth device GUID.</param>
		/// <seealso cref="Guid"/>
		/// <seealso cref="IConnectionInterface"/>
		public static IConnectionInterface CreateConnectionInterface(Guid deviceGuid)
		{
			IConnectionInterface connectionInterface = new BluetoothInterface(deviceGuid);
			return connectionInterface;
		}
	}
}