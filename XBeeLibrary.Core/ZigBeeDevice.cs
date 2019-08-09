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
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class represents a local ZigBee device.
	/// </summary>
	/// <seealso cref="CellularDevice"/>
	/// <seealso cref="DigiMeshDevice"/>
	/// <seealso cref="DigiPointDevice"/>
	/// <seealso cref="Raw802Device"/>
	/// <seealso cref="XBeeDevice"/>
	public class ZigBeeDevice : XBeeDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="ZigBeeDevice"/> object with the given 
		/// connection interface.
		/// </summary>
		/// <param name="connectionInterface">The connection interface with the physical Zigbee 
		/// device.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="connectionInterface"/> == null</c>.
		/// </exception>
		/// <seealso cref="IConnectionInterface"/>
		public ZigBeeDevice(IConnectionInterface connectionInterface)
			: base(connectionInterface) { }

		// Events.
		/// <summary>
		/// Represents the method that will handle the explicit data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="ExplicitDataReceivedEventArgs"/>
		public new event EventHandler<ExplicitDataReceivedEventArgs> ExplicitDataReceived
		{
			add
			{
				base.ExplicitDataReceived += value;
			}
			remove
			{
				base.ExplicitDataReceived -= value;
			}
		}

		// Properties.
		/// <summary>
		/// The protocol of the XBee device.
		/// </summary>
		/// <seealso cref="Models.XBeeProtocol.ZIGBEE"/>
		public override XBeeProtocol XBeeProtocol => XBeeProtocol.ZIGBEE;

		/// <summary>
		/// The API output mode of the XBee device.
		/// </summary>
		/// <remarks>
		/// The API output mode determines the format that the received data is
		/// output through the serial interface of the XBee device.
		/// </remarks>
		/// <exception cref="ATCommandEmptyException">If the returned value of the API Output Mode command 
		/// is <c>null</c> or empty.</exception>
		/// <seealso cref="Models.APIOutputMode"/>
		public new APIOutputMode APIOutputMode
		{
			get
			{
				return base.APIOutputMode;
			}
			set
			{
				base.APIOutputMode = value;
			}
		}

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
			if (base.XBeeProtocol != XBeeProtocol.ZIGBEE)
				throw new XBeeDeviceException("XBee device is not a " + XBeeProtocol.ZIGBEE.GetDescription() 
					+ " device, it is a " + base.XBeeProtocol.GetDescription() + " device.");
		}

		/// <summary>
		/// Returns the network associated with this XBee device.
		/// </summary>
		/// <returns>The XBee network of the device.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="XBeeNetwork"/>
		/// <seealso cref="ZigBeeNetwork"/>
		public override XBeeNetwork GetNetwork()
		{
			if (network == null)
				network = new ZigBeeNetwork(this);

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
		/// Reads new explicit data received by this XBee device during the configured receive timeout.
		/// </summary>
		/// <remarks>This method blocks until new explicit data is received or the configured receive 
		/// timeout expires.
		/// 
		/// For non-blocking operations, register an event handler to
		/// <see cref="AbstractXBeeDevice.ExplicitDataReceived"/>.</remarks>
		/// <returns>An <see cref="ExplicitXBeeMessage"/> object containing the explicit data, the source 
		/// address of the remote node that sent the data and other values related to the transmission. 
		/// <c>null</c> if this did not receive new explicit data during the configured receive timeout.</returns>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="ExplicitXBeeMessage"/>
		/// <seealso cref="AbstractXBeeDevice.ReadExplicitData()"/>
		public new ExplicitXBeeMessage ReadExplicitData()
		{
			return base.ReadExplicitData();
		}

		/// <summary>
		/// Reads new explicit data received by this XBee device during the configured receive timeout.
		/// </summary>
		/// <remarks>This method blocks until new explicit data is received or the configured receive 
		/// timeout expires.
		/// 
		/// For non-blocking operations, register an event handler to
		/// <see cref="AbstractXBeeDevice.ExplicitDataReceived"/>.</remarks>
		/// <param name="timeout">The time to wait for new explicit data in milliseconds.</param>
		/// <returns>An <see cref="ExplicitXBeeMessage"/> object containing the explicit data, the source 
		/// address of the remote node that sent the data and other values related to the transmission. 
		/// <c>null</c> if this did not receive new explicit data during the configured receive timeout.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="timeout"/> <![CDATA[<]]> 0</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="ExplicitXBeeMessage"/>
		/// <seealso cref="AbstractXBeeDevice.ReadExplicitData(int)"/>
		public new ExplicitXBeeMessage ReadExplicitData(int timeout)
		{
			return base.ReadExplicitData(timeout);
		}

		/// <summary>
		/// Reads new explicit data received from the given remote XBee device during the configured 
		/// receive timeout.
		/// </summary>
		/// <remarks>This method blocks until new explicit data from the provided remote XBee device is 
		/// received or the configured receive timeout expires.
		/// 
		/// For non-blocking operations, register an event handler to
		/// <see cref="AbstractXBeeDevice.ExplicitDataReceived"/>.</remarks>
		/// <param name="remoteXBeeDevice">The remote device to read explicit data from.</param>
		/// <returns>An <see cref="ExplicitXBeeMessage"/> object containing the explicit data, the source 
		/// address of the remote node that sent the data and other values related to the transmission. 
		/// <c>null</c> if this device did not receive new explicit data from the provided remote XBee 
		/// device during the configured receive timeout.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteXBeeDevice"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="ExplicitXBeeMessage"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="AbstractXBeeDevice.ReadExplicitDataFrom(RemoteXBeeDevice)"/>
		public new ExplicitXBeeMessage ReadExplicitDataFrom(RemoteXBeeDevice remoteXBeeDevice)
		{
			return base.ReadExplicitDataFrom(remoteXBeeDevice);
		}

		/// <summary>
		/// Reads new explicit data received from the given remote XBee device during the provided 
		/// timeout.
		/// </summary>
		/// <remarks>This method blocks until new explicit data from the provided remote XBee device 
		/// is received or the given timeout expires.
		/// 
		/// For non-blocking operations, register an event handler to
		/// <see cref="AbstractXBeeDevice.ExplicitDataReceived"/>.</remarks>
		/// <param name="remoteXBeeDevice">The remote device to read explicit data from.</param>
		/// <param name="timeout">The time to wait for new explicit data in milliseconds.</param>
		/// <returns>An <see cref="ExplicitXBeeMessage"/> object containing the explicit data, the source 
		/// address of the remote node that sent the data and other values related to the transmission. 
		/// <c>null</c> if this device did not receive new data from the provided remote XBee device 
		/// during <paramref name="timeout"/> milliseconds.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="timeout"/> <![CDATA[<]]> 0</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteXBeeDevice"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="ExplicitXBeeMessage"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="AbstractXBeeDevice.ReadExplicitDataFrom(RemoteXBeeDevice, int)"/>
		public new ExplicitXBeeMessage ReadExplicitDataFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			return base.ReadExplicitDataFrom(remoteXBeeDevice, timeout);
		}

		/// <summary>
		/// Sends asynchronously the provided data in application layer mode to the XBee device of 
		/// the network corresponding to the given 64-bit/16-bit address. Application layer mode means 
		/// that you need to specify the application layer fields to be sent with the data.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for 
		/// transmit status packet.
		/// </remarks>
		/// <param name="address64Bits">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="address16Bits">The 16-bit address of the XBee that will receive the data.
		/// If it is unknown the <see cref="XBee16BitAddress.UNKNOWN_ADDRESS"/> must be used.</param>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address64Bits"/> == null</c> 
		/// or if <c><paramref name="address16Bits"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		/// <seealso cref="AbstractXBeeDevice.SendExplicitDataAsync(XBee64BitAddress, byte, byte, byte[], byte[], byte[])"/>
		public new void SendExplicitDataAsync(XBee64BitAddress address64Bits, XBee16BitAddress address16Bits,
			byte sourceEndpoint, byte destEndpoint, byte[] clusterID, byte[] profileID, byte[] data)
		{
			base.SendExplicitDataAsync(address64Bits, address16Bits, sourceEndpoint, destEndpoint, clusterID, profileID, data);
		}

		/// <summary>
		/// Sends asynchronously the provided data in application layer mode to the provided XBee 
		/// device choosing the optimal send method depending on the protocol of the local XBee device. 
		/// Application layer mode means that you need to specify the application layer fields to be 
		/// sent with the data.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or 
		/// for transmit status packet.</remarks>
		/// <param name="remoteXBeeDevice">The XBee device of the network that will receive the data.</param>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="AbstractXBeeDevice.SendExplicitDataAsync(RemoteXBeeDevice, byte, byte, byte[], byte[], byte[])"/>
		public new void SendExplicitDataAsync(RemoteXBeeDevice remoteXBeeDevice, byte sourceEndpoint, byte destEndpoint,
			byte[] clusterID, byte[] profileID, byte[] data)
		{
			base.SendExplicitDataAsync(remoteXBeeDevice, sourceEndpoint, destEndpoint, clusterID, profileID, data);
		}

		/// <summary>
		/// Sends the provided data in application layer mode to the XBee device of the network 
		/// corresponding to the given 64-bit/16-bit address. Application layer mode means that 
		/// you need to specify the application layer fields to be sent with the data.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured 
		/// receive timeout expires.</remarks>
		/// <param name="address64Bits">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="address16Bits">The 16-bit address of the XBee that will receive the data.
		/// If it is unknown the <see cref="XBee16BitAddress.UNKNOWN_ADDRESS"/> must be used.</param>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address64Bits"/> == null</c> 
		/// or if <c><paramref name="address16Bits"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="Packet.Common.TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="Packet.Raw.TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		/// <seealso cref="AbstractXBeeDevice.SendExplicitData(XBee64BitAddress, byte, byte, byte[], byte[], byte[])"/>
		public new void SendExplicitData(XBee64BitAddress address64Bits, XBee16BitAddress address16Bits,
			byte sourceEndpoint, byte destEndpoint, byte[] clusterID, byte[] profileID, byte[] data)
		{
			base.SendExplicitData(address64Bits, address16Bits, sourceEndpoint, destEndpoint, clusterID, profileID, data);
		}

		/// <summary>
		/// Sends the provided data in application layer mode to the provided XBee device choosing the 
		/// optimal send method depending on the protocol of the local XBee device. Application layer 
		/// mode means that you need to specify the application layer fields to be sent with the data.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured 
		/// receive timeout expires.</remarks>
		/// <param name="remoteXBeeDevice">The XBee device of the network that will receive the data.</param>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="Packet.Common.TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="Packet.Raw.TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="AbstractXBeeDevice.SendExplicitData(RemoteXBeeDevice, byte, byte, byte[], byte[], byte[])"/>
		public new void SendExplicitData(RemoteXBeeDevice remoteXBeeDevice, byte sourceEndpoint, byte destEndpoint,
			byte[] clusterID, byte[] profileID, byte[] data)
		{
			base.SendExplicitData(remoteXBeeDevice, sourceEndpoint, destEndpoint, clusterID, profileID, data);
		}

		/// <summary>
		/// Sends the provided data to all the XBee nodes of the network (broadcast) in application 
		/// layer mode.Application layer mode means that you need to specify the application layer 
		/// fields to be sent with the data.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured 
		/// receive timeout expires.</remarks>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="Packet.Common.TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="Packet.Raw.TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="AbstractXBeeDevice.SendBroadcastExplicitData(byte, byte, byte[], byte[], byte[])"/>
		public new void SendBroadcastExplicitData(byte sourceEndpoint, byte destEndpoint, byte[] clusterID, byte[] profileID,
			byte[] data)
		{
			base.SendBroadcastExplicitData(sourceEndpoint, destEndpoint, clusterID, profileID, data);
		}
	}
}