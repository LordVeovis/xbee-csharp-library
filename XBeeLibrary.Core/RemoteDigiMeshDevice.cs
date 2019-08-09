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
using XBeeLibrary.Core.Models;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class represents a remote DigiMesh device.
	/// </summary>
	/// <seealso cref="RemoteXBeeDevice"/>
	/// <seealso cref="RemoteDigiPointDevice"/>
	/// <seealso cref="RemoteRaw802Device"/>
	/// <seealso cref="RemoteZigBeeDevice"/>
	public class RemoteDigiMeshDevice : RemoteXBeeDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteDigiMeshDevice"/> object with the given 
		/// local <see cref="DigiMeshDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local DigiMesh device that will behave as connection 
		/// interface to communicate with this remote DigiMesh device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote DigiMesh device.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="DigiMeshDevice"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteDigiMeshDevice(DigiMeshDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteDigiMeshDevice"/> object with the given 
		/// local <see cref="XBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection 
		/// interface to communicate with this remote DigiMesh device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote DigiMesh device.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote 
		/// or if <c><paramref name="localXBeeDevice"/> != <see cref="XBeeProtocol.DIGI_MESH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteDigiMeshDevice(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{
			// Verify the local device has DigiMesh protocol.
			if (localXBeeDevice.XBeeProtocol != XBeeProtocol.DIGI_MESH)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.DIGI_MESH.GetDescription() + ".");
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteDigiMeshDevice"/> object with the given 
		/// local <see cref="XBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection 
		/// interface to communicate with this remote DigiMesh device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote DigiMesh device.</param>
		/// <param name="id">The node identifier of this remote DigiMesh device. It might be <c>null</c>.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote 
		/// or if <c><paramref name="localXBeeDevice"/> != <see cref="XBeeProtocol.DIGI_MESH"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteDigiMeshDevice(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64, string id)
			: base(localXBeeDevice, addr64, null, id)
		{
			// Verify the local device has DigiMesh protocol.
			if (localXBeeDevice.XBeeProtocol != XBeeProtocol.DIGI_MESH)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.DIGI_MESH.GetDescription() + ".");
		}

		// Properties.
		/// <summary>
		/// The protocol of the XBee device.
		/// </summary>
		/// <seealso cref="Models.XBeeProtocol.DIGI_MESH"/>
		public override XBeeProtocol XBeeProtocol => XBeeProtocol.DIGI_MESH;
	}
}