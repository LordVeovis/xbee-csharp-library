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
using XBeeLibrary.Core.Connection;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.Raw;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class represents a local 802.15.4 device.
	/// </summary>
	/// <seealso cref="CellularDevice"/>
	/// <seealso cref="DigiMeshDevice"/>
	/// <seealso cref="DigiPointDevice"/>
	/// <seealso cref="XBeeDevice"/>
	/// <seealso cref="ZigBeeDevice"/>
	public class Raw802Device : XBeeDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="Raw802Device"/> object with the given 
		/// connection interface.
		/// </summary>
		/// <param name="connectionInterface">The connection interface with the physical 802.15.4 device.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="connectionInterface"/> == null</c>.
		/// </exception>
		/// <seealso cref="XBeeDevice(IConnectionInterface)"/>
		/// <seealso cref="IConnectionInterface"/>
		public Raw802Device(IConnectionInterface connectionInterface)
			: base(connectionInterface) { }

		// Properties.
		/// <summary>
		/// The protocol of the XBee device.
		/// </summary>
		/// <seealso cref="Models.XBeeProtocol.RAW_802_15_4"/>
		public override XBeeProtocol XBeeProtocol => XBeeProtocol.RAW_802_15_4;

		/// <summary>
		/// Opens the connection interface associated with this XBee device.
		/// </summary>
		/// <remarks>When opening the device an information reading process is automatically performed. 
		/// This includes:</remarks>
		/// <list type="bullet">
		/// <item><description>64-bit address.</description></item>
		/// <item><description>Node Identifier.</description></item>
		/// <item><description>Hardware version</description></item>
		/// <item><description>Firmware version.</description></item>
		/// <item><description>XBee device protocol.</description></item>
		/// <item><description>16-bit address (not for DigiMesh modules).</description></item>
		/// </list>
		/// <exception cref="InterfaceAlreadyOpenException">If this device connection is already open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode of the device is 
		/// <see cref="OperatingMode.UNKNOWN"/> or <see cref="OperatingMode.AT"/></exception>
		/// <exception cref="BluetoothAuthenticationException">If the BLE authentication process fails.</exception>
		/// <exception cref="TimeoutException">If the timeout to read settings when initializing the 
		/// device elapses without response.</exception>
		/// <exception cref="XBeeException">If there is any problem opening this device connection.</exception>
		/// <seealso cref="AbstractXBeeDevice.IsOpen"/>
		/// <seealso cref="AbstractXBeeDevice.Close"/>
		public override void Open()
		{
			base.Open();

			if (IsRemote)
				return;

			if (base.XBeeProtocol != XBeeProtocol.RAW_802_15_4)
				throw new XBeeDeviceException("XBee device is not a " + XBeeProtocol.RAW_802_15_4.GetDescription() 
					+ " device, it is a " + base.XBeeProtocol.GetDescription() + " device.");
		}

		/// <summary>
		/// Returns the network associated with this XBee device.
		/// </summary>
		/// <returns>The XBee network of the device.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="Raw802Network"/>
		/// <seealso cref="XBeeNetwork"/>
		public override XBeeNetwork GetNetwork()
		{
			if (network == null)
				network = new Raw802Network(this);

			return network;
		}

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

		/// <summary>
		/// Sends the provided data to the XBee device of the network corresponding to the given 16-bit 
		/// address asynchronously.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for 
		/// transmit status packet.</remarks>
		/// <param name="address">The 16-bit address of the XBee that will receive the data.</param>
		/// <param name="data">Byte array containing data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the device is remote.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error.</exception>
		/// <seealso cref="XBeeDevice.SendData(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendData(XBee16BitAddress, byte[])"/>
		/// <seealso cref="AbstractXBeeDevice.SendData(XBee64BitAddress, byte[])"/>
		/// <seealso cref="AbstractXBeeDevice.SendDataAsync(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="AbstractXBeeDevice.SendDataAsync(XBee64BitAddress, byte[])"/>
		/// <seealso cref="XBee16BitAddress"/>
		public void SendDataAsync(XBee16BitAddress address, byte[] data)
		{
			// Verify the parameters are not null, if they are null, throw an exception.
			if (address == null)
				throw new ArgumentNullException("Address cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send data to a remote device from a remote device.");

			logger.InfoFormat(ToString() + "Sending data asynchronously to {0} >> {1}.", address, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new TX16Packet(GetNextFrameID(), address, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, true);
		}

		/// <summary>
		/// Sends the provided data to the XBee device of the network corresponding to the given 16-bit 
		/// address.
		/// </summary>
		/// <remarks>This method blocks until a success or error response arrives or the configured 
		/// receive timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="XBeeDevice.ReceiveTimeout"/> 
		/// property.
		/// 
		/// For non-blocking operations use the method 
		/// <see cref="SendData(XBee16BitAddress, byte[])"/>.</remarks>
		/// <param name="address">The 16-bit address of the XBee that will receive the data.</param>
		/// <param name="data">Byte array containing data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the device is remote.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error.</exception>
		/// <seealso cref="AbstractXBeeDevice.SendData(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="AbstractXBeeDevice.SendData(XBee64BitAddress, byte[])"/>
		/// <seealso cref="AbstractXBeeDevice.SendDataAsync(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendDataAsync(XBee16BitAddress, byte[])"/>
		/// <seealso cref="AbstractXBeeDevice.SendDataAsync(XBee64BitAddress, byte[])"/>
		/// <seealso cref="XBee16BitAddress"/>
		public void SendData(XBee16BitAddress address, byte[] data)
		{
			// Verify the parameters are not null, if they are null, throw an exception.
			if (address == null)
				throw new ArgumentNullException("Address cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send data to a remote device from a remote device.");

			logger.InfoFormat(ToString() + "Sending data to {0} >> {1}.", address, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new TX16Packet(GetNextFrameID(), address, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, false);
		}
	}
}