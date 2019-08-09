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
using System.IO;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class represents a remote XBee device.
	/// </summary>
	/// <seealso cref="RemoteDigiMeshDevice"/>
	/// <seealso cref="RemoteDigiPointDevice"/>
	/// <seealso cref="RemoteRaw802Device"/>
	/// <seealso cref="RemoteZigBeeDevice"/>
	public class RemoteXBeeDevice : AbstractXBeeDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteXBeeDevice"/> object with the given 
		/// local <see cref="XBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection 
		/// interface to communicate with this remote XBee device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote XBee device.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteXBeeDevice(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="RemoteXBeeDevice"/> object with the given 
		/// local <see cref="XBeeDevice"/> which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection 
		/// interface to communicate with this remote XBee device.</param>
		/// <param name="addr64">The 64-bit address to identify this remote XBee device.</param>
		/// <param name="addr16">The 16-bit address to identify this remote XBee device. It might 
		/// be <c>null</c>.</param>
		/// <param name="ni">The node identifier of this remote XBee device. It might be <c>null</c>.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public RemoteXBeeDevice(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64,
				XBee16BitAddress addr16, string ni)
			: base(localXBeeDevice, addr64, addr16, ni) { }

		/// <summary>
		/// Always <c>true</c>, since this is a remote device.
		/// </summary>
		public override bool IsRemote { get; } = true;

		/// <summary>
		/// Performs a software reset on this XBee device and blocks until the process is 
		/// completed.
		/// </summary>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public override void Reset()
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			logger.InfoFormat(ToString() + "Resetting the remote module ({0})...", XBee64BitAddr);

			ATCommandResponse response = null;
			try
			{
				response = SendATCommand(new ATCommand("FR"));
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}
			catch (Exceptions.TimeoutException e)
			{
				// Remote 802.15.4 devices do not respond to the AT command.
				if (localXBeeDevice.XBeeProtocol == XBeeProtocol.RAW_802_15_4)
					return;
				else
					throw e;
			}

			// Check if AT Command response is valid.
			CheckATCommandResponseIsValid(response);
		}

		/// <summary>
		/// Returns the string representation of this device.
		/// </summary>
		/// <returns>The string representation of this device.</returns>
		public override string ToString()
		{
			return string.Format("{0} - {1}", XBee64BitAddr, NodeID ?? "");
		}

		/// <summary>
		/// Returns the local XBee device.
		/// </summary>
		/// <returns>The local XBee device.</returns>
		public AbstractXBeeDevice GetLocalXBeeDevice()
		{
			return localXBeeDevice;
		}
	}
}