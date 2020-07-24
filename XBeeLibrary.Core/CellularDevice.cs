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
using XBeeLibrary.Core.Connection;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.Cellular;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class represents a local Cellular device.
	/// </summary>
	/// <see cref="DigiMeshDevice"/>
	/// <see cref="DigiPointDevice"/>
	/// <see cref="Raw802Device"/>
	/// <see cref="XBeeDevice"/>
	/// <see cref="ZigBeeDevice"/>
	public class CellularDevice : IPDevice
	{
		/// <summary>
		/// Class constructor. Instantiates a new <see cref="CellularDevice"/> object with the given 
		/// connection interface.
		/// </summary>
		/// <param name="connectionInterface">The connection interface with the physical 
		/// Cellular device.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="connectionInterface"/> == null</c>.
		/// </exception>
		/// <seealso cref="XBeeDevice(IConnectionInterface)"/>
		/// <seealso cref="IConnectionInterface"/>
		public CellularDevice(IConnectionInterface connectionInterface)
			: base(connectionInterface) { }

		// Events.
		/// <summary>
		/// Represents the method that will handle the SMS received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="SMSReceivedEventArgs"/>
		public new event EventHandler<SMSReceivedEventArgs> SMSReceived
		{
			add
			{
				base.SMSReceived += value;
			}
			remove
			{
				base.SMSReceived -= value;
			}
		}

		// Properties.
		/// <summary>
		/// The protocol of the XBee device.
		/// </summary>
		/// <seealso cref="Models.XBeeProtocol.CELLULAR"/>
		public override XBeeProtocol XBeeProtocol => XBeeProtocol.CELLULAR;

		/// <summary>
		/// The IMEI address for this Cellular device.
		/// </summary>
		/// <seealso cref="XBeeIMEIAddress"/>
		public XBeeIMEIAddress IMEIAddress { get; private set; }

		/// <summary>
		/// The node identifier of this XBee device. This is not supported in Cellular 
		/// devices, so it always returns <c>null</c>.
		/// </summary>
		/// <seealso cref="XBee64BitAddress"/>
		public override string NodeID
		{
			get
			{
				// Cellular protocol does not have Node Identifier.
				return null;
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
			if (XBeeProtocol != XBeeProtocol.CELLULAR)
				throw new XBeeDeviceException("XBee device is not a " + XBeeProtocol.CELLULAR.GetDescription() 
					+ " device, it is a " + XBeeProtocol.GetDescription() + " device.");
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
		/// <item><description>IMEI address.</description></item>
		/// </list></remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the parameters.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public override void ReadDeviceInfo()
		{
			base.ReadDeviceInfo();
			IMEIAddress = new XBeeIMEIAddress(XBee64BitAddr.Value);
		}

		/// <summary>
		/// Returns the current association status of this Cellular device.
		/// </summary>
		/// <remarks>It indicates occurrences of errors during the modem initialization and connection.</remarks>
		/// <returns>The current <see cref="CellularAssociationIndicationStatus"/> of the device.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the cellular association 
		/// indication status.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="CellularAssociationIndicationStatus"/>
		public CellularAssociationIndicationStatus GetCellularAssociationIndicationStatus()
		{
			byte[] associationIndicationValue = GetParameter("AI");
			return CellularAssociationIndicationStatus.UNKNOWN.Get(ByteUtils.ByteArrayToInt(associationIndicationValue));
		}

		/// <summary>
		/// Indicates whether the device is connected to the Internet or not.
		/// </summary>
		/// <returns><c>true</c> if the device is connected to the Internet, <c>false</c> otherwise.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the cellular association 
		/// indication status.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public bool IsConnected()
		{
			return GetCellularAssociationIndicationStatus() == CellularAssociationIndicationStatus.SUCCESSFULLY_CONNECTED;
		}

		/// <summary>
		/// Sends the provided SMS message to the given phone number.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured 
		/// receive timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="XBeeDevice.ReceiveTimeout"/> 
		/// property.
		/// 
		/// For non-blocking operations use the method <see cref="SendSMSAsync(string, string)"/>.
		/// </remarks>
		/// <param name="phoneNumber">The phone number to send the SMS to.</param>
		/// <param name="data">String containing the text of the SMS.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="phoneNumber"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the device is remote (remote devices 
		/// cannot send SMS).</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the received packet is not an instance of 
		/// <see cref="Packet.Common.TransmitStatusPacket"/> 
		/// or if its transmit status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBeeDevice.ReceiveTimeout"/>
		/// <seealso cref="SendSMSAsync(string, string)"/>
		public void SendSMS(string phoneNumber, string data)
		{
			if (phoneNumber == null)
				throw new ArgumentNullException("Phone number cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send SMS from a remote device.");

			logger.Debug(ToString() + "Sending SMS to " + phoneNumber + " >> " + data + ".");
			XBeePacket xbeePacket = new TXSMSPacket(GetNextFrameID(), phoneNumber, data);
			SendAndCheckXBeePacket(xbeePacket, false);
		}

		/// <summary>
		/// Sends asynchronously the provided SMS to the given phone number.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer or for transmit status packet.</remarks>
		/// <param name="phoneNumber">The phone number to send the SMS to.</param>
		/// <param name="data">String containing the text of the SMS.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="phoneNumber"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the device is remote (remote devices 
		/// cannot send SMS).</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SendSMS(string, string)"/>
		public void SendSMSAsync(string phoneNumber, string data)
		{
			if (phoneNumber == null)
				throw new ArgumentNullException("Phone number cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send SMS from a remote device.");

			logger.Debug(ToString() + "Sending SMS asynchronously to " + phoneNumber + " >> " + data + ".");
			XBeePacket xbeePacket = new TXSMSPacket(GetNextFrameID(), phoneNumber, data);
			SendAndCheckXBeePacket(xbeePacket, true);
		}
	}
}
