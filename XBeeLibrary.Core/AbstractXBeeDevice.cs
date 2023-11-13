/*
 * Copyright 2019-2023, Digi International Inc.
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

using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XBeeLibrary.Core.Connection;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Events.Relay;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.IO;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.Common;
using XBeeLibrary.Core.Packet.Raw;
using XBeeLibrary.Core.Packet.Relay;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class provides common functionality for all XBee devices.
	/// </summary>
	/// <seealso cref="XBeeDevice"/>
	/// <seealso cref="RemoteXBeeDevice"/>
	public abstract class AbstractXBeeDevice
	{
		// Constants.
		/// <summary>
		/// Default receive timeout used to wait for a response in synchronous operations.
		/// </summary>
		/// <seealso cref="ReceiveTimeout"/>
		internal const int DEFAULT_RECEIVE_TIMETOUT = 2000; // 2.0 seconds of timeout to receive packet and command responses.

		/// <summary>
		/// Timeout to wait before entering in command mode.
		/// </summary>
		/// <remarks>It is used to determine the operating mode of the module (this library only 
		/// supports API modes, not transparent mode).
		/// 
		/// This value depends on the <c>GT</c>, <c>AT</c> and/or <c>BT</c> parameters.</remarks>
		/// <seealso cref="DetermineOperatingMode"/>
		protected const int TIMEOUT_BEFORE_COMMAND_MODE = 1200;

		/// <summary>
		/// Timeout to wait after entering in command mode.
		/// </summary>
		/// <remarks>It is used to determine the operating mode of the module (this library only supports 
		/// API modes, not transparent mode).
		/// 
		/// This value depends on the <c>GT</c>, <c>AT</c> and/or <c>BT</c> parameters.</remarks>
		/// <seealso cref="DetermineOperatingMode"/>
		protected const int TIMEOUT_ENTER_COMMAND_MODE = 1500;

		protected const string PARAMETER_NODE_ID = "NI";

		protected static int TIMEOUT_READ_PACKET = 3000;

		private static int TIMEOUT_RESET = 5000;

		private static string ERROR_OPENING_INTERFACE = "Error opening the connection interface > {0}";

		private static string COMMAND_MODE_CHAR = "+";
		private static string COMMAND_MODE_OK = "OK\r";

		// Variables.
		protected DataReader dataReader = null;

		protected byte currentFrameID = 0xFF;
		protected int receiveTimeout = DEFAULT_RECEIVE_TIMETOUT;

		protected AbstractXBeeDevice localXBeeDevice;

		protected ILog logger;

		protected string bluetoothPassword;

		protected XBeeNetwork network;

		private OperatingMode operatingMode = OperatingMode.UNKNOWN;

		private object ioLock = new object();
		private object resetLock = new object();

		private bool ioPacketReceived = false;
		private bool modemStatusReceived = false;

		private byte[] ioPacketPayload;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="AbstractXBeeDevice"/> object with the given 
		/// connection interface.
		/// </summary>
		/// <param name="connectionInterface">The connection interface with the physical XBee device.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="connectionInterface"/> == null</c>.</exception>
		/// <seealso cref="AbstractXBeeDevice(AbstractXBeeDevice, XBee64BitAddress)"/>
		/// <seealso cref="AbstractXBeeDevice(AbstractXBeeDevice, XBee64BitAddress, XBee16BitAddress, string)"/>
		/// <seealso cref="IConnectionInterface"/>
		public AbstractXBeeDevice(IConnectionInterface connectionInterface)
		{
			XBeeProtocol = XBeeProtocol.UNKNOWN;
			ConnectionInterface = connectionInterface ?? throw new ArgumentNullException("ConnectionInterface cannot be null.");

			// Obtain logger.
			logger = LogManager.GetLogger(GetType());

			logger.DebugFormat(ToString() + "Using the connection interface {0}.",
					ConnectionInterface.GetType().Name);
		}

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="AbstractXBeeDevice"/> object with the given 
		/// local XBee device which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection interface 
		/// to communicate with the remote XBee device.</param>
		/// <param name="addr64">The 64-bit address to identify this XBee device.</param>
		/// <exception cref="ArgumentException">If <c>localXBeeDevice.IsRemote == true</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="AbstractXBeeDevice(IConnectionInterface)"/>
		/// <seealso cref="AbstractXBeeDevice(AbstractXBeeDevice, XBee64BitAddress, XBee16BitAddress, string)"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public AbstractXBeeDevice(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: this(localXBeeDevice, addr64, null, null) { }

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="AbstractXBeeDevice"/> object with the given 
		/// local XBee device which contains the connection interface to be used.
		/// </summary>
		/// <param name="localXBeeDevice">The local XBee device that will behave as connection interface to 
		/// communicate with the remote XBee device.</param>
		/// <param name="addr64">The 64-bit address to identify this XBee device.</param>
		/// <param name="addr16">The 16-bit address to identify this XBee device. It might be <c>null</c>.</param>
		/// <param name="id">The node identifier of this XBee device. It might be <c>null</c>.</param>
		/// <exception cref="ArgumentException">If <paramref name="localXBeeDevice"/> is remote.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="localXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="addr64"/> == null</c>.</exception>
		/// <seealso cref="AbstractXBeeDevice(IConnectionInterface)"/>
		/// <seealso cref="AbstractXBeeDevice(AbstractXBeeDevice, XBee64BitAddress)"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		public AbstractXBeeDevice(AbstractXBeeDevice localXBeeDevice, XBee64BitAddress addr64,
				XBee16BitAddress addr16, string id)
		{
			if (localXBeeDevice == null)
				throw new ArgumentNullException("Local XBee device cannot be null.");
			if (localXBeeDevice.IsRemote)
				throw new ArgumentException("The given local XBee device is remote.");

			XBeeProtocol = XBeeProtocol.UNKNOWN;
			this.localXBeeDevice = localXBeeDevice;
			ConnectionInterface = localXBeeDevice.ConnectionInterface;
			XBee64BitAddr = addr64 ?? throw new ArgumentNullException("XBee 64-bit address of the device cannot be null.");
			XBee16BitAddr = addr16;
			if (addr16 == null)
				XBee16BitAddr = XBee16BitAddress.UNKNOWN_ADDRESS;
			NodeID = id;
			logger = LogManager.GetLogger(GetType());
			logger.DebugFormat(ToString() + "Using the connection interface {0}.",
					ConnectionInterface.GetType().Name);
		}

		// Events.
		/// <summary>
		/// Represents the method that will handle the Packet received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="PacketReceivedEventArgs"/>
		protected internal event EventHandler<PacketReceivedEventArgs> PacketReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.AddPacketReceivedHandler(value);
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.RemovePacketReceivedHandler(value);
			}
		}

		/// <summary>
		/// Represents the method that will handle the Data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="DataReceivedEventArgs"/>
		protected event EventHandler<DataReceivedEventArgs> DataReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.DataReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.DataReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the IO Sample received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="IOSampleReceivedEventArgs"/>
		protected event EventHandler<IOSampleReceivedEventArgs> IOSampleReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.IOSampleReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.IOSampleReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the Modem status received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="ModemStatusReceivedEventArgs"/>
		protected event EventHandler<ModemStatusReceivedEventArgs> ModemStatusReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.ModemStatusReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.ModemStatusReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the explicit data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="ExplicitDataReceivedEventArgs"/>
		protected event EventHandler<ExplicitDataReceivedEventArgs> ExplicitDataReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.ExplicitDataReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.ExplicitDataReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the User Data Relay received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="UserDataRelayReceivedEventArgs"/>
		protected event EventHandler<UserDataRelayReceivedEventArgs> UserDataRelayReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.UserDataRelayReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.UserDataRelayReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the Bluetooth data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="BluetoothDataReceivedEventArgs"/>
		protected event EventHandler<BluetoothDataReceivedEventArgs> BluetoothDataReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.BluetoothDataReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.BluetoothDataReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the MicroPython data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="MicroPythonDataReceivedEventArgs"/>
		protected event EventHandler<MicroPythonDataReceivedEventArgs> MicroPythonDataReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.MicroPythonDataReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.MicroPythonDataReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the serial data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="SerialDataReceivedEventArgs"/>
		protected event EventHandler<SerialDataReceivedEventArgs> SerialDataReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.SerialDataReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.SerialDataReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the SMS received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="SMSReceivedEventArgs"/>
		protected event EventHandler<SMSReceivedEventArgs> SMSReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.SMSReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.SMSReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the IP data received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="IPDataReceivedEventArgs"/>
		protected event EventHandler<IPDataReceivedEventArgs> IPDataReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.IPDataReceived += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.IPDataReceived -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the IO Packet received event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="PacketReceivedEventArgs"/>
		private event EventHandler<PacketReceivedEventArgs> IOPacketReceived
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.AddPacketReceivedHandler(value);
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (dataReader != null)
					dataReader.RemovePacketReceivedHandler(value);
			}
		}

		// Properties.
		/// <summary>
		/// The firmware version (hexadecimal string value) of this XBee device.
		/// </summary>
		/// <remarks>To refresh this value use the <see cref="ReadDeviceInfo"/> method.</remarks>
		public string FirmwareVersion { get; private set; }

		/// <summary>
		/// The hardware version of this XBee device.
		/// </summary>
		/// <remarks>If this value is <c>null</c>, use the <see cref="ReadDeviceInfo"/> method 
		/// to get its value.</remarks>
		/// <seealso cref="Models.HardwareVersion"/>
		/// <seealso cref="HardwareVersionEnum"/>
		public HardwareVersion HardwareVersion { get; private set; }

		/// <summary>
		/// The hardware version of this XBee device in string format (including the 0x prefix).
		/// </summary>
		/// <remarks>If this value is empty, use the <see cref="ReadDeviceInfo"/> method 
		/// to get its value.</remarks>
		/// <seealso cref="HardwareVersion"/>
		public string HardwareVersionString
		{
			get
			{
				HardwareVersion hwVersion = HardwareVersion;
				if (hwVersion == null)
					return "";
				string hexValue = HexUtils.IntegerToHexString(hwVersion.Value, 1);
				int numZeros = 2 - hexValue.Length;
				for (int i = 0; i < numZeros; i++)
					hexValue = "0" + hexValue;
				return "0x" + hexValue;
			}
		}

		/// <summary>
		/// The 16-bit address of this XBee device.
		/// </summary>
		/// <remarks>To refresh this value, use the <see cref="ReadDeviceInfo"/> 
		/// method.</remarks>
		/// <seealso cref="XBee16BitAddress"/>
		public virtual XBee16BitAddress XBee16BitAddr { get; internal set; } = XBee16BitAddress.UNKNOWN_ADDRESS;

		/// <summary>
		/// The 64-bit address of this XBee device.
		/// </summary>
		/// <remarks>If this value is <c>null</c> or 
		/// <see cref="XBee64BitAddress.UNKNOWN_ADDRESS"/>, use the 
		/// <see cref="ReadDeviceInfo"/> method to get its value.</remarks>
		/// <seealso cref="XBee64BitAddress"/>
		public virtual XBee64BitAddress XBee64BitAddr { get; internal set; } = XBee64BitAddress.UNKNOWN_ADDRESS;

		/// <summary>
		/// The XBee protocol of this XBee device.
		/// </summary>
		/// <remarks>To refresh this value, use the <see cref="ReadDeviceInfo"/> method.</remarks>
		/// <seealso cref="Models.XBeeProtocol"/>
		public virtual XBeeProtocol XBeeProtocol { get; internal set; }

		/// <summary>
		/// The Signal strength of the device with the parent node. This value is cached at discovery 
		/// time and it is not read directly from the device.
		/// </summary>
		public int SignalStrength { get; set; }

		/// <summary>
		/// The Operating mode (AT, API or API escaped) of this XBee device for a local device, 
		/// and the operating mode of the local device used as communication interface for a remote device.
		/// </summary>
		/// <seealso cref="IsRemote"/>
		/// <seealso cref="Models.OperatingMode"/>
		protected OperatingMode OperatingMode
		{
			get
			{
				if (IsRemote)
					return localXBeeDevice.OperatingMode;
				return operatingMode;
			}
			set
			{
				operatingMode = value;
			}
		}

		/// <summary>
		/// The 'apply configuration changes' option for this device.
		/// </summary>
		/// <remarks>Enabling this option means that when any parameter of this XBee device is set, it 
		/// will be also applied.
		/// 
		/// If this option is disabled, the method <see cref="ApplyChanges"/> must be used in order to 
		/// apply the changes in all the parameters that were previously set.</remarks>
		public bool ApplyConfigurationChangesEnabled { get; set; } = true;

		/// <summary>
		/// The node identifier of this XBee device.
		/// </summary>
		/// <remarks>To refresh this value use the <see cref="ReadDeviceInfo"/> method.</remarks>
		public virtual string NodeID { get; internal set; }

		/// <summary>
		/// Indicates whether the XBee device is initialized (basic parameters have been read) or not.
		/// </summary>
		public bool IsInitialized { get; private set; } = false;

		/// <summary>
		/// Indicates whether this XBee device is a remote device.
		/// </summary>
		abstract public bool IsRemote { get; }

		/// <summary>
		/// The connection interface associated to this XBee device.
		/// </summary>
		/// <seealso cref="IConnectionInterface"/>
		protected IConnectionInterface ConnectionInterface { get; private set; }

		/// <summary>
		/// The status of the connection interface associated to this device. It indicates if the connection 
		/// is open or not.
		/// </summary>
		/// <seealso cref="Close"/>
		/// <seealso cref="Open"/>
		protected internal bool IsOpen
		{
			get
			{
				if (ConnectionInterface != null)
					return ConnectionInterface.IsOpen;
				return false;
			}
		}

		/// <summary>
		/// The XBee device timeout in milliseconds for received packets in synchronous operations.
		/// </summary>
		/// <exception cref="ArgumentException">If the value to be set is lesser than 0.</exception>
		protected int ReceiveTimeout
		{
			get
			{
				return receiveTimeout;
			}
			set
			{
				if (value < 0)
					throw new ArgumentException("Receive timeout cannot be less than 0.");

				receiveTimeout = value;
			}
		}

		/// <summary>
		/// Indicates the API output mode of the XBee device.
		/// </summary>
		/// <remarks>The API output mode determines the format that the received data is output through 
		/// the serial interface of the XBee device.</remarks>
		/// <exception cref="ATCommandEmptyException">If the returned value of the API Output Mode command 
		/// is <c>null</c> or empty.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="Models.APIOutputMode"/>
		protected APIOutputMode APIOutputMode
		{
			get
			{
				var value = GetParameter("AO");
				if (value == null || value.Length < 1)
					throw new ATCommandEmptyException("AO");
				return APIOutputMode.MODE_UNKNOWN.Get(value[0]);
			}
			set
			{
				if (value == APIOutputMode.MODE_UNKNOWN)
					throw new ArgumentException("API output mode cannot be unknown.");
				SetParameter("AO", new byte[] { value.GetValue() });
			}
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
		/// <item><description>16-bit address (not for DigiMesh modules).</description></item>
		/// </list></remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the parameters.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		/// <seealso cref="HardwareVersion"/>
		/// <seealso cref="Models.XBeeProtocol"/>
		public virtual void ReadDeviceInfo()
		{
			byte[] response = null;
			// Get the 64-bit address.
			if (XBee64BitAddr == null || XBee64BitAddr.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
			{
				string addressHigh;
				string addressLow;

				response = GetParameter("SH");
				if (response == null || response.Length < 1)
					throw new ATCommandEmptyException("SH");
				addressHigh = HexUtils.ByteArrayToHexString(response);

				response = GetParameter("SL");
				if (response == null || response.Length < 1)
					throw new ATCommandEmptyException("SL");
				addressLow = HexUtils.ByteArrayToHexString(response);

				while (addressLow.Length < 8)
					addressLow = "0" + addressLow;

				XBee64BitAddr = new XBee64BitAddress(addressHigh + addressLow);
			}
			// Get the Node ID.
			response = GetParameter("NI");
			if (response == null || response.Length < 1)
				throw new ATCommandEmptyException("NI");
			NodeID = Encoding.UTF8.GetString(response, 0, response.Length);

			// Get the hardware version.
			if (HardwareVersion == null)
			{
				response = GetParameter("HV");
				if (response == null || response.Length < 1)
					throw new ATCommandEmptyException("HV");
				try
				{
					HardwareVersion = HardwareVersion.Get(response[0]);
				}
				catch (Exception)
				{
					throw new XBeeException("XBee device not supported (hardware version 0x"
						+ HexUtils.IntegerToHexString(response[0], 1) + ").");
				}
			}

			// Get the firmware version.
			response = GetParameter("VR");
			if (response == null || response.Length < 1)
				throw new ATCommandEmptyException("VR");
			FirmwareVersion = HexUtils.ByteArrayToHexString(response);
			// Remove leading 0s.
			if (FirmwareVersion.Length > 1)
				FirmwareVersion = FirmwareVersion.TrimStart('0');

			// Original value of the protocol.
			XBeeProtocol origProtocol = XBeeProtocol;

			// Obtain the device protocol.
			XBeeProtocol currentProtocol = XBeeProtocol.UNKNOWN.DetermineProtocol(HardwareVersion, FirmwareVersion);

			if (origProtocol != XBeeProtocol.UNKNOWN && origProtocol != currentProtocol)
			{
				throw new XBeeException("Error reading device information: Your module seems to be " + currentProtocol
					+ " and NOT " + origProtocol + ". Check if you are using the appropriate device class.");
			}

			XBeeProtocol = XBeeProtocol.UNKNOWN.DetermineProtocol(HardwareVersion, FirmwareVersion);

			// Get the 16-bit address. This must be done after obtaining the protocol because 
			// DigiMesh and Point-to-Multipoint protocols don't have 16-bit addresses.
			if (XBeeProtocol == XBeeProtocol.ZIGBEE
				|| XBeeProtocol == XBeeProtocol.RAW_802_15_4
				|| XBeeProtocol == XBeeProtocol.XTEND
				|| XBeeProtocol == XBeeProtocol.SMART_ENERGY
				|| XBeeProtocol == XBeeProtocol.ZNET)
			{
				response = GetParameter("MY");
				if (response == null || response.Length < 1)
					throw new ATCommandEmptyException("MY");
				XBee16BitAddr = new XBee16BitAddress(response);
			}

			IsInitialized = true;
		}

		/// <summary>
		/// Sets the configuration of the given IO line of this XBee device.
		/// </summary>
		/// <param name="ioLine">The IO line to configure.</param>
		/// <param name="ioMode">The IO mode to set to the IO line.</param>
		/// <exception cref="ArgumentException">If <paramref name="ioLine"/> is <see cref="IOLine.UNKNOWN"/> 
		/// or if <paramref name="ioMode"/> is <see cref="IOMode.UNKOWN"/>.</exception>
		/// <exception cref="InterfaceNotOpenException"> If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the set configuration command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetIOConfiguration(IOLine)"/>
		/// <seealso cref="IOLine"/>
		/// <seealso cref="IOMode"/>
		public virtual void SetIOConfiguration(IOLine ioLine, IOMode ioMode)
		{
			// Check IO line.
			if (ioLine.Equals(IOLine.UNKNOWN))
				throw new ArgumentException("IO line cannot be Unknown.");
			if (ioMode.Equals(IOMode.UNKOWN))
				throw new ArgumentException("IO mode cannot be Unknown.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			SetParameter(ioLine.GetConfigurationATCommand(), new byte[] { (byte)ioMode.GetId() });
		}

		/// <summary>
		/// Returns the configuration mode of the provided IO line of this XBee device.
		/// </summary>
		/// <param name="ioLine">The IO line to get its configuration.</param>
		/// <returns>The IO mode (configuration) of the provided IO line.</returns>
		/// <exception cref="ArgumentException">If <paramref name="ioLine"/> is <see cref="IOLine.UNKNOWN"/>.</exception>
		/// <exception cref="ATCommandEmptyException">If the IO line AT command is empty.</exception>
		/// <exception cref="InterfaceNotOpenException"> If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the received configuration mode is not valid.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the IO command response.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SetIOConfiguration(IOLine, IOMode)"/>
		/// <seealso cref="IOLine"/>
		/// <seealso cref="IOMode"/>
		public virtual IOMode GetIOConfiguration(IOLine ioLine)
		{
			// Check IO line.
			if (ioLine.Equals(IOLine.UNKNOWN))
				throw new ArgumentException("DIO pin cannot be unknown.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			// Check if the received configuration mode is valid.
			var ioModeAnswer = GetParameter(ioLine.GetConfigurationATCommand());
			if (ioModeAnswer == null || ioModeAnswer.Length < 1)
				throw new ATCommandEmptyException(ioLine.GetConfigurationATCommand());
			int ioModeValue = ioModeAnswer[0];
			IOMode dioMode = IOMode.UNKOWN.GetIOMode(ioModeValue, ioLine);
			if (dioMode.Equals(IOMode.UNKOWN))
				throw new OperationNotSupportedException("Received configuration mode '"
					+ HexUtils.IntegerToHexString(ioModeValue, 1) + "' is not valid.");

			// Return the configuration mode.
			return dioMode;
		}

		/// <summary>
		/// Sets the digital value (high or low) to the provided IO line of this XBee device.
		/// </summary>
		/// <param name="ioLine">The IO line to set its value.</param>
		/// <param name="ioValue">The IOValue to set to the IO line <see cref="IOValue.HIGH"/> or 
		/// <see cref="IOValue.LOW"/>.</param>
		/// <exception cref="ArgumentException">If <paramref name="ioLine"/> is <see cref="IOLine.UNKNOWN"/> 
		/// or if <paramref name="ioValue"/> is <see cref="IOValue.UNKNOWN"/>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the set configuration command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetIOConfiguration(IOLine)"/>
		/// <seealso cref="SetIOConfiguration(IOLine, IOMode)"/>
		/// <seealso cref="IOLine"/>
		/// <seealso cref="IOValue"/>
		/// <seealso cref="IOMode.DIGITAL_OUT_HIGH"/>
		/// <seealso cref="IOMode.DIGITAL_OUT_LOW"/>
		public virtual void SetDIOValue(IOLine ioLine, IOValue ioValue)
		{
			// Check IO line.
			if (ioLine.Equals(IOLine.UNKNOWN))
				throw new ArgumentException("IO line cannot be unknown.");
			// Check IO value.
			if (ioValue.Equals(IOValue.UNKNOWN))
				throw new ArgumentException("IO value cannot be unknown.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			SetParameter(ioLine.GetConfigurationATCommand(), new byte[] { (byte)ioValue.GetID() });
		}

		/// <summary>
		/// Returns the digital value of the provided IO line of this XBee device.
		/// </summary>
		/// <remarks>The provided IO line must be previously configured as digital I/O. To do so, use 
		/// <see cref="SetIOConfiguration(IOLine, IOMode)"/> and the following <see cref="IOMode"/>:
		/// <list type="bullet">
		/// <item><description><see cref="IOMode.DIGITAL_IN"/> to configure as digital input.</description></item>
		/// <item><description><see cref="IOMode.DIGITAL_OUT_HIGH"/> to configure as digital output, high.</description></item>
		/// <item><description><see cref="IOMode.DIGITAL_OUT_LOW"/> to configure as digital output, low.</description></item>
		/// </list></remarks>
		/// <param name="ioLine">The IO line to get its digital value.</param>
		/// <exception cref="ArgumentException">If <paramref name="ioLine"/> is <see cref="IOLine.UNKNOWN"/>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sample does not contain the expected IO line and value.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetIOConfiguration(IOLine)"/>
		/// <seealso cref="SetIOConfiguration(IOLine, IOMode)"/>
		/// <seealso cref="IOLine"/>
		/// <seealso cref="IOValue"/>
		/// <seealso cref="IOMode.DIGITAL_IN"/>
		/// <seealso cref="IOMode.DIGITAL_OUT_HIGH"/>
		/// <seealso cref="IOMode.DIGITAL_OUT_LOW"/>
		public virtual IOValue GetDIOValue(IOLine ioLine)
		{
			// Check IO line.
			if (ioLine.Equals(IOLine.UNKNOWN))
				throw new ArgumentException("IO line cannot be unknown.");

			// Obtain an IO Sample from the XBee device.
			IOSample ioSample = ReadIOSample();

			// Check if the IO sample contains the expected IO line and value.
			if (!ioSample.HasDigitalValues || !ioSample.DigitalValues.ContainsKey(ioLine))
				throw new OperationNotSupportedException("Answer does not contain digital data for "
					+ ioLine.GetName() + ".");

			// Return the digital value. 
			return ioSample.DigitalValues[ioLine];
		}

		/// <summary>
		/// Sets the duty cycle (in %) of the provided IO line of this XBee device.
		/// </summary>
		/// <remarks>The provided IO line must be:
		/// <list type="bullet">
		/// <item><description>PWM capable (<c>IOLine.UNKNOWN.HasPWMCapability()</c>).</description></item>
		/// <item><description>Previously configured as PWM Output (use <see cref="SetIOConfiguration(IOLine, IOMode)"/> 
		/// and <see cref="IOMode.PWM"/>).</description></item>
		/// </list></remarks>
		/// <param name="ioLine">The IO line to set its duty cycle value.</param>
		/// <param name="dutyCycle">The duty cycle of the PWM.</param>
		/// <exception cref="ArgumentException">If <c>ioLine.HasPWMCapability() == false</c> 
		/// or if <c>value <![CDATA[<]]> 0</c> or if <c>value <![CDATA[>]]> 100</c> 
		/// or if <paramref name="ioLine"/> is <see cref="IOLine.UNKNOWN"/>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the set configuration command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetIOConfiguration(IOLine)"/>
		/// <seealso cref="SetIOConfiguration(IOLine, IOMode)"/>
		/// <seealso cref="GetPWMDutyCycle(IOLine)"/>
		/// <seealso cref="IOLine"/>
		/// <seealso cref="IOMode.PWM"/>
		public virtual void SetPWMDutyCycle(IOLine ioLine, double dutyCycle)
		{
			// Check IO line.
			if (ioLine.Equals(IOLine.UNKNOWN))
				throw new ArgumentException("IO line cannot be unknown.");
			// Check if the IO line has PWM capability.
			if (!ioLine.HasPWMCapability())
				throw new ArgumentException("Provided IO line does not have PWM capability.");
			// Check duty cycle limits.
			if (dutyCycle < 0 || dutyCycle > 100)
				throw new ArgumentException("Duty Cycle must be between 0% and 100%.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			// Convert the value.
			int finaldutyCycle = (int)(dutyCycle * 1023.0 / 100.0);

			SetParameter(ioLine.GetPWMDutyCycleATCommand(), ByteUtils.IntToByteArray(finaldutyCycle));
		}

		/// <summary>
		/// Gets the duty cycle (in %) corresponding to the provided IO line of this XBee device.
		/// </summary>
		/// <remarks>The provided IO line must be:
		/// <list type="bullet">
		/// <item><description>PWM capable (<c>IOLine.UNKNOWN.HasPWMCapability()"</c>).</description></item>
		/// <item><description>Previously configured as PWM Output (use <see cref="SetIOConfiguration(IOLine, IOMode)"/> 
		/// and <see cref="IOMode.PWM"/>).</description></item>
		/// </list></remarks>
		/// <param name="ioLine">The IO line to get its PWM duty cycle.</param>
		/// <returns>The PWM duty cycle value corresponding to the provided IO line (0% - 100%).</returns>
		/// <exception cref="ArgumentException">If <c>ioLine.HasPWMCapability() == false</c> 
		/// or if <paramref name="ioLine"/> is <see cref="IOLine.UNKNOWN"/>.</exception>
		/// <exception cref="ATCommandEmptyException">If the parameter of the IO line is empty.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the PWM command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetIOConfiguration(IOLine)"/>
		/// <seealso cref="SetIOConfiguration(IOLine, IOMode)"/>
		/// <seealso cref="SetPWMDutyCycle(IOLine, double)"/>
		/// <seealso cref="IOLine"/>
		/// <seealso cref="IOMode.PWM"/>
		public virtual double GetPWMDutyCycle(IOLine ioLine)
		{
			// Check IO line.
			if (ioLine.Equals(IOLine.UNKNOWN))
				throw new ArgumentException("IO line cannot be unknown.");
			// Check if the IO line has PWM capability.
			if (!ioLine.HasPWMCapability())
				throw new ArgumentException("Provided IO line does not have PWM capability.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			var value = GetParameter(ioLine.GetPWMDutyCycleATCommand());
			if (value == null || value.Length < 1)
				throw new ATCommandEmptyException(ioLine.GetPWMDutyCycleATCommand());

			// Return the PWM duty cycle value.
			int readValue = ByteUtils.ByteArrayToInt(value);
			return Math.Round((readValue * 100.0 / 1023.0) * 100.0) / 100.0;
		}

		/// <summary>
		/// Returns the analog value of the provided IO line of this XBee device.
		/// </summary>
		/// <remarks>The provided IO line must be previously configured as ADC. To do so, use 
		/// <see cref="SetIOConfiguration(IOLine, IOMode)"/> and <see cref="IOMode.ADC"/>.</remarks>
		/// <param name="ioLine">The IO line to get its analog value.</param>
		/// <returns>The analog value corresponding to the provided IO line.</returns>
		/// <exception cref="ArgumentException">If <paramref name="ioLine"/> is 
		/// <see cref="IOLine.UNKNOWN"/>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the IO sample does not contain the 
		/// expeceted IO line and value.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the IO sample data.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetIOConfiguration(IOLine)"/>
		/// <seealso cref="SetIOConfiguration(IOLine, IOMode)"/>
		/// <seealso cref="IOLine"/>
		/// <seealso cref="IOMode.ADC"/>
		public virtual int GetADCValue(IOLine ioLine)
		{
			// Check IO line.
			if (ioLine.Equals(IOLine.UNKNOWN))
				throw new ArgumentException("IO line cannot be null.");

			// Obtain an IO Sample from the XBee device.
			IOSample ioSample = ReadIOSample();

			// Check if the IO sample contains the expected IO line and value.
			if (!ioSample.HasAnalogValues || !ioSample.AnalogValues.ContainsKey(ioLine))
				throw new OperationNotSupportedException("Answer does not contain analog data for "
					+ ioLine.GetName() + ".");

			// Return the analog value.
			return ioSample.AnalogValues[ioLine];
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
		/// <exception cref="TimeoutException">If there is a timeout setting the destination address.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		protected virtual void SetDestinationAddress(XBee64BitAddress xbee64BitAddress)
		{
			if (xbee64BitAddress == null)
				throw new ArgumentNullException("Address cannot be null.");

			// This method needs to apply changes after modifying the destination 
			// address, but only if the destination address could be set successfully.
			bool applyChanges = ApplyConfigurationChangesEnabled;
			if (applyChanges)
				ApplyConfigurationChangesEnabled = false;

			byte[] address = xbee64BitAddress.Value;
			try
			{
				var dh = new byte[4];
				var dl = new byte[4];
				Array.Copy(address, 0, dh, 0, 4);
				Array.Copy(address, 4, dl, 0, 4);
				SetParameter("DH", dh);
				SetParameter("DL", dl);
				ApplyChanges();
			}
			finally
			{
				// Always restore the old value of the AC.
				ApplyConfigurationChangesEnabled = applyChanges;
			}
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
		/// <exception cref="TimeoutException">If there is a timeout reading the destination address.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="XBee64BitAddress"/>
		protected virtual XBee64BitAddress GetDestinationAddress()
		{
			var dh = GetParameter("DH");
			if (dh == null || dh.Length < 1)
				throw new ATCommandEmptyException("DH");
			var dl = GetParameter("DL");
			if (dl == null || dl.Length < 1)
				throw new ATCommandEmptyException("DL");

			byte[] address = new byte[dh.Length + dl.Length];

			Array.Copy(dh, 0, address, 0, dh.Length);
			Array.Copy(dl, 0, address, dh.Length, dl.Length);

			return new XBee64BitAddress(address);
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
		/// <exception cref="TimeoutException">If there is a timeout sending the set IO sampling rate 
		/// command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="GetIOSamplingRate"/>
		protected virtual void SetIOSamplingRate(int rate)
		{
			// Check range.
			if (rate < 0 || rate > 0xFFFF)
				throw new ArgumentException("Rate must be between 0 and 0xFFFF.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			SetParameter("IR", ByteUtils.IntToByteArray(rate));
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
		/// <exception cref="TimeoutException">If there is a timeout reading the IO sampling rate command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="SetIOSamplingRate(int)"/>
		protected virtual int GetIOSamplingRate()
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			var rate = GetParameter("IR");
			if (rate == null || rate.Length < 1)
				throw new ATCommandEmptyException("IR");
			return ByteUtils.ByteArrayToInt(rate);
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
		/// <exception cref="TimeoutException">If there is a timeout sending the set IO change detect 
		/// command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="GetDIOChangeDetection"/>
		protected virtual void SetDIOChangeDetection(ISet<IOLine> lines)
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			byte[] bitfield = new byte[2];

			if (lines != null)
			{
				foreach (IOLine line in lines)
				{
					int i = (byte)line;
					if (i < 8)
						bitfield[1] = (byte)(bitfield[1] | (1 << i));
					else
						bitfield[0] = (byte)(bitfield[0] | (1 << i - 8));
				}
			}

			SetParameter("IC", bitfield);
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
		/// <exception cref="TimeoutException">If there is a timeout reading the IO change detect command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetDestinationAddress"/>
		/// <seealso cref="SetDestinationAddress(XBee64BitAddress)"/>
		/// <seealso cref="SetDIOChangeDetection(ISet{IOLine})"/>
		/// <seealso cref="IOLine"/>
		protected virtual ISet<IOLine> GetDIOChangeDetection()
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			var bitfield = GetParameter("IC");
			if (bitfield == null || bitfield.Length < 1)
				throw new ATCommandEmptyException("IC");
			var lines = new HashSet<IOLine>();
			int mask = (bitfield[0] << 8) + (bitfield[1] & 0xFF);

			for (int i = 0; i < 16; i++)
			{
				if (ByteUtils.IsBitEnabled(mask, i))
					lines.Add(IOLine.UNKNOWN.GetDIO(i));
			}

			if (lines.Count > 0)
				return lines;
			return null;
		}

		/// <summary>
		/// Applies changes to all command registers causing queued command register values to be applied.
		/// </summary>
		/// <remarks>This method must be invoked if the 'apply configuration changes' option is disabled 
		/// and the changes to this XBee device parameters must be applied.
		/// 
		/// To know if the 'apply configuration changes' option is enabled, use the 
		/// <see cref="ApplyConfigurationChangesEnabled"/> property. Use it also to enable/disable this feature.
		/// 
		/// Applying changes does not imply the modifications will persist through subsequent resets. To do 
		/// so, use the <see cref="WriteChanges"/> method.</remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending apply changes command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ApplyConfigurationChangesEnabled"/>
		/// <seealso cref="SetParameter(string, byte[])"/>
		/// <seealso cref="WriteChanges"/>
		public void ApplyChanges()
		{
			ExecuteParameter("AC");
		}

		/// <summary>
		/// Returns an IO sample from this XBee device containing the value of all enabled digital IO and 
		/// analog input channels.
		/// </summary>
		/// <returns>An IO sample containing the value of all enabled digital IO and analog input channels.</returns>
		/// <exception cref="ATCommandEmptyException">If <c>IS</c> value is empty.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout getting the IO sample.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="IOSample"/>
		public virtual IOSample ReadIOSample()
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			// Try to build an IO Sample from the sample payload.
			byte[] samplePayload = null;
			IOSample ioSample;

			// The response to the IS command in local 802.15.4 devices is empty, 
			// so we have to create a packet listener to receive the IO sample.
			if (!IsRemote && XBeeProtocol == XBeeProtocol.RAW_802_15_4)
			{
				ExecuteParameter("IS");
				samplePayload = ReceiveRaw802IOPacket();
				if (samplePayload == null)
					throw new Exceptions.TimeoutException("Timeout waiting for the IO response packet.");
			}
			else
			{
				samplePayload = GetParameter("IS");
				if (samplePayload == null || samplePayload.Length < 1)
					throw new ATCommandEmptyException("IS");
			}

			try
			{
				ioSample = new IOSample(samplePayload);
			}
			catch (ArgumentException e)
			{
				throw new XBeeException("Couldn't create the IO sample.", e);
			}
			return ioSample;
		}

		/// <summary>
		/// Sets the given parameter with the provided value in this XBee device.
		/// </summary>
		/// <remarks>If the 'apply configuration changes' option is enabled in this device, the configured 
		/// value for the given parameter will be immediately applied, if not the method <see cref="ApplyChanges"/> 
		/// must be invoked to apply it.
		/// 
		/// Use <see cref="ApplyConfigurationChangesEnabled"/> to know if the 'apply configuration changes' option 
		/// is enabled, and also to enable/disable it.
		/// 
		/// To make parameter modifications persist through subsequent resets use the <see cref="WriteChanges"/> 
		/// method.</remarks>
		/// <param name="parameter">The name of the parameter to be set.</param>
		/// <param name="parameterValue">The value of the parameter to set.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="parameterValue"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the set configuration command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ApplyChanges"/>
		/// <seealso cref="ApplyConfigurationChangesEnabled"/>
		/// <seealso cref="ExecuteParameter(string)"/>
		/// <seealso cref="GetParameter(string)"/>
		/// <seealso cref="WriteChanges"/>
		public void SetParameter(string parameter, byte[] parameterValue)
		{
			if (parameterValue == null)
				throw new ArgumentNullException("Value of the parameter cannot be null.");

			SendParameter(parameter, parameterValue);
		}

		/// <summary>
		/// Gets the value of the given parameter from this XBee device.
		/// </summary>
		/// <param name="parameter">The name of the parameter to retrieve its value.</param>
		/// <returns>A byte array containing the value of the parameter.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ExecuteParameter(string)"/>
		/// <seealso cref="SetParameter(string, byte[])"/>
		public byte[] GetParameter(string parameter)
		{
			return SendParameter(parameter, null);
		}

		/// <summary>
		/// Executes the given command in this XBee device.
		/// </summary>
		/// <remarks>This method is intended to be used for those AT parameters that cannot be read 
		/// or written, they just execute some action in the XBee module.</remarks>
		/// <param name="parameter">The AT command to be executed.</param>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout executing the given command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetParameter(string)"/>
		/// <seealso cref="SetParameter(string, byte[])"/>
		public void ExecuteParameter(string parameter)
		{
			SendParameter(parameter, null);
		}

		/// <summary>
		/// Performs a software reset on this XBee device and blocks until the process is 
		/// completed.
		/// </summary>
		abstract public void Reset();

		/// <summary>
		/// Performs a software reset on this XBee device and blocks until the process is 
		/// completed.
		/// </summary>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout resetting the device.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		protected void SoftwareReset()
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			logger.Info(ToString() + "Resetting the local module...");

			ATCommandResponse response = null;
			try
			{
				response = SendATCommand(new ATCommand("FR"));
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}

			// Check if AT Command response is valid.
			CheckATCommandResponseIsValid(response);

			// Wait for a Modem Status packet.
			if (!WaitForModemResetStatusPacket())
				throw new Exceptions.TimeoutException("Timeout waiting for the Modem Status packet.");

			logger.Info(ToString() + "Module reset successfully.");
		}

		/// <summary>
		/// Returns the string representation of this device.
		/// </summary>
		/// <returns>The string representation of this device.</returns>
		public override string ToString()
		{
			string id = NodeID ?? "";
			string addr64 = XBee64BitAddr == null || XBee64BitAddr.Equals(XBee64BitAddress.UNKNOWN_ADDRESS) ?
					"" : XBee64BitAddr.ToString();

			if (id.Length == 0 && addr64.Length == 0)
				return ConnectionInterface.ToString();

			StringBuilder message = new StringBuilder(ConnectionInterface.ToString());
			message.Append(addr64);
			if (id.Length > 0)
			{
				message.Append(" (");
				message.Append(id);
				message.Append(")");
			}
			message.Append(" - ");

			return message.ToString();
		}

		/// <summary>
		/// Sets the Node Identifier on the device.
		/// </summary>
		/// <param name="nodeId">The new Node ID.</param>
		/// <exception cref="ArgumentException">If the length of the value to be set is greater than 
		/// 20 characters.</exception>
		/// <exception cref="ArgumentNullException">If the value to set is <c>null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout setting the node identifier command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public virtual void SetNodeID(string nodeId)
		{
			if (nodeId == null)
				throw new ArgumentNullException("Node ID cannot be null.");
			if (nodeId.Length > 20)
				throw new ArgumentException("Node ID length must be less than 21.");

			SetParameter("NI", Encoding.UTF8.GetBytes(nodeId));
			NodeID = nodeId;
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
		/// <exception cref="TimeoutException">If there is a timeout reading the operating PAN ID.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SetPANID(byte[])"/>
		protected byte[] GetPANID()
		{
			string parameter;
			switch (XBeeProtocol)
			{
				case XBeeProtocol.ZIGBEE:
					parameter = "OP";
					break;
				default:
					parameter = "ID";
					break;
			}
			var panIDAnswer = GetParameter(parameter);
			if (panIDAnswer == null || panIDAnswer.Length < 1)
				throw new ATCommandEmptyException(parameter);
			return panIDAnswer;
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
		/// <exception cref="TimeoutException">If there is a timeout setting the operating PAN ID.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetPANID"/>
		protected virtual void SetPANID(byte[] panID)
		{
			if (panID == null)
				throw new ArgumentNullException("PAN ID cannot be null.");
			if (panID.Length == 0)
				throw new ArgumentException("Length of the PAN ID cannot be 0.");
			if (panID.Length > 8)
				throw new ArgumentException("Length of the PAN ID cannot be longer than 8 bytes.");

			SetParameter("ID", panID);
		}

		/// <summary>
		/// Returns the output power level at which this XBee device transmits conducted power.
		/// </summary>
		/// <returns>The output power level of this XBee device.</returns>
		/// <exception cref="ATCommandEmptyException">If the <c>PL</c> command returns a <c>null</c> or an 
		/// empty value.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the power level command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SetPowerLevel(PowerLevel)"/>
		/// <seealso cref="PowerLevel"/>
		protected virtual PowerLevel GetPowerLevel()
		{
			var powerLevelValue = GetParameter("PL");
			if (powerLevelValue == null || powerLevelValue.Length < 1)
				throw new ATCommandEmptyException("PL");
			return PowerLevel.LEVEL_UNKNOWN.Get((byte)ByteUtils.ByteArrayToInt(powerLevelValue));
		}

		/// <summary>
		/// Sets the output power level at which this XBee device transmits conducted power.
		/// </summary>
		/// <param name="powerLevel">The new output power level to be set in this XBee device.</param>
		/// <exception cref="ArgumentException">If <paramref name="powerLevel"/>
		/// is <see cref="PowerLevel.LEVEL_UNKNOWN"/>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout setting the power level command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetPowerLevel"/>
		/// <seealso cref="PowerLevel"/>
		protected virtual void SetPowerLevel(PowerLevel powerLevel)
		{
			if (powerLevel.Equals(PowerLevel.LEVEL_UNKNOWN))
				throw new ArgumentException("Power level cannot be unknown.");

			SetParameter("PL", ByteUtils.IntToByteArray(powerLevel.GetValue()));
		}

		/// <summary>
		/// Writes configurable parameter values to the non-volatile memory of this XBee device so that parameter 
		/// modifications persist through subsequent resets.
		/// </summary>
		/// <remarks>Parameters values remain in this device's memory until overwritten by subsequent use of this 
		/// method.
		/// 
		/// If changes are made without writing them to non-volatile memory, the module reverts back to previously 
		/// saved parameters the next time the module is powered-on.
		/// 
		/// Writing the parameter modifications does not mean those values are immediately applied, this depends 
		/// on the status of the 'apply configuration changes' option. Use <see cref="ApplyConfigurationChangesEnabled"/> 
		/// to get its status and to enable/disable the option. If it is disabled, method <see cref="ApplyChanges"/> 
		/// can be used in order to manually apply the changes.</remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout executing the write settings command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ApplyChanges"/>
		/// <seealso cref="ApplyConfigurationChangesEnabled"/>
		/// <seealso cref="SetParameter(string, byte[])"/>
		public void WriteChanges()
		{
			ExecuteParameter("WR");
		}

		/// <summary>
		/// Enables the Bluetooth interface of this XBee device.
		/// </summary>
		/// <remarks>
		/// To work with this interface, you must also configure the Bluetooth password if not done
		/// previously. You can use the <see cref="UpdateBluetoothPassword(string)"/> method for
		/// that purpose.
		/// Note that your device must have Bluetooth Low Energy support to use this method.
		/// </remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout enabling the interface.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="DisableBluetooth"/>
		/// <seealso cref="UpdateBluetoothPassword(string)"/>
		public void EnableBluetooth()
		{
			EnableBluetooth(true);
		}

		/// <summary>
		/// Disables the Bluetooth interface of this XBee device.
		/// </summary>
		/// <remarks>
		/// Note that your device must have Bluetooth Low Energy support to use this method.
		/// </remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout disabling the interface.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="EnableBluetooth()"/>
		public void DisableBluetooth()
		{
			EnableBluetooth(false);
		}

		/// <summary>
		/// Enables or disables the Bluetooth interface of this XBee device.
		/// </summary>
		/// <param name="enable"><c>true</c> to enable the Bluetooth interface, <c>false</c> to
		/// disable it.</param>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout enabling or disabling the interface.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		private void EnableBluetooth(bool enable)
		{
			SetParameter("BT", new byte[] { (byte)(enable ? 0x01 : 0x00) });
			WriteChanges();
		}

		/// <summary>
		/// Reads and returns the EUI-48 Bluetooth MAC address of this XBee device in a format such
		/// as <c>00112233AABB</c>.
		/// </summary>
		/// <remarks>
		/// Note that your device must have Bluetooth Low Energy support to use this method.
		/// </remarks>
		/// <returns>The Bluetooth MAC address.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout reading the MAC address.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public string GetBluetoothMacAddress()
		{
			return HexUtils.ByteArrayToHexString(GetParameter("BL"));
		}

		/// <summary>
		/// Changes the password of this Bluetooth device with the new one provided.
		/// </summary>
		/// <remarks>
		/// Note that your device must have Bluetooth Low Energy support to use this method.
		/// </remarks>
		/// <param name="newPassword">New Bluetooth password.</param>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout changing the Bluetooth password.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		public void UpdateBluetoothPassword(string newPassword)
		{
			// Generate a new salt and verifier.
			byte[] salt = SRP.Utils.GenerateSalt();
			byte[] verifier = SRP.Utils.GenerateVerifier(salt, newPassword);

			// Set the salt.
			SetParameter("$S", salt);

			// Set the verifier (split in 4 settings).
			int index = 0;
			int atLength = verifier.Length / 4;
			byte[] part = new byte[atLength];

			Array.Copy(verifier, index, part, 0, atLength);
			SetParameter("$V", part);
			index += atLength;
			Array.Copy(verifier, index, part, 0, atLength);
			SetParameter("$W", part);
			index += atLength;
			Array.Copy(verifier, index, part, 0, atLength);
			SetParameter("$X", part);
			index += atLength;
			Array.Copy(verifier, index, part, 0, atLength);
			SetParameter("$Y", part);

			// Write changes.
			WriteChanges();
		}

		/// <summary>
		/// Returns the address of the device in string format.
		/// </summary>
		/// <returns>The address of the device in string format.</returns>
		public string GetAddressString()
		{
			if (XBee64BitAddr != null)
				return XBee64BitAddr.ToString();
			else if (XBee16BitAddr != null)
				return XBee16BitAddr.ToString();
			else
				return "";
		}

		/// <summary>
		/// Configures the 16-bit address (network address) of this XBee device with the provided one.
		/// </summary>
		/// <param name="xbee16BitAddress">The new 16-bit address.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="xbee16BitAddress"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="XBee16BitAddr"/>
		/// <seealso cref="XBee16BitAddress"/>
		internal void Set16BitAddress(XBee16BitAddress xbee16BitAddress)
		{
			if (xbee16BitAddress == null)
				throw new ArgumentNullException("16-bit address canot be null.");

			SetParameter("MY", xbee16BitAddress.Value);
			XBee16BitAddr = xbee16BitAddress;
		}

		/// <summary>
		/// Updates the current device reference with the data provided for the given device.
		/// </summary>
		/// <remarks>This is only for internal use.</remarks>
		/// <param name="device">The XBee Device to get the data from.</param>
		internal void UpdateDeviceDataFrom(AbstractXBeeDevice device)
		{
			// Only update the Node Identifier if the provided is not null.
			if (device.NodeID != null)
				NodeID = device.NodeID;

			// Only update the 64-bit address if the original is <c>null</c> or unknown.
			XBee64BitAddress addr64 = device.XBee64BitAddr;
			if (addr64 != null && !addr64.Equals(XBee64BitAddress.UNKNOWN_ADDRESS)
					&& !addr64.Equals(XBee64BitAddr)
					&& (XBee64BitAddr == null
						|| XBee64BitAddr.Equals(XBee64BitAddress.UNKNOWN_ADDRESS)))
			{
				XBee64BitAddr = addr64;
			}

			XBee16BitAddress addr16 = device.XBee16BitAddr;
			if (addr16 != null && !addr16.Equals(XBee16BitAddr))
			{
				XBee16BitAddr = addr16;
			}
		}

		/// <summary>
		/// Sends the given AT command and waits for answer or until the configured receive timeout 
		/// expires.
		/// </summary>
		/// <remarks>The receive timeout can be consulted/configured using the <see cref="ReceiveTimeout"/> 
		/// property.</remarks>
		/// <param name="command">The AT command to be sent.</param>
		/// <returns>An <see cref="ATCommandResponse"/> object containing the response of the command or 
		/// <c>null</c> if there is no response.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/>is <c>null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode is different from 
		/// <see cref="OperatingMode.API"/> and <see cref="OperatingMode.API_ESCAPE"/>.</exception>
		/// <exception cref="InvalidCastException">If the received packet is invalid.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given AT command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBeeDevice.ReceiveTimeout"/>
		/// <seealso cref="ATCommand"/>
		/// <seealso cref="ATCommandResponse"/>
		protected ATCommandResponse SendATCommand(ATCommand command)
		{
			// Check if command is null.
			if (command == null)
				throw new ArgumentNullException("AT command cannot be null.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			ATCommandResponse response = null;
			OperatingMode operatingMode = OperatingMode;
			switch (operatingMode)
			{
				case OperatingMode.AT:
				case OperatingMode.UNKNOWN:
				default:
					throw new InvalidOperatingModeException(operatingMode);
				case OperatingMode.API:
				case OperatingMode.API_ESCAPE:
					// Create the corresponding AT command packet depending on if the device is local or remote.
					XBeePacket packet;
					if (IsRemote)
					{
						XBee16BitAddress remote16BitAddress = XBee16BitAddr;
						if (remote16BitAddress == null)
							remote16BitAddress = XBee16BitAddress.UNKNOWN_ADDRESS;
						RemoteATCommandOptions remoteATCommandOptions = RemoteATCommandOptions.OPTION_NONE;
						if (ApplyConfigurationChangesEnabled)
							remoteATCommandOptions |= RemoteATCommandOptions.OPTION_APPLY_CHANGES;
						packet = new RemoteATCommandPacket(GetNextFrameID(), XBee64BitAddr, remote16BitAddress,
							(byte)remoteATCommandOptions, command.Command, command.Parameter);
					}
					else
					{
						if (ApplyConfigurationChangesEnabled)
							packet = new ATCommandPacket(GetNextFrameID(), command.Command, command.Parameter);
						else
							packet = new ATCommandQueuePacket(GetNextFrameID(), command.Command, command.Parameter);
					}
					if (command.Parameter == null)
						logger.DebugFormat(ToString() + "Sending AT command '{0}'.", command.Command);
					else
						logger.DebugFormat(ToString() + "Sending AT command '{0} {1}'.", command.Command, HexUtils.PrettyHexString(command.Parameter));
					try
					{
						// Send the packet and build the corresponding response depending on if the device is local or remote.
						XBeePacket answerPacket;
						if (IsRemote)
							answerPacket = localXBeeDevice.SendXBeePacket(packet);
						else
							answerPacket = SendXBeePacket(packet);

						if (answerPacket is ATCommandResponsePacket c)
							response = new ATCommandResponse(command, c.CommandValue, c.Status);
						else if (answerPacket is RemoteATCommandResponsePacket r)
							response = new ATCommandResponse(command, r.CommandValue, r.Status);

						if (response != null && response.Response != null)
							logger.DebugFormat(ToString() + "AT command response: {0}.", HexUtils.PrettyHexString(response.Response));
						else
							logger.Debug(ToString() + "AT command response: null.");
					}
					catch (InvalidCastException e)
					{
						logger.Error("Received an invalid packet type after sending an AT command packet." + e);
					}
					break;
			}
			return response;
		}

		/// <summary>
		/// Sends the given XBee packet asynchronously.
		/// </summary>
		/// <remarks>The method will not wait for an answer for the packet.
		/// 
		/// To be notified when the answer is received, use 
		/// <see cref="SendXBeePacket(XBeePacket, EventHandler{PacketReceivedEventArgs})"/>.</remarks>
		/// <param name="packet">XBee packet to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packet"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode is different from 
		/// <see cref="OperatingMode.API"/> and <see cref="OperatingMode.API_ESCAPE"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SendXBeePacket(XBeePacket)"/>
		/// <seealso cref="SendXBeePacket(XBeePacket, EventHandler{PacketReceivedEventArgs})"/>
		/// <seealso cref="SendXBeePacketAsync(XBeePacket)"/>
		/// <seealso cref="PacketReceivedEventArgs"/>
		/// <seealso cref="XBeePacket"/>
		protected void SendXBeePacketAsync(XBeePacket packet)
		{
			SendXBeePacket(packet, null);
		}

		/// <summary>
		/// Sends the given XBee packet asynchronously and registers the given packet event handler (if 
		/// not <c>null</c>) to wait for an answer.
		/// </summary>
		/// <param name="packet">XBee packet to be sent.</param>
		/// <param name="handler">Event handler for the operation, <c>null</c> not to be notified when 
		/// the answer arrives.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packet"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode is different from 
		/// <see cref="OperatingMode.API"/> and <see cref="OperatingMode.API_ESCAPE"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SendXBeePacket(XBeePacket)"/>
		/// <seealso cref="SendXBeePacketAsync(XBeePacket)"/>
		/// <seealso cref="PacketReceivedEventArgs"/>
		/// <seealso cref="XBeePacket"/>
		protected void SendXBeePacket(XBeePacket packet, EventHandler<PacketReceivedEventArgs> handler)
		{
			// Check if the packet to send is <c>null</c>.
			if (packet == null)
				throw new ArgumentNullException("XBee packet cannot be null.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			OperatingMode operatingMode = OperatingMode;
			switch (operatingMode)
			{
				case OperatingMode.AT:
				case OperatingMode.UNKNOWN:
				default:
					throw new InvalidOperatingModeException(operatingMode);
				case OperatingMode.API:
				case OperatingMode.API_ESCAPE:
					// Add the required frame ID and event handler.
					if (packet is XBeeAPIPacket)
					{
						XBeeAPIPacket apiPacket = (XBeeAPIPacket)packet;

						if (handler != null && apiPacket.NeedsAPIFrameID)
							dataReader.AddPacketReceivedHandler(handler, apiPacket.FrameID);
						else if (handler != null)
							dataReader.AddPacketReceivedHandler(handler);
					}

					// Write packet data.
					WritePacket(packet);
					break;
			}
		}

		/// <summary>
		/// Sends the given XBee packet synchronously and and blocks until response is received or receive 
		/// timeout is reached.
		/// </summary>
		/// <remarks>The receive timeout can be consulted/configured using the <see cref="ReceiveTimeout"/> 
		/// property.
		/// 
		/// Use <see cref="SendXBeePacketAsync(XBeePacket)"/> for non-blocking operations.</remarks>
		/// <param name="packet">XBee packet to be sent.</param>
		/// <returns>An <see cref="XBeePacket"/> that contains the response of the sent packet or <c>null</c> 
		/// if there is no response.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="packet"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode is different from 
		/// <see cref="OperatingMode.API"/> and <see cref="OperatingMode.API_ESCAPE"/>.</exception>
		/// <exception cref="TimeoutException">If the configured time expires while waiting for the 
		/// packet reply.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SendXBeePacket(XBeePacket)"/>
		/// <seealso cref="SendXBeePacket(XBeePacket, EventHandler{PacketReceivedEventArgs})"/>
		/// <seealso cref="XBeePacket"/>
		protected XBeePacket SendXBeePacket(XBeePacket packet)
		{
			// Check if the packet to send is <c>null</c>.
			if (packet == null)
				throw new ArgumentNullException("XBee packet cannot be null.");
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			OperatingMode operatingMode = OperatingMode;
			switch (operatingMode)
			{
				case OperatingMode.AT:
				case OperatingMode.UNKNOWN:
				default:
					throw new InvalidOperatingModeException(operatingMode);
				case OperatingMode.API:
				case OperatingMode.API_ESCAPE:
					// Build response container.
					var responseList = new List<XBeePacket>();

					// If the packet does not need frame ID, send it async. and return null.
					if (packet is XBeeAPIPacket)
					{
						if (!((XBeeAPIPacket)packet).NeedsAPIFrameID)
						{
							SendXBeePacketAsync(packet);
							return null;
						}
					}
					else
					{
						SendXBeePacketAsync(packet);
						return null;
					}

					// Add the packet received event handler to the data reader.
					EventHandler<PacketReceivedEventArgs> handler = (sender, e) => XBeePaketReceived(sender, e, packet, responseList);
					dataReader.AddPacketReceivedHandler(handler);

					// Write the packet data.
					WritePacket(packet);
					try
					{
						// Wait for response or timeout.
						lock (responseList)
						{
							Monitor.Wait(responseList, receiveTimeout);
						}
						// After the wait check if we received any response, if not throw timeout exception.
						if (responseList.Count < 1)
							throw new Exceptions.TimeoutException();
						// Return the received packet.
						return responseList[0];
					}
					finally
					{
						// Always remove the packet received event handler from the list.
						dataReader.RemovePacketReceivedHandler(handler);
					}
			}
		}

		/// <summary>
		/// Gets the next Frame ID of this XBee device.
		/// </summary>
		/// <returns>The next Frame ID.</returns>
		protected byte GetNextFrameID()
		{
			if (IsRemote)
				return localXBeeDevice.GetNextFrameID();
			if (currentFrameID == 0xff)
				currentFrameID = 1; // Reset counter.
			else
				currentFrameID++;
			return currentFrameID;
		}

		/// <summary>
		/// Sends the provided <see cref="XBeePacket"/> and determines if the transmission 
		/// status is success for synchronous transmissions.
		/// </summary>
		/// <remarks>If the status is not success, an <see cref="TransmitException"/> is thrown.</remarks>
		/// <param name="packet">The <see cref="XBeePacket"/> to be sent.</param>
		/// <param name="asyncTransmission">Determines whether the transmission must be asynchronous.</param>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the received packet is not an instance of <see cref="TransmitStatusPacket"/> 
		/// or if <paramref name="packet"/> is not an instance of <see cref="TXStatusPacket"/>
		/// or if its transmit status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBeePacket"/>
		protected void SendAndCheckXBeePacket(XBeePacket packet, bool asyncTransmission)
		{
			XBeePacket receivedPacket = null;

			// Send the XBee packet.
			try
			{
				if (asyncTransmission)
					SendXBeePacketAsync(packet);
				else
					receivedPacket = SendXBeePacket(packet);
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}

			// If the transmission is async. we are done.
			if (asyncTransmission)
				return;

			// Check if the packet received is a valid transmit status packet.
			if (receivedPacket == null)
				throw new TransmitException(XBeeTransmitStatus.UNKNOWN);

			XBeeTransmitStatus status = XBeeTransmitStatus.UNKNOWN;
			if (receivedPacket is TransmitStatusPacket)
				status = ((TransmitStatusPacket)receivedPacket).TransmitStatus;
			else if (receivedPacket is TXStatusPacket)
				status = ((TXStatusPacket)receivedPacket).TransmitStatus;

			if (status != XBeeTransmitStatus.SUCCESS
					&& status != XBeeTransmitStatus.SELF_ADDRESSED)
				throw new TransmitException(status);
		}

		/// <summary>
		/// Checks if the provided <see cref="ATCommandResponse"/> is valid throwing an <see cref="ATCommandException"/> 
		/// in case it is not.
		/// </summary>
		/// <param name="response">The <see cref="ATCommandResponse"/> to check.</param>
		/// <exception cref="ATCommandException">If <c><paramref name="response"/> == null</c>
		/// or if <c>response.Status != <see cref="ATCommandStatus.OK"/></c>.</exception>
		/// <seealso cref="ATCommandResponse"/>
		protected void CheckATCommandResponseIsValid(ATCommandResponse response)
		{
			if (response == null)
				throw new ATCommandException(ATCommandStatus.UNKNOWN);
			else if (response.Status != ATCommandStatus.OK)
				throw new ATCommandException(response.Status);
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
		protected virtual AssociationIndicationStatus GetAssociationIndicationStatus()
		{
			var associationIndicationValue = GetParameter("AI");
			if (associationIndicationValue == null || associationIndicationValue.Length < 1)
				throw new ATCommandEmptyException("AI");
			return AssociationIndicationStatus.NJ_EXPIRED.Get((byte)ByteUtils.ByteArrayToInt(associationIndicationValue));
		}

		/// <summary>
		/// Forces this XBee device to immediately disassociate from the network and re-attempt to associate.
		/// </summary>
		/// <remarks>Only valid for End Devices.</remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout executing the force disassociate command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetAssociationIndicationStatus"/>
		protected void ForceDisassociate()
		{
			ExecuteParameter("DA");
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
		/// <seealso cref="SendSerialData(byte[])"/>
		protected void SendUserDataRelay(XBeeLocalInterface destinationInterface, byte[] data)
		{
			if (destinationInterface == XBeeLocalInterface.UNKNOWN)
				throw new ArgumentException("Destination interface cannot be unknown.");
			if (data.Length > 255)
				throw new ArgumentException("Data length cannot be greater than 255 bytes.");

			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send User Data Relay from a remote device.");

			logger.DebugFormat(ToString() + "Sending User Data Relay to {0} >> {1}.",
				destinationInterface.GetDescription(), HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new UserDataRelayPacket(GetNextFrameID(), destinationInterface, data);
			// Send the packet asynchronously since User Data Relay frames do not receive any transmit status.
			SendAndCheckXBeePacket(xbeePacket, true);
		}

		/// <summary>
		/// Sends the given data to the XBee Bluetooth interface in a User Data Relay frame.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error sending the Bluetooth
		/// data.</exception>
		/// <seealso cref="SendMicroPythonData(byte[])"/>
		/// <seealso cref="SendSerialData(byte[])"/>
		/// <seealso cref="SendUserDataRelay(XBeeLocalInterface, byte[])"/>
		protected void SendBluetoothData(byte[] data)
		{
			SendUserDataRelay(XBeeLocalInterface.BLUETOOTH, data);
		}

		/// <summary>
		/// Sends the given data to the XBee MicroPython interface in a User Data Relay frame.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error sending the
		/// MicroPython data.</exception>
		/// <seealso cref="SendBluetoothData(byte[])"/>
		/// <seealso cref="SendSerialData(byte[])"/>
		/// <seealso cref="SendUserDataRelay(XBeeLocalInterface, byte[])"/>
		protected void SendMicroPythonData(byte[] data)
		{
			SendUserDataRelay(XBeeLocalInterface.MICROPYTHON, data);
		}

		/// <summary>
		/// Sends the given data to the XBee serial interface in a User Data Relay frame.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <exception cref="ArgumentException">If data length is greater than 255 bytes.</exception>
		/// <exception cref="XBeeException">If there is any XBee related error sending the serial
		/// data.</exception>
		/// <seealso cref="SendBluetoothData(byte[])"/>
		/// <seealso cref="SendMicroPythonData(byte[])"/>
		/// <seealso cref="SendUserDataRelay(XBeeLocalInterface, byte[])"/>
		protected void SendSerialData(byte[] data)
		{
			SendUserDataRelay(XBeeLocalInterface.SERIAL, data);
		}

		/// <summary>
		/// Callback called after an XBee packet is received and the corresponding Packet Received event
		/// has been fired.
		/// </summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="e">The Packet Received event.</param>
		/// <param name="sentPacket">The sent packet to get its response.</param>
		/// <param name="responseList">The list to add the received response to.</param>
		/// <seealso cref="PacketReceivedEventArgs"/>
		/// <seealso cref="XBeePacket"/>
		private void XBeePaketReceived(object sender, PacketReceivedEventArgs e, XBeePacket sentPacket, IList<XBeePacket> responseList)
		{
			// Check if it is the packet we are waiting for.
			if (((XBeeAPIPacket)e.ReceivedPacket).CheckFrameID((((XBeeAPIPacket)sentPacket).FrameID)))
			{
				// Security check to avoid class cast exceptions. It has been observed that parallel processes 
				// using the same connection but with different frame index may collide and cause this exception at some point.
				if (sentPacket is XBeeAPIPacket && e.ReceivedPacket is XBeeAPIPacket)
				{
					XBeeAPIPacket sentAPIPacket = (XBeeAPIPacket)sentPacket;
					XBeeAPIPacket receivedAPIPacket = (XBeeAPIPacket)e.ReceivedPacket;

					// If the packet sent is an AT command, verify that the received one is an AT command response and 
					// the command matches in both packets.
					if (sentAPIPacket.FrameType == APIFrameType.AT_COMMAND)
					{
						if (receivedAPIPacket.FrameType != APIFrameType.AT_COMMAND_RESPONSE)
							return;
						if (!((ATCommandPacket)sentAPIPacket).Command.Equals(((ATCommandResponsePacket)e.ReceivedPacket).Command, StringComparison.CurrentCultureIgnoreCase))
							return;
					}
					// If the packet sent is a remote AT command, verify that the received one is a remote AT command response and 
					// the command matches in both packets.
					if (sentAPIPacket.FrameType == APIFrameType.REMOTE_AT_COMMAND_REQUEST)
					{
						if (receivedAPIPacket.FrameType != APIFrameType.REMOTE_AT_COMMAND_RESPONSE)
							return;
						if (!((RemoteATCommandPacket)sentAPIPacket).Command.Equals(((RemoteATCommandResponsePacket)e.ReceivedPacket).Command, StringComparison.CurrentCultureIgnoreCase))
							return;
					}
				}

				// Verify that the sent packet is not the received one! This can happen when the echo mode is enabled in the 
				// serial port.
				if (!IsSamePacket(sentPacket, e.ReceivedPacket))
				{
					responseList.Add(e.ReceivedPacket);
					lock (responseList)
					{
						Monitor.Pulse(responseList);
					}
				}
			}
		}

		/// <summary>
		/// Returns whether the sent packet is the same than the received one.
		/// </summary>
		/// <param name="sentPacket">The packet sent.</param>
		/// <param name="receivedPacket">The packet received.</param>
		/// <returns><c>true</c> if the sent packet is the same than the received one, <c>false</c> 
		/// otherwise.</returns>
		/// <seealso cref="XBeePacket"/>
		private bool IsSamePacket(XBeePacket sentPacket, XBeePacket receivedPacket)
		{
			// TODO Should not we implement the Equals method in the XBeePacket??
			if (HexUtils.ByteArrayToHexString(sentPacket.GenerateByteArray()).Equals(HexUtils.ByteArrayToHexString(receivedPacket.GenerateByteArray())))
				return true;
			return false;
		}

		/// <summary>
		/// Writes the given XBee packet in the connection interface of this device.
		/// </summary>
		/// <param name="packet">The XBee packet to be written.</param>
		/// <exception cref="XBeeException">If there is any error writing the packet.</exception>
		/// <seealso cref="XBeePacket"/>
		private void WritePacket(XBeePacket packet)
		{
			logger.DebugFormat(ToString() + "Sending XBee packet: \n{0}", packet.ToPrettyString());
			// Write bytes with the required escaping mode.
			switch (operatingMode)
			{
				case OperatingMode.API:
				default:
					var buf = packet.GenerateByteArray();
					ConnectionInterface.WriteData(buf, 0, buf.Length);
					break;
				case OperatingMode.API_ESCAPE:
					var buf2 = packet.GenerateByteArrayEscaped();
					ConnectionInterface.WriteData(buf2, 0, buf2.Length);
					break;
			}
		}

		/// <summary>
		/// Returns the latest 802.15.4 IO packet and returns its value.
		/// </summary>
		/// <returns>The value of the latest received 802.15.4 IO packet.</returns>
		private byte[] ReceiveRaw802IOPacket()
		{
			ioPacketReceived = false;
			ioPacketPayload = null;
			IOPacketReceived += ReceiveIOPacket;
			lock (ioLock)
			{
				Monitor.Wait(ioLock, receiveTimeout);
			}
			IOPacketReceived -= ReceiveIOPacket;
			if (ioPacketReceived)
				return ioPacketPayload;
			return null;
		}

		/// <summary>
		/// Callback called after an IO sample packet is received and the corresponding Packet Received 
		/// event has been fired.
		/// </summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="e">The Packet Received event.</param>
		/// <seealso cref="PacketReceivedEventArgs"/>
		private void ReceiveIOPacket(object sender, PacketReceivedEventArgs e)
		{
			// Discard non API packets.
			if (!(e.ReceivedPacket is XBeeAPIPacket))
				return;
			// If we already have received an IO packet, ignore this packet.
			if (ioPacketReceived)
				return;

			// Save the packet value (IO sample payload)
			switch (((XBeeAPIPacket)e.ReceivedPacket).FrameType)
			{
				case APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR:
					ioPacketPayload = ((IODataSampleRxIndicatorPacket)e.ReceivedPacket).RFData;
					break;
				case APIFrameType.RX_IO_16:
					ioPacketPayload = ((RX16IOPacket)e.ReceivedPacket).RFData;
					break;
				case APIFrameType.RX_IO_64:
					ioPacketPayload = ((RX64IOPacket)e.ReceivedPacket).RFData;
					break;
				default:
					return;
			}
			// Set the IO packet received flag.
			ioPacketReceived = true;

			// Continue execution by notifying the lock object.
			lock (ioLock)
			{
				Monitor.Pulse(ioLock);
			}
		}

		/// <summary>
		/// Sends the given AT parameter to this XBee device with an optional argument or value and 
		/// returns the response (likely the value) of that parameter in a byte array format.
		/// </summary>
		/// <param name="parameter">The name of the AT command to be executed.</param>
		/// <param name="parameterValue">The value of the parameter to set (if any).</param>
		/// <returns>A byte array containing the value of the parameter.</returns>
		/// <exception cref="ArgumentException">If <paramref name="parameter"/> is not 2 characters long.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="parameter"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given AT command.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="GetParameter(string)"/>
		/// <seealso cref="ExecuteParameter(string)"/>
		/// <seealso cref="SetParameter(string, byte[])"/>
		private byte[] SendParameter(string parameter, byte[] parameterValue)
		{
			if (parameter == null)
				throw new ArgumentNullException("Parameter cannot be null.");
			if (parameter.Length != 2)
				throw new ArgumentException("Parameter must contain exactly 2 characters.");

			ATCommand atCommand = new ATCommand(parameter, parameterValue);

			// Create and send the AT Command.
			ATCommandResponse response = null;
			try
			{
				response = SendATCommand(atCommand);
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}

			// Check if AT Command response is valid.
			CheckATCommandResponseIsValid(response);

			// If the AT Command was written and corresponds to the Node Identifier, 
			// update it internally.
			if (parameter.Equals(PARAMETER_NODE_ID) 
					&& parameterValue != null 
					&& parameterValue.Length > 0 
					&& response.Status == ATCommandStatus.OK)
				NodeID = Encoding.UTF8.GetString(parameterValue, 0, parameterValue.Length);

			// Return the response value.
			return response.Response;
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
		protected virtual void Open()
		{
			logger.Info(ToString() + "Opening the connection interface...");

			// First, verify that the connection is not already open.
			if (ConnectionInterface.IsOpen)
				throw new InterfaceAlreadyOpenException();

			// Connect the interface.
			try
			{
				ConnectionInterface.Open();
			}
			catch (Exception e)
			{
				throw new XBeeException(string.Format(ERROR_OPENING_INTERFACE, e.Message));
			}

			logger.Info(ToString() + "Connection interface open.");

			// Initialize the data reader.
			dataReader = new DataReader(ConnectionInterface, OperatingMode, this);
			dataReader.Start();

			// Wait 10 milliseconds until the dataReader thread is started.
			// This is because when the connection is opened immediately after 
			// closing it, there is sometimes a concurrency problem and the 
			// dataReader thread never dies.
			Task.Delay(10).Wait();

			if (ConnectionInterface.GetConnectionType() == ConnectionType.BLUETOOTH)
			{
				// The communication in bluetooth is always done through API frames
				// regardless of the AP setting.
				OperatingMode = OperatingMode.API;
				dataReader.SetXBeeReaderMode(OperatingMode);

				// Perform the bluetooth authentication.
				try
				{
					logger.Info(ToString() + "Starting bluetooth authentication...");
					BluetoothAuthentication auth = new BluetoothAuthentication(this, bluetoothPassword);
					auth.Authenticate();
					ConnectionInterface.SetEncryptionKeys(auth.Key, auth.TxNonce, auth.RxNonce);
					logger.Info(ToString() + "Authentication finished successfully.");
				}
				catch (Exception e)
				{
					Close();
					throw e;
				}
			}
			else
			{
				// Determine the operating mode of the XBee device if it is unknown.
				if (OperatingMode == OperatingMode.UNKNOWN)
					OperatingMode = DetermineOperatingMode();

				// Check if the operating mode is a valid and supported one.
				if (OperatingMode == OperatingMode.UNKNOWN)
				{
					Close();
					throw new InvalidOperatingModeException("Could not determine operating mode.");
				}
				else if (OperatingMode == OperatingMode.AT)
				{
					Close();
					throw new InvalidOperatingModeException(OperatingMode);
				}
			}

			// Read the device info (obtain its parameters and protocol).
			ReadDeviceInfo();
		}

		/// <summary>
		/// Closes the connection interface associated with this XBee device.
		/// </summary>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Open"/>
		protected void Close()
		{
			// Stop XBee reader.
			if (dataReader != null && dataReader.IsRunning)
				dataReader.StopReader();
			// Close interface.
			ConnectionInterface.Close();
			logger.Info(ToString() + "Connection interface closed.");
		}

		/// <summary>
		/// Determines the operating mode of this XBee device.
		/// </summary>
		/// <returns>The operating mode of the XBee device.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the packet is being sent from a remote 
		/// device.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="OperatingMode"/>
		protected OperatingMode DetermineOperatingMode()
		{
			try
			{
				// Check if device is in API or API Escaped operating modes.
				OperatingMode = OperatingMode.API;
				dataReader.SetXBeeReaderMode(OperatingMode);

				ATCommandResponse response = SendATCommand(new ATCommand("AP"));
				if (response.Response != null && response.Response.Length > 0)
				{
					if (response.Response[0] != OperatingMode.API.GetID())
					{
						OperatingMode = OperatingMode.API_ESCAPE;
						dataReader.SetXBeeReaderMode(OperatingMode);
					}
					logger.DebugFormat(ToString() + "Using {0}.", OperatingMode.GetName());
					return OperatingMode;
				}
			}
			catch (Exceptions.TimeoutException)
			{
				// Check if device is in AT operating mode.
				OperatingMode = OperatingMode.AT;
				dataReader.SetXBeeReaderMode(OperatingMode);

				try
				{
					// It is necessary to wait at least 1 second to enter in 
					// command mode after sending any data to the device.
					Task.Delay(TIMEOUT_BEFORE_COMMAND_MODE).Wait();
					// Try to enter in AT command mode, if so the module is in AT mode.
					bool success = EnterATCommandMode();
					if (success)
						return OperatingMode.AT;
				}
				catch (Exceptions.TimeoutException e1)
				{
					logger.Error(e1.Message, e1);
				}
				catch (System.TimeoutException e1)
				{
					logger.Error(e1.Message, e1);
				}
				catch (InvalidOperatingModeException e1)
				{
					logger.Error(e1.Message, e1);
				}
			}
			catch (InvalidOperatingModeException e)
			{
				logger.Error("Invalid operating mode", e);
			}
			catch (IOException e)
			{
				logger.Error(e.Message, e);
			}
			return OperatingMode.UNKNOWN;
		}

		/// <summary>
		/// Attempts to put this device in AT Command mode. Only valid if device is working in AT mode.
		/// </summary>
		/// <returns><c>true</c> if the device entered in AT command mode, <c>false</c> otherwise.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperatingModeException">If the operating mode cannot be determined 
		/// or is not supported.</exception>
		/// <exception cref="TimeoutException">If the configured time for this device expires.</exception>
		private bool EnterATCommandMode()
		{
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			if (OperatingMode != OperatingMode.AT)
				throw new InvalidOperatingModeException("Invalid mode. Command mode can be only accessed while in AT mode.");

			// Enter in AT command mode (send '+++'). The process waits 1,5 seconds for the 'OK\n'.
			byte[] readData = new byte[256];
			try
			{
				// Send the command mode sequence.
				var rawCmdModeChar = Encoding.UTF8.GetBytes(COMMAND_MODE_CHAR);
				ConnectionInterface.WriteData(rawCmdModeChar, 0, rawCmdModeChar.Length);
				ConnectionInterface.WriteData(rawCmdModeChar, 0, rawCmdModeChar.Length);
				ConnectionInterface.WriteData(rawCmdModeChar, 0, rawCmdModeChar.Length);

				// Wait some time to let the module generate a response.
				Task.Delay(TIMEOUT_ENTER_COMMAND_MODE).Wait();

				// Read data from the device (it should answer with 'OK\r').
				int readBytes = ConnectionInterface.ReadData(readData, 0, readData.Length);
				if (readBytes < COMMAND_MODE_OK.Length)
					throw new Exceptions.TimeoutException();

				// Check if the read data is 'OK\r'.
				string readString = Encoding.UTF8.GetString(readData, 0, readBytes);
				if (!readString.Contains(COMMAND_MODE_OK))
					return false;

				// Read data was 'OK\r'.
				return true;
			}
			catch (IOException e)
			{
				logger.Error(e.Message, e);
			}
			return false;
		}

		/// <summary>
		/// Returns the network associated with this XBee device.
		/// </summary>
		/// <returns>The XBee network of the device.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="XBeeNetwork"/>
		protected internal virtual XBeeNetwork GetNetwork()
		{
			if (IsRemote)
				throw new Exception("Remote devices do not have network.");

			if (!IsOpen)
				throw new InterfaceNotOpenException();

			if (network == null)
				network = new XBeeNetwork(this);
			return network;
		}

		/// <summary>
		/// Sends the given XBee packet and registers the given packet handler (if not <c>null</c>) to 
		/// manage what happens when the answer is received.
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
		protected void SendPacket(XBeePacket packet, EventHandler<PacketReceivedEventArgs> handler)
		{
			try
			{
				SendXBeePacket(packet, handler);
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}
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
		protected internal void SendPacketAsync(XBeePacket packet)
		{
			try
			{
				SendXBeePacket(packet, null);
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}
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
		protected XBeePacket SendPacket(XBeePacket packet)
		{
			try
			{
				return SendXBeePacket(packet);
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}
		}

		/// <summary>
		/// Sends the provided data to the provided XBee device asynchronously.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for 
		/// transmit status packet.</remarks>
		/// <param name="xbeeDevice">The XBee device of the network that will receive the data.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="xbeeDevice"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="SendData(RemoteXBeeDevice, byte[])"/>
		protected virtual void SendDataAsync(RemoteXBeeDevice xbeeDevice, byte[] data)
		{
			if (xbeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null");
			SendDataAsync(xbeeDevice.XBee64BitAddr, data);
		}

		/// <summary>
		/// Sends the provided data to the given XBee device choosing the optimal send method depending 
		/// on the protocol of the local XBee device.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="ReceiveTimeout"/> property.
		/// 
		/// For non-blocking operations use the method <see cref="SendDataAsync(RemoteXBeeDevice, byte[])"/>.</remarks>
		/// <param name="xbeeDevice">The XBee device of the network that will receive the data.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="xbeeDevice"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the data.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="SendDataAsync(RemoteXBeeDevice, byte[])"/>
		protected virtual void SendData(RemoteXBeeDevice xbeeDevice, byte[] data)
		{
			if (xbeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null");

			switch (XBeeProtocol)
			{
				case XBeeProtocol.ZIGBEE:
				case XBeeProtocol.DIGI_POINT:
					if (xbeeDevice.XBee64BitAddr != null && xbeeDevice.XBee16BitAddr != null)
						SendData(xbeeDevice.XBee64BitAddr, xbeeDevice.XBee16BitAddr, data);
					else
						SendData(xbeeDevice.XBee64BitAddr, data);
					break;
				case XBeeProtocol.RAW_802_15_4:
					if (this is Raw802Device)
					{
						if (xbeeDevice.XBee64BitAddr != null)
							((Raw802Device)this).SendData(xbeeDevice.XBee64BitAddr, data);
						else
							((Raw802Device)this).SendData(xbeeDevice.XBee16BitAddr, data);
					}
					else
						SendData(xbeeDevice.XBee64BitAddr, data);
					break;
				case XBeeProtocol.DIGI_MESH:
				default:
					SendData(xbeeDevice.XBee64BitAddr, data);
					break;
			}
		}

		/// <summary>
		/// Sends the provided data to all the XBee nodes of the network (broadcast).
		/// </summary>
		/// <remarks>This method blocks till a success or error transmit status arrives or the configured 
		/// receive timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="ReceiveTimeout"/> property.
		/// </remarks>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the data.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		protected virtual void SendBroadcastData(byte[] data)
		{
			SendData(XBee64BitAddress.BROADCAST_ADDRESS, data);
		}

		/// <summary>
		/// Reads new data received by this XBee device during the configured receive timeout.
		/// </summary>
		/// <remarks>This method blocks until new data is received or the configured receive 
		/// timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the 
		/// <see cref="ReceiveTimeout"/> property.</remarks>
		/// <returns>An <see cref="XBeeMessage"/> object containing the data and the source address 
		/// of the remote node that sent the data. <c>null</c> if this did not receive new data during 
		/// the configured receive timeout.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="ReadData(int)"/>
		/// <seealso cref="ReadDataFrom(RemoteXBeeDevice)"/>
		/// <seealso cref="ReadDataFrom(RemoteXBeeDevice, int)"/>
		/// <seealso cref="XBeeMessage"/>
		protected virtual XBeeMessage ReadData()
		{
			return ReadDataPacket(null, TIMEOUT_READ_PACKET);
		}

		/// <summary>
		/// Reads new data received by this XBee device during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new data is received or the provided timeout expires.</remarks>
		/// <param name="timeout">The time to wait for new data in milliseconds.</param>
		/// <returns>An <see cref="XBeeMessage"/> object containing the data and the source address of 
		/// the remote node that sent the data. <c>null</c> if this did not receive new data during 
		/// <paramref name="timeout"/> milliseconds.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="timeout"/> <![CDATA[<]]> 0</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="ReadData()"/>
		/// <seealso cref="ReadDataFrom(RemoteXBeeDevice)"/>
		/// <seealso cref="ReadDataFrom(RemoteXBeeDevice, int)"/>
		/// <seealso cref="XBeeMessage"/>
		protected virtual XBeeMessage ReadData(int timeout)
		{
			if (timeout < 0)
				throw new ArgumentException("Read timeout must be 0 or greater.");

			return ReadDataPacket(null, timeout);
		}

		/// <summary>
		/// Reads new data received from the given remote XBee device during the configured received 
		/// timeout.
		/// </summary>
		/// <remarks>This method blocks until new data is received or the provided timeout expires.
		/// 
		/// The receive timeout can be consulted/configured using the <see cref="ReceiveTimeout"/> 
		/// property.</remarks>
		/// <param name="remoteXBeeDevice">The remote device to read data from.</param>
		/// <returns>An <see cref="XBeeMessage"/> object containing the data and the source address of 
		/// the remote node that sent the data. <c>null</c> if this did not receive new data during 
		/// the configured received timeout.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="remoteXBeeDevice"/> is <c>null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="ReadData()"/>
		/// <seealso cref="ReadData(int)"/>
		/// <seealso cref="ReadDataFrom(RemoteXBeeDevice, int)"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="XBeeMessage"/>
		protected virtual XBeeMessage ReadDataFrom(RemoteXBeeDevice remoteXBeeDevice)
		{
			if (remoteXBeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null.");

			return ReadDataPacket(remoteXBeeDevice, TIMEOUT_READ_PACKET);
		}

		/// <summary>
		/// Reads new data received from the given remote XBee device during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new data from the provided remote XBee device is 
		/// received or the given timeout expires.</remarks>
		/// <param name="remoteXBeeDevice">The remote device to read data from.</param>
		/// <param name="timeout">The time to wait for new data in milliseconds.</param>
		/// <returns>An <see cref="XBeeMessage"/> object containing the data and the source address 
		/// of the remote node that sent the data. <c>null</c> if this did not receive new data during 
		/// <paramref name="timeout"/> milliseconds.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="timeout"/> <![CDATA[<]]> 0</c>.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="remoteXBeeDevice"/> is <c>null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="ReadData()"/>
		/// <seealso cref="ReadData(int)"/>
		/// <seealso cref="ReadDataFrom(RemoteXBeeDevice)"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="XBeeMessage"/>
		protected virtual XBeeMessage ReadDataFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			if (remoteXBeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null.");
			if (timeout < 0)
				throw new ArgumentException("Read timeout must be 0 or greater.");

			return ReadDataPacket(remoteXBeeDevice, timeout);
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
		protected UserDataRelayMessage ReadUserDataRelay()
		{
			return ReadUserDataRelay(TIMEOUT_READ_PACKET);
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
		protected UserDataRelayMessage ReadUserDataRelay(int timeout)
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			XBeePacketsQueue xbeePacketsQueue = dataReader.XBeePacketsQueue;
			XBeePacket xbeePacket = xbeePacketsQueue.GetFirstUserDataRelayPacket(timeout);

			if (xbeePacket == null)
				return null;

			// Verify the packet is a User Data Relay packet.
			APIFrameType packetType = ((XBeeAPIPacket)xbeePacket).FrameType;
			if (packetType != APIFrameType.USER_DATA_RELAY_OUTPUT)
				return null;

			// Obtain the necessary data from the packet.
			UserDataRelayOutputPacket relayPacket = (UserDataRelayOutputPacket)xbeePacket;

			// Create and return the User Data Relay message.
			return new UserDataRelayMessage(relayPacket.SourceInterface, relayPacket.Data);
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
		protected ExplicitXBeeMessage ReadExplicitData()
		{
			return ReadExplicitDataPacket(null, TIMEOUT_READ_PACKET);
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
		protected ExplicitXBeeMessage ReadExplicitData(int timeout)
		{
			if (timeout < 0)
				throw new ArgumentException("Read timeout must be 0 or greater.");

			return ReadExplicitDataPacket(null, timeout);
		}

		/// <summary>
		/// Reads new explicit data received from the given remote XBee device during the configured 
		/// receive timeout.
		/// </summary>
		/// <remarks>This method blocks until new explicit data from the provided remote XBee device 
		/// is received or the configured receive timeout expires.
		/// 
		/// For non-blocking operations, register an event handler to
		/// <see cref="AbstractXBeeDevice.ExplicitDataReceived"/>.</remarks>
		/// <param name="remoteXBeeDevice">The remote device to read explicit data from.</param>
		/// <returns>An <see cref="ExplicitXBeeMessage"/> object containing the explicit data, the 
		/// source address of the remote node that sent the data and other values related to the 
		/// transmission. <c>null</c> if this device did not receive new explicit data from the provided 
		/// remote XBee device during the configured receive timeout.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteXBeeDevice"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="ExplicitXBeeMessage"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		protected ExplicitXBeeMessage ReadExplicitDataFrom(RemoteXBeeDevice remoteXBeeDevice)
		{
			if (remoteXBeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null.");

			return ReadExplicitDataPacket(remoteXBeeDevice, TIMEOUT_READ_PACKET);
		}

		/// <summary>
		/// Reads new explicit data received from the given remote XBee device during the provided 
		/// timeout.
		/// </summary>
		/// <remarks>This method blocks until new explicit data from the provided remote XBee device is 
		/// received or the given timeout expires.
		/// 
		/// For non-blocking operations, register an event handler to
		/// <see cref="AbstractXBeeDevice.ExplicitDataReceived"/>.</remarks>
		/// <param name="remoteXBeeDevice">The remote device to read explicit data from.</param>
		/// <param name="timeout">The time to wait for new explicit data in milliseconds.</param>
		/// <returns>An <see cref="ExplicitXBeeMessage"/> object containing the explicit data, the source 
		/// address of the remote node that sent the data and other values related to the transmission. 
		/// <c>null</c> if this device did not receive new data from the provided remote XBee device during 
		/// <paramref name="timeout"/> milliseconds.</returns>
		/// <exception cref="ArgumentException">If <c><paramref name="timeout"/> <![CDATA[<]]> 0</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteXBeeDevice"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="ExplicitXBeeMessage"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		protected ExplicitXBeeMessage ReadExplicitDataFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			if (remoteXBeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null.");
			if (timeout < 0)
				throw new ArgumentException("Read timeout must be 0 or greater.");

			return ReadExplicitDataPacket(remoteXBeeDevice, timeout);
		}

		/// <summary>
		/// Sends asynchronously the provided data in application layer mode to the XBee device of the 
		/// network corresponding to the given 64-bit address. Application layer mode means that you need 
		/// to specify the application layer fields to be sent with the data.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for 
		/// transmit status packet.</remarks>
		/// <param name="address">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBee64BitAddress"/>
		protected void SendExplicitDataAsync(XBee64BitAddress address, byte sourceEndpoint, byte destEndpoint,
			byte[] clusterID, byte[] profileID, byte[] data)
		{
			SendExplicitDataAsync(address, XBee16BitAddress.UNKNOWN_ADDRESS, sourceEndpoint, destEndpoint, clusterID, profileID, data);
		}

		/// <summary>
		/// Sends asynchronously the provided data in application layer mode to the XBee device of the 
		/// network corresponding to the given 64-bit/16-bit address. Application layer mode means that 
		/// you need to specify the application layer fields to be sent with the data.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for 
		/// transmit status packet.</remarks>
		/// <param name="address64Bits">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="address16Bits">The 16-bit address of the XBee that will receive the data.
		/// If it is unknown the <see cref="XBee16BitAddress.UNKNOWN_ADDRESS"/> must be used.</param>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address64Bits"/> == null</c> 
		/// or if <c><paramref name="address16Bits"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		protected void SendExplicitDataAsync(XBee64BitAddress address64Bits, XBee16BitAddress address16Bits,
			byte sourceEndpoint, byte destEndpoint, byte[] clusterID, byte[] profileID, byte[] data)
		{
			if (address64Bits == null)
				throw new ArgumentNullException("64-bit address cannot be null.");
			if (address16Bits == null)
				throw new ArgumentNullException("16-bit address cannot be null.");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null.");
			if (clusterID.Length != 2)
				throw new ArgumentException("Cluster ID must be 2 bytes.");
			if (profileID.Length != 2)
				throw new ArgumentException("Profile ID must be 2 bytes.");

			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send explicit data to a remote device from a remote device.");

			logger.DebugFormat(ToString() + "Sending explicit data asynchronously to {0}[{1}] [{2} - {3} - {4} - {5}] >> {6}.",
				address64Bits, address16Bits,
					string.Format("%02X", sourceEndpoint), string.Format("%02X", destEndpoint),
					string.Format("%04X", clusterID), string.Format("%04X", profileID),
					HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new ExplicitAddressingPacket(GetNextFrameID(), address64Bits, address16Bits, sourceEndpoint,
				destEndpoint, clusterID, profileID, 0, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, true);
		}

		/// <summary>
		/// Sends asynchronously the provided data in application layer mode to the provided XBee device 
		/// choosing the optimal send method depending on the protocol of the local XBee device. Application 
		/// layer mode means that you need to specify the application layer fields to be sent with the data.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for transmit 
		/// status packet.</remarks>
		/// <param name="remoteXBeeDevice">The XBee device of the network that will receive the data.</param>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteXBeeDevice"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote or if the protocol 
		/// is <seealso cref="XBeeProtocol.RAW_802_15_4"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="RemoteXBeeDevice"/>
		protected void SendExplicitDataAsync(RemoteXBeeDevice remoteXBeeDevice, byte sourceEndpoint, byte destEndpoint,
			byte[] clusterID, byte[] profileID, byte[] data)
		{
			if (remoteXBeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null.");

			switch (remoteXBeeDevice.XBeeProtocol)
			{
				case XBeeProtocol.ZIGBEE:
				case XBeeProtocol.DIGI_POINT:
					if (remoteXBeeDevice.XBee64BitAddr != null && remoteXBeeDevice.XBee16BitAddr != null)
						SendExplicitDataAsync(remoteXBeeDevice.XBee64BitAddr, remoteXBeeDevice.XBee16BitAddr, sourceEndpoint,
							destEndpoint, clusterID, profileID, data);
					else
						SendExplicitDataAsync(remoteXBeeDevice.XBee64BitAddr, sourceEndpoint, destEndpoint, clusterID, profileID, data);
					break;
				case XBeeProtocol.RAW_802_15_4:
					throw new OperationNotSupportedException("802.15.4. protocol does not support explicit data transmissions.");
				case XBeeProtocol.DIGI_MESH:
				default:
					SendExplicitDataAsync(remoteXBeeDevice.XBee64BitAddr, sourceEndpoint, destEndpoint, clusterID, profileID, data);
					break;
			}
		}

		/// <summary>
		/// Sends the provided data in application layer mode to the XBee device of the network corresponding 
		/// to the given 64-bit address. Application layer mode means that you need to specify the application 
		/// layer fields to be sent with the data.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.</remarks>
		/// <param name="address">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBee64BitAddress"/>
		protected void SendExplicitData(XBee64BitAddress address, byte sourceEndpoint, byte destEndpoint,
			byte[] clusterID, byte[] profileID, byte[] data)
		{
			SendExplicitData(address, XBee16BitAddress.UNKNOWN_ADDRESS, sourceEndpoint, destEndpoint, clusterID, profileID, data);
		}

		/// <summary>
		/// Sends the provided data in application layer mode to the XBee device of the network corresponding 
		/// to the given 64-bit/16-bit address. Application layer mode means that you need to specify the 
		/// application layer fields to be sent with the data.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.
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
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		protected void SendExplicitData(XBee64BitAddress address64Bits, XBee16BitAddress address16Bits,
			byte sourceEndpoint, byte destEndpoint, byte[] clusterID, byte[] profileID, byte[] data)
		{
			if (address64Bits == null)
				throw new ArgumentNullException("64-bit address cannot be null.");
			if (address16Bits == null)
				throw new ArgumentNullException("16-bit address cannot be null.");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null.");
			if (clusterID.Length != 2)
				throw new ArgumentException("Cluster ID must be 2 bytes.");
			if (profileID.Length != 2)
				throw new ArgumentException("Profile ID must be 2 bytes.");

			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send explicit data to a remote device from a remote device.");

			logger.DebugFormat(ToString() + "Sending explicit data to {0}[{1}] [{2} - {3} - {4} - {5}] >> {6}.",
				address64Bits, address16Bits,
					string.Format("%02X", sourceEndpoint), string.Format("%02X", destEndpoint),
					string.Format("%04X", clusterID), string.Format("%04X", profileID),
					HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new ExplicitAddressingPacket(GetNextFrameID(), address64Bits, address16Bits, sourceEndpoint,
				destEndpoint, clusterID, profileID, 0, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, false);
		}

		/// <summary>
		/// Sends the provided data in application layer mode to the provided XBee device choosing the 
		/// optimal send method depending on the protocol of the local XBee device. Application layer 
		/// mode means that you need to specify the application layer fields to be sent with the data.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.</remarks>
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
		/// <exception cref="OperationNotSupportedException">If the device protocol is <see cref="XBeeProtocol.RAW_802_15_4"/> 
		/// or if the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="RemoteXBeeDevice"/>
		protected void SendExplicitData(RemoteXBeeDevice remoteXBeeDevice, byte sourceEndpoint, byte destEndpoint,
			byte[] clusterID, byte[] profileID, byte[] data)
		{
			if (remoteXBeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null.");

			switch (remoteXBeeDevice.XBeeProtocol)
			{
				case XBeeProtocol.ZIGBEE:
				case XBeeProtocol.DIGI_POINT:
					if (remoteXBeeDevice.XBee64BitAddr != null && remoteXBeeDevice.XBee16BitAddr != null)
						SendExplicitData(remoteXBeeDevice.XBee64BitAddr, remoteXBeeDevice.XBee16BitAddr, sourceEndpoint,
							destEndpoint, clusterID, profileID, data);
					else
						SendExplicitData(remoteXBeeDevice.XBee64BitAddr, sourceEndpoint, destEndpoint, clusterID, profileID, data);
					break;
				case XBeeProtocol.RAW_802_15_4:
					throw new OperationNotSupportedException("802.15.4. protocol does not support explicit data transmissions.");
				case XBeeProtocol.DIGI_MESH:
				default:
					SendExplicitData(remoteXBeeDevice.XBee64BitAddr, sourceEndpoint, destEndpoint, clusterID, profileID, data);
					break;
			}
		}

		/// <summary>
		/// Sends the provided data to all the XBee nodes of the network (broadcast) in application layer mode. 
		/// Application layer mode means that you need to specify the application layer fields to be sent with 
		/// the data.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.</remarks>
		/// <param name="sourceEndpoint">Source endpoint for the transmission.</param>
		/// <param name="destEndpoint">Destination endpoint for the transmission.</param>
		/// <param name="clusterID">Cluster ID used in the transmission.</param>
		/// <param name="profileID">Profile ID used in the transmission.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="ArgumentException">If <c>clusterID.Length != 2</c> 
		/// or if <c>profileID.Length != 2</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the protocol of the device is
		/// <see cref="XBeeProtocol.RAW_802_15_4"/> or if the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the given packet synchronously.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		protected void SendBroadcastExplicitData(byte sourceEndpoint, byte destEndpoint, byte[] clusterID, byte[] profileID,
			byte[] data)
		{
			if (XBeeProtocol == XBeeProtocol.RAW_802_15_4)
				throw new OperationNotSupportedException("802.15.4. protocol does not support explicit data transmissions.");

			SendExplicitData(XBee64BitAddress.UNKNOWN_ADDRESS, sourceEndpoint, destEndpoint, clusterID, profileID, data);
		}

		/// <summary>
		/// Sends the provided data to the XBee device of the network corresponding to the given 64-bit 
		/// address.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.
		/// 
		/// The receive timeout is consulted/configured using the <see cref="ReceiveTimeout"/> property.
		/// 
		/// For non-blocking operations use the method <see cref="SendDataAsync(XBee64BitAddress, byte[])"/>.</remarks>
		/// <param name="address">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the data.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="SendData(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendData(XBee64BitAddress, XBee16BitAddress, byte[])"/>
		/// <seealso cref="SendDataAsync(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendDataAsync(XBee64BitAddress, byte[])"/>
		/// <seealso cref="SendDataAsync(XBee64BitAddress, XBee16BitAddress, byte[])"/>
		/// <seealso cref="XBee64BitAddress"/>
		protected void SendData(XBee64BitAddress address, byte[] data)
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

			logger.DebugFormat(ToString() + "Sending data to {0} >> {1}.", address, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket;
			switch (XBeeProtocol)
			{
				case XBeeProtocol.RAW_802_15_4:
					xbeePacket = new TX64Packet(GetNextFrameID(), address, (byte)XBeeTransmitOptions.NONE, data);
					break;
				default:
					xbeePacket = new TransmitPacket(GetNextFrameID(), address, XBee16BitAddress.UNKNOWN_ADDRESS, 0, (byte)XBeeTransmitOptions.NONE, data);
					break;
			}
			SendAndCheckXBeePacket(xbeePacket, false);
		}

		/// <summary>
		/// Sends the provided data to the XBee device of the network corresponding to the given 
		/// 64-bit/16-bit address.
		/// </summary>
		/// <remarks>This method blocks till a success or error response arrives or the configured receive 
		/// timeout expires.
		/// 
		/// The receive timeout is consulted/configured using the <see cref="ReceiveTimeout"/> property.
		/// 
		/// For non-blocking operations use the method 
		/// <see cref="SendDataAsync(XBee64BitAddress, XBee16BitAddress, byte[])"/>.</remarks>
		/// <param name="address64Bit">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="address16bit">The 16-bit address of the XBee that will receive the data.If it is 
		/// unknown the <see cref="XBee16BitAddress.UNKNOWN_ADDRESS"/> must be used.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address64Bit"/> == null</c> 
		/// or if <c><paramref name="address16bit"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="TimeoutException">If there is a timeout sending the data.</exception>
		/// <exception cref="TransmitException">If the transmit status generated when sending the packet is not 
		/// an instance of <see cref="TransmitStatusPacket"/> 
		/// or if it is not an instance of <see cref="TXStatusPacket"/>
		/// or if when it is correct, its status is different from <see cref="XBeeTransmitStatus.SUCCESS"/>.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="SendData(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendData(XBee64BitAddress, byte[])"/>
		/// <seealso cref="SendDataAsync(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendDataAsync(XBee64BitAddress, byte[])"/>
		/// <seealso cref="SendDataAsync(XBee64BitAddress, XBee16BitAddress, byte[])"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		protected void SendData(XBee64BitAddress address64Bit, XBee16BitAddress address16bit, byte[] data)
		{
			// Verify the parameters are not null, if they are null, throw an exception.
			if (address64Bit == null)
				throw new ArgumentNullException("64-bit address cannot be null");
			if (address16bit == null)
				throw new ArgumentNullException("16-bit address cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send data to a remote device from a remote device.");

			logger.DebugFormat(ToString() + "Sending data to {0}[{1}] >> {2}.",
					address64Bit, address16bit, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new TransmitPacket(GetNextFrameID(), address64Bit, address16bit, 0, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, false);
		}

		/// <summary>
		/// Sends asynchronously the provided data to the XBee device of the network corresponding to the 
		/// given 64-bit address.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for 
		/// transmit status packet.</remarks>
		/// <param name="address">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="SendData(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendData(XBee64BitAddress, byte[])"/>
		/// <seealso cref="SendData(XBee64BitAddress, XBee16BitAddress, byte[])"/>
		/// <seealso cref="SendDataAsync(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendDataAsync(XBee64BitAddress, XBee16BitAddress, byte[])"/>
		/// <seealso cref="XBee64BitAddress"/>
		protected void SendDataAsync(XBee64BitAddress address, byte[] data)
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

			logger.DebugFormat(ToString() + "Sending data asynchronously to {0} >> {1}.", address, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket;
			switch (XBeeProtocol)
			{
				case XBeeProtocol.RAW_802_15_4:
					xbeePacket = new TX64Packet(GetNextFrameID(), address, (byte)XBeeTransmitOptions.NONE, data);
					break;
				default:
					xbeePacket = new TransmitPacket(GetNextFrameID(), address, XBee16BitAddress.UNKNOWN_ADDRESS, 0, (byte)XBeeTransmitOptions.NONE, data);
					break;
			}
			SendAndCheckXBeePacket(xbeePacket, true);
		}

		/// <summary>
		/// Sends asynchronously the provided data to the XBee device of the network corresponding to the 
		/// given 64-bit/16-bit address.
		/// </summary>
		/// <remarks>Asynchronous transmissions do not wait for answer from the remote device or for 
		/// transmit status packet.</remarks>
		/// <param name="address64Bit">The 64-bit address of the XBee that will receive the data.</param>
		/// <param name="address16bit">The 16-bit address of the XBee that will receive the data.If it 
		/// is unknown the <see cref="XBee16BitAddress.UNKNOWN_ADDRESS"/> must be used.</param>
		/// <param name="data">Byte array containing the data to be sent.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address64Bit"/> == null</c> 
		/// or if <c><paramref name="address16bit"/> == null</c> 
		/// or if <c><paramref name="data"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="OperationNotSupportedException">If the sender device is remote.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related error.</exception>
		/// <seealso cref="ReceiveTimeout"/>
		/// <seealso cref="SendData(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendData(XBee64BitAddress, byte[])"/>
		/// <seealso cref="SendData(XBee64BitAddress, XBee16BitAddress, byte[])"/>
		/// <seealso cref="SendDataAsync(RemoteXBeeDevice, byte[])"/>
		/// <seealso cref="SendDataAsync(XBee64BitAddress, byte[])"/>
		/// <seealso cref="XBee16BitAddress"/>
		/// <seealso cref="XBee64BitAddress"/>
		protected void SendDataAsync(XBee64BitAddress address64Bit, XBee16BitAddress address16bit, byte[] data)
		{
			// Verify the parameters are not null, if they are null, throw an exception.
			if (address64Bit == null)
				throw new ArgumentNullException("64-bit address cannot be null");
			if (address16bit == null)
				throw new ArgumentNullException("16-bit address cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send data to a remote device from a remote device.");

			logger.DebugFormat(ToString() + "Sending data asynchronously to {0}[{1}] >> {2}.",
					address64Bit, address16bit, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new TransmitPacket(GetNextFrameID(), address64Bit, address16bit, 0, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, true);
		}

		/// <summary>
		/// Reads a new data packet received by this XBee device during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new data is received or the given timeout expires.
		/// 
		/// If the provided remote XBee device is <c>null</c> the method returns the first data 
		/// packet read from any remote device.
		/// 
		/// If the remote device is not <c>null</c> the method returns the first data package 
		/// read from the provided device.</remarks>
		/// <param name="remoteXBeeDevice">The remote device to get a data packet from. <c>null</c> 
		/// to read a data packet sent by any remote XBee device.</param>
		/// <param name="timeout">The time to wait for a data packet in milliseconds.</param>
		/// <returns>An <see cref="XBeeMessage"/> received by this device, containing the data and 
		/// the source address of the remote node that sent the data. <c>null</c> if this device 
		/// did not receive new data during <paramref name="timeout"/> milliseconds, or if any error occurs 
		/// while trying to get the source of the message.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="XBeeMessage"/>
		private XBeeMessage ReadDataPacket(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			XBeePacketsQueue xbeePacketsQueue = dataReader.XBeePacketsQueue;
			XBeePacket xbeePacket = null;

			if (remoteXBeeDevice != null)
				xbeePacket = xbeePacketsQueue.GetFirstDataPacketFrom(remoteXBeeDevice, timeout);
			else
				xbeePacket = xbeePacketsQueue.GetFirstDataPacket(timeout);

			if (xbeePacket == null)
				return null;

			// Obtain the remote device from the packet.
			RemoteXBeeDevice remoteDevice = null;
			try
			{
				remoteDevice = dataReader.GetRemoteXBeeDeviceFromPacket((XBeeAPIPacket)xbeePacket);
				// If the provided device is not null, add it to the network, so the 
				// device provided is the one that will remain in the network.
				if (remoteXBeeDevice != null)
					remoteDevice = GetNetwork().AddRemoteDevice(remoteXBeeDevice);

				// The packet always contains information of the source so the 
				// remote device should never be null.
				if (remoteDevice == null)
					return null;

			}
			catch (XBeeException e)
			{
				logger.Error(e.Message, e);
				return null;
			}

			// Obtain the data from the packet.
			byte[] data = null;

			switch (((XBeeAPIPacket)xbeePacket).FrameType)
			{
				case APIFrameType.RECEIVE_PACKET:
					ReceivePacket receivePacket = (ReceivePacket)xbeePacket;
					data = receivePacket.RFData;
					break;
				case APIFrameType.RX_16:
					RX16Packet rx16Packet = (RX16Packet)xbeePacket;
					data = rx16Packet.RFData;
					break;
				case APIFrameType.RX_64:
					RX64Packet rx64Packet = (RX64Packet)xbeePacket;
					data = rx64Packet.RFData;
					break;
				default:
					return null;
			}

			// Create and return the XBee message.
			return new XBeeMessage(remoteDevice, data, ((XBeeAPIPacket)xbeePacket).IsBroadcast);
		}

		/// <summary>
		/// Reads a new explicit data packet received by this XBee device during the provided timeout.
		/// </summary>
		/// <remarks>This method blocks until new explicit data is received or the given timeout 
		/// expires.
		/// 
		/// If the provided remote XBee device is <c>null</c> the method returns the first explicit 
		/// data packet read from any remote device. If the remote device is not <c>null</c> the method 
		/// returns the first explicit data package read from the provided device.</remarks>
		/// <param name="remoteXBeeDevice">The remote device to read explicit data from.</param>
		/// <param name="timeout">The time to wait for new explicit data in milliseconds.</param>
		/// <returns>An <see cref="ExplicitXBeeMessage"/> object containing the explicit data, the 
		/// source address of the remote node that sent the data and other values related to the 
		/// transmission. <c>null</c> if this device did not receive new data from the provided remote 
		/// XBee device during <paramref name="timeout"/> milliseconds.</returns>
		/// <exception cref="InterfaceNotOpenException">If the interface is not open.</exception>
		/// <seealso cref="ExplicitXBeeMessage"/>
		/// <seealso cref="RemoteXBeeDevice"/>
		private ExplicitXBeeMessage ReadExplicitDataPacket(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			// Check connection.
			if (!ConnectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			XBeePacketsQueue xbeePacketsQueue = dataReader.XBeePacketsQueue;
			XBeePacket xbeePacket = null;

			if (remoteXBeeDevice != null)
				xbeePacket = xbeePacketsQueue.GetFirstExplicitDataPacketFrom(remoteXBeeDevice, timeout);
			else
				xbeePacket = xbeePacketsQueue.GetFirstExplicitDataPacket(timeout);

			if (xbeePacket == null)
				return null;

			// Verify the packet is an explicit data packet.
			APIFrameType packetType = ((XBeeAPIPacket)xbeePacket).FrameType;
			if (packetType != APIFrameType.EXPLICIT_RX_INDICATOR)
				return null;

			// Obtain the necessary data from the packet.
			ExplicitRxIndicatorPacket explicitDataPacket = (ExplicitRxIndicatorPacket)xbeePacket;
			RemoteXBeeDevice remoteDevice = GetNetwork().GetDevice(explicitDataPacket.SourceAddress64);
			if (remoteDevice == null)
			{
				if (remoteXBeeDevice != null)
					remoteDevice = remoteXBeeDevice;
				else
					remoteDevice = new RemoteXBeeDevice(this, explicitDataPacket.SourceAddress64);
				GetNetwork().AddRemoteDevice(remoteDevice);
			}
			byte sourceEndpoint = explicitDataPacket.SourceEndpoint;
			byte destEndpoint = explicitDataPacket.DestEndpoint;
			byte[] clusterID = explicitDataPacket.ClusterID;
			byte[] profileID = explicitDataPacket.ProfileID;
			byte[] data = explicitDataPacket.RFData;

			// Create and return the XBee message.
			return new ExplicitXBeeMessage(remoteDevice, sourceEndpoint, destEndpoint, clusterID, profileID,
				data, ((XBeeAPIPacket)xbeePacket).IsBroadcast);
		}

		/// <summary>
		/// Waits until a Modem Status packet with a reset status, 
		/// <see cref="ModemStatusEvent.STATUS_HARDWARE_RESET"/> (0x00), or a watchdog timer reset, 
		/// <see cref="ModemStatusEvent.STATUS_WATCHDOG_TIMER_RESET"/> (0x01), is received or the 
		/// timeout expires.
		/// </summary>
		/// <remarks>Reads modem status events from this XBee device during the configured 
		/// receive timeout.</remarks>
		/// <returns><c>true</c> if the Modem Status packet is received, <c>false</c> otherwise.</returns>
		/// <seealso cref="ModemStatusEvent.STATUS_HARDWARE_RESET"/>
		/// <seealso cref="ModemStatusEvent.STATUS_WATCHDOG_TIMER_RESET"/>
		private bool WaitForModemResetStatusPacket()
		{
			modemStatusReceived = false;

			ModemStatusReceived += ResetModemStatusReceived;
			lock (resetLock)
			{
				Monitor.Wait(resetLock, TIMEOUT_RESET);
			}
			ModemStatusReceived -= ResetModemStatusReceived;
			return modemStatusReceived;
		}

		/// <summary>
		/// Callback called after a Modem status packet is received and the corresponding 
		/// event has been fired.
		/// </summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="e">The Packet Received event.</param>
		/// <seealso cref="ModemStatusReceivedEventArgs"/>
		private void ResetModemStatusReceived(object sender, ModemStatusReceivedEventArgs e)
		{
			if (e.ModemStatusEvent == ModemStatusEvent.STATUS_HARDWARE_RESET
						|| e.ModemStatusEvent == ModemStatusEvent.STATUS_WATCHDOG_TIMER_RESET)
			{
				modemStatusReceived = true;
				// Continue execution by notifying the lock object.
				lock (resetLock)
				{
					Monitor.Pulse(resetLock);
				}
			}
		}

		/// <summary>
		/// Sets the password of this Bluetooth device in order to connect to it.
		/// </summary>
		/// <remarks>
		/// The Bluetooth password must be provided before calling the <see cref="Open"/> method.
		/// </remarks>
		/// <param name="password">The password of this Bluetooth device.</param>
		protected void SetBluetoothPassword(string password)
		{
			bluetoothPassword = password;
		}

		/// <summary>
		/// Updates the firmware of this XBee device with the given binary stream.
		/// </summary>
		/// <remarks>This method only works for those devices that support GPM firmware update.</remarks>
		/// <param name="firmwareBinaryStream">Firmware binary stream.</param>
		/// <exception cref="GpmException"></exception>
		protected void UpdateFirmware(Stream firmwareBinaryStream)
		{
			UpdateFirmware(firmwareBinaryStream, null);
		}

		/// <summary>
		/// Updates the firmware of this XBee device with the given binary stream.
		/// </summary>
		/// <remarks>This method only works for those devices that support GPM firmware update.</remarks>
		/// <param name="firmwareBinaryStream">Firmware binary stream.</param>
		/// <param name="eventHandler">Event handler to get notified about any process event.</param>
		/// <exception cref="GpmException"></exception>
		protected void UpdateFirmware(Stream firmwareBinaryStream, EventHandler<GpmUpdateEventArgs> eventHandler)
		{
			GpmManager manager = new GpmManager(this, firmwareBinaryStream);
			if (eventHandler != null)
				manager.GpmUpdateEventHandler += eventHandler;
			try
			{
				manager.UpdateFirmware();
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				if (eventHandler != null)
					manager.GpmUpdateEventHandler -= eventHandler;
			}
		}
	}
}