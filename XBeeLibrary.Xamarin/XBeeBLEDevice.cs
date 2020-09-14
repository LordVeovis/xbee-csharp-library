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

using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Events.Relay;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.IO;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet;

namespace XBeeLibrary.Xamarin
{
	/// <summary>
	/// This class represents an XBee Bluetooth Low Energy (BLE) device.
	/// </summary>
	/// <seealso cref="CellularBLEDevice"/>
	/// <seealso cref="DigiMeshBLEDevice"/>
	/// <seealso cref="Raw802BLEDevice"/>
	/// <seealso cref="ZigBeeBLEDevice"/>
	public class XBeeBLEDevice : Core.AbstractXBeeDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeBLEDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Open"/> method,
		/// either through this constructor or the <see cref="SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="device">Bluetooth device to connect to.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <seealso cref="IDevice"/>
		public XBeeBLEDevice(IDevice device, string password)
			: base(XBee.CreateConnectionInterface(device))
		{
			bluetoothPassword = password;
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeBLEDevice"/> object with the given 
		/// parameters.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Open"/> method,
		/// either through this constructor or the <see cref="SetBluetoothPassword(string)"/> method.
		/// </remarks>
		/// <param name="deviceAddress">The address of the Bluetooth device. It must follow the
		/// format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c>.</param>
		/// <param name="password">Bluetooth password (can be <c>null</c>).</param>
		/// <exception cref="ArgumentException">If <paramref name="deviceAddress"/> does not follow
		/// the format <c>00112233AABB</c> or <c>00:11:22:33:AA:BB</c>.</exception>
		public XBeeBLEDevice(string deviceAddress, string password)
			: base(XBee.CreateConnectionInterface(deviceAddress))
		{
			bluetoothPassword = password;
		}

		// Events.
		/// <summary>
		/// Represents the method that will handle the Data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="PacketReceivedEventArgs"/>
		public new event EventHandler<PacketReceivedEventArgs> PacketReceived
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
		/// Represents the method that will handle the serial data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="SerialDataReceivedEventArgs"/>
		public new event EventHandler<SerialDataReceivedEventArgs> SerialDataReceived
		{
			add
			{
				base.SerialDataReceived += value;
			}
			remove
			{
				base.SerialDataReceived -= value;
			}
		}

		// Properties.
		/// <summary>
		/// Indicates whether this XBee device is a remote device.
		/// </summary>
		public override bool IsRemote => false;

		/// <summary>
		/// The status of the connection interface associated to this device. It indicates if the connection 
		/// is open or not.
		/// </summary>
		/// <seealso cref="Close"/>
		/// <seealso cref="Open"/>
		public new bool IsOpen => base.IsOpen;

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
		/// Performs a software reset on this XBee device and blocks until the process is 
		/// completed.
		/// </summary>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout resetting the device.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public override void Reset()
		{
			SoftwareReset();
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
		/// <exception cref="Core.Exceptions.TimeoutException">If the timeout to read settings when initializing the 
		/// device elapses without response.</exception>
		/// <exception cref="XBeeException">If there is any problem opening this device connection.</exception>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Close"/>
		public new void Open()
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
		/// Gets the next Frame ID of this XBee device.
		/// </summary>
		/// <returns>The next Frame ID.</returns>
		public new byte GetNextFrameID()
		{
			return base.GetNextFrameID();
		}

		/// <summary>
		/// Returns the 64-bit destination extended address of this XBee device.
		/// </summary>
		/// <remarks><see cref="XBee64BitAddress.BROADCAST_ADDRESS"/> is the broadcast address for the 
		/// PAN. <see cref="XBee64BitAddress.COORDINATOR_ADDRESS"/> can be used to address the Pan 
		/// Coordinator.</remarks>
		/// <returns>64-bit destination address.</returns>
		/// <exception cref="ATCommandEmptyException">If <c>DH</c> or <c>DL</c> values are empty.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout reading the destination address.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="XBee64BitAddress"/>
		public new XBee64BitAddress GetDestinationAddress()
		{
			return base.GetDestinationAddress();
		}

		/// <summary>
		/// Sets the 64-bit destination extended address of this XBee device.
		/// </summary>
		/// <remarks><see cref="XBee64BitAddress.BROADCAST_ADDRESS"/> is the broadcast address for 
		/// the PAN. <see cref="XBee64BitAddress.COORDINATOR_ADDRESS"/> can be used to address the 
		/// Pan Coordinator.</remarks>
		/// <param name="xbee64BitAddress">64-bit destination address to be configured.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="xbee64BitAddress"/> is <c>null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout setting the destination address.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public new void SetDestinationAddress(XBee64BitAddress xbee64BitAddress)
		{
			base.SetDestinationAddress(xbee64BitAddress);
		}

		/// <summary>
		/// Returns the operating PAN ID (Personal Area Network Identifier) of this XBee device.
		/// </summary>
		/// <remarks>For modules to communicate they must be configured with the same identifier. Only modules 
		/// with matching IDs can communicate with each other. This parameter allows multiple networks to 
		/// co-exist on the same physical channel.</remarks>
		/// <returns>The operating PAN ID of this XBee device.</returns>
		/// <exception cref="ATCommandEmptyException">If the <c>ID</c> command returns a <c>null</c> or an 
		/// empty value.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout reading the operating PAN ID.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SetPANID(byte[])"/>
		public new byte[] GetPANID()
		{
			return base.GetPANID();
		}

		/// <summary>
		/// Sets the PAN ID (Personal Area Network Identifier) of this XBee device.
		/// </summary>
		/// <remarks>For modules to communicate they must be configured with the same identifier. Only 
		/// modules with matching IDs can communicate with each other. This parameter allows multiple 
		/// networks to co-exist on the same physical channel.</remarks>
		/// <param name="panID">The new PAN ID of this XBee device.</param>
		/// <exception cref="ArgumentException">If the length of <paramref name="panID"/> is <c>0</c> 
		/// or if <c><paramref name="panID"/> <![CDATA[>]]> 8</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="panID"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout setting the operating PAN ID.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetPANID"/>
		public new void SetPANID(byte[] panID)
		{
			base.SetPANID(panID);
		}

		/// <summary>
		/// Returns the set of IO lines of this device that are monitored for change detection.
		/// </summary>
		/// <remarks>A <c>null</c> set means the DIO change detection feature is disabled.
		/// 
		/// Modules can be configured to transmit to the configured destination address a data sample 
		/// immediately whenever a monitored digital IO line changes state.</remarks>
		/// <returns>Set of digital IO lines that are monitored for change detection, <c>null</c> if 
		/// there are no monitored lines.</returns>
		/// <exception cref="ATCommandEmptyException">If <c>IC</c> value is empty.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout reading the IO change detect command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="SetDIOChangeDetection(ISet{IOLine})"/>
		/// <seealso cref="IOLine"/>
		public new ISet<IOLine> GetDIOChangeDetection()
		{
			return base.GetDIOChangeDetection();
		}

		/// <summary>
		/// Sets the digital IO lines of this XBee device to be monitored and sampled whenever their status 
		/// changes.
		/// </summary>
		/// <remarks>A <c>null</c> set of lines disables this feature.
		/// 
		/// If a change is detected on an enabled digital IO pin, a digital IO sample is immediately 
		/// transmitted to the configured destination address.
		/// 
		/// The destination address can be configured using the <see cref="SetDestinationAddress(XBee64BitAddress)"/> 
		/// method and retrieved by <see cref="GetDestinationAddress"/>.</remarks>
		/// <param name="lines">Set of IO lines to be monitored, <c>null</c> to disable this feature.</param>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout sending the set IO change detect 
		/// command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="GetDIOChangeDetection"/>
		public new void SetDIOChangeDetection(ISet<IOLine> lines)
		{
			base.SetDIOChangeDetection(lines);
		}

		/// <summary>
		/// Returns the IO sampling rate of this XBee device.
		/// </summary>
		/// <remarks>A sample rate of <c>0</c> ms. means the IO sampling feature is disabled.
		/// 
		/// Periodic sampling allows this XBee module to take an IO sample and transmit it to a remote 
		/// device (configured in the destination address) at the configured periodic rate (ms).</remarks>
		/// <returns>IO sampling rate in milliseconds.</returns>
		/// <exception cref="ATCommandEmptyException">If <c>IR</c> value is empty.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout reading the IO sampling rate command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="SetIOSamplingRate(int)"/>
		public new int GetIOSamplingRate()
		{
			return base.GetIOSamplingRate();
		}

		/// <summary>
		/// Sets the IO sampling rate to enable periodic sampling in this XBee device.
		/// </summary>
		/// <remarks>A sample rate of <c>0</c> ms. disables this feature.
		/// 
		/// All enabled digital IO and analog inputs will be sampled and transmitted every <paramref name="rate"/> 
		/// milliseconds to the configured destination address.</remarks>
		/// <param name="rate">IO sampling rate in milliseconds.</param>
		/// <exception cref="ArgumentException">If <c><paramref name="rate"/> <![CDATA[<]]> 0</c> 
		/// or if <c><paramref name="rate"/> <![CDATA[>]]> 0xFFFF</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout sending the set IO sampling rate 
		/// command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="GetIOSamplingRate"/>
		public new void SetIOSamplingRate(int rate)
		{
			base.SetIOSamplingRate(rate);
		}

		/// <summary>
		/// Returns the output power level at which this XBee device transmits conducted power.
		/// </summary>
		/// <returns>The output power level of this XBee device.</returns>
		/// <exception cref="ATCommandEmptyException">If the <c>PL</c> command returns a <c>null</c> or an 
		/// empty value.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout reading the power level command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SetPowerLevel(PowerLevel)"/>
		/// <seealso cref="PowerLevel"/>
		public new PowerLevel GetPowerLevel()
		{
			return base.GetPowerLevel();
		}

		/// <summary>
		/// Sets the output power level at which this XBee device transmits conducted power.
		/// </summary>
		/// <param name="powerLevel">The new output power level to be set in this XBee device.</param>
		/// <exception cref="ArgumentException">If <paramref name="powerLevel"/>
		/// is <see cref="PowerLevel.LEVEL_UNKNOWN"/>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="Core.Exceptions.TimeoutException">If there is a timeout setting the power level command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetPowerLevel"/>
		/// <seealso cref="PowerLevel"/>
		public new void SetPowerLevel(PowerLevel powerLevel)
		{
			base.SetPowerLevel(powerLevel);
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
		/// <seealso cref="SendMicroPythonData(byte[])"/>
		/// <seealso cref="SendSerialData(byte[])"/>
		public new void SendUserDataRelay(XBeeLocalInterface destinationInterface, byte[] data)
		{
			base.SendUserDataRelay(destinationInterface, data);
		}

		/// <summary>
		/// Sends the given data to the XBee MicroPython interface in a User Data Relay frame.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error sending the
		/// MicroPython data.</exception>
		/// <seealso cref="SendSerialData(byte[])"/>
		/// <seealso cref="SendUserDataRelay(XBeeLocalInterface, byte[])"/>
		public new void SendMicroPythonData(byte[] data)
		{
			base.SendMicroPythonData(data);
		}

		/// <summary>
		/// Sends the given data to the XBee serial interface in a User Data Relay frame.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error sending the serial
		/// data.</exception>
		/// <seealso cref="SendMicroPythonData(byte[])"/>
		/// <seealso cref="SendUserDataRelay(XBeeLocalInterface, byte[])"/>
		public new void SendSerialData(byte[] data)
		{
			base.SendSerialData(data);
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
		/// <exception cref="Core.Exceptions.TimeoutException">If the configured time expires while waiting for the 
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
		/// Sets the password of this Bluetooth device in order to connect to it.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Open"/> method.
		/// </remarks>
		/// <param name="password">The password of this Bluetooth device.</param>
		public new void SetBluetoothPassword(string password)
		{
			base.SetBluetoothPassword(password);
		}

		/// <summary>
		/// Returns the string representation of this device.
		/// </summary>
		/// <returns>The string representation of this device.</returns>
		public override string ToString()
		{
			return base.ToString();
		}
	}
}
