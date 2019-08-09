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
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class represents a remote 802.15.4 device.
	/// </summary>
	/// <seealso cref="RemoteXBeeDevice"/>
	/// <seealso cref="RemoteDigiMeshDevice"/>
	/// <seealso cref="RemoteDigiPointDevice"/>
	/// <seealso cref="RemoteZigBeeDevice"/>
	public class RemoteRaw802Device : RemoteXBeeDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteRaw802Device"/> object with the given 
		/// local <see cref="Raw802Device"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local 802.15.4 device that will behave as connection 
		/// interface to communicate with this remote 802.15.4 device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote 802.15.4 device.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="Raw802Device"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteRaw802Device(Raw802Device localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteRaw802Device"/> object with the given 
		/// local <see cref="XBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection 
		/// interface to communicate with this remote 802.15.4 device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote 802.15.4 device.</param>
		/// <param name="addr16">The 16-bit address to identify this remote 802.15.4 device. It might 
		/// be<c>null</c>.</param>
		/// <param name="id">The node identifier of this remote 802.15.4 device. It might be <c>null</c>.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote 
		/// or if <c><paramref name="localXBeeDevice"/> != <see cref="XBeeProtocol.RAW_802_15_4"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteRaw802Device(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64,
				XBee16BitAddress addr16, string id)
			: base(localXBeeDevice, addr64, addr16, id)
		{
			// Verify the local device has 802.15.4 protocol.
			if (localXBeeDevice.XBeeProtocol != XBeeProtocol.RAW_802_15_4)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.RAW_802_15_4.GetDescription() + ".");
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteRaw802Device"/> object with the given 
		/// local <see cref="RemoteRaw802Device"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local 802.15.4 device that will behave as connection 
		/// interface to communicate with this remote 802.15.4 device.</param>
		/// <param name="addr16">The 16-bit address to identify this remote 802.15.4 device.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote 
		/// or if <c><paramref name="localXBeeDevice"/> != <see cref="XBeeProtocol.RAW_802_15_4"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr16"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee16BitAddress"/>
		public RemoteRaw802Device(Raw802Device localXBeeDevice, XBee16BitAddress addr16)
			: base(localXBeeDevice, XBee64BitAddress.UNKNOWN_ADDRESS)
		{
			XBee16BitAddr = addr16;
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteRaw802Device"/> object with the given 
		/// local <see cref="XBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection 
		/// interface to communicate with this remote 802.15.4 device.</param>
		/// <param name="addr16">The 16-bit address to identify this remote 802.15.4 device.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote 
		/// or if <c><paramref name="localXBeeDevice"/> != <see cref="XBeeProtocol.RAW_802_15_4"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr16"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee16BitAddress"/>
		public RemoteRaw802Device(AbstractXBeeDevice localXBeeDevice, XBee16BitAddress addr16)
			: base(localXBeeDevice, XBee64BitAddress.UNKNOWN_ADDRESS)
		{
			// Verify the local device has 802.15.4 protocol.
			if (localXBeeDevice.XBeeProtocol != XBeeProtocol.RAW_802_15_4)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.RAW_802_15_4.GetDescription() + ".");

			XBee16BitAddr = addr16;
		}

		/// <summary>
		/// Sets the XBee64BitAddress of this remote 802.15.4 device.
		/// </summary>
		/// <param name="addr64">The 64-bit address to be set to the device.</param>
		/// <seealso cref="XBee64BitAddress"/>
		public void Set64BitAddress(XBee64BitAddress addr64)
		{
			XBee64BitAddr = addr64;
		}

		// Properties.
		/// <summary>
		/// The protocol of the XBee device.
		/// </summary>
		/// <seealso cref="Models.XBeeProtocol.RAW_802_15_4"/>
		public override XBeeProtocol XBeeProtocol => XBeeProtocol.RAW_802_15_4;

		/// <summary>
		/// Returns the current association status of this XBee device.
		/// </summary>
		/// <remarks>It indicates occurrences of errors during the last association request.</remarks>
		/// <returns>The association indication status of the XBee device.</returns>
		/// <exception cref="ATCommandEmptyException">If the <c>AI</c> command returns a <c>null</c> or an 
		/// empty value.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the association indication status.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ForceDisassociate"/>
		/// <seealso cref="AssociationIndicationStatus"/>
		public new AssociationIndicationStatus GetAssociationIndicationStatus()
		{
			return base.GetAssociationIndicationStatus();
		}

		/// <summary>
		/// Forces this XBee device to immediately disassociate from the network and re-attempt to associate.
		/// </summary>
		/// <remarks>Only valid for End Devices.</remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout executing the force disassociate command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetAssociationIndicationStatus"/>
		public new void ForceDisassociate()
		{
			base.ForceDisassociate();
		}
	}
}