using Common.Logging;
using Kveer.XBeeApi.Connection;
using Kveer.XBeeApi.Connection.Serial;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.IO;
using Kveer.XBeeApi.listeners;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Packet;
using Kveer.XBeeApi.Packet.Common;
using Kveer.XBeeApi.Packet.Raw;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Kveer.XBeeApi
{
	/// <summary>
	/// This class provides common functionality for all XBee devices.
	/// </summary>
	/// <seealso cref="XBeeDevice"/>
	/// <seealso cref="RemoteXBeeDevice"/>
	public abstract class AbstractXBeeDevice
	{
		// Constants.

		/**
		 * Default receive timeout used to wait for a response in synchronous 
		 * operations: {@value} ms.
		 * 
		 * @see XBeeDevice#getReceiveTimeout()
		 * @see XBeeDevice#setReceiveTimeout(int)
		 */
		protected const int DEFAULT_RECEIVE_TIMETOUT = 2000; // 2.0 seconds of timeout to receive packet and command responses.

		/**
		 * Timeout to wait before entering in command mode: {@value} ms.
		 * 
		 * <p>It is used to determine the operating mode of the module (this 
		 * library only supports API modes, not transparent mode).</p>
		 * 
		 * <p>This value depends on the {@code GT}, {@code AT} and/or {@code BT} 
		 * parameters.</p>
		 * 
		 * @see XBeeDevice#determineOperatingMode()
		 */
		protected const int TIMEOUT_BEFORE_COMMAND_MODE = 1200;

		/**
		 * Timeout to wait after entering in command mode: {@value} ms.
		 * 
		 * <p>It is used to determine the operating mode of the module (this 
		 * library only supports API modes, not transparent mode).</p>
		 * 
		 * <p>This value depends on the {@code GT}, {@code AT} and/or {@code BT} 
		 * parameters.</p>
		 * 
		 * @see XBeeDevice#determineOperatingMode()
		 */
		protected const int TIMEOUT_ENTER_COMMAND_MODE = 1500;

		// Variables.
		protected IConnectionInterface connectionInterface;

		protected DataReader dataReader = null;

		protected XBeeProtocol xbeeProtocol = XBeeProtocol.UNKNOWN;

		protected OperatingMode operatingMode = OperatingMode.UNKNOWN;

		protected XBee16BitAddress xbee16BitAddress = XBee16BitAddress.UNKNOWN_ADDRESS;
		protected XBee64BitAddress xbee64BitAddress = XBee64BitAddress.UNKNOWN_ADDRESS;

		protected byte currentFrameID = 0xFF;
		protected int receiveTimeout = DEFAULT_RECEIVE_TIMETOUT;

		protected AbstractXBeeDevice localXBeeDevice;

		protected ILog logger;

		private string nodeID;
		private string firmwareVersion;

		private HardwareVersion hardwareVersion;

		private object ioLock = new object();

		private bool ioPacketReceived = false;
		private bool applyConfigurationChanges = true;

		private byte[] ioPacketPayload;

		/**
		 * Class constructor. Instantiates a new {@code XBeeDevice} object in the 
		 * given port name and baud rate.
		 * 
		 * @param port Serial port name where XBee device is attached to.
		 * @param baudRate Serial port baud rate to communicate with the device. 
		 *                 Other connection parameters will be set as default (8 
		 *                 data bits, 1 stop bit, no parity, no flow control).
		 * 
		 * @throws ArgumentException if {@code baudRate < 0}.
		 * @throws ArgumentNullException if {@code port == null}.
		 * 
		 * @see #AbstractXBeeDevice(IConnectionInterface)
		 * @see #AbstractXBeeDevice(String, SerialPortParameters)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress, XBee16BitAddress, String)
		 * @see #AbstractXBeeDevice(String, int, int, int, int, int)
		 */
		public AbstractXBeeDevice(String port, int baudRate)
			: this(XBee.CreateConnectiontionInterface(port, baudRate))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code XBeeDevice} object in the 
		 * given serial port name and settings.
		 * 
		 * @param port Serial port name where XBee device is attached to.
		 * @param baudRate Serial port baud rate to communicate with the device.
		 * @param dataBits Serial port data bits.
		 * @param stopBits Serial port data bits.
		 * @param parity Serial port data bits.
		 * @param flowControl Serial port data bits.
		 * 
		 * @throws ArgumentException if {@code baudRate < 0} or
		 *                                  if {@code dataBits < 0} or
		 *                                  if {@code stopBits < 0} or
		 *                                  if {@code parity < 0} or
		 *                                  if {@code flowControl < 0}.
		 * @throws ArgumentNullException if {@code port == null}.
		 * 
		 * @see #AbstractXBeeDevice(IConnectionInterface)
		 * @see #AbstractXBeeDevice(String, int)
		 * @see #AbstractXBeeDevice(String, SerialPortParameters)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress, XBee16BitAddress, String)
		 */
		public AbstractXBeeDevice(String port, int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl)
			: this(port, new SerialPortParameters(baudRate, dataBits, stopBits, parity, flowControl))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code XBeeDevice} object in the 
		 * given serial port name and parameters.
		 * 
		 * @param port Serial port name where XBee device is attached to.
		 * @param serialPortParameters Object containing the serial port parameters.
		 * 
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code serialPortParameters == null}.
		 * 
		 * @see #AbstractXBeeDevice(IConnectionInterface)
		 * @see #AbstractXBeeDevice(String, int)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress, XBee16BitAddress, String)
		 * @see #AbstractXBeeDevice(String, int, int, int, int, int)
		 * @see com.digi.xbee.api.connection.serial.SerialPortParameters
		 */
		public AbstractXBeeDevice(String port, SerialPortParameters serialPortParameters)
			: this(XBee.CreateConnectiontionInterface(port, serialPortParameters))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code XBeeDevice} object with the 
		 * given connection interface.
		 * 
		 * @param connectionInterface The connection interface with the physical 
		 *                            XBee device.
		 * 
		 * @throws ArgumentNullException if {@code connectionInterface == null}.
		 * 
		 * @see #AbstractXBeeDevice(String, int)
		 * @see #AbstractXBeeDevice(String, SerialPortParameters)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress, XBee16BitAddress, String)
		 * @see #AbstractXBeeDevice(String, int, int, int, int, int)
		 * @see com.digi.xbee.api.connection.IConnectionInterface
		 */
		public AbstractXBeeDevice(IConnectionInterface connectionInterface)
		{
			Contract.Requires<ArgumentNullException>(connectionInterface != null, "ConnectionInterface cannot be null.");

			this.connectionInterface = connectionInterface;
			this.logger = LogManager.GetLogger(this.GetType());
			logger.DebugFormat(ToString() + "Using the connection interface {0}.",
					connectionInterface.GetType().Name);
			this.IOPacketReceiveListener = new CustomPacketReceiveListener(this);
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteXBeeDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote XBee device.
		 * @param addr64 The 64-bit address to identify this XBee device.
		 * 
		 * @throws ArgumentException If {@code localXBeeDevice.isRemote() == true}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see #AbstractXBeeDevice(IConnectionInterface)
		 * @see #AbstractXBeeDevice(String, int)
		 * @see #AbstractXBeeDevice(String, SerialPortParameters)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress, XBee16BitAddress, String)
		 * @see #AbstractXBeeDevice(String, int, int, int, int, int)
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public AbstractXBeeDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: this(localXBeeDevice, addr64, null, null)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteXBeeDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote XBee device.
		 * @param addr64 The 64-bit address to identify this XBee device.
		 * @param addr16 The 16-bit address to identify this XBee device. It might 
		 *               be {@code null}.
		 * @param id The node identifier of this XBee device. It might be 
		 *           {@code null}.
		 * 
		 * @throws ArgumentException If {@code localXBeeDevice.isRemote() == true}.
		 * @throws ArgumentNullException If {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see #AbstractXBeeDevice(IConnectionInterface)
		 * @see #AbstractXBeeDevice(String, int)
		 * @see #AbstractXBeeDevice(String, SerialPortParameters)
		 * @see #AbstractXBeeDevice(XBeeDevice, XBee64BitAddress)
		 * @see #AbstractXBeeDevice(String, int, int, int, int, int)
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public AbstractXBeeDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64,
				XBee16BitAddress addr16, String id)
		{
			if (localXBeeDevice == null)
				throw new ArgumentNullException("Local XBee device cannot be null.");
			if (addr64 == null)
				throw new ArgumentNullException("XBee 64-bit address of the device cannot be null.");
			if (localXBeeDevice.IsRemote)
				throw new ArgumentException("The given local XBee device is remote.");

			this.localXBeeDevice = localXBeeDevice;
			this.connectionInterface = localXBeeDevice.GetConnectionInterface();
			this.xbee64BitAddress = addr64;
			this.xbee16BitAddress = addr16;
			if (addr16 == null)
				xbee16BitAddress = XBee16BitAddress.UNKNOWN_ADDRESS;
			this.nodeID = id;
			this.logger = LogManager.GetLogger(this.GetType());
			logger.DebugFormat(ToString() + "Using the connection interface {0}.",
					connectionInterface.GetType().Name);
			this.IOPacketReceiveListener = new CustomPacketReceiveListener(this);
		}

		/**
		 * Returns the connection interface associated to this XBee device.
		 * 
		 * @return XBee device's connection interface.
		 * 
		 * @see com.digi.xbee.api.connection.IConnectionInterface
		 */
		public IConnectionInterface GetConnectionInterface()
		{
			return connectionInterface;
		}

		/// <summary>
		/// Indicates whether this XBee device is a remote device.
		/// </summary>
		abstract public bool IsRemote { get; }

		/**
		 * Reads some parameters from this device and obtains its protocol.
		 * 
		 * <p>This method refresh the values of:</p>
		 * <ul>
		 * <li>64-bit address only if it is not initialized.</li>
		 * <li>Node Identifier.</li>
		 * <li>Hardware version if it is not initialized.</li>
		 * <li>Firmware version.</li>
		 * <li>XBee device protocol.</li>
		 * <li>16-bit address (not for DigiMesh modules).</li>
		 * </ul>
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout reading the parameters.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #get16BitAddress()
		 * @see #get64BitAddress()
		 * @see #getHardwareVersion()
		 * @see #getNodeID()
		 * @see #getFirmwareVersion()
		 * @see #getXBeeProtocol()
		 * @see #setNodeID(String)
		 */
		public void readDeviceInfo() /*throws TimeoutException, XBeeException*/ {
			byte[] response = null;
			// Get the 64-bit address.
			if (xbee64BitAddress == null || xbee64BitAddress == XBee64BitAddress.UNKNOWN_ADDRESS)
			{
				String addressHigh;
				String addressLow;

				response = GetParameter("SH");
				addressHigh = HexUtils.ByteArrayToHexString(response);

				response = GetParameter("SL");
				addressLow = HexUtils.ByteArrayToHexString(response);

				while (addressLow.Length < 8)
					addressLow = "0" + addressLow;

				xbee64BitAddress = new XBee64BitAddress(addressHigh + addressLow);
			}
			// Get the Node ID.
			response = GetParameter("NI");
			nodeID = Encoding.UTF8.GetString(response);

			// Get the hardware version.
			if (hardwareVersion == null)
			{
				response = GetParameter("HV");
				hardwareVersion = HardwareVersion.Get(response[0]);
			}
			// Get the firmware version.
			response = GetParameter("VR");
			firmwareVersion = HexUtils.ByteArrayToHexString(response);

			// Obtain the device protocol.
			xbeeProtocol = XBeeProtocol.UNKNOWN.DetermineProtocol(hardwareVersion, firmwareVersion);

			// Get the 16-bit address. This must be done after obtaining the protocol because 
			// DigiMesh and Point-to-Multipoint protocols don't have 16-bit addresses.
			if (GetXBeeProtocol() != XBeeProtocol.DIGI_MESH
					&& GetXBeeProtocol() != XBeeProtocol.DIGI_POINT)
			{
				response = GetParameter("MY");
				xbee16BitAddress = new XBee16BitAddress(response);
			}
		}

		/**
		 * Returns the 16-bit address of this XBee device.
		 * 
		 * <p>To refresh this value use the {@link #readDeviceInfo()} method.</p>
		 * 
		 * @return The 16-bit address of this XBee device.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public XBee16BitAddress Get16BitAddress()
		{
			return xbee16BitAddress;
		}

		/**
		 * Returns the 64-bit address of this XBee device.
		 * 
		 * <p>If this value is {@code null} or 
		 * {@code XBee64BitAddress.UNKNOWN_ADDRESS}, use the 
		 * {@link #readDeviceInfo()} method to get its value.</p>
		 * 
		 * @return The 64-bit address of this XBee device.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public XBee64BitAddress Get64BitAddress()
		{
			return xbee64BitAddress;
		}

		/**
		 * Returns the Operating mode (AT, API or API escaped) of this XBee device 
		 * for a local device, and the operating mode of the local device used as 
		 * communication interface for a remote device.
		 * 
		 * @return The operating mode of the local XBee device.
		 * 
		 * @see #isRemote()
		 * @see com.digi.xbee.api.models.OperatingMode
		 */
		protected OperatingMode GetOperatingMode()
		{
			if (IsRemote)
				return localXBeeDevice.GetOperatingMode();
			return operatingMode;
		}

		/**
		 * Returns the XBee Protocol of this XBee device.
		 * 
		 * <p>To refresh this value use the {@link #readDeviceInfo()} method.</p>
		 * 
		 * @return The XBee device protocol.
		 * 
		 * @see com.digi.xbee.api.models.XBeeProtocol
		 */
		public virtual XBeeProtocol GetXBeeProtocol()
		{
			return xbeeProtocol;
		}

		/// <summary>
		/// Gets or sets the node identifier of thix XBee device.
		/// <remarks>To refresh this value use the <see cref="ReadDeviceInfo"/> method.</remarks>
		/// </summary>
		public string NodeID
		{
			get
			{
				return nodeID;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("Node ID cannot be null.");
				if (value.Length > 20)
					throw new ArgumentException("Node ID length must be less than 21.");

				SetParameter("NI", Encoding.UTF8.GetBytes(nodeID));

				this.nodeID = value;
			}
		}

		/**
		 * Returns the firmware version (hexadecimal string value) of this XBee 
		 * device.
		 * 
		 * <p>To refresh this value use the {@link #readDeviceInfo()} method.</p>
		 * 
		 * @return The firmware version of the XBee device.
		 */
		public String GetFirmwareVersion()
		{
			return firmwareVersion;
		}

		/**
		 * Returns the hardware version of this XBee device.
		 * 
		 * <p>If this value is {@code null}, use the {@link #readDeviceInfo()} 
		 * method to get its value.</p>
		 * 
		 * @return The hardware version of the XBee device.
		 * 
		 * @see com.digi.xbee.api.models.HardwareVersion
		 * @see com.digi.xbee.api.models.HardwareVersionEnum
		 */
		public HardwareVersion GetHardwareVersion()
		{
			return hardwareVersion;
		}

		/**
		 * Updates the current device reference with the data provided for the 
		 * given device.
		 * 
		 * <p><b>This is only for internal use.</b></p>
		 * 
		 * @param device The XBee Device to get the data from.
		 */
		public void UpdateDeviceDataFrom(AbstractXBeeDevice device)
		{
			// TODO Should the devices have the same protocol??
			// TODO Should be allow to update a local from a remote or viceversa?? Maybe 
			// this must be in the Local/Remote device class(es) and not here... 

			// Only update the Node Identifier if the provided is not null.
			if (device.NodeID != null)
				this.nodeID = device.NodeID;

			// Only update the 64-bit address if the original is null or unknown.
			XBee64BitAddress addr64 = device.Get64BitAddress();
			if (addr64 != null && addr64 != XBee64BitAddress.UNKNOWN_ADDRESS
					&& !addr64.Equals(xbee64BitAddress)
					&& (xbee64BitAddress == null
						|| xbee64BitAddress.Equals(XBee64BitAddress.UNKNOWN_ADDRESS)))
			{
				xbee64BitAddress = addr64;
			}

			// TODO Change here the 16-bit address or maybe in ZigBee and 802.15.4?
			// TODO Should the 16-bit address be always updated? Or following the same rule as the 64-bit address.
			XBee16BitAddress addr16 = device.Get16BitAddress();
			if (addr16 != null && !addr16.Equals(xbee16BitAddress))
			{
				xbee16BitAddress = addr16;
			}

			//this.deviceType = device.deviceType; // This is not yet done.

			// The operating mode: only API/API2. Do we need this for a remote device?
			// The protocol of the device should be the same.
			// The hardware version should be the same.
			// The firmware version can change...
		}

		/**
		 * Adds the provided listener to the list of listeners to be notified
		 * when new packets are received. 
		 * 
		 * <p>If the listener has been already included, this method does nothing.
		 * </p>
		 * 
		 * @param listener Listener to be notified when new packets are received.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}
		 * 
		 * @see #removePacketListener(IPacketReceiveListener)
		 * @see com.digi.xbee.api.listeners.IPacketReceiveListener
		 */
		protected void AddPacketListener(IPacketReceiveListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("Listener cannot be null.");

			if (dataReader == null)
				return;
			dataReader.AddPacketReceiveListener(listener);
		}

		/**
		 * Removes the provided listener from the list of packets listeners. 
		 * 
		 * <p>If the listener was not in the list this method does nothing.</p>
		 * 
		 * @param listener Listener to be removed from the list of listeners.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}
		 * 
		 * @see #addPacketListener(IPacketReceiveListener)
		 * @see com.digi.xbee.api.listeners.IPacketReceiveListener
		 */
		protected void RemovePacketListener(IPacketReceiveListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("Listener cannot be null.");

			if (dataReader == null)
				return;
			dataReader.RemovePacketReceiveListener(listener);
		}

		/**
		 * Adds the provided listener to the list of listeners to be notified
		 * when new data is received. 
		 * 
		 * <p>If the listener has been already included this method does nothing.
		 * </p>
		 * 
		 * @param listener Listener to be notified when new data is received.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}
		 * 
		 * @see #removeDataListener(IDataReceiveListener)
		 * @see com.digi.xbee.api.listeners.IDataReceiveListener
		 */
		protected void AddDataListener(IDataReceiveListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("Listener cannot be null.");

			if (dataReader == null)
				return;
			dataReader.AddDataReceiveListener(listener);
		}

		/**
		 * Removes the provided listener from the list of data listeners. 
		 * 
		 * <p>If the listener was not in the list this method does nothing.</p>
		 * 
		 * @param listener Listener to be removed from the list of listeners.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}
		 * 
		 * @see #addDataListener(IDataReceiveListener)
		 * @see com.digi.xbee.api.listeners.IDataReceiveListener
		 */
		protected void RemoveDataListener(IDataReceiveListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("Listener cannot be null.");

			if (dataReader == null)
				return;
			dataReader.RemoveDataReceiveListener(listener);
		}

		/**
		 * Adds the provided listener to the list of listeners to be notified
		 * when new IO samples are received. 
		 * 
		 * <p>If the listener has been already included this method does nothing.
		 * </p>
		 * 
		 * @param listener Listener to be notified when new IO samples are received.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}
		 * 
		 * @see #removeIOSampleListener(IIOSampleReceiveListener)
		 * @see com.digi.xbee.api.listeners.IIOSampleReceiveListener
		 */
		protected void AddIOSampleListener(EventHandler<IOSampleReceivedEventArgs> action)
		{
			if (action == null)
				throw new ArgumentNullException("Listener cannot be null.");

			if (dataReader == null)
				return;

			dataReader.IOSampleReceived += action;
		}

		/**
		 * Removes the provided listener from the list of IO samples listeners. 
		 * 
		 * <p>If the listener was not in the list this method does nothing.</p>
		 * 
		 * @param listener Listener to be removed from the list of listeners.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}
		 * 
		 * @see #addIOSampleListener(IIOSampleReceiveListener)
		 * @see com.digi.xbee.api.listeners.IIOSampleReceiveListener
		 */
		protected void RemoveIOSampleListener(EventHandler<IOSampleReceivedEventArgs> action)
		{
			if (action == null)
				throw new ArgumentNullException("Listener cannot be null.");

			if (dataReader == null)
				return;
			dataReader.IOSampleReceived -= action;
		}

		/**
		 * Adds the provided listener to the list of listeners to be notified
		 * when new Modem Status events are received.
		 * 
		 * <p>If the listener has been already included this method does nothing.
		 * </p>
		 * 
		 * @param listener Listener to be notified when new Modem Status events are 
		 *                 received.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}
		 * 
		 * @see #removeModemStatusListener(IModemStatusReceiveListener)
		 * @see com.digi.xbee.api.listeners.IModemStatusReceiveListener
		 */
		protected void AddModemStatusListener(IModemStatusReceiveListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("Listener cannot be null.");

			if (dataReader == null)
				return;
			dataReader.AddModemStatusReceiveListener(listener);
		}

		/**
		 * Removes the provided listener from the list of Modem Status listeners.
		 * 
		 * <p>If the listener was not in the list this method does nothing.</p>
		 * 
		 * @param listener Listener to be removed from the list of listeners.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}
		 * 
		 * @see #addModemStatusListener(IModemStatusReceiveListener)
		 * @see com.digi.xbee.api.listeners.IModemStatusReceiveListener
		 */
		protected void removeModemStatusListener(IModemStatusReceiveListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("Listener cannot be null.");
			if (dataReader == null)
				return;
			dataReader.RemoveModemStatusReceiveListener(listener);
		}

		/**
		 * Sends the given AT command and waits for answer or until the configured 
		 * receive timeout expires.
		 * 
		 * <p>The receive timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * @param command AT command to be sent.
		 * @return An {@code ATCommandResponse} object containing the response of 
		 *         the command or {@code null} if there is no response.
		 *         
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws InvalidOperatingModeException if the operating mode is different 
		 *                                       than {@link OperatingMode#API} and 
		 *                                       {@link OperatingMode#API_ESCAPE}.
		 * @throws IOException if an I/O error occurs while sending the AT command.
		 * @throws ArgumentNullException if {@code command == null}.
		 * @throws TimeoutException if the configured time expires while waiting 
		 *                          for the command reply.
		 * 
		 * @see XBeeDevice#getReceiveTimeout()
		 * @see XBeeDevice#setReceiveTimeout(int)
		 * @see com.digi.xbee.api.models.ATCommand
		 * @see com.digi.xbee.api.models.ATCommandResponse
		 */
		protected ATCommandResponse SendATCommand(ATCommand command)
		/*throws InvalidOperatingModeException, TimeoutException, IOException*/ {
			// Check if command is null.
			if (command == null)
				throw new ArgumentNullException("AT command cannot be null.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			ATCommandResponse response = null;
			OperatingMode operatingMode = GetOperatingMode();
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
						XBee16BitAddress remote16BitAddress = Get16BitAddress();
						if (remote16BitAddress == null)
							remote16BitAddress = XBee16BitAddress.UNKNOWN_ADDRESS;
						RemoteATCommandOptions remoteATCommandOptions = RemoteATCommandOptions.OPTION_NONE;
						if (IsApplyConfigurationChangesEnabled())
							remoteATCommandOptions |= RemoteATCommandOptions.OPTION_APPLY_CHANGES;
						packet = new RemoteATCommandPacket(GetNextFrameID(), Get64BitAddress(), remote16BitAddress, (byte)remoteATCommandOptions, command.Command, command.Parameter);
					}
					else
					{
						if (IsApplyConfigurationChangesEnabled())
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
						if (answerPacket is ATCommandResponsePacket)
							response = new ATCommandResponse(command, ((ATCommandResponsePacket)answerPacket).CommandValue, ((ATCommandResponsePacket)answerPacket).Status);
						else if (answerPacket is RemoteATCommandResponsePacket)
							response = new ATCommandResponse(command, ((RemoteATCommandResponsePacket)answerPacket).getCommandValue(), ((RemoteATCommandResponsePacket)answerPacket).Status);

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

		/**
		 * Sends the given XBee packet asynchronously.
		 * 
		 * <p>The method will not wait for an answer for the packet.</p>
		 * 
		 * <p>To be notified when the answer is received, use 
		 * {@link #sendXBeePacket(XBeePacket, IPacketReceiveListener)}.</p>
		 * 
		 * @param packet XBee packet to be sent asynchronously.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws InvalidOperatingModeException if the operating mode is different 
		 *                                       than {@link OperatingMode#API} and 
		 *                                       {@link OperatingMode#API_ESCAPE}.
		 * @throws IOException if an I/O error occurs while sending the XBee packet.
		 * @throws ArgumentNullException if {@code packet == null}.
		 * 
		 * @see #sendXBeePacket(XBeePacket)
		 * @see #sendXBeePacket(XBeePacket, IPacketReceiveListener)
		 * @see #sendXBeePacketAsync(XBeePacket)
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		protected void SendXBeePacketAsync(XBeePacket packet)
		/*throws InvalidOperatingModeException, IOException*/ {
			SendXBeePacket(packet, null);
		}

		/**
		 * Sends the given XBee packet asynchronously and registers the given 
		 * packet listener (if not {@code null}) to wait for an answer.
		 * 
		 * <p>The method will not wait for an answer for the packet, but the given 
		 * listener will be notified when the answer arrives.</p>
		 * 
		 * @param packet XBee packet to be sent.
		 * @param packetReceiveListener Listener for the operation, {@code null} 
		 *                              not to be notified when the answer arrives.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws InvalidOperatingModeException if the operating mode is different 
		 *                                       than {@link OperatingMode#API} and 
		 *                                       {@link OperatingMode#API_ESCAPE}.
		 * @throws IOException if an I/O error occurs while sending the XBee packet.
		 * @throws ArgumentNullException if {@code packet == null}.
		 * 
		 * @see #sendXBeePacket(XBeePacket)
		 * @see #sendXBeePacket(XBeePacket, IPacketReceiveListener)
		 * @see #sendXBeePacketAsync(XBeePacket)
		 * @see com.digi.xbee.api.listeners.IPacketReceiveListener
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		protected void SendXBeePacket(XBeePacket packet, IPacketReceiveListener packetReceiveListener)
		/*throws InvalidOperatingModeException, IOException*/ {
			// Check if the packet to send is null.
			if (packet == null)
				throw new ArgumentNullException("XBee packet cannot be null.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			OperatingMode operatingMode = GetOperatingMode();
			switch (operatingMode)
			{
				case OperatingMode.AT:
				case OperatingMode.UNKNOWN:
				default:
					throw new InvalidOperatingModeException(operatingMode);
				case OperatingMode.API:
				case OperatingMode.API_ESCAPE:
					// Add the required frame ID and subscribe listener if given.
					if (packet is XBeeAPIPacket)
					{
						if (((XBeeAPIPacket)packet).NeedsAPIFrameID)
						{
							if (((XBeeAPIPacket)packet).FrameID == XBeeAPIPacket.NO_FRAME_ID)
								((XBeeAPIPacket)packet).FrameID = GetNextFrameID();
							if (packetReceiveListener != null)
								dataReader.AddPacketReceiveListener(packetReceiveListener, ((XBeeAPIPacket)packet).FrameID);
						}
						else if (packetReceiveListener != null)
							dataReader.AddPacketReceiveListener(packetReceiveListener);
					}

					// Write packet data.
					WritePacket(packet);
					break;
			}
		}

		/**
		 * Sends the given XBee packet synchronously and blocks until response is 
		 * received or receive timeout is reached.
		 * 
		 * <p>The receive timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * <p>Use {@link #sendXBeePacketAsync(XBeePacket)} for non-blocking 
		 * operations.</p>
		 * 
		 * @param packet XBee packet to be sent.
		 * @return An {@code XBeePacket} containing the response of the sent packet 
		 *         or {@code null} if there is no response.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws InvalidOperatingModeException if the operating mode is different 
		 *                                       than {@link OperatingMode#API} and 
		 *                                       {@link OperatingMode#API_ESCAPE}.
		 * @throws IOException if an I/O error occurs while sending the XBee packet.
		 * @throws ArgumentNullException if {@code packet == null}.
		 * @throws TimeoutException if the configured time expires while waiting for
		 *                          the packet reply.
		 * 
		 * @see #sendXBeePacket(XBeePacket)
		 * @see #sendXBeePacket(XBeePacket, IPacketReceiveListener)
		 * @see #sendXBeePacketAsync(XBeePacket)
		 * @see XBeeDevice#setReceiveTimeout(int)
		 * @see XBeeDevice#getReceiveTimeout()
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		protected XBeePacket SendXBeePacket(XBeePacket packet)
		/*throws InvalidOperatingModeException, TimeoutException, IOException*/ {
			// Check if the packet to send is null.
			if (packet == null)
				throw new ArgumentNullException("XBee packet cannot be null.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			OperatingMode operatingMode = GetOperatingMode();
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

					// Add the required frame ID to the packet if necessary.
					InsertFrameID(packet);

					// Generate a packet received listener for the packet to be sent.
					IPacketReceiveListener packetReceiveListener = CreatePacketReceivedListener(packet, responseList);

					// Add the packet listener to the data reader.
					AddPacketListener(packetReceiveListener);

					// Write the packet data.
					WritePacket(packet);
					try
					{
						// Wait for response or timeout.
						lock (responseList)
						{
							try
							{
								Monitor.Wait(responseList, receiveTimeout);
							}
							catch (ThreadInterruptedException) { }
						}
						// After the wait check if we received any response, if not throw timeout exception.
						if (responseList.Count < 1)
							throw new Kveer.XBeeApi.Exceptions.TimeoutException();
						// Return the received packet.
						return responseList[0];
					}
					finally
					{
						// Always remove the packet listener from the list.
						RemovePacketListener(packetReceiveListener);
					}
			}
		}

		/**
		 * Insert (if possible) the next frame ID stored in the device to the 
		 * provided packet.
		 * 
		 * @param xbeePacket The packet to add the frame ID.
		 * 
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		private void InsertFrameID(XBeePacket xbeePacket)
		{
			if (xbeePacket is XBeeAPIPacket)
				return;

			if (((XBeeAPIPacket)xbeePacket).NeedsAPIFrameID && ((XBeeAPIPacket)xbeePacket).FrameID == XBeeAPIPacket.NO_FRAME_ID)
				((XBeeAPIPacket)xbeePacket).FrameID = GetNextFrameID();
		}

		class MyPacketReceiveListener : IPacketReceiveListener
		{
			XBeePacket _sentPacket;
			IList<XBeePacket> _responseList;
			AbstractXBeeDevice _device;

			public MyPacketReceiveListener(AbstractXBeeDevice device, XBeePacket sentPacket, IList<XBeePacket> responseList)
			{
				_device = device;
				_sentPacket = sentPacket;
				_responseList = responseList;
			}
			public void PacketReceived(XBeePacket receivedPacket)
			{
				// Check if it is the packet we are waiting for.
				if (((XBeeAPIPacket)receivedPacket).CheckFrameID((((XBeeAPIPacket)_sentPacket).FrameID)))
				{
					// Security check to avoid class cast exceptions. It has been observed that parallel processes 
					// using the same connection but with different frame index may collide and cause this exception at some point.
					if (_sentPacket is XBeeAPIPacket
							&& receivedPacket is XBeeAPIPacket)
					{
						XBeeAPIPacket sentAPIPacket = (XBeeAPIPacket)_sentPacket;
						XBeeAPIPacket receivedAPIPacket = (XBeeAPIPacket)receivedPacket;

						// If the packet sent is an AT command, verify that the received one is an AT command response and 
						// the command matches in both packets.
						if (sentAPIPacket.FrameType == APIFrameType.AT_COMMAND)
						{
							if (receivedAPIPacket.FrameType != APIFrameType.AT_COMMAND_RESPONSE)
								return;
							if (!((ATCommandPacket)sentAPIPacket).Command.Equals(((ATCommandResponsePacket)receivedPacket).Command, StringComparison.InvariantCultureIgnoreCase))
								return;
						}
						// If the packet sent is a remote AT command, verify that the received one is a remote AT command response and 
						// the command matches in both packets.
						if (sentAPIPacket.FrameType == APIFrameType.REMOTE_AT_COMMAND_REQUEST)
						{
							if (receivedAPIPacket.FrameType != APIFrameType.REMOTE_AT_COMMAND_RESPONSE)
								return;
							if (!((RemoteATCommandPacket)sentAPIPacket).Command.Equals(((RemoteATCommandResponsePacket)receivedPacket).Command, StringComparison.InvariantCultureIgnoreCase))
								return;
						}
					}

					// Verify that the sent packet is not the received one! This can happen when the echo mode is enabled in the 
					// serial port.
					if (!_device.isSamePacket(_sentPacket, receivedPacket))
					{
						_responseList.Add(receivedPacket);
						lock (_responseList)
						{
							Monitor.Pulse(_responseList);
						}
					}
				}
			}
		}
		/**
		 * Returns the packet listener corresponding to the provided sent packet. 
		 * 
		 * <p>The listener will filter those packets  matching with the Frame ID of 
		 * the sent packet storing them in the provided responseList array.</p>
		 * 
		 * @param sentPacket The packet sent.
		 * @param responseList List of packets received that correspond to the 
		 *                     frame ID of the packet sent.
		 * 
		 * @return A packet receive listener that will filter the packets received 
		 *         corresponding to the sent one.
		 * 
		 * @see com.digi.xbee.api.listeners.IPacketReceiveListener
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		private IPacketReceiveListener CreatePacketReceivedListener(XBeePacket sentPacket, IList<XBeePacket> responseList)
		{
			IPacketReceiveListener packetReceiveListener = new MyPacketReceiveListener(this, sentPacket, responseList);

			return packetReceiveListener;
		}

		/**
		 * Returns whether the sent packet is the same than the received one.
		 * 
		 * @param sentPacket The packet sent.
		 * @param receivedPacket The packet received.
		 * 
		 * @return {@code true} if the sent packet is the same than the received 
		 *         one, {@code false} otherwise.
		 * 
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		private bool isSamePacket(XBeePacket sentPacket, XBeePacket receivedPacket)
		{
			// TODO Should not we implement the {@code equals} method in the XBeePacket??
			if (HexUtils.ByteArrayToHexString(sentPacket.GenerateByteArray()).Equals(HexUtils.ByteArrayToHexString(receivedPacket.GenerateByteArray())))
				return true;
			return false;
		}

		/**
		 * Writes the given XBee packet in the connection interface of this device.
		 * 
		 * @param packet XBee packet to be written.
		 * 
		 * @throws IOException if an I/O error occurs while writing the XBee packet 
		 *                     in the connection interface.
		 * 
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		private void WritePacket(XBeePacket packet)/*throws IOException */{
			logger.DebugFormat(ToString() + "Sending XBee packet: \n{0}", packet.ToPrettyString());
			// Write bytes with the required escaping mode.
			switch (operatingMode)
			{
				case OperatingMode.API:
				default:
					var buf = packet.GenerateByteArray();
					connectionInterface.SerialPort.Write(buf, 0, buf.Length);
					break;
				case OperatingMode.API_ESCAPE:
					var buf2 = packet.GenerateByteArrayEscaped();
					connectionInterface.SerialPort.Write(buf2, 0, buf2.Length);
					break;
			}
		}

		/// <summary>
		/// Gets the next Frame ID of this XBee device.
		/// </summary>
		/// <returns></returns>
		protected byte GetNextFrameID()
		{
			if (IsRemote)
				return localXBeeDevice.GetNextFrameID();
			if (currentFrameID == 0xff)
			{
				// Reset counter.
				currentFrameID = 1;
			}
			else
				currentFrameID++;
			return currentFrameID;
		}

		/**
		 * Sends the provided {@code XBeePacket} and determines if the transmission 
		 * status is success for synchronous transmissions.
		 * 
		 * <p>If the status is not success, an {@code TransmitException} is thrown.</p>
		 * 
		 * @param packet The {@code XBeePacket} to be sent.
		 * @param asyncTransmission Determines whether the transmission must be 
		 *                          asynchronous.
		 * 
		 * @throws TransmitException if {@code packet} is not an instance of 
		 *                           {@code TransmitStatusPacket} or 
		 *                           if {@code packet} is not an instance of 
		 *                           {@code TXStatusPacket} or 
		 *                           if its transmit status is different than 
		 *                           {@code XBeeTransmitStatus.SUCCESS}.
		 * @throws XBeeException if there is any other XBee related error.
		 * 
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		protected void SendAndCheckXBeePacket(XBeePacket packet, bool asyncTransmission)/*throws TransmitException, XBeeException */{
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
			if (receivedPacket is TransmitStatusPacket)
			{
				if (((TransmitStatusPacket)receivedPacket).getTransmitStatus() == null)
					throw new TransmitException(XBeeTransmitStatus.UNKNOWN);
				else if (((TransmitStatusPacket)receivedPacket).getTransmitStatus() != XBeeTransmitStatus.SUCCESS)
					throw new TransmitException(((TransmitStatusPacket)receivedPacket).getTransmitStatus());
			}
			else if (receivedPacket is TXStatusPacket)
			{
				if (((TXStatusPacket)receivedPacket).TransmitStatus == null)
					throw new TransmitException(XBeeTransmitStatus.UNKNOWN);
				else if (((TXStatusPacket)receivedPacket).TransmitStatus != XBeeTransmitStatus.SUCCESS)
					throw new TransmitException(((TXStatusPacket)receivedPacket).TransmitStatus);
			}
			else
				throw new TransmitException(XBeeTransmitStatus.UNKNOWN);
		}

		/**
		 * Sets the configuration of the given IO line of this XBee device.
		 * 
		 * @param ioLine The IO line to configure.
		 * @param ioMode The IO mode to set to the IO line.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code ioLine == null} or
		 *                              if {@code ioMode == null}.
		 * @throws TimeoutException if there is a timeout sending the set 
		 *                          configuration command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getIOConfiguration(IOLine)
		 * @see com.digi.xbee.api.io.IOLine
		 * @see com.digi.xbee.api.io.IOMode
		 */
		public void setIOConfiguration(IOLine ioLine, IOMode ioMode)/*throws TimeoutException, XBeeException */{
			// Check IO line.
			if (ioLine == null)
				throw new ArgumentNullException("IO line cannot be null.");
			if (ioMode == null)
				throw new ArgumentNullException("IO mode cannot be null.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			SetParameter(ioLine.GetConfigurationATCommand(), new byte[] { (byte)ioMode.GetId() });
		}

		/**
		 * Returns the configuration mode of the provided IO line of this XBee 
		 * device.
		 * 
		 * @param ioLine The IO line to get its configuration.
		 * 
		 * @return The IO mode (configuration) of the provided IO line.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code ioLine == null}.
		 * @throws TimeoutException if there is a timeout sending the get 
		 *                          configuration command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #setIOConfiguration(IOLine, IOMode)
		 * @see com.digi.xbee.api.io.IOLine
		 * @see com.digi.xbee.api.io.IOMode
		 */
		public IOMode GetIOConfiguration(IOLine ioLine)/*throws TimeoutException, XBeeException */{
			// Check IO line.
			if (ioLine == null)
				throw new ArgumentNullException("DIO pin cannot be null.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			// Check if the received configuration mode is valid.
			int ioModeValue = GetParameter(ioLine.GetConfigurationATCommand())[0];
			IOMode dioMode = IOMode.UNKOWN.GetIOMode(ioModeValue, ioLine);
			if (dioMode == null)
				throw new OperationNotSupportedException("Received configuration mode '" + HexUtils.IntegerToHexString(ioModeValue, 1) + "' is not valid.");

			// Return the configuration mode.
			return dioMode;
		}

		/**
		 * Sets the digital value (high or low) to the provided IO line of this 
		 * XBee device.
		 * 
		 * @param ioLine The IO line to set its value.
		 * @param ioValue The IOValue to set to the IO line ({@code HIGH} or 
		 *              {@code LOW}).
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code ioLine == null} or 
		 *                              if {@code ioValue == null}.
		 * @throws TimeoutException if there is a timeout sending the set DIO 
		 *                          command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getIOConfiguration(IOLine)
		 * @see #setIOConfiguration(IOLine, IOMode)
		 * @see com.digi.xbee.api.io.IOLine
		 * @see com.digi.xbee.api.io.IOValue
		 * @see com.digi.xbee.api.io.IOMode#DIGITAL_OUT_HIGH
		 * @see com.digi.xbee.api.io.IOMode#DIGITAL_OUT_LOW
		 */
		public void setDIOValue(IOLine ioLine, IOValue ioValue)/*throws TimeoutException, XBeeException */{
			// Check IO line.
			if (ioLine == null)
				throw new ArgumentNullException("IO line cannot be null.");
			// Check IO value.
			if (ioValue == null)
				throw new ArgumentNullException("IO value cannot be null.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			SetParameter(ioLine.GetConfigurationATCommand(), new byte[] { (byte)ioValue.GetID() });
		}

		/**
		 * Returns the digital value of the provided IO line of this XBee device.
		 * 
		 * <p>The provided <b>IO line must be previously configured as digital I/O
		 * </b>. To do so, use {@code setIOConfiguration} and the following 
		 * {@code IOMode}:</p>
		 * 
		 * <ul>
		 * <li>{@code IOMode.DIGITAL_IN} to configure as digital input.</li>
		 * <li>{@code IOMode.DIGITAL_OUT_HIGH} to configure as digital output, high.
		 * </li>
		 * <li>{@code IOMode.DIGITAL_OUT_LOW} to configure as digital output, low.
		 * </li>
		 * </ul>
		 * 
		 * @param ioLine The IO line to get its digital value.
		 * 
		 * @return The digital value corresponding to the provided IO line.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code ioLine == null}.
		 * @throws TimeoutException if there is a timeout sending the get IO values 
		 *                          command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getIOConfiguration(IOLine)
		 * @see #setIOConfiguration(IOLine, IOMode)
		 * @see com.digi.xbee.api.io.IOLine
		 * @see com.digi.xbee.api.io.IOValue
		 * @see com.digi.xbee.api.io.IOMode#DIGITAL_IN
		 * @see com.digi.xbee.api.io.IOMode#DIGITAL_OUT_HIGH
		 * @see com.digi.xbee.api.io.IOMode#DIGITAL_OUT_LOW
		 */
		public IOValue GetDIOValue(IOLine ioLine)/*throws TimeoutException, XBeeException */{
			// Check IO line.
			if (ioLine == null)
				throw new ArgumentNullException("IO line cannot be null.");

			// Obtain an IO Sample from the XBee device.
			IOSample ioSample = readIOSample();

			// Check if the IO sample contains the expected IO line and value.
			if (!ioSample.HasDigitalValues || !ioSample.DigitalValues.ContainsKey(ioLine))
				throw new OperationNotSupportedException("Answer does not contain digital data for " + ioLine.GetName() + ".");

			// Return the digital value. 
			return ioSample.DigitalValues[ioLine];
		}

		/**
		 * Sets the duty cycle (in %) of the provided IO line of this XBee device. 
		 * 
		 * <p>The provided <b>IO line must be</b>:</p>
		 * 
		 * <ul>
		 * <li><b>PWM capable</b> ({@link IOLine#hasPWMCapability()}).</li>
		 * <li>Previously <b>configured as PWM Output</b> (use 
		 * {@code setIOConfiguration} and {@code IOMode.PWM}).</li>
		 * </ul>
		 * 
		 * @param ioLine The IO line to set its duty cycle value.
		 * @param dutyCycle The duty cycle of the PWM.
		 * 
		 * @throws ArgumentException if {@code ioLine.hasPWMCapability() == false} or 
		 *                                  if {@code value < 0} or
		 *                                  if {@code value > 1023}.
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code ioLine == null}.
		 * @throws TimeoutException if there is a timeout sending the set PWM duty 
		 *                          cycle command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getIOConfiguration(IOLine)
		 * @see #getIOConfiguration(IOLine)
		 * @see #setIOConfiguration(IOLine, IOMode)
		 * @see #getPWMDutyCycle(IOLine)
		 * @see com.digi.xbee.api.io.IOLine
		 * @see com.digi.xbee.api.io.IOMode#PWM
		 */
		public void setPWMDutyCycle(IOLine ioLine, double dutyCycle)/*throws TimeoutException, XBeeException */{
			// Check IO line.
			if (ioLine == null)
				throw new ArgumentNullException("IO line cannot be null.");
			// Check if the IO line has PWM capability.
			if (!ioLine.HasPWMCapability())
				throw new ArgumentException("Provided IO line does not have PWM capability.");
			// Check duty cycle limits.
			if (dutyCycle < 0 || dutyCycle > 100)
				throw new ArgumentException("Duty Cycle must be between 0% and 100%.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			// Convert the value.
			int finaldutyCycle = (int)(dutyCycle * 1023.0 / 100.0);

			SetParameter(ioLine.GetPWMDutyCycleATCommand(), ByteUtils.IntToByteArray(finaldutyCycle));
		}

		/**
		 * Gets the PWM duty cycle (in %) corresponding to the provided IO line of 
		 * this XBee device.
		 * 
		 * <p>The provided <b>IO line must be</b>:</p>
		 * 
		 * <ul>
		 * <li><b>PWM capable</b> ({@link IOLine#hasPWMCapability()}).</li>
		 * <li>Previously <b>configured as PWM Output</b> (use 
		 * {@code setIOConfiguration} and {@code IOMode.PWM}).</li>
		 * </ul>
		 * 
		 * @param ioLine The IO line to get its PWM duty cycle.
		 * 
		 * @return The PWM duty cycle value corresponding to the provided IO line 
		 *         (0% - 100%).
		 * 
		 * @throws ArgumentException if {@code ioLine.hasPWMCapability() == false}.
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code ioLine == null}.
		 * @throws TimeoutException if there is a timeout sending the get PWM duty 
		 *                          cycle command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getIOConfiguration(IOLine)
		 * @see #setIOConfiguration(IOLine, IOMode)
		 * @see #setPWMDutyCycle(IOLine, double)
		 * @see com.digi.xbee.api.io.IOLine
		 * @see com.digi.xbee.api.io.IOMode#PWM
		 */
		public double GetPWMDutyCycle(IOLine ioLine)/*throws TimeoutException, XBeeException */{
			// Check IO line.
			if (ioLine == null)
				throw new ArgumentNullException("IO line cannot be null.");
			// Check if the IO line has PWM capability.
			if (!ioLine.HasPWMCapability())
				throw new ArgumentException("Provided IO line does not have PWM capability.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			byte[] value = GetParameter(ioLine.GetPWMDutyCycleATCommand());

			// Return the PWM duty cycle value.
			int readValue = ByteUtils.ByteArrayToInt(value);
			return Math.Round((readValue * 100.0 / 1023.0) * 100.0) / 100.0;
		}

		/**
		 * Returns the analog value of the provided IO line of this XBee device.
		 * 
		 * <p>The provided <b>IO line must be previously configured as ADC</b>. To 
		 * do so, use {@code setIOConfiguration} and {@code IOMode.ADC}.</p>
		 * 
		 * @param ioLine The IO line to get its analog value.
		 * 
		 * @return The analog value corresponding to the provided IO line.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code ioLine == null}.
		 * @throws TimeoutException if there is a timeout sending the get IO values
		 *                          command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getIOConfiguration(IOLine)
		 * @see #setIOConfiguration(IOLine, IOMode)
		 * @see com.digi.xbee.api.io.IOLine
		 * @see com.digi.xbee.api.io.IOMode#ADC
		 */
		public int GetADCValue(IOLine ioLine)/*throws TimeoutException, XBeeException */{
			// Check IO line.
			if (ioLine == null)
				throw new ArgumentNullException("IO line cannot be null.");

			// Obtain an IO Sample from the XBee device.
			IOSample ioSample = readIOSample();

			// Check if the IO sample contains the expected IO line and value.
			if (!ioSample.HasAnalogValues || !ioSample.AnalogValues.ContainsKey(ioLine))
				throw new OperationNotSupportedException("Answer does not contain analog data for " + ioLine.GetName() + ".");

			// Return the analog value.
			return ioSample.AnalogValues[ioLine];
		}

		/**
		 * Sets the 64-bit destination extended address of this XBee device.
		 * 
		 * <p>{@link XBee64BitAddress#BROADCAST_ADDRESS} is the broadcast address 
		 * for the PAN. {@link XBee64BitAddress#COORDINATOR_ADDRESS} can be used to 
		 * address the Pan Coordinator.</p>
		 * 
		 * @param xbee64BitAddress 64-bit destination address to be configured.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code xbee64BitAddress == null}.
		 * @throws TimeoutException if there is a timeout sending the set 
		 *                          destination address command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getDestinationAddress()
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public void setDestinationAddress(XBee64BitAddress xbee64BitAddress)/*throws TimeoutException, XBeeException */{
			if (xbee64BitAddress == null)
				throw new ArgumentNullException("Address cannot be null.");

			// This method needs to apply changes after modifying the destination 
			// address, but only if the destination address could be set successfully.
			bool applyChanges = IsApplyConfigurationChangesEnabled();
			if (applyChanges)
				EnableApplyConfigurationChanges(false);

			byte[] address = xbee64BitAddress.Value;
			try
			{
				var dh = new byte[4];
				var dl = new byte[4];
				Array.Copy(address, 0, dh, 0, 4);
				Array.Copy(address, 4, dl, 0, 4);
				SetParameter("DH", dh);
				SetParameter("DL", dl);
				this.ApplyChanges();
			}
			finally
			{
				// Always restore the old value of the AC.
				EnableApplyConfigurationChanges(applyChanges);
			}
		}

		/**
		 * Returns the 64-bit destination extended address of this XBee device.
		 * 
		 * <p>{@link XBee64BitAddress#BROADCAST_ADDRESS} is the broadcast address 
		 * for the PAN. {@link XBee64BitAddress#COORDINATOR_ADDRESS} can be used to 
		 * address the Pan Coordinator.</p>
		 * 
		 * @return 64-bit destination address.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout sending the get
		 *                          destination address command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #setDestinationAddress(XBee64BitAddress)
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public XBee64BitAddress GetDestinationAddress()/*throws TimeoutException, XBeeException */{
			byte[] dh = GetParameter("DH");
			byte[] dl = GetParameter("DL");
			byte[] address = new byte[dh.Length + dl.Length];

			Array.Copy(dh, 0, address, 0, dh.Length);
			Array.Copy(dl, 0, address, dh.Length, dl.Length);

			return new XBee64BitAddress(address);
		}

		/**
		 * Sets the IO sampling rate to enable periodic sampling in this XBee 
		 * device.
		 * 
		 * <p>A sample rate of {@code 0} ms. disables this feature.</p>
		 * 
		 * <p>All enabled digital IO and analog inputs will be sampled and
		 * transmitted every {@code rate} milliseconds to the configured destination
		 * address.</p>
		 * 
		 * <p>The destination address can be configured using the 
		 * {@code setDestinationAddress(XBee64BitAddress)} method and retrieved by 
		 * {@code getDestinationAddress()}.</p>
		 * 
		 * @param rate IO sampling rate in milliseconds.
		 * 
		 * @throws ArgumentException if {@code rate < 0} or {@code rate >
		 *                                  0xFFFF}.
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout sending the set IO
		 *                          sampling rate command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getDestinationAddress()
		 * @see #setDestinationAddress(XBee64BitAddress)
		 * @see #getIOSamplingRate()
		 */
		public void setIOSamplingRate(int rate)/*throws TimeoutException, XBeeException */{
			// Check range.
			if (rate < 0 || rate > 0xFFFF)
				throw new ArgumentException("Rate must be between 0 and 0xFFFF.");
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			SetParameter("IR", ByteUtils.IntToByteArray(rate));
		}

		/**
		 * Returns the IO sampling rate of this XBee device.
		 * 
		 * <p>A sample rate of {@code 0} means the IO sampling feature is disabled.
		 * </p>
		 * 
		 * <p>Periodic sampling allows this XBee module to take an IO sample and 
		 * transmit it to a remote device (configured in the destination address) 
		 * at the configured periodic rate (ms).</p>
		 * 
		 * @return IO sampling rate in milliseconds.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout sending the get IO
		 *                          sampling rate command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getDestinationAddress()
		 * @see #setDestinationAddress(XBee64BitAddress)
		 * @see #setIOSamplingRate(int)
		 */
		public int GetIOSamplingRate()/*throws TimeoutException, XBeeException */{
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			byte[] rate = GetParameter("IR");
			return ByteUtils.ByteArrayToInt(rate);
		}

		/**
		 * Sets the digital IO lines of this XBee device to be monitored and 
		 * sampled whenever their status changes.
		 * 
		 * <p>A {@code null} set of lines disables this feature.</p>
		 * 
		 * <p>If a change is detected on an enabled digital IO pin, a digital IO
		 * sample is immediately transmitted to the configured destination address.
		 * </p>
		 * 
		 * <p>The destination address can be configured using the 
		 * {@code setDestinationAddress(XBee64BitAddress)} method and retrieved by 
		 * {@code getDestinationAddress()}.</p>
		 * 
		 * @param lines Set of IO lines to be monitored, {@code null} to disable 
		 *              this feature.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout sending the set DIO
		 *                          change detection command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getDestinationAddress()
		 * @see #getDIOChangeDetection()
		 * @see #setDestinationAddress(XBee64BitAddress)
		 */
		public void SetDIOChangeDetection(ISet<IOLine> lines)/*throws TimeoutException, XBeeException */{
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
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

		/**
		 * Returns the set of IO lines of this device that are monitored for 
		 * change detection.
		 * 
		 * <p>A {@code null} set means the DIO change detection feature is disabled.
		 * </p>
		 * 
		 * <p>Modules can be configured to transmit to the configured destination 
		 * address a data sample immediately whenever a monitored digital IO line 
		 * changes state.</p> 
		 * 
		 * @return Set of digital IO lines that are monitored for change detection,
		 *         {@code null} if there are no monitored lines.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout sending the get DIO
		 *                          change detection command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getDestinationAddress()
		 * @see #setDestinationAddress(XBee64BitAddress)
		 * @see #setDIOChangeDetection(Set)
		 */
		public ISet<IOLine> GetDIOChangeDetection()/*throws TimeoutException, XBeeException */{
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			byte[] bitfield = GetParameter("IC");
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

		/**
		 * Applies changes to all command registers causing queued command register
		 * values to be applied.
		 * 
		 * <p>This method must be invoked if the 'apply configuration changes' 
		 * option is disabled and the changes to this XBee device parameters must 
		 * be applied.</p>
		 * 
		 * <p>To know if the 'apply configuration changes' option is enabled, use 
		 * the {@code isApplyConfigurationChangesEnabled()} method. And to 
		 * enable/disable this feature, the method 
		 * {@code enableApplyConfigurationChanges(boolean)}.</p>
		 * 
		 * <p>Applying changes does not imply the modifications will persist 
		 * through subsequent resets. To do so, use the {@code writeChanges()} 
		 * method.</p>
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout sending the get Apply
		 *                          Changes command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #enableApplyConfigurationChanges(boolean)
		 * @see #isApplyConfigurationChangesEnabled()
		 * @see #setParameter(String, byte[])
		 * @see #writeChanges()
		 */
		public void ApplyChanges()/*throws TimeoutException, XBeeException */{
			ExecuteParameter("AC");
		}

		/**
		 * Checks if the provided {@code ATCommandResponse} is valid throwing an 
		 * {@code ATCommandException} in case it is not.
		 * 
		 * @param response The {@code ATCommandResponse} to check.
		 * 
		 * @throws ATCommandException if {@code response == null} or 
		 *                            if {@code response.getResponseStatus() != ATCommandStatus.OK}.
		 * 
		 * @see com.digi.xbee.api.models.ATCommandResponse
		 */
		protected void CheckATCommandResponseIsValid(ATCommandResponse response)/*throws ATCommandException */{
			if (response == null || response.Status == null)
				throw new ATCommandException(ATCommandStatus.UNKNOWN);
			else if (response.Status != ATCommandStatus.OK)
				throw new ATCommandException(response.Status);
		}

		/**
		 * Returns an IO sample from this XBee device containing the value of all
		 * enabled digital IO and analog input channels.
		 * 
		 * @return An IO sample containing the value of all enabled digital IO and
		 *         analog input channels.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout getting the IO sample.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see com.digi.xbee.api.io.IOSample
		 */
		public IOSample readIOSample()/*throws TimeoutException, XBeeException */{
			// Check connection.
			if (!connectionInterface.SerialPort.IsOpen)
				throw new InterfaceNotOpenException();

			// Try to build an IO Sample from the sample payload.
			byte[] samplePayload = null;
			IOSample ioSample;

			// The response to the IS command in local 802.15.4 devices is empty, 
			// so we have to create a packet listener to receive the IO sample.
			if (!IsRemote && GetXBeeProtocol() == XBeeProtocol.RAW_802_15_4)
			{
				ExecuteParameter("IS");
				samplePayload = receiveRaw802IOPacket();
				if (samplePayload == null)
					throw new Kveer.XBeeApi.Exceptions.TimeoutException("Timeout waiting for the IO response packet.");
			}
			else
				samplePayload = GetParameter("IS");

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

		/**
		 * Returns the latest 802.15.4 IO packet and returns its value.
		 * 
		 * @return The value of the latest received 802.15.4 IO packet.
		 */
		private byte[] receiveRaw802IOPacket()
		{
			ioPacketReceived = false;
			ioPacketPayload = null;
			AddPacketListener(IOPacketReceiveListener);
			lock (ioLock)
			{
				try
				{
					Monitor.Wait(ioLock, receiveTimeout);
				}
				catch (ThreadInterruptedException) { }
			}
			RemovePacketListener(IOPacketReceiveListener);
			if (ioPacketReceived)
				return ioPacketPayload;
			return null;
		}

		/**
		 * Custom listener for 802.15.4 IO packets. It will try to receive an 
		 * 802.15.4 IO sample packet.
		 * 
		 * <p>When an IO sample packet is received, it saves its payload and 
		 * notifies the object that was waiting for the reception.</p>
		 */
		class CustomPacketReceiveListener : IPacketReceiveListener
		{
			AbstractXBeeDevice _xbeeDevice;

			public CustomPacketReceiveListener(AbstractXBeeDevice device)
			{
				_xbeeDevice = device;
			}
			public void PacketReceived(XBeePacket receivedPacket)
			{
				// Discard non API packets.
				if (!(receivedPacket is XBeeAPIPacket))
					return;
				// If we already have received an IO packet, ignore this packet.
				if (_xbeeDevice.ioPacketReceived)
					return;

				// Save the packet value (IO sample payload)
				switch (((XBeeAPIPacket)receivedPacket).FrameType)
				{
					case APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR:
						_xbeeDevice.ioPacketPayload = ((IODataSampleRxIndicatorPacket)receivedPacket).RFData;
						break;
					case APIFrameType.RX_IO_16:
						_xbeeDevice.ioPacketPayload = ((RX16IOPacket)receivedPacket).RFData;
						break;
					case APIFrameType.RX_IO_64:
						_xbeeDevice.ioPacketPayload = ((RX64IOPacket)receivedPacket).RFData;
						break;
					default:
						return;
				}
				// Set the IO packet received flag.
				_xbeeDevice.ioPacketReceived = true;

				// Continue execution by notifying the lock object.
				lock (_xbeeDevice.ioLock)
				{
					Monitor.Pulse(_xbeeDevice.ioLock);
				}
			}
		}
		private IPacketReceiveListener IOPacketReceiveListener;

		/**
		 * Performs a software reset on this XBee device and blocks until the 
		 * process is completed.
		 * 
		 * @throws TimeoutException if the configured time expires while waiting 
		 *                          for the command reply.
		 * @throws XBeeException if there is any other XBee related exception.
		 */
		abstract public void reset()/*throws TimeoutException, XBeeException*/;

		/**
		 * Sets the given parameter with the provided value in this XBee device.
		 * 
		 * <p>If the 'apply configuration changes' option is enabled in this device,
		 * the configured value for the given parameter will be immediately applied, 
		 * if not the method {@code applyChanges()} must be invoked to apply it.</p>
		 * 
		 * <p>Use:</p>
		 * <ul>
		 * <li>Method {@code isApplyConfigurationChangesEnabled()} to know 
		 * if the 'apply configuration changes' option is enabled.</li>
		 * <li>Method {@code enableApplyConfigurationChanges(boolean)} to enable or
		 * disable this option.</li>
		 * </ul>
		 * 
		 * <p>To make parameter modifications persist through subsequent resets use 
		 * the {@code writeChanges()} method.</p>
		 * 
		 * @param parameter The name of the parameter to be set.
		 * @param parameterValue The value of the parameter to set.
		 * 
		 * @throws ArgumentException if {@code parameter.length() != 2}.
		 * @throws ArgumentNullException if {@code parameter == null} or 
		 *                              if {@code parameterValue == null}.
		 * @throws TimeoutException if there is a timeout setting the parameter.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #applyChanges()
		 * @see #enableApplyConfigurationChanges(boolean)
		 * @see #executeParameter(String) 
		 * @see #getParameter(String)
		 * @see #isApplyConfigurationChangesEnabled()
		 * @see #writeChanges()
		 */
		public void SetParameter(String parameter, byte[] parameterValue)/*throws TimeoutException, XBeeException */{
			if (parameterValue == null)
				throw new ArgumentNullException("Value of the parameter cannot be null.");

			SendParameter(parameter, parameterValue);
		}

		/**
		 * Gets the value of the given parameter from this XBee device.
		 * 
		 * @param parameter The name of the parameter to retrieve its value.
		 * 
		 * @return A byte array containing the value of the parameter.
		 * 
		 * @throws ArgumentException if {@code parameter.length() != 2}.
		 * @throws ArgumentNullException if {@code parameter == null}.
		 * @throws TimeoutException if there is a timeout getting the parameter value.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #executeParameter(String)
		 * @see #setParameter(String, byte[])
		 */
		public byte[] GetParameter(string parameter) /*throws TimeoutException, XBeeException */{
			byte[] parameterValue = SendParameter(parameter, null);

			// Check if the response is null, if so throw an exception (maybe it was a write-only parameter).
			if (parameterValue == null)
				throw new OperationNotSupportedException("Couldn't get the '" + parameter + "' value.");
			return parameterValue;
		}

		/**
		 * Executes the given command in this XBee device.
		 * 
		 * <p>This method is intended to be used for those AT parameters that cannot 
		 * be read or written, they just execute some action in the XBee module.</p>
		 * 
		 * @param parameter The AT command to be executed.
		 * 
		 * @throws ArgumentException if {@code parameter.length() != 2}.
		 * @throws ArgumentNullException if {@code parameter == null}.
		 * @throws TimeoutException if there is a timeout executing the parameter.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getParameter(String)
		 * @see #setParameter(String, byte[])
		 */
		public void ExecuteParameter(string parameter)/*throws TimeoutException, XBeeException */{
			SendParameter(parameter, null);
		}

		/**
		 * Sends the given AT parameter to this XBee device with an optional 
		 * argument or value and returns the response (likely the value) of that 
		 * parameter in a byte array format.
		 * 
		 * @param parameter The name of the AT command to be executed.
		 * @param parameterValue The value of the parameter to set (if any).
		 * 
		 * @return A byte array containing the value of the parameter.
		 * 
		 * @throws ArgumentException if {@code parameter.length() != 2}.
		 * @throws ArgumentNullException if {@code parameter == null}.
		 * @throws TimeoutException if there is a timeout executing the parameter.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getParameter(String)
		 * @see #executeParameter(String)
		 * @see #setParameter(String, byte[])
		 */
		private byte[] SendParameter(string parameter, byte[] parameterValue)/*throws TimeoutException, XBeeException */{
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

			// Return the response value.
			return response.Response;
		}

		public override string ToString()
		{
			return connectionInterface.ToString();
		}

		/**
		 * Enables or disables the 'apply configuration changes' option for this 
		 * device.
		 * 
		 * <p>Enabling this option means that when any parameter of this XBee 
		 * device is set, it will be also applied.</p>
		 * 
		 * <p>If this option is disabled, the method {@code applyChanges()} must be 
		 * used in order to apply the changes in all the parameters that were 
		 * previously set.</p>
		 * 
		 * @param enabled {@code true} to apply configuration changes when an XBee 
		 *                parameter is set, {@code false} otherwise.
		 * 
		 * @see #applyChanges()
		 * @see #isApplyConfigurationChangesEnabled()
		 */
		public void EnableApplyConfigurationChanges(bool enabled)
		{
			applyConfigurationChanges = enabled;
		}

		/**
		 * Returns whether the 'apply configuration changes' option is enabled in 
		 * this device.
		 * 
		 * <p>If this option is enabled, when any parameter of this XBee device is 
		 * set, it will be also applied.</p>
		 * 
		 * <p>If this option is disabled, the method {@code applyChanges()} must be 
		 * used in order to apply the changes in all the parameters that were 
		 * previously set.</p>
		 * 
		 * @return {@code true} if the option is enabled, {@code false} otherwise.
		 * 
		 * @see #applyChanges()
		 * @see #enableApplyConfigurationChanges(boolean)
		 */
		public bool IsApplyConfigurationChangesEnabled()
		{
			return applyConfigurationChanges;
		}

		/**
		 * Configures the 16-bit address (network address) of this XBee device with 
		 * the provided one.
		 * 
		 * @param xbee16BitAddress The new 16-bit address.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code xbee16BitAddress == null}.
		 * @throws TimeoutException if there is a timeout setting the address.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #get16BitAddress()
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		protected void Set16BitAddress(XBee16BitAddress xbee16BitAddress)/*throws TimeoutException, XBeeException */{
			if (xbee16BitAddress == null)
				throw new ArgumentNullException("16-bit address canot be null.");

			SetParameter("MY", xbee16BitAddress.Value);

			this.xbee16BitAddress = xbee16BitAddress;
		}

		/**
		 * Returns the operating PAN ID (Personal Area Network Identifier) of 
		 * this XBee device.
		 * 
		 * <p>For modules to communicate they must be configured with the same 
		 * identifier. Only modules with matching IDs can communicate with each 
		 * other.This parameter allows multiple networks to co-exist on the same 
		 * physical channel.</p>
		 * 
		 * @return The operating PAN ID of this XBee device.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout getting the PAN ID.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #setPANID(byte[])
		 */
		public byte[] GetPANID()/*throws TimeoutException, XBeeException */{
			switch (GetXBeeProtocol())
			{
				case XBeeProtocol.ZIGBEE:
					return GetParameter("OP");
				default:
					return GetParameter("ID");
			}
		}

		/**
		 * Sets the PAN ID (Personal Area Network Identifier) of this XBee device.
		 * 
		 * <p>For modules to communicate they must be configured with the same 
		 * identifier. Only modules with matching IDs can communicate with each 
		 * other.This parameter allows multiple networks to co-exist on the same 
		 * physical channel.</p>
		 * 
		 * @param panID The new PAN ID of this XBee device.
		 * 
		 * @throws ArgumentException if {@code panID.length == 0} or 
		 *                                  if {@code panID.length > 8}.
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code panID == null}.
		 * @throws TimeoutException if there is a timeout setting the PAN ID.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getPANID()
		 */
		public void SetPANID(byte[] panID)/*throws TimeoutException, XBeeException */{
			if (panID == null)
				throw new ArgumentNullException("PAN ID cannot be null.");
			if (panID.Length == 0)
				throw new ArgumentException("Length of the PAN ID cannot be 0.");
			if (panID.Length > 8)
				throw new ArgumentException("Length of the PAN ID cannot be longer than 8 bytes.");

			SetParameter("ID", panID);
		}

		/**
		 * Returns the output power level at which this XBee device transmits 
		 * conducted power.
		 * 
		 * @return The output power level of this XBee device.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout getting the power level.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #setPowerLevel(PowerLevel)
		 * @see com.digi.xbee.api.models.PowerLevel
		 */
		public PowerLevel GetPowerLevel()/*throws TimeoutException, XBeeException */{
			byte[] powerLevelValue = GetParameter("PL");
			return PowerLevel.LEVEL_UNKNOWN.Get((byte)ByteUtils.ByteArrayToInt(powerLevelValue));
		}

		/**
		 * Sets the output power level at which this XBee device transmits 
		 * conducted power.
		 * 
		 * @param powerLevel The new output power level to be set in this XBee 
		 *                   device.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code powerLevel == null}.
		 * @throws TimeoutException if there is a timeout setting the power level.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getPowerLevel()
		 * @see com.digi.xbee.api.models.PowerLevel
		 */
		public void SetPowerLevel(PowerLevel powerLevel)/*throws TimeoutException, XBeeException */{
			if (powerLevel == null)
				throw new ArgumentNullException("Power level cannot be null.");

			SetParameter("PL", ByteUtils.IntToByteArray(powerLevel.GetValue()));
		}

		/**
		 * Returns the current association status of this XBee device.
		 * 
		 * <p>It indicates occurrences of errors during the last association 
		 * request.</p>
		 * 
		 * @return The association indication status of the XBee device.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout getting the association 
		 *                          indication status.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #forceDisassociate()
		 * @see com.digi.xbee.api.models.AssociationIndicationStatus
		 */
		protected AssociationIndicationStatus GetAssociationIndicationStatus()/*throws TimeoutException, XBeeException */{
			byte[] associationIndicationValue = GetParameter("AI");
			return AssociationIndicationStatus.NJ_EXPIRED.Get((byte)ByteUtils.ByteArrayToInt(associationIndicationValue));
		}

		/**
		 * Forces this XBee device to immediately disassociate from the network and 
		 * re-attempt to associate.
		 * 
		 * <p>Only valid for End Devices.</p>
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout executing the 
		 *         disassociation command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getAssociationIndicationStatus()
		 */
		protected void ForceDisassociate()/*throws TimeoutException, XBeeException */{
			ExecuteParameter("DA");
		}

		/**
		 * Writes configurable parameter values to the non-volatile memory of this 
		 * XBee device so that parameter modifications persist through subsequent 
		 * resets.
		 * 
		 * <p>Parameters values remain in this device's memory until overwritten by 
		 * subsequent use of this method.</p>
		 * 
		 * <p>If changes are made without writing them to non-volatile memory, the 
		 * module reverts back to previously saved parameters the next time the 
		 * module is powered-on.</p>
		 * 
		 * <p>Writing the parameter modifications does not mean those values are 
		 * immediately applied, this depends on the status of the 'apply 
		 * configuration changes' option. Use method 
		 * {@code isApplyConfigurationChangesEnabled()} to get its status and 
		 * {@code enableApplyConfigurationChanges(boolean)} to enable/disable the 
		 * option. If it is disable method {@code applyChanges()} can be used in 
		 * order to manually apply the changes.</p>
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws TimeoutException if there is a timeout executing the write 
		 *                          changes command.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #applyChanges()
		 * @see #enableApplyConfigurationChanges(boolean)
		 * @see #isApplyConfigurationChangesEnabled()
		 * @see #setParameter(String, byte[])
		 */
		public void WriteChanges() /*throws TimeoutException, XBeeException*/ {
			ExecuteParameter("WR");
		}
	}
}