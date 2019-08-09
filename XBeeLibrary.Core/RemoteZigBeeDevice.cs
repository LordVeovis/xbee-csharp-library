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
	/// This class represents a remote ZigBee device.
	/// </summary>
	/// <seealso cref="RemoteXBeeDevice"/>
	/// <seealso cref="RemoteDigiMeshDevice"/>
	/// <seealso cref="RemoteDigiPointDevice"/>
	/// <seealso cref="RemoteRaw802Device"/>
	public class RemoteZigBeeDevice : RemoteXBeeDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteZigBeeDevice"/> object with the given 
		/// local <see cref="ZigBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local ZigBee device that will behave as connection 
		/// interface to communicate with this remote ZigBee device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote ZigBee device.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="XBee64BitAddress"/>
		/// <seealso cref="ZigBeeDevice"/>
		public RemoteZigBeeDevice(ZigBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteZigBeeDevice"/> object with the given 
		/// local <see cref="XBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection 
		/// interface to communicate with this remote ZigBee device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote ZigBee device.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote 
		/// or if <c><paramref name="localXBeeDevice"/> != <see cref="XBeeProtocol.ZIGBEE"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteZigBeeDevice(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{
			// Verify the local device has ZigBee protocol.
			if (localXBeeDevice.XBeeProtocol != XBeeProtocol.ZIGBEE)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.ZIGBEE.GetDescription() + ".");
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteZigBeeDevice"/> object with the given 
		/// local <see cref="XBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection 
		/// interface to communicate with this remote ZigBee device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote ZigBee device.</param>
		/// <param name="addr16">The 16-bit address to identify this remote ZigBee device. It might 
		/// be<c>null</c>.</param>
		/// <param name="ni">The node identifier of this remote ZigBee device. It might be <c>null</c>.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote 
		/// or if <c><paramref name="localXBeeDevice"/> != <see cref="XBeeProtocol.ZIGBEE"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteZigBeeDevice(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64,
				XBee16BitAddress addr16, string ni)
			: base(localXBeeDevice, addr64, addr16, ni)
		{
			// Verify the local device has ZigBee protocol.
			if (localXBeeDevice.XBeeProtocol != XBeeProtocol.ZIGBEE)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.ZIGBEE.GetDescription() + ".");
		}

		// Properties.
		/// <summary>
		/// The protocol of the XBee device.
		/// </summary>
		/// <seealso cref="Models.XBeeProtocol.ZIGBEE"/>
		public override XBeeProtocol XBeeProtocol => XBeeProtocol.ZIGBEE;

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