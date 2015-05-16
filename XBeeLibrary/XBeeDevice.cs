using Kveer.XBeeApi.Connection;
using Kveer.XBeeApi.Connection.Serial;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.listeners;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Packet;
using Kveer.XBeeApi.Packet.Common;
using Kveer.XBeeApi.Packet.Raw;
using Kveer.XBeeApi.Utils;
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Kveer.XBeeApi
{
	/**
	 * This class represents a local XBee device.
	 * 
	 * @see DigiMeshDevice
	 * @see DigiPointDevice
	 * @see Raw802Device
	 * @see ZigBeeDevice
	 */
	public class XBeeDevice : AbstractXBeeDevice
	{

		// Constants.
		private static int TIMEOUT_RESET = 5000;
		private static int TIMEOUT_READ_PACKET = 3000;

		private static string COMMAND_MODE_CHAR = "+";
		private static string COMMAND_MODE_OK = "OK\r";

		// Variables.
		protected XBeeNetwork network;

		private object resetLock = new object();

		private bool modemStatusReceived = false;

		/**
		 * Class constructor. Instantiates a new {@code XBeeDevice} object 
		 * physically connected to the given port name and configured at the 
		 * provided baud rate.
		 * 
		 * @param port Serial port name where XBee device is attached to.
		 * @param baudRate Serial port baud rate to communicate with the device. 
		 *                 Other connection parameters will be set as default (8 
		 *                 data bits, 1 stop bit, no parity, no flow control).
		 * 
		 * @throws ArgumentException if {@code baudRate < 0}.
		 * @throws ArgumentNullException if {@code port == null}.
		 * 
		 * @see #XBeeDevice(IConnectionInterface)
		 * @see #XBeeDevice(String, SerialPortParameters)
		 * @see #XBeeDevice(String, int, int, int, int, int)
		 */
		public XBeeDevice(String port, int baudRate)
			: base(port, baudRate)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code XBeeDevice} object 
		 * physically connected to the given port name and configured to communicate 
		 * with the provided serial settings.
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
		 * @see #XBeeDevice(IConnectionInterface)
		 * @see #XBeeDevice(String, int)
		 * @see #XBeeDevice(String, SerialPortParameters)
		 */
		public XBeeDevice(String port, int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl)
			: base(port, baudRate, dataBits, stopBits, parity, flowControl)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code XBeeDevice} object 
		 * physically connected to the given port name and configured to communicate 
		 * with the provided serial settings.
		 * 
		 * @param port Serial port name where XBee device is attached to.
		 * @param serialPortParameters Object containing the serial port parameters.
		 * 
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code serialPortParameters == null}.
		 * 
		 * @see #XBeeDevice(IConnectionInterface)
		 * @see #XBeeDevice(String, int)
		 * @see #XBeeDevice(String, int, int, int, int, int)
		 * @see com.digi.xbee.api.connection.serial.SerialPortParameters
		 */
		public XBeeDevice(String port, SerialPortParameters serialPortParameters)
			: base(port, serialPortParameters)
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
		 * @see #XBeeDevice(String, int)
		 * @see #XBeeDevice(String, SerialPortParameters)
		 * @see #XBeeDevice(String, int, int, int, int, int)
		 * @see com.digi.xbee.api.connection.IConnectionInterface
		 */
		public XBeeDevice(IConnectionInterface connectionInterface)
			: base(connectionInterface)
		{
		}

		/**
		 * Opens the connection interface associated with this XBee device.
		 * 
		 * <p>When opening the device an information reading process is 
		 * automatically performed. This includes:</p>
		 * 
		 * <ul>
		 * <li>64-bit address.</li>
		 * <li>Node Identifier.</li>
		 * <li>Hardware version.</li>
		 * <li>Firmware version.</li>
		 * <li>XBee device protocol.</li>
		 * <li>16-bit address (not for DigiMesh modules).</li>
		 * </ul>
		 * 
		 * @throws InterfaceAlreadyOpenException if this device connection is 
		 *                                       already open.
		 * @throws XBeeException if there is any problem opening this device 
		 *                       connection.
		 * 
		 * @see #close()
		 * @see #isOpen()
		 */
		public virtual void Open()/*throws XBeeException */{
			logger.Info(ToString() + "Opening the connection interface...");

			// First, verify that the connection is not already open.
			if (connectionInterface.IsOpen)
				throw new InterfaceAlreadyOpenException();

			// Connect the interface.
			connectionInterface.Open();

			logger.Info(ToString() + "Connection interface open.");

			// Initialize the data reader.
			dataReader = new DataReader(connectionInterface, operatingMode, this);
			dataReader.start();

			// Wait 10 milliseconds until the dataReader thread is started.
			// This is because when the connection is opened immediately after 
			// closing it, there is sometimes a concurrency problem and the 
			// dataReader thread never dies.
			try
			{
				Thread.Sleep(10);
			}
			catch (ThreadInterruptedException e) { }

			// Determine the operating mode of the XBee device if it is unknown.
			if (operatingMode == OperatingMode.UNKNOWN)
				operatingMode = DetermineOperatingMode();

			// Check if the operating mode is a valid and supported one.
			if (operatingMode == OperatingMode.UNKNOWN)
			{
				Close();
				throw new InvalidOperatingModeException("Could not determine operating mode.");
			}
			else if (operatingMode == OperatingMode.AT)
			{
				Close();
				throw new InvalidOperatingModeException(operatingMode);
			}

			// Read the device info (obtain its parameters and protocol).
			readDeviceInfo();
		}

		/**
		 * Closes the connection interface associated with this XBee device.
		 * 
		 * @see #isOpen()
		 * @see #open()
		 */
		public void Close()
		{
			// Stop XBee reader.
			if (dataReader != null && dataReader.IsRunning)
				dataReader.StopReader();
			// Close interface.
			connectionInterface.Close();
			logger.Info(ToString() + "Connection interface closed.");
		}

		/**
		 * Returns whether the connection interface associated to this device is 
		 * already open.
		 * 
		 * @return {@code true} if the interface is open, {@code false} otherwise.
		 * 
		 * @see #close()
		 * @see #open()
		 */
		public bool IsOpen
		{
			get
			{
				if (connectionInterface != null)
					return connectionInterface.IsOpen;
				return false;
			}
		}

		/**
		 * Always returns {@code false}, since this is always a local device.
		 * 
		 * @return {@code false} since it is a local device.
		 */
		public override bool IsRemote
		{
			get
			{
				return false;
			}
		}

		/**
		 * Returns the network associated with this XBee device.
		 * 
		 * @return The XBee network of the device.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * 
		 * @see XBeeNetwork
		 */
		public virtual XBeeNetwork GetNetwork()
		{
			if (!IsOpen)
				throw new InterfaceNotOpenException();

			if (network == null)
				network = new XBeeNetwork(this);
			return network;
		}

		/// <summary>
		/// Gets or sets this XBee device timeout in milliseconds for received packets in synchronous operations.
		/// </summary>
		public int ReceiveTimeout
		{
			get
			{
				return receiveTimeout;
			}
			set
			{
				if (value < 0)
					throw new ArgumentException("Receive timeout cannot be less than 0.");

				this.receiveTimeout = receiveTimeout;
			}
		}

		/**
		 * Determines the operating mode of this XBee device.
		 * 
		 * @return The operating mode of the XBee device.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws OperationNotSupportedException if the packet is being sent from 
		 *                                        a remote device.
		 * 
		 * @see com.digi.xbee.api.models.OperatingMode
		 */
		protected OperatingMode DetermineOperatingMode()/*throws OperationNotSupportedException */{
			try
			{
				// Check if device is in API or API Escaped operating modes.
				operatingMode = OperatingMode.API;
				dataReader.SetXBeeReaderMode(operatingMode);

				ATCommandResponse response = SendATCommand(new ATCommand("AP"));
				if (response.Response != null && response.Response.Length > 0)
				{
					if (response.Response[0] != OperatingMode.API.GetID())
					{
						operatingMode = OperatingMode.API_ESCAPE;
						dataReader.SetXBeeReaderMode(operatingMode);
					}
					logger.DebugFormat(toString() + "Using {0}.", operatingMode.GetName());
					return operatingMode;
				}
			}
			catch (Kveer.XBeeApi.Exceptions.TimeoutException )
			{
				// Check if device is in AT operating mode.
				operatingMode = OperatingMode.AT;
				dataReader.SetXBeeReaderMode(operatingMode);

				try
				{
					// It is necessary to wait at least 1 second to enter in 
					// command mode after sending any data to the device.
					Thread.Sleep(TIMEOUT_BEFORE_COMMAND_MODE);
					// Try to enter in AT command mode, if so the module is in AT mode.
					bool success = EnterATCommandMode();
					if (success)
						return OperatingMode.AT;
				}
				catch (Kveer.XBeeApi.Exceptions.TimeoutException e1)
				{
					logger.Error(e1.Message, e1);
				}
				catch (InvalidOperatingModeException e1)
				{
					logger.Error(e1.Message, e1);
				}
				catch (ThreadInterruptedException e1)
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

		/**
		 * Attempts to put this device in AT Command mode. Only valid if device is 
		 * working in AT mode.
		 * 
		 * @return {@code true} if the device entered in AT command mode, 
		 *         {@code false} otherwise.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws InvalidOperatingModeException if the operating mode cannot be 
		 *                                       determined or is not supported.
		 * @throws TimeoutException if the configured time for this device expires.
		 */
		private bool EnterATCommandMode()/*throws InvalidOperatingModeException, TimeoutException */{
			if (!connectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			if (operatingMode != OperatingMode.AT)
				throw new InvalidOperatingModeException("Invalid mode. Command mode can be only accessed while in AT mode.");

			// Enter in AT command mode (send '+++'). The process waits 1,5 seconds for the 'OK\n'.
			byte[] readData = new byte[256];
			try
			{
				// Send the command mode sequence.
				connectionInterface.WriteData(Encoding.UTF8.GetBytes(COMMAND_MODE_CHAR));
				connectionInterface.WriteData(Encoding.UTF8.GetBytes(COMMAND_MODE_CHAR));
				connectionInterface.WriteData(Encoding.UTF8.GetBytes(COMMAND_MODE_CHAR));

				// Wait some time to let the module generate a response.
				Thread.Sleep(TIMEOUT_ENTER_COMMAND_MODE);

				// Read data from the device (it should answer with 'OK\r').
				int readBytes = connectionInterface.ReadData(readData);
				if (readBytes < COMMAND_MODE_OK.Length)
					throw new Kveer.XBeeApi.Exceptions.TimeoutException();

				// Check if the read data is 'OK\r'.
				String readString = Encoding.UTF8.GetString(readData, 0, readBytes);
				if (!readString.Contains(COMMAND_MODE_OK))
					return false;

				// Read data was 'OK\r'.
				return true;
			}
			catch (IOException e)
			{
				logger.Error(e.Message, e);
			}
			catch (ThreadInterruptedException e)
			{
				logger.Error(e.Message, e);
			}
			return false;
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#addPacketListener(com.digi.xbee.api.listeners.IPacketReceiveListener)
		 */
		//@Override
		public new void addPacketListener(IPacketReceiveListener listener)
		{
			base.addPacketListener(listener);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#removePacketListener(com.digi.xbee.api.listeners.IPacketReceiveListener)
		 */
		//@Override
		public new void removePacketListener(IPacketReceiveListener listener)
		{
			base.removePacketListener(listener);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#addDataListener(com.digi.xbee.api.listeners.IDataReceiveListener)
		 */
		//@Override
		public new void addDataListener(IDataReceiveListener listener)
		{
			base.addDataListener(listener);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#removeDataListener(com.digi.xbee.api.listeners.IDataReceiveListener)
		 */
		//@Override
		public new void removeDataListener(IDataReceiveListener listener)
		{
			base.removeDataListener(listener);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#addIOSampleListener(com.digi.xbee.api.listeners.IIOSampleReceiveListener)
		 */
		//@Override
		public new void addIOSampleListener(IIOSampleReceiveListener listener)
		{
			base.addIOSampleListener(listener);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#removeIOSampleListener(com.digi.xbee.api.listeners.IIOSampleReceiveListener)
		 */
		//@Override
		public new void removeIOSampleListener(IIOSampleReceiveListener listener)
		{
			base.removeIOSampleListener(listener);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#addModemStatusListener(com.digi.xbee.api.listeners.IModemStatusReceiveListener)
		 */
		//@Override
		public new void addModemStatusListener(IModemStatusReceiveListener listener)
		{
			base.addModemStatusListener(listener);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#removeModemStatusListener(com.digi.xbee.api.listeners.IModemStatusReceiveListener)
		 */
		//@Override
		public new void removeModemStatusListener(IModemStatusReceiveListener listener)
		{
			base.removeModemStatusListener(listener);
		}

		/**
		 * Sends asynchronously the provided data to the XBee device of the network 
		 * corresponding to the given 64-bit address.
		 * 
		 * <p>Asynchronous transmissions do not wait for answer from the remote 
		 * device or for transmit status packet.</p>
		 * 
		 * @param address The 64-bit address of the XBee that will receive the data.
		 * @param data Byte array containing the data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code address == null} or 
		 *                              if {@code data == null}.
		 * @throws XBeeException if there is any XBee related exception.
		 * 
		 * @see #sendData(RemoteXBeeDevice, byte[])
		 * @see #sendData(XBee64BitAddress, byte[])
		 * @see #sendData(XBee64BitAddress, XBee16BitAddress, byte[])
		 * @see #sendDataAsync(RemoteXBeeDevice, byte[])
		 * @see #sendDataAsync(XBee64BitAddress, XBee16BitAddress, byte[])
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		protected void SendDataAsync(XBee64BitAddress address, byte[] data)/*throws XBeeException */{
			// Verify the parameters are not null, if they are null, throw an exception.
			if (address == null)
				throw new ArgumentNullException("Address cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check connection.
			if (!connectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send data to a remote device from a remote device.");

			logger.DebugFormat(toString() + "Sending data asynchronously to {0} >> {1}.", address, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket;
			switch (getXBeeProtocol())
			{
				case XBeeProtocol.RAW_802_15_4:
					xbeePacket = new TX64Packet(getNextFrameID(), address, (byte)XBeeTransmitOptions.NONE, data);
					break;
				default:
					xbeePacket = new TransmitPacket(getNextFrameID(), address, XBee16BitAddress.UNKNOWN_ADDRESS, 0, (byte)XBeeTransmitOptions.NONE, data);
					break;
			}
			SendAndCheckXBeePacket(xbeePacket, true);
		}

		/**
		 * Sends asynchronously the provided data to the XBee device of the network 
		 * corresponding to the given 64-bit/16-bit address.
		 * 
		 * <p>Asynchronous transmissions do not wait for answer from the remote 
		 * device or for transmit status packet.</p>
		 * 
		 * @param address64Bit The 64-bit address of the XBee that will receive the 
		 *                     data.
		 * @param address16bit The 16-bit address of the XBee that will receive the 
		 *                     data. If it is unknown the 
		 *                     {@code XBee16BitAddress.UNKNOWN_ADDRESS} must be 
		 *                     used.
		 * @param data Byte array containing the data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code address64Bit == null} or 
		 *                              if {@code address16bit == null} or
		 *                              if {@code data == null}.
		 * @throws XBeeException if a remote device is trying to send data or 
		 *                       if there is any other XBee related exception.
		 * 
		 * @see #getReceiveTimeout()
		 * @see #setReceiveTimeout(int)
		 * @see #sendData(RemoteXBeeDevice, byte[])
		 * @see #sendData(XBee64BitAddress, byte[])
		 * @see #sendData(XBee64BitAddress, XBee16BitAddress, byte[])
		 * @see #sendDataAsync(RemoteXBeeDevice, byte[])
		 * @see #sendDataAsync(XBee64BitAddress, byte[])
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		protected void SendDataAsync(XBee64BitAddress address64Bit, XBee16BitAddress address16bit, byte[] data)/*throws XBeeException */{
			// Verify the parameters are not null, if they are null, throw an exception.
			if (address64Bit == null)
				throw new ArgumentNullException("64-bit address cannot be null");
			if (address16bit == null)
				throw new ArgumentNullException("16-bit address cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check connection.
			if (!connectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send data to a remote device from a remote device.");

			logger.DebugFormat(toString() + "Sending data asynchronously to {0}[{1}] >> {2}.",
					address64Bit, address16bit, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new TransmitPacket(getNextFrameID(), address64Bit, address16bit, 0, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, true);
		}

		/**
		 * Sends the provided data to the provided XBee device asynchronously.
		 * 
		 * <p>Asynchronous transmissions do not wait for answer from the remote 
		 * device or for transmit status packet.</p>
		 * 
		 * @param xbeeDevice The XBee device of the network that will receive the 
		 *                   data.
		 * @param data Byte array containing the data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code xbeeDevice == null} or 
		 *                              if {@code data == null}.
		 * @throws XBeeException if there is any XBee related exception.
		 * 
		 * @see #sendData(RemoteXBeeDevice, byte[])
		 * @see RemoteXBeeDevice
		 */
		public void SendDataAsync(RemoteXBeeDevice xbeeDevice, byte[] data)/*throws XBeeException */{
			if (xbeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null");
			SendDataAsync(xbeeDevice.get64BitAddress(), data);
		}

		/**
		 * Sends the provided data to the XBee device of the network corresponding 
		 * to the given 64-bit address.
		 * 
		 * <p>This method blocks till a success or error response arrives or the 
		 * configured receive timeout expires.</p>
		 * 
		 * <p>The received timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * <p>For non-blocking operations use the method 
		 * {@link #sendData(XBee64BitAddress, byte[])}.</p>
		 * 
		 * @param address The 64-bit address of the XBee that will receive the data.
		 * @param data Byte array containing the data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code address == null} or 
		 *                              if {@code data == null}.
		 * @throws TimeoutException if there is a timeout sending the data.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getReceiveTimeout()
		 * @see #setReceiveTimeout(int)
		 * @see #sendData(RemoteXBeeDevice, byte[])
		 * @see #sendData(XBee64BitAddress, XBee16BitAddress, byte[])
		 * @see #sendDataAsync(RemoteXBeeDevice, byte[])
		 * @see #sendDataAsync(XBee64BitAddress, byte[])
		 * @see #sendDataAsync(XBee64BitAddress, XBee16BitAddress, byte[])
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		protected void SendData(XBee64BitAddress address, byte[] data)/*throws TimeoutException, XBeeException */{
			// Verify the parameters are not null, if they are null, throw an exception.
			if (address == null)
				throw new ArgumentNullException("Address cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check connection.
			if (!connectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send data to a remote device from a remote device.");

			logger.DebugFormat(toString() + "Sending data to {0} >> {1}.", address, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket;
			switch (getXBeeProtocol())
			{
				case XBeeProtocol.RAW_802_15_4:
					xbeePacket = new TX64Packet(getNextFrameID(), address, (byte)XBeeTransmitOptions.NONE, data);
					break;
				default:
					xbeePacket = new TransmitPacket(getNextFrameID(), address, XBee16BitAddress.UNKNOWN_ADDRESS, 0, (byte)XBeeTransmitOptions.NONE, data);
					break;
			}
			SendAndCheckXBeePacket(xbeePacket, false);
		}

		/**
		 * Sends the provided data to the XBee device of the network corresponding 
		 * to the given 64-bit/16-bit address.
		 * 
		 * <p>This method blocks till a success or error response arrives or the 
		 * configured receive timeout expires.</p>
		 * 
		 * <p>The received timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * <p>For non-blocking operations use the method 
		 * {@link #sendDataAsync(XBee64BitAddress, XBee16BitAddress, byte[])}.</p>
		 * 
		 * @param address64Bit The 64-bit address of the XBee that will receive the 
		 *                     data.
		 * @param address16bit The 16-bit address of the XBee that will receive the 
		 *                     data. If it is unknown the 
		 *                     {@code XBee16BitAddress.UNKNOWN_ADDRESS} must be 
		 *                     used.
		 * @param data Byte array containing the data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code address64Bit == null} or 
		 *                              if {@code address16bit == null} or
		 *                              if {@code data == null}.
		 * @throws TimeoutException if there is a timeout sending the data.
		 * @throws XBeeException if a remote device is trying to send data or 
		 *                       if there is any other XBee related exception.
		 * 
		 * @see #getReceiveTimeout()
		 * @see #setReceiveTimeout(int)
		 * @see #sendData(RemoteXBeeDevice, byte[])
		 * @see #sendData(XBee64BitAddress, byte[])
		 * @see #sendDataAsync(RemoteXBeeDevice, byte[])
		 * @see #sendDataAsync(XBee64BitAddress, byte[])
		 * @see #sendDataAsync(XBee64BitAddress, XBee16BitAddress, byte[])
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		protected void SendData(XBee64BitAddress address64Bit, XBee16BitAddress address16bit, byte[] data)/*throws TimeoutException, XBeeException */{
			// Verify the parameters are not null, if they are null, throw an exception.
			if (address64Bit == null)
				throw new ArgumentNullException("64-bit address cannot be null");
			if (address16bit == null)
				throw new ArgumentNullException("16-bit address cannot be null");
			if (data == null)
				throw new ArgumentNullException("Data cannot be null");

			// Check connection.
			if (!connectionInterface.IsOpen)
				throw new InterfaceNotOpenException();
			// Check if device is remote.
			if (IsRemote)
				throw new OperationNotSupportedException("Cannot send data to a remote device from a remote device.");

			logger.DebugFormat(toString() + "Sending data to {0}[{1}] >> {2}.",
					address64Bit, address16bit, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new TransmitPacket(getNextFrameID(), address64Bit, address16bit, 0, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, false);
		}

		/**
		 * Sends the provided data to the given XBee device choosing the optimal 
		 * send method depending on the protocol of the local XBee device.
		 * 
		 * <p>This method blocks till a success or error response arrives or the 
		 * configured receive timeout expires.</p>
		 * 
		 * <p>The received timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * <p>For non-blocking operations use the method 
		 * {@link #sendDataAsync(RemoteXBeeDevice, byte[])}.</p>
		 * 
		 * @param xbeeDevice The XBee device of the network that will receive the 
		 *                   data.
		 * @param data Byte array containing the data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code xbeeDevice == null} or 
		 *                              if {@code data == null}.
		 * @throws TimeoutException if there is a timeout sending the data.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getReceiveTimeout()
		 * @see #setReceiveTimeout(int)
		 * @see #sendDataAsync(RemoteXBeeDevice, byte[])
		 */
		public void SendData(RemoteXBeeDevice xbeeDevice, byte[] data)/*throws TimeoutException, XBeeException */{
			if (xbeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null");

			switch (getXBeeProtocol())
			{
				case XBeeProtocol.ZIGBEE:
				case XBeeProtocol.DIGI_POINT:
					if (xbeeDevice.get64BitAddress() != null && xbeeDevice.get16BitAddress() != null)
						SendData(xbeeDevice.get64BitAddress(), xbeeDevice.get16BitAddress(), data);
					else
						SendData(xbeeDevice.get64BitAddress(), data);
					break;
				case XBeeProtocol.RAW_802_15_4:
					if (this is Raw802Device)
					{
						if (xbeeDevice.get64BitAddress() != null)
							((Raw802Device)this).SendData(xbeeDevice.get64BitAddress(), data);
						else
							((Raw802Device)this).SendData(xbeeDevice.get16BitAddress(), data);
					}
					else
						SendData(xbeeDevice.get64BitAddress(), data);
					break;
				case XBeeProtocol.DIGI_MESH:
				default:
					SendData(xbeeDevice.get64BitAddress(), data);
					break;
			}
		}

		/**
		 * Sends the provided data to all the XBee nodes of the network (broadcast).
		 * 
		 * <p>This method blocks till a success or error transmit status arrives or 
		 * the configured receive timeout expires.</p>
		 * 
		 * <p>The received timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * @param data Byte array containing the data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code data == null}.
		 * @throws TimeoutException if there is a timeout sending the data.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getReceiveTimeout()
		 * @see #setReceiveTimeout(int)
		 */
		public void SendBroadcastData(byte[] data)/*throws TimeoutException, XBeeException */{
			SendData(XBee64BitAddress.BROADCAST_ADDRESS, data);
		}

		/**
		 * Sends the given XBee packet and registers the given packet listener 
		 * (if not {@code null}) to be notified when the answers is received.
		 * 
		 * <p>This is a non-blocking operation. To wait for the answer use 
		 * {@code sendPacket(XBeePacket)}.</p>
		 * 
		 * @param packet XBee packet to be sent.
		 * @param packetReceiveListener Listener for the operation, {@code null} 
		 *                              not to be notified when the answer arrives.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code packet == null}.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #sendPacket(XBeePacket)
		 * @see #sendPacketAsync(XBeePacket)
		 * @see com.digi.xbee.api.listeners.IPacketReceiveListener
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		public void SendPacket(XBeePacket packet, IPacketReceiveListener packetReceiveListener)/*throws XBeeException */{
			try
			{
				SendXBeePacket(packet, packetReceiveListener);
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}
		}

		/**
		 * Sends the given XBee packet asynchronously.
		 * 
		 * <p>This is a non-blocking operation that do not wait for the answer and 
		 * is never notified when it arrives.</p>
		 * 
		 * <p>To be notified when the answer is received, use 
		 * {@link #sendXBeePacket(XBeePacket, IPacketReceiveListener)}.</p>
		 * 
		 * @param packet XBee packet to be sent asynchronously.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code packet == null}.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #sendXBeePacket(XBeePacket)
		 * @see #sendXBeePacket(XBeePacket, IPacketReceiveListener)
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		public void SendPacketAsync(XBeePacket packet)/*throws XBeeException */{
			try
			{
				base.SendXBeePacket(packet, null);
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}
		}

		/**
		 * Sends the given XBee packet synchronously and blocks until the response 
		 * is received or the configured receive timeout expires.
		 * 
		 * <p>The received timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * <p>Use {@code sendXBeePacketAsync(XBeePacket)} or 
		 * {@code #sendXBeePacket(XBeePacket, IPacketReceiveListener)} for 
		 * non-blocking operations.</p>
		 * 
		 * @param packet XBee packet to be sent.
		 * 
		 * @return An {@code XBeePacket} object containing the response of the sent
		 *         packet or {@code null} if there is no response.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code packet == null}.
		 * @throws TimeoutException if there is a timeout sending the XBee packet.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #getReceiveTimeout()
		 * @see #sendXBeePacket(XBeePacket, IPacketReceiveListener)
		 * @see #sendXBeePacketAsync(XBeePacket)
		 * @see #setReceiveTimeout(int)
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		public XBeePacket SendPacket(XBeePacket packet)/*throws TimeoutException, XBeeException */{
			try
			{
				return base.SendXBeePacket(packet);
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}
		}

		/**
		 * Waits until a Modem Status packet with a reset status, 
		 * {@code ModemStatusEvent.STATUS_HARDWARE_RESET} (0x00), or a watchdog 
		 * timer reset, {@code ModemStatusEvent.STATUS_WATCHDOG_TIMER_RESET} (0x01),
		 * is received or the timeout expires.
		 * 
		 * @return {@code true} if the Modem Status packet is received, 
		 *                      {@code false} otherwise.
		 * 
		 * @see com.digi.xbee.api.models.ModemStatusEvent#STATUS_HARDWARE_RESET
		 * @see com.digi.xbee.api.models.ModemStatusEvent#STATUS_WATCHDOG_TIMER_RESET
		 */
		private bool waitForModemResetStatusPacket()
		{
			modemStatusReceived = false;
			addModemStatusListener(resetStatusListener);
			lock (resetLock)
			{
				try
				{
					Monitor.Wait(resetLock, TIMEOUT_RESET);
				}
				catch (ThreadInterruptedException ) { }
			}
			removeModemStatusListener(resetStatusListener);
			return modemStatusReceived;
		}

		class CustomModemStatusReceiveListener : IModemStatusReceiveListener
		{
			XBeeDevice _device;

			public CustomModemStatusReceiveListener(XBeeDevice device)
			{
				_device = device;
			}

			public void modemStatusEventReceived(ModemStatusEvent modemStatusEvent)
			{
				if (modemStatusEvent == ModemStatusEvent.STATUS_HARDWARE_RESET
						|| modemStatusEvent == ModemStatusEvent.STATUS_WATCHDOG_TIMER_RESET)
				{
					_device.modemStatusReceived = true;
					// Continue execution by notifying the lock object.
					lock (_device.resetLock)
					{
						Monitor.Pulse(_device.resetLock);
					}
				}
			}
		}
		/**
		 * Custom listener for modem reset packets.
		 * 
		 * <p>When a Modem Status packet is received with status 
		 * {@code ModemStatusEvent.STATUS_HARDWARE_RESET} or 
		 * {@code ModemStatusEvent.STATUS_WATCHDOG_TIMER_RESET}, it 
		 * notifies the object that was waiting for the reception.</p>
		 * 
		 * @see com.digi.xbee.api.listeners.IModemStatusReceiveListener
		 * @see com.digi.xbee.api.models.ModemStatusEvent#STATUS_HARDWARE_RESET
		 * @see com.digi.xbee.api.models.ModemStatusEvent#STATUS_WATCHDOG_TIMER_RESET
		 */
		private IModemStatusReceiveListener resetStatusListener = new CustomModemStatusReceiveListener(this);

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#reset()
		 */
		//@Override
		public override void reset()/*throws TimeoutException, XBeeException */{
			// Check connection.
			if (!connectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			logger.Info(toString() + "Resetting the local module...");

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
			checkATCommandResponseIsValid(response);

			// Wait for a Modem Status packet.
			if (!waitForModemResetStatusPacket())
				throw new Kveer.XBeeApi.Exceptions.TimeoutException("Timeout waiting for the Modem Status packet.");

			logger.Info(toString() + "Module reset successfully.");
		}

		/**
		 * Reads new data received by this XBee device during the configured 
		 * received timeout.
		 * 
		 * <p>This method blocks until new data is received or the configured 
		 * receive timeout expires.</p>
		 * 
		 * <p>The received timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * <p>For non-blocking operations, register a {@code IDataReceiveListener} 
		 * using the method {@link #addDataListener(IDataReceiveListener)}.</p>
		 * 
		 * @return An {@code XBeeMessage} object containing the data and the source 
		 *         address of the remote node that sent the data. {@code null} if 
		 *         this did not receive new data during the configured receive 
		 *         timeout.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * 
		 * @see #readData(int)
		 * @see #getReceiveTimeout()
		 * @see #setReceiveTimeout(int)
		 * @see #readDataFrom(RemoteXBeeDevice)
		 * @see #readDataFrom(RemoteXBeeDevice, int)
		 * @see com.digi.xbee.api.models.XBeeMessage
		 */
		public XBeeMessage readData()
		{
			return readDataPacket(null, TIMEOUT_READ_PACKET);
		}

		/**
		 * Reads new data received by this XBee device during the provided timeout.
		 * 
		 * <p>This method blocks until new data is received or the provided timeout 
		 * expires.</p>
		 * 
		 * <p>For non-blocking operations, register a {@code IDataReceiveListener} 
		 * using the method {@link #addDataListener(IDataReceiveListener)}.</p>
		 * 
		 * @param timeout The time to wait for new data in milliseconds.
		 * 
		 * @return An {@code XBeeMessage} object containing the data and the source 
		 *         address of the remote node that sent the data. {@code null} if 
		 *         this device did not receive new data during {@code timeout} 
		 *         milliseconds.
		 * 
		 * @throws ArgumentException if {@code timeout < 0}.
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * 
		 * @see #readData()
		 * @see #readDataFrom(RemoteXBeeDevice)
		 * @see #readDataFrom(RemoteXBeeDevice, int)
		 * @see com.digi.xbee.api.models.XBeeMessage
		 */
		public XBeeMessage readData(int timeout)
		{
			if (timeout < 0)
				throw new ArgumentException("Read timeout must be 0 or greater.");

			return readDataPacket(null, timeout);
		}

		/**
		 * Reads new data received from the given remote XBee device during the 
		 * configured received timeout.
		 * 
		 * <p>This method blocks until new data from the provided remote XBee 
		 * device is received or the configured receive timeout expires.</p>
		 * 
		 * <p>The received timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * <p>For non-blocking operations, register a {@code IDataReceiveListener} 
		 * using the method {@link #addDataListener(IDataReceiveListener)}.</p>
		 * 
		 * @param remoteXBeeDevice The remote device to read data from.
		 * 
		 * @return An {@code XBeeMessage} object containing the data and the source
		 *         address of the remote node that sent the data. {@code null} if 
		 *         this device did not receive new data from the provided remote 
		 *         XBee device during the configured receive timeout.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code remoteXBeeDevice == null}.
		 * 
		 * @see #readDataFrom(RemoteXBeeDevice, int)
		 * @see #getReceiveTimeout()
		 * @see #setReceiveTimeout(int)
		 * @see #readData()
		 * @see #readData(int)
		 * @see RemoteXBeeDevice
		 * @see com.digi.xbee.api.models.XBeeMessage
		 */
		public XBeeMessage readDataFrom(RemoteXBeeDevice remoteXBeeDevice)
		{
			if (remoteXBeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null.");

			return readDataPacket(remoteXBeeDevice, TIMEOUT_READ_PACKET);
		}

		/**
		 * Reads new data received from the given remote XBee device during the 
		 * provided timeout.
		 * 
		 * <p>This method blocks until new data from the provided remote XBee 
		 * device is received or the given timeout expires.</p>
		 * 
		 * <p>For non-blocking operations, register a {@code IDataReceiveListener} 
		 * using the method {@link #addDataListener(IDataReceiveListener)}.</p>
		 * 
		 * @param remoteXBeeDevice The remote device to read data from.
		 * @param timeout The time to wait for new data in milliseconds.
		 * 
		 * @return An {@code XBeeMessage} object containing the data and the source
		 *         address of the remote node that sent the data. {@code null} if 
		 *         this device did not receive new data from the provided remote 
		 *         XBee device during {@code timeout} milliseconds.
		 * 
		 * @throws ArgumentException if {@code timeout < 0}.
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * @throws ArgumentNullException if {@code remoteXBeeDevice == null}.
		 * 
		 * @see #readDataFrom(RemoteXBeeDevice)
		 * @see #getReceiveTimeout()
		 * @see #setReceiveTimeout(int)
		 * @see #readData()
		 * @see #readData(int)
		 * @see RemoteXBeeDevice
		 * @see com.digi.xbee.api.models.XBeeMessage
		 */
		public XBeeMessage readDataFrom(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			if (remoteXBeeDevice == null)
				throw new ArgumentNullException("Remote XBee device cannot be null.");
			if (timeout < 0)
				throw new ArgumentException("Read timeout must be 0 or greater.");

			return readDataPacket(remoteXBeeDevice, timeout);
		}

		/**
		 * Reads a new data packet received by this XBee device during the provided 
		 * timeout.
		 * 
		 * <p>This method blocks until new data is received or the given timeout 
		 * expires.</p>
		 * 
		 * <p>If the provided remote XBee device is {@code null} the method returns 
		 * the first data packet read from any remote device.
		 * <br>
		 * If it the remote device is not {@code null} the method returns the first 
		 * data package read from the provided device.
		 * </p>
		 * 
		 * @param remoteXBeeDevice The remote device to get a data packet from. 
		 *                         {@code null} to read a data packet sent by any 
		 *                         remote XBee device.
		 * @param timeout The time to wait for a data packet in milliseconds.
		 * 
		 * @return An {@code XBeeMessage} received by this device, containing the 
		 *         data and the source address of the remote node that sent the 
		 *         data. {@code null} if this device did not receive new data 
		 *         during {@code timeout} milliseconds, or if any error occurs while
		 *         trying to get the source of the message.
		 * 
		 * @throws InterfaceNotOpenException if this device connection is not open.
		 * 
		 * @see RemoteXBeeDevice
		 * @see com.digi.xbee.api.models.XBeeMessage
		 */
		private XBeeMessage readDataPacket(RemoteXBeeDevice remoteXBeeDevice, int timeout)
		{
			// Check connection.
			if (!connectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			XBeePacketsQueue xbeePacketsQueue = dataReader.XBeePacketsQueue;
			XBeePacket xbeePacket = null;

			if (remoteXBeeDevice != null)
				xbeePacket = xbeePacketsQueue.getFirstDataPacketFrom(remoteXBeeDevice, timeout);
			else
				xbeePacket = xbeePacketsQueue.getFirstDataPacket(timeout);

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
					remoteDevice = GetNetwork().addRemoteDevice(remoteXBeeDevice);

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

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#toString()
		 */
		//@Override
		public String toString()
		{
			String id = getNodeID() == null ? "" : getNodeID();
			String addr64 = get64BitAddress() == null || get64BitAddress() == XBee64BitAddress.UNKNOWN_ADDRESS ?
					"" : get64BitAddress().ToString();

			if (id.Length == 0 && addr64.Length == 0)
				return base.ToString();

			StringBuilder message = new StringBuilder(base.ToString());
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
	}
}