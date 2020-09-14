/*
 * Copyright 2019, 2020, Digi International Inc.
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
using System.Net;
using XBeeLibrary.Core.Connection;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Events.Relay;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.IP;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class provides common functionality for XBee IP devices.
	/// </summary>
	/// <seealso cref="CellularDevice"/>
	/// <seealso cref="DigiMeshDevice"/>
	/// <seealso cref="DigiPointDevice"/>
	/// <seealso cref="Raw802Device"/>
	/// <seealso cref="XBeeDevice"/>
	/// <seealso cref="ZigBeeDevice"/>
	public class IPDevice : AbstractXBeeDevice
	{
		// Constants.
		private const string BROADCAST_IP = "255.255.255.255";

		protected const short DEFAULT_SOURCE_PORT = 9750;

		protected const IPProtocol DEFAULT_PROTOCOL = IPProtocol.TCP;

		// Variables.
		protected IPAddress ipAddress;

		protected int sourcePort = DEFAULT_SOURCE_PORT;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="IPDevice"/> object in the given connection 
		/// interface.
		/// </summary>
		/// <param name="connectionInterface">The connection interface with the physical IP device.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="connectionInterface"/> == null</c>.
		/// </exception>
		/// <seealso cref="XBeeDevice(IConnectionInterface)"/>
		/// <seealso cref="IConnectionInterface"/>
		public IPDevice(IConnectionInterface connectionInterface)
			: base(connectionInterface) { }

		// Events.
		/// <summary>
		/// Represents the method that will handle the Data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="PacketReceivedEventArgs"/>
		public virtual new event EventHandler<PacketReceivedEventArgs> PacketReceived
		{
			add
			{
				base.PacketReceived += value;
			}
			remove
			{
				base.PacketReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the Modem status received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="ModemStatusReceivedEventArgs"/>
		public new event EventHandler<ModemStatusReceivedEventArgs> ModemStatusReceived
		{
			add
			{
				base.ModemStatusReceived += value;
			}
			remove
			{
				base.ModemStatusReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the User Data Relay received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="UserDataRelayReceivedEventArgs"/>
		public new event EventHandler<UserDataRelayReceivedEventArgs> UserDataRelayReceived
		{
			add
			{
				base.UserDataRelayReceived += value;
			}
			remove
			{
				base.UserDataRelayReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the Bluetooth data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="BluetoothDataReceivedEventArgs"/>
		public new event EventHandler<BluetoothDataReceivedEventArgs> BluetoothDataReceived
		{
			add
			{
				base.BluetoothDataReceived += value;
			}
			remove
			{
				base.BluetoothDataReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the MicroPython data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="MicroPythonDataReceivedEventArgs"/>
		public new event EventHandler<MicroPythonDataReceivedEventArgs> MicroPythonDataReceived
		{
			add
			{
				base.MicroPythonDataReceived += value;
			}
			remove
			{
				base.MicroPythonDataReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the IP data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="IPDataReceivedEventArgs"/>
		public new event EventHandler<IPDataReceivedEventArgs> IPDataReceived
		{
			add
			{
				base.IPDataReceived += value;
			}
			remove
			{
				base.IPDataReceived -= value;
			}
		}

		// Properties.
		/// <summary>
		/// The IP address of this IP device.
		/// </summary>
		public IPAddress IPAddress { get; private set; }

		/// <summary>
		/// The 16-bit address of this XBee device. IP devices don't have a 16-bit address, so this 
		/// property returns <c>null</c>.
		/// </summary>
		/// <remarks>To refresh this value, use the <see cref="ReadDeviceInfo"/> 
		/// method.</remarks>
		/// <seealso cref="XBee16BitAddress"/>
		public override XBee16BitAddress XBee16BitAddr
		{
			get
			{
				// IP modules do not have 16-bit address.
				return null;
			}
		}

		/// <summary>
		/// The Operating mode (AT, API or API escaped) of this XBee device for a local device, 
		/// and the operating mode of the local device used as communication interface for a remote device.
		/// </summary>
		/// <seealso cref="IsRemote"/>
		/// <seealso cref="Models.OperatingMode"/>
		public new OperatingMode OperatingMode
		{
			get
			{
				return base.OperatingMode;
			}
		}

		/// <summary>
		/// The status of the connection interface associated to this device. It indicates if the connection 
		/// is open or not.
		/// </summary>
		/// <seealso cref="Close"/>
		/// <seealso cref="Open"/>
		public new bool IsOpen => base.IsOpen;

		/// <summary>
		/// Always <c>false</c>, since this is always a local device.
		/// </summary>
		public override bool IsRemote => false;

		/// <summary>
		/// The XBee device timeout in milliseconds for received packets in synchronous operations.
		/// </summary>
		/// <exception cref="ArgumentException">If the value to be set is lesser than 0.</exception>
		public new int ReceiveTimeout
		{
			get
			{
				return base.ReceiveTimeout;
			}
			set
			{
				base.ReceiveTimeout = value;
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
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Close"/>
		public virtual new void Open()
		{
			base.Open();
		}

		/// <summary>
		/// Closes the connection interface associated with this XBee device.
		/// </summary>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Open"/>
		public new void Close()
		{
			base.Close();
		}

		/// <summary>
		/// Gets the destination IP address.
		/// </summary>
		/// <returns>The destination IP address.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="System.Net.IPAddress"/>
		public IPAddress GetDestinationIPAddress()
		{
			return new IPAddress(GetParameter("DL"));
		}

		/// <summary>
		/// Sets the Destination IP address.
		/// </summary>
		/// <param name="destAddress">The new destination <see cref="System.Net.IPAddress"/> of the device.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="destAddress"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the set configuration command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="System.Net.IPAddress"/>
		public void SetDestinationIPAddress(IPAddress destAddress)
		{
			if (destAddress == null)
				throw new ArgumentNullException("Destination IP address cannot be null.");
			SetParameter("DL", destAddress.GetAddressBytes());
		}

		/// <summary>
		/// Reads some parameters from this device and obtains its protocol.
		/// </summary>
		/// <remarks>This method refresh the values of:
		/// <list type="bullet">
		/// <item><description>64-bit address only if it is not initialized.</description></item>
		/// <item><description>Node Identifier.</description></item>
		/// <item><description>Hardware version if it is not initialized.</description></item>
		/// <item><description>Firmware version.</description></item>
		/// <item><description>XBee device protocol.</description></item>
		/// <item><description>IP address.</description></item>
		/// </list></remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the parameters.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="System.Net.IPAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		/// <seealso cref="HardwareVersion"/>
		/// <seealso cref="Models.XBeeProtocol"/>
		public override void ReadDeviceInfo()
		{
			base.ReadDeviceInfo();

			// Read the module's IP address.
			byte[] response = GetParameter("MY");
			IPAddress = new IPAddress(response);
		}

		/// <summary>
		/// Gets the next Frame ID of this XBee device.
		/// </summary>
		/// <returns>The next Frame ID.</returns>
		public new byte GetNextFrameID()
		{
			return base.GetNextFrameID();
		}

		/// <summary>
		/// Sends the provided data to the given XBee local interface.
		/// </summary>
		/// <param name="destinationInterface">Destination XBee local interface.</param>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If the destination interface is unknown.</exception>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error sending the User
		/// Data Relay.</exception>
		/// <seealso cref="XBeeLocalInterface"/>
		/// <seealso cref="SendBluetoothData(byte[])"/>
		/// <seealso cref="SendMicroPythonData(byte[])"/>
		public new void SendUserDataRelay(XBeeLocalInterface destinationInterface, byte[] data)
		{
			base.SendUserDataRelay(destinationInterface, data);
		}

		/// <summary>
		/// Sends the given data to the XBee Bluetooth interface in a User Data Relay frame.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error sending the Bluetooth
		/// data.</exception>
		/// <seealso cref="SendMicroPythonData(byte[])"/>
		/// <seealso cref="SendUserDataRelay(XBeeLocalInterface, byte[])"/>
		public new void SendBluetoothData(byte[] data)
		{
			base.SendBluetoothData(data);
		}

		/// <summary>
		/// Sends the given data to the XBee MicroPython interface in a User Data Relay frame.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error sending the
		/// MicroPython data.</exception>
		/// <seealso cref="SendBluetoothData(byte[])"/>
		/// <seealso cref="SendUserDataRelay(XBeeLocalInterface, byte[])"/>
		public new void SendMicroPythonData(byte[] data)
		{
			base.SendMicroPythonData(data);
		}

		/// <summary>
		/// Sends the given XBee packet and registers the given packet handler (if not <c>null</c>) to 
		/// manage what happens when the answers is received.
		/// </summary>
		/// <remarks>This is a non-blocking operation. To wait for the answer use 
		/// <see cref="SendPacket(XBeePacket)"/>.</remarks>
		/// <param name="packet">XBee packet to be sent.</param>
		/// <param name="handler">Event handler for the operation, <c>null</c> not to be notified when 
		/// the answer arrives.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packet"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode is different from 
		/// <see cref="OperatingMode.API"/> and <see cref="OperatingMode.API_ESCAPE"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SendPacket(XBeePacket)"/>
		/// <seealso cref="SendPacketAsync(XBeePacket)"/>
		/// <seealso cref="PacketReceivedEventArgs"/>
		/// <seealso cref="XBeePacket"/>
		public new void SendPacket(XBeePacket packet, EventHandler<PacketReceivedEventArgs> handler)
		{
			base.SendPacket(packet, handler);
		}

		/// <summary>
		/// Sends the given XBee packet asynchronously.
		/// </summary>
		/// <remarks>To be notified when the answer is received, use the <see cref="PacketReceived"/> 
		/// event handler.</remarks>
		/// <param name="packet">The XBee packet to be sent asynchronously.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packet"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode is different from 
		/// <see cref="OperatingMode.API"/> and <see cref="OperatingMode.API_ESCAPE"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SendPacket(XBeePacket)"/>
		/// <seealso cref="SendPacket(XBeePacket, EventHandler{PacketReceivedEventArgs})"/>
		/// <seealso cref="XBeePacket"/>
		public new void SendPacketAsync(XBeePacket packet)
		{
			base.SendPacketAsync(packet);
		}

		/// <summary>
		/// Sends the given XBee packet synchronously and blocks until the response is received or 
		/// the configured receive timeout expires.
		/// </summary>
		/// <remarks>The receive timeout is consulted/configured using the <see cref="ReceiveTimeout"/>property.
		/// 
		/// Use <see cref="SendPacketAsync(XBeePacket)"/> or 
		/// <see cref="SendPacket(XBeePacket, EventHandler{PacketReceivedEventArgs})"/> for non-blocking 
		/// operations.</remarks>
		/// <param name="packet">The XBee packet to be sent.</param>
		/// <returns>An <see cref="XBeePacket"/> object containing the response of the sent packet or 
		/// <c>null</c> if there is no response.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packet"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode is different from 
		/// <see cref="OperatingMode.API"/> and <see cref="OperatingMode.API_ESCAPE"/>.</exception>
		/// <exception cref="TimeoutException">If the configured time expires while waiting for the 
		/// packet reply.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="SendPacket(XBeePacket, EventHandler{PacketReceivedEventArgs})"/>
		/// <seealso cref="SendPacketAsync(XBeePacket)"/>
		/// <seealso cref="XBeePacket"/>
		public new XBeePacket SendPacket(XBeePacket packet)
		{
			return base.SendPacket(packet);
		}

		/// <summary>
		/// Performs a software reset on this XBee device and blocks until the process is 
		/// completed.
		/// </summary>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout resetting the device.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public override void Reset()
		{
			SoftwareReset();
		}

		/// <summary>
		/// Reads a new User Data Relay packet received by this XBee device during the configured receive 
		/// timeout.
		/// </summary>
		/// <remarks>This method blocks until new User Data Relay is received or the configured receive 
		/// timeout expires.</remarks>
		/// <returns>A <see cref="UserDataRelayMessage"/> object containing the source interface and data. 
		/// <c>null</c> if this device did not receive new User Data Relay during the configured receive 
		/// timeout.</returns>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="UserDataRelayMessage"/>
		public new UserDataRelayMessage ReadUserDataRelay()
		{
			return base.ReadUserDataRelay();
		}

		/// <summary>
		/// Reads a new User Data Relay packet received by this XBee device during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new User Data Relay is received or the given timeout 
		/// expires.</remarks>
		/// <param name="timeout">The time to wait for new User Data Relay in 
		/// milliseconds.</param>
		/// <returns>A <see cref="UserDataRelayMessage"/> object containing the source interface and data. 
		/// <c>null</c> if this device did not receive new User Data Relay during <paramref name="timeout"/> 
		/// milliseconds.</returns>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="UserDataRelayMessage"/>
		public new UserDataRelayMessage ReadUserDataRelay(int timeout)
		{
			return base.ReadUserDataRelay(timeout);
		}

		/// <summary>
		/// Starts listening for incoming IP transmissions in the provided port.
		/// </summary>
		/// <param name="sourcePort">Port to listen for incoming transmissions.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="sourcePort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="sourcePort"/> <![CDATA[>]]> 65535</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the set configuration command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public void StartListening(int sourcePort)
		{
			if (sourcePort < 0 || sourcePort > 65535)
				throw new ArgumentException("Source port must be between 0 and 65535.");

			SetParameter("C0", ByteUtils.ShortToByteArray((short)sourcePort));
			this.sourcePort = sourcePort;
		}

		/// <summary>
		/// Stops listening for incoming IP transmissions.
		/// </summary>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the set configuration command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public void StopListening()
		{
			SetParameter("C0", ByteUtils.ShortToByteArray((short)0));
			sourcePort = 0;
		}

		/// <summary>
		/// Sends the provided IP data to the given IP address and port using the specified IP protocol. For 
		/// TCP and TCP SSL protocols, you can also indicate if the socket should be closed when data is sent.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="XBeeDevice.ReceiveTimeout"/> 
		/// property.
		/// 
		/// For non-blocking operations use the method 
		/// <see cref="SendIPDataAsync(IPAddress, int, IPProtocol, bool, byte[])"/>.</remarks>
		/// <param name="ipAddress">The IP address to send IP data to.</param>
		/// <param name="destPort">The destination port of the transmission.</param>
		/// <param name="protocol">The IP protocol used for the transmission.</param>
		/// <param name="closeSocket"><c>true</c> to close the socket just after the transmission. <c>false</c> 
		/// to keep it open.</param>
		/// <param name="data">Byte array containing the IP data to be sent.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c> 
		/// or if <c><paramref name="protocol"/> == <see cref="IPProtocol.UNKNOWN"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ipAddress"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperationException">If the device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sendting the 
		/// data is not an instance of <see cref="Packet.Common.TransmitStatusPacket"/> 
		/// or if its transmit status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBeeDevice.ReceiveTimeout"/>
		/// <seealso cref="SendIPDataImpl(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="SendBroadcastIPData(int, byte[])"/>
		/// <seealso cref="SendIPData(IPAddress, int, IPProtocol, byte[])"/>
		/// <seealso cref="SendIPDataAsync(IPAddress, int, IPProtocol, byte[])"/>
		/// <seealso cref="SendIPDataAsync(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="IPProtocol"/>
		/// <seealso cref="System.Net.IPAddress"/>
		public void SendIPData(IPAddress ipAddress, int destPort, IPProtocol protocol, bool closeSocket, byte[] data)
		{
			SendIPDataImpl(ipAddress, destPort, protocol, closeSocket, data);
		}

		/// <summary>
		/// Sends the provided IP data to the given IP address and port using the specified IP protocol.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="XBeeDevice.ReceiveTimeout"/> 
		/// property.
		/// 
		/// For non-blocking operations use the method 
		/// <see cref="SendIPDataAsync(IPAddress, int, IPProtocol, byte[])"/>.</remarks>
		/// <param name="ipAddress">The IP address to send IP data to.</param>
		/// <param name="destPort">The destination port of the transmission.</param>
		/// <param name="protocol">The IP protocol used for the transmission.</param>
		/// <param name="data">Byte array containing the IP data to be sent.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c>
		/// or if <c><paramref name="protocol"/> == <see cref="IPProtocol.UNKNOWN"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ipAddress"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperationException">If the device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sendting the 
		/// data is not an instance of <see cref="Packet.Common.TransmitStatusPacket"/> 
		/// or if its transmit status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBeeDevice.ReceiveTimeout"/>
		/// <seealso cref="SendIPDataImpl(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="SendBroadcastIPData(int, byte[])"/>
		/// <seealso cref="SendIPData(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="SendIPDataAsync(IPAddress, int, IPProtocol, byte[])"/>
		/// <seealso cref="SendIPDataAsync(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="IPProtocol"/>
		/// <seealso cref="System.Net.IPAddress"/>
		public void SendIPData(IPAddress ipAddress, int destPort, IPProtocol protocol, byte[] data)
		{
			SendIPDataImpl(ipAddress, destPort, protocol, false, data);
		}

		/// <summary>
		/// Sends the provided IP data to the given IP address and port asynchronously using the specified 
		/// IP protocol. For TCP and TCP SSL protocols, you can also indicate if the socket should be closed 
		/// when data is sent.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for transmit 
		/// status packet.</remarks>
		/// <param name="ipAddress">The IP address to send IP data to.</param>
		/// <param name="destPort">The destination port of the transmission.</param>
		/// <param name="protocol">The IP protocol used for the transmission.</param>
		/// <param name="closeSocket"><c>true</c> to close the socket just after the transmission. <c>false</c> 
		/// to keep it open.</param>
		/// <param name="data">Byte array containing the IP data to be sent.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c>
		/// or if <c><paramref name="protocol"/> == <see cref="IPProtocol.UNKNOWN"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ipAddress"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperationException">If the device is remote.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SendIPDataAsyncImpl(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="SendBroadcastIPData(int, byte[])"/>
		/// <seealso cref="SendIPData(IPAddress, int, IPProtocol, byte[])"/>
		/// <seealso cref="SendIPData(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="SendIPDataAsync(IPAddress, int, IPProtocol, byte[])"/>
		/// <seealso cref="IPProtocol"/>
		/// <seealso cref="System.Net.IPAddress"/>
		public void SendIPDataAsync(IPAddress ipAddress, int destPort, IPProtocol protocol, bool closeSocket, byte[] data)
		{
			SendIPDataAsyncImpl(ipAddress, destPort, protocol, closeSocket, data);
		}

		/// <summary>
		/// Sends the provided IP data to the given IP address and port asynchronously using the 
		/// specified IP protocol.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for 
		/// transmit status packet.</remarks>
		/// <param name="ipAddress">The IP address to send IP data to.</param>
		/// <param name="destPort">The destination port of the transmission.</param>
		/// <param name="protocol">The IP protocol used for the transmission.</param>
		/// <param name="data">Byte array containing the IP data to be sent.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c>
		/// or if <c><paramref name="protocol"/> == <see cref="IPProtocol.UNKNOWN"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ipAddress"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperationException">If the device is remote.</exception>
		/// <seealso cref="SendIPDataAsyncImpl(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="SendBroadcastIPData(int, byte[])"/>
		/// <seealso cref="SendIPData(IPAddress, int, IPProtocol, byte[])"/>
		/// <seealso cref="SendIPData(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="SendIPDataAsync(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="IPProtocol"/>
		/// <seealso cref="System.Net.IPAddress"/>
		public void SendIPDataAsync(IPAddress ipAddress, int destPort, IPProtocol protocol, byte[] data)
		{
			SendIPDataAsyncImpl(ipAddress, destPort, protocol, false, data);
		}

		/// <summary>
		/// Sends the provided IP data to all clients.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="XBeeDevice.ReceiveTimeout"/> 
		/// property.</remarks>
		/// <param name="destPort">The destination port of the transmission.</param>
		/// <param name="data">Byte array containing the IP data to be sent.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperationException">If the device is remote.</exception>
		/// <exception cref="TransmitException"> If the transmit status generated when sending 
		/// the packet is not an instance of <see cref="Packet.Common.TransmitStatusPacket"/> 
		/// or if its transmit status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBeeDevice.ReceiveTimeout"/>
		/// <seealso cref="SendIPData(IPAddress, int, IPProtocol, byte[])"/>
		/// <seealso cref="SendIPData(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="SendIPDataAsync(IPAddress, int, IPProtocol, byte[])"/>
		/// <seealso cref="SendIPDataAsync(IPAddress, int, IPProtocol, bool, byte[])"/>
		/// <seealso cref="IPProtocol"/>
		/// <seealso cref="System.Net.IPAddress"/>
		public void SendBroadcastIPData(int destPort, byte[] data)
		{
			SendIPData(IPAddress.Parse(BROADCAST_IP), destPort, IPProtocol.UDP, false, data);
		}

		/// <summary>
		/// Reads new IP data received by this XBee device during the configured receive timeout.
		/// </summary>
		/// <remarks>This method blocks until new IP data is received or the configured receive 
		/// timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="XBeeDevice.ReceiveTimeout"/> 
		/// property.
		/// 
		/// Before reading IP data you need to start listening for incoming IP data at a specific port. 
		/// Use the <see cref="StartListening(int)"/> method for that purpose. When finished, you can use 
		/// the <see cref="StopListening"/> method to stop listening for incoming IP data.</remarks>
		/// <returns>An <see cref="IPMessage"/> object containing the IP data and the IP address that 
		/// sent the data. <c>null</c> if this did not receive new IP data during the configured receive 
		/// timeout.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="XBeeDevice.ReceiveTimeout"/>
		/// <seealso cref="ReadIPDataPacket(IPAddress, int)"/>
		/// <seealso cref="ReadIPData(int)"/>
		/// <seealso cref="ReadIPDataFrom(IPAddress)"/>
		/// <seealso cref="ReadIPDataFrom(IPAddress, int)"/>
		/// <seealso cref="StartListening(int)"/>
		/// <seealso cref="StopListening"/>
		/// <seealso cref="IPMessage"/>
		public IPMessage ReadIPData()
		{
			return ReadIPDataPacket(null, TIMEOUT_READ_PACKET);
		}

		/// <summary>
		/// Reads new IP data received by this XBee device during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new IP data is received or the configured receive 
		/// timeout expires.</remarks>
		/// <returns>A <see cref="IPMessage"/> object containing the IP data and the IP address that 
		/// sent the data. <c>null</c> if this did not receive new IP data during <c>timeout</c> 
		/// milliseconds.</returns>
		/// <param name="timeout">The time to wait for new IP data in milliseconds.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="timeout"/> <![CDATA[<]]> 0</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="IPMessage"/>
		public IPMessage ReadIPData(int timeout)
		{
			if (timeout < 0)
				throw new ArgumentException("Read timeout must be 0 or greater.");
			return ReadIPDataPacket(null, timeout);
		}

		/// <summary>
		/// Reads new IP data received from the given IP address during the configured receive timeout.
		/// </summary>
		/// <remarks>This method blocks until new IP data is received or the configured receive timeout 
		/// expires.</remarks>
		/// <returns>A <see cref="IPMessage"/> object containing the IP data and the IP address that sent 
		/// the data. <c>null</c> if this did not receive new IP data during the configured receive 
		/// timeout.</returns>
		/// <param name="ipAddress">The IP address to read data from.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ipAddress"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="IPMessage"/>
		/// <seealso cref="System.Net.IPAddress"/>
		public IPMessage ReadIPDataFrom(IPAddress ipAddress)
		{
			if (ipAddress == null)
				throw new ArgumentNullException("IP address cannot be null.");

			return ReadIPDataPacket(ipAddress, TIMEOUT_READ_PACKET);
		}

		/// <summary>
		/// Reads new IP data received from the given IP address during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new IP data is received or the configured receive 
		/// timeout expires.</remarks>
		/// <returns>A <see cref="IPMessage"/> object containing the IP data and the IP address that 
		/// sent the data. <c>null</c> if this did not receive new IP data during <c>timeout</c> 
		/// milliseconds.</returns>
		/// <param name="ipAddress">The IP address to read data from.</param>
		/// <param name="timeout">The time to wait for new IP data in milliseconds.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="timeout"/> <![CDATA[<]]> 0</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ipAddress"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="IPMessage"/>
		/// <seealso cref="System.Net.IPAddress"/>
		public IPMessage ReadIPDataFrom(IPAddress ipAddress, int timeout)
		{
			if (ipAddress == null)
				throw new ArgumentNullException("IP address cannot be null.");
			if (timeout < 0)
				throw new ArgumentException("Read timeout must be 0 or greater.");

			return ReadIPDataPacket(ipAddress, timeout);
		}

		/// <summary>
		/// Reads a new IP data packet received by this IP XBee device during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new IP data is received or the given timeout expires.</remarks>
		/// <param name="remoteIPAddress">The IP address to get a IP data packet from. <c>null</c> to 
		/// read a IP data packet from any IP address.</param>
		/// <param name="timeout">The time to wait for a IP data packet in milliseconds.</param>
		/// <returns>A <see cref="IPMessage"/> object containing the IP data and the IP address that 
		/// sent the data. <c>null</c> if this did not receive new IP data during <c>timeout</c> 
		/// milliseconds,or if any error occurs while trying to get the source of the message.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="IPMessage"/>
		/// <seealso cref="System.Net.IPAddress"/>
		private IPMessage ReadIPDataPacket(IPAddress remoteIPAddress, int timeout)
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			XBeePacketsQueue xbeePacketsQueue = dataReader.XBeePacketsQueue;
			XBeePacket xbeePacket = null;

			if (remoteIPAddress != null)
				xbeePacket = xbeePacketsQueue.GetFirstIPDataPacketFrom(remoteIPAddress, timeout);
			else
				xbeePacket = xbeePacketsQueue.GetFirstIPDataPacket(timeout);

			if (xbeePacket == null)
				return null;

			// Obtain the data and IP address from the packet.
			byte[] data = null;
			IPAddress ipAddress = null;
			int sourcePort;
			int destPort;
			IPProtocol protocol = IPProtocol.TCP;

			switch ((xbeePacket as XBeeAPIPacket).FrameType)
			{
				case APIFrameType.RX_IPV4:
					RXIPv4Packet receivePacket = (RXIPv4Packet)xbeePacket;
					data = receivePacket.Data;
					ipAddress = receivePacket.SourceAddress;
					sourcePort = receivePacket.SourcePort;
					destPort = receivePacket.DestPort;
					break;
				default:
					return null;
			}

			// Create and return the IP message.
			return new IPMessage(ipAddress, sourcePort, destPort, protocol, data);
		}

		/// <summary>
		/// Sends the provided IP data to the given IP address and port using the specified IP protocol. 
		/// For TCP and TCP SSL protocols, you can  also indicate if the socket should be closed when data 
		/// is sent.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.</remarks>
		/// <param name="ipAddress">The IP address to send IP data to.</param>
		/// <param name="destPort">The destination port of the transmission.</param>
		/// <param name="protocol">The IP protocol used for the transmission.</param>
		/// <param name="closeSocket"><c>true</c> to close the socket just after the transmission. <c>false</c> 
		/// to keep it open.</param>
		/// <param name="data">Byte array containing the IP data to be sent.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c> 
		/// or if <c><paramref name="protocol"/> == <see cref="IPProtocol.UNKNOWN"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ipAddress"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperationException">If the device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the 
		/// data is not an instance of <see cref="Packet.Common.TransmitStatusPacket"/> 
		/// or if its transmit status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="IPProtocol"/>
		/// <seealso cref="System.Net.IPAddress"/>
		private void SendIPDataImpl(IPAddress ipAddress, int destPort, 
			IPProtocol protocol, bool closeSocket, byte[] data)
		{
			if (ipAddress == null)
				throw new ArgumentNullException("IP address cannot be null");
			if (protocol == IPProtocol.UNKNOWN)
				throw new ArgumentException("Protocol cannot be unknown");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			if (destPort < 0 || destPort > 65535)
				throw new ArgumentException("Destination port must be between 0 and 65535.");

			// Check if device is remote.
			if (IsRemote)
				throw new InvalidOperationException("Cannot send IP data from a remote device.");

			// The source port value depends on the protocol used in the transmission. For UDP, source port 
			// value must be the same as 'C0' one. For TCP it must be 0.
			int sourcePort = protocol == IPProtocol.UDP ? this.sourcePort : 0;

			logger.DebugFormat(ToString() + "Sending IP data to {}:{} >> {}.", ipAddress, destPort, HexUtils.PrettyHexString(data));
			XBeePacket xbeePacket = new TXIPv4Packet(GetNextFrameID(), ipAddress, destPort,
					sourcePort, protocol, closeSocket ? TXIPv4Packet.OPTIONS_CLOSE_SOCKET : TXIPv4Packet.OPTIONS_LEAVE_SOCKET_OPEN, data);

			SendAndCheckXBeePacket(xbeePacket, false);
		}

		/// <summary>
		/// Sends the provided IP data to the given IP address and port asynchronously using the specified IP 
		/// protocol. For TCP and TCP SSL protocols, you can also indicate if the socket should be closed when 
		/// data is sent.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for transmit 
		/// status packet.</remarks>
		/// <param name="ipAddress">The IP address to send IP data to.</param>
		/// <param name="destPort">The destination port of the transmission.</param>
		/// <param name="protocol">The IP protocol used for the transmission.</param>
		/// <param name="closeSocket"><c>true</c> to close the socket just after the transmission. <c>false</c> 
		/// to keep it open.</param>
		/// <param name="data">Byte array containing the IP data to be sent.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="destPort"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="destPort"/> <![CDATA[>]]> 65535</c>
		/// or if <c><paramref name="protocol"/> == <see cref="IPProtocol.UNKNOWN"/></c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ipAddress"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperationException">If the device is remote.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="IPProtocol"/>
		/// <seealso cref="System.Net.IPAddress"/>
		private void SendIPDataAsyncImpl(IPAddress ipAddress, int destPort,
			IPProtocol protocol, bool closeSocket, byte[] data)
		{
			if (ipAddress == null)
				throw new ArgumentNullException("IP address cannot be null");
			if (protocol == IPProtocol.UNKNOWN)
				throw new ArgumentException("Protocol cannot be unknown");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			if (destPort < 0 || destPort > 65535)
				throw new ArgumentException("Destination port must be between 0 and 65535.");

			// Check if device is remote.
			if (IsRemote)
				throw new InvalidOperationException("Cannot send IP data from a remote device.");

			// The source port value depends on the protocol used in the transmission. For UDP, source port 
			// value must be the same as 'C0' one. For TCP it must be 0.
			int sourcePort = protocol == IPProtocol.UDP ? this.sourcePort : 0;

			logger.DebugFormat(ToString() + "Sending IP data asynchronously to {}:{} >> {}.", ipAddress, destPort, HexUtils.PrettyHexString(data));
			XBeePacket xbeePacket = new TXIPv4Packet(GetNextFrameID(), ipAddress, destPort,
					sourcePort, protocol, closeSocket ? TXIPv4Packet.OPTIONS_CLOSE_SOCKET : TXIPv4Packet.OPTIONS_LEAVE_SOCKET_OPEN, data);

			SendAndCheckXBeePacket(xbeePacket, true);
		}
	}
}
