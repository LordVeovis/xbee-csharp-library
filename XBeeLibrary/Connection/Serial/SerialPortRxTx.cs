using Common.Logging;
using Kveer.XBeeApi.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Kveer.XBeeApi.Connection.Serial
{
	/**
	 * This class represents a serial port using the RxTx library to communicate
	 * with it.
	 */
	public class SerialPortRxTx : AbstractSerialPort, SerialPortEventListener, CommPortOwnershipListener
	{

		// Variables.
		private object @lock = new object();

		private RXTXPort serialPort;

		private Stream inputStream;

		private Stream outputStream;

		private Thread breakThread;

		private bool breakEnabled = false;

		private CommPortIdentifier portIdentifier = null;

		private ILog logger;

		/**
		 * Class constructor. Instances a new {@code SerialPortRxTx} object using
		 * the given parameters.
		 * 
		 * @param port Serial port name to use.
		 * @param parameters Serial port parameters.
		 * 
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code parameters == null}.
		 * 
		 * @see #SerialPortRxTx(String, int)
		 * @see #SerialPortRxTx(String, int, int)
		 * @see #SerialPortRxTx(String, SerialPortParameters, int)
		 * @see SerialPortParameters
		 */
		public SerialPortRxTx(string port, SerialPortParameters parameters)
			: this(port, parameters, DEFAULT_PORT_TIMEOUT)
		{
		}

		/**
		 * Class constructor. Instances a new {@code SerialPortRxTx} object using
		 * the given parameters.
		 * 
		 * @param port Serial port name to use.
		 * @param parameters Serial port parameters.
		 * @param receiveTimeout Serial port receive timeout in milliseconds.
		 * 
		 * @throws ArgumentException if {@code receiveTimeout < 0}.
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code parameters == null}.
		 * 
		 * @see #SerialPortRxTx(String, int)
		 * @see #SerialPortRxTx(String, int, int)
		 * @see #SerialPortRxTx(String, SerialPortParameters)
		 * @see SerialPortParameters
		 */
		public SerialPortRxTx(string port, SerialPortParameters parameters, int receiveTimeout)
			: base(port, parameters, receiveTimeout)
		{
			this.logger = LogManager.GetLogger<SerialPortRxTx>();
		}

		/**
		 * Class constructor. Instances a new {@code SerialPortRxTx} object using
		 * the given parameters.
		 * 
		 * @param port Serial port name to use.
		 * @param baudRate Serial port baud rate, the rest of parameters will be 
		 *                 set by default.
		 * 
		 * @throws ArgumentNullException if {@code port == null}.
		 * 
		 * @see #DEFAULT_DATA_BITS
		 * @see #DEFAULT_FLOW_CONTROL
		 * @see #DEFAULT_PARITY
		 * @see #DEFAULT_STOP_BITS
		 * @see #DEFAULT_PORT_TIMEOUT
		 * @see #SerialPortRxTx(String, int, int)
		 * @see #SerialPortRxTx(String, SerialPortParameters)
		 * @see #SerialPortRxTx(String, SerialPortParameters, int)
		 * @see SerialPortParameters
		 */
		public SerialPortRxTx(String port, int baudRate)
			: this(port, baudRate, DEFAULT_PORT_TIMEOUT)
		{
		}

		/**
		 * Class constructor. Instances a new {@code SerialPortRxTx} object using
		 * the given parameters.
		 * 
		 * @param port Serial port name to use.
		 * @param baudRate Serial port baud rate, the rest of parameters will be 
		 *                 set by default.
		 * @param receiveTimeout Serial port receive timeout in milliseconds.
		 * 
		 * @throws ArgumentException if {@code receiveTimeout < 0}.
		 * @throws ArgumentNullException if {@code port == null}.
		 * 
		 * @see #DEFAULT_DATA_BITS
		 * @see #DEFAULT_FLOW_CONTROL
		 * @see #DEFAULT_PARITY
		 * @see #DEFAULT_STOP_BITS
		 * @see #SerialPortRxTx(String, int)
		 * @see #SerialPortRxTx(String, SerialPortParameters)
		 * @see #SerialPortRxTx(String, SerialPortParameters, int)
		 * @see SerialPortParameters
		 */
		public SerialPortRxTx(String port, int baudRate, int receiveTimeout)
			: base(port, baudRate, receiveTimeout)
		{
			this.logger = LogManager.GetLogger<SerialPortRxTx>();
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.IConnectionInterface#open()
		 */
		//@Override
		public void Open() /*throws InterfaceInUseException, InvalidInterfaceException, InvalidConfigurationException, PermissionDeniedException*/ {
			// Check that the given serial port exists.
			try
			{
				portIdentifier = CommPortIdentifier.getPortIdentifier(port);
			}
			catch (NoSuchPortException e)
			{
				throw new InvalidInterfaceException("No such port: " + port, e);
			}
			try
			{
				// Get the serial port.
				serialPort = (RXTXPort)portIdentifier.open(PORT_ALIAS + " " + port, receiveTimeout);
				// Set port as connected.
				connectionOpen = true;
				// Configure the port.
				if (parameters == null)
					parameters = new SerialPortParameters(baudRate, DEFAULT_DATA_BITS, DEFAULT_STOP_BITS, DEFAULT_PARITY, DEFAULT_FLOW_CONTROL);
				serialPort.setSerialPortParams(baudRate, parameters.DataBits, parameters.StopBits, parameters.Parity);
				serialPort.setFlowControlMode(parameters.FlowControl);

				serialPort.enableReceiveTimeout(receiveTimeout);

				// Set the port ownership.
				portIdentifier.addPortOwnershipListener(this);

				// Initialize input and output streams before setting the listener.
				inputStream = serialPort.getInputStream();
				outputStream = serialPort.getOutputStream();
				// Activate data received event.
				serialPort.notifyOnDataAvailable(true);
				// Register serial port event listener to be notified when data is available.
				serialPort.addEventListener(this);
			}
			catch (PortInUseException e)
			{
				throw new InterfaceInUseException("Port " + port + " is already in use by other application(s)", e);
			}
			catch (UnsupportedCommOperationException e)
			{
				throw new InvalidConfigurationException(e.getMessage(), e);
			}
			catch (TooManyListenersException e)
			{
				throw new InvalidConfigurationException(e.getMessage(), e);
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.IConnectionInterface#close()
		 */
		//@Override
		public void close()
		{
			try
			{
				if (inputStream != null)
				{
					inputStream.Close();
					inputStream = null;
				}
				if (outputStream != null)
				{
					outputStream.Close();
					outputStream = null;
				}
			}
			catch (IOException e)
			{
				logger.Error(e.Message, e);
			}
			lock (@lock)
			{
				if (serialPort != null)
				{
					try
					{
						serialPort.notifyOnDataAvailable(false);
						serialPort.removeEventListener();
						portIdentifier.removePortOwnershipListener(this);
						serialPort.close();
						serialPort = null;
						connectionOpen = false;
					}
					catch (Exception e) { }
				}
			}
		}

		/*
		 * (non-Javadoc)
		 * @see gnu.io.SerialPortEventListener#serialEvent(gnu.io.SerialPortEvent)
		 */
		//@Override
		public void serialEvent(SerialPortEvent @event)
		{
			// Listen only to data available event.
			switch (@event.getEventType())
			{
				case SerialPortEvent.DATA_AVAILABLE:
					// Check if serial device has been disconnected or not.
					try
					{
						getInputStream().available();
					}
					catch (Exception e)
					{
						// Serial device has been disconnected.
						close();
						lock (this)
						{
							this.notify();
						}
						break;
					}
					// Notify data is available by waking up the read thread.
					try
					{
						if (getInputStream().available() > 0)
						{
							lock (this)
							{
								this.notify();
							}
						}
					}
					catch (Exception e)
					{
						logger.Error(e.Message, e);
					}
					break;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see java.lang.Object#toString()
		 */
		//@Override
		public override string toString()
		{
			return base.ToString();
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#setBreak(bool)
		 */
		//@Override
		public void setBreak(bool enabled)
		{
			breakEnabled = enabled;
			if (breakEnabled)
			{
				if (breakThread == null)
				{
					breakThread = new Thread(new ThreadStart(() =>
					{
						while (breakEnabled && serialPort != null)
							serialPort.sendBreak(100);
					}));
					breakThread.Start();
				}
			}
			else
			{
				if (breakThread != null)
					breakThread.Interrupt();
				breakThread = null;
				serialPort.sendBreak(0);
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.IConnectionInterface#getInputStream()
		 */
		//@Override
		public Stream getInputStream()
		{
			return inputStream;
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.IConnectionInterface#getOutputStream()
		 */
		//@Override
		public Stream getOutputStream()
		{
			return outputStream;
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#setReadTimeout(int)
		 */
		//@Override
		public void SetReadTimeout(int timeout)
		{
			serialPort.disableReceiveTimeout();
			serialPort.enableReceiveTimeout(timeout);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#getReadTimeout()
		 */
		//@Override
		public int GetReadTimeout()
		{
			return serialPort.getReceiveTimeout();
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#setDTR(bool)
		 */
		//@Override
		public void SetDTR(bool state)
		{
			serialPort.setDTR(state);
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#setRTS(bool)
		 */
		//@Override
		public void SetRTS(bool state)
		{
			serialPort.setRTS(state);
		}


		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#setPortParameters(int, int, int, int, int)
		 */
		//@Override
		public void setPortParameters(int baudRate, int dataBits, int stopBits,
				int parity, int flowControl) /*throws InvalidConfigurationException, ConnectionException*/ {
					parameters = new SerialPortParameters(baudRate, dataBits, stopBits, parity, flowControl);

					if (serialPort != null)
					{
						try
						{
							serialPort.setSerialPortParams(baudRate, dataBits, stopBits, parity);
							serialPort.setFlowControlMode(flowControl);
						}
						catch (UnsupportedCommOperationException e)
						{
							throw new InvalidConfigurationException(e.getMessage(), e);
						}
					}
				}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#sendBreak(int)
		 */
		//@Override
		public void SendBreak(int duration)
		{
			if (serialPort != null)
				serialPort.sendBreak(duration);
		}

		/*
		 * (non-Javadoc)
		 * @see gnu.io.CommPortOwnershipListener#ownershipChange(int)
		 */
		//@Override
		public void OwnershipChange(int nType)
		{
			switch (nType)
			{
				case CommPortOwnershipListener.PORT_OWNERSHIP_REQUESTED:
					onSerialOwnershipRequested(null);
					break;
			}
		}

		/**
		 * Releases the port on any ownership request in the same application 
		 * instance.
		 * 
		 * @param data The port requester.
		 */
		private void onSerialOwnershipRequested(Object data) {
		try {
			throw new Exception();
		} catch (Exception e) {
			StackTraceElement[] elems = e.StackTrace();
			String requester = elems[elems.Length - 4].getClassName();
			lock (this) {
				this.notify();
			}
			close();
			String myPackage = this.getClass().getPackage().getName();
			if (requester.startsWith(myPackage))
				requester = "another AT connection";
			logger.warn("Connection for port {} canceled due to ownership request from {}.", port, requester);
		}
	}

		/**
		 * Retrieves the list of available serial ports in the system.
		 * 
		 * @return List of available serial ports.
		 * 
		 * @see #listSerialPortsInfo()
		 */
		public static string[] ListSerialPorts() {
		List<String> serialPorts = new List<String>();
		
		//@SuppressWarnings("unchecked")
		Enumeration<CommPortIdentifier> comPorts = CommPortIdentifier.getPortIdentifiers();
		if (comPorts == null)
			return serialPorts.toArray(new String[serialPorts.size()]);
		
		while (comPorts.hasMoreElements()) {
			CommPortIdentifier identifier = (CommPortIdentifier)comPorts.nextElement();
			if (identifier == null)
				continue;
			String strName = identifier.getName();
			serialPorts.add(strName);
		}
		return serialPorts.toArray(new String[serialPorts.size()]);
	}

		/**
		 * Retrieves the list of available serial ports with their information.
		 * 
		 * @return List of available serial ports with their information.
		 * 
		 * @see #listSerialPorts()
		 * @see SerialPortInfo
		 */
		public static IList<SerialPortInfo> listSerialPortsInfo() {
		List<SerialPortInfo> ports = new List<SerialPortInfo>();
		
		//@SuppressWarnings("unchecked")
		Enumeration<CommPortIdentifier> comPorts = CommPortIdentifier.getPortIdentifiers();
		if (comPorts == null)
			return ports;
		
		while (comPorts.hasMoreElements()) {
			CommPortIdentifier identifier = (CommPortIdentifier)comPorts.nextElement();
			if (identifier == null)
				continue;
			ports.add(new SerialPortInfo(identifier.getName()));
		}
		return ports;
	}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#isCTS()
		 */
		//@Override
		public bool IsCTS()
		{
			return serialPort.isCTS();
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#isDSR()
		 */
		//@Override
		public bool IsDSR()
		{
			return serialPort.isDSR();
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.connection.serial.AbstractSerialPort#isCD()
		 */
		//@Override
		public bool IsCD()
		{
			return serialPort.isCD();
		}
	}
}