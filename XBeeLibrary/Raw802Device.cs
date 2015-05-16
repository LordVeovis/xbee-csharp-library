using Kveer.XBeeApi.Connection;
using Kveer.XBeeApi.Connection.Serial;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Packet;
using Kveer.XBeeApi.Packet.Raw;
using Kveer.XBeeApi.Utils;
using System;
using System.IO.Ports;
namespace Kveer.XBeeApi
{
	/**
	 * This class represents a local 802.15.4 device.
	 * 
	 * @see XBeeDevice
	 * @see DigiMeshDevice
	 * @see DigiPointDevice
	 * @see ZigBeeDevice
	 */
	public class Raw802Device : XBeeDevice
	{

		/**
		 * Class constructor. Instantiates a new {@code Raw802Device} object in the 
		 * given port name and baud rate.
		 * 
		 * @param port Serial port name where 802.15.4 device is attached to.
		 * @param baudRate Serial port baud rate to communicate with the device. 
		 *                 Other connection parameters will be set as default (8 
		 *                 data bits, 1 stop bit, no parity, no flow control).
		 * 
		 * @throws ArgumentException if {@code baudRate < 0}.
		 * @throws ArgumentNullException if {@code port == null}.
		 */
		public Raw802Device(string port, int baudRate)
			: this(XBee.createConnectiontionInterface(port, baudRate))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code Raw802Device} object in the 
		 * given serial port name and settings.
		 * 
		 * @param port Serial port name where 802.15.4 device is attached to.
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
		 */
		public Raw802Device(string port, int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl)
			: this(port, new SerialPortParameters(baudRate, dataBits, stopBits, parity, flowControl))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code Raw802Device} object in the 
		 * given serial port name and parameters.
		 * 
		 * @param port Serial port name where 802.15.4 device is attached to.
		 * @param serialPortParameters Object containing the serial port parameters.
		 * 
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code serialPortParameters == null}.
		 * 
		 * @see SerialPortParameters
		 */
		public Raw802Device(String port, SerialPortParameters serialPortParameters)
			: this(XBee.createConnectiontionInterface(port, serialPortParameters))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code Raw802Device} object with the 
		 * given connection interface.
		 * 
		 * @param connectionInterface The connection interface with the physical 
		 *                            802.15.4 device.
		 * 
		 * @throws ArgumentNullException if {@code connectionInterface == null}
		 * 
		 * @see IConnectionInterface
		 */
		public Raw802Device(IConnectionInterface connectionInterface)
			: base(connectionInterface)
		{
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.XBeeDevice#open()
		 */
		//@Override
		public override void Open()/*throws XBeeException */{
			base.Open();

			if (IsRemote)
				return;
			if (xbeeProtocol != XBeeProtocol.RAW_802_15_4)
				throw new XBeeDeviceException("XBee device is not a " + getXBeeProtocol().GetDescription() + " device, it is a " + xbeeProtocol.GetDescription() + " device.");
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.XBeeDevice#GetNetwork()
		 */
		//@Override
		public override XBeeNetwork GetNetwork()
		{
			if (!IsOpen)
				throw new InterfaceNotOpenException();

			if (network == null)
				network = new Raw802Network(this);
			return network;
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.XBeeDevice#getXBeeProtocol()
		 */
		//@Override
		public override XBeeProtocol getXBeeProtocol()
		{
			return XBeeProtocol.RAW_802_15_4;
		}

		/**
		 * Sends the provided data to the XBee device of the network corresponding 
		 * to the given 16-bit address asynchronously.
		 * 
		 * <p>Asynchronous transmissions do not wait for answer from the remote 
		 * device or for transmit status packet</p>
		 * 
		 * @param address The 16-bit address of the XBee that will receive the data.
		 * @param data Byte array containing data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if the device is not open.
		 * @throws ArgumentNullException if {@code address == null} or 
		 *                              if {@code data == null}.
		 * @throws XBeeException if there is any XBee related exception.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see #sendData(RemoteXBeeDevice, byte[])
		 * @see #sendData(XBee16BitAddress, byte[])
		 * @see #sendData(XBee64BitAddress, byte[])
		 * @see #sendDataAsync(RemoteXBeeDevice, byte[])
		 * @see #sendDataAsync(XBee64BitAddress, byte[])
		 */
		public void SendDataAsync(XBee16BitAddress address, byte[] data)/*throws XBeeException */{
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

			logger.InfoFormat(toString() + "Sending data asynchronously to {0} >> {1}.", address, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new TX16Packet(getNextFrameID(), address, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, true);
		}

		/**
		 * Sends the provided data to the XBee device of the network corresponding 
		 * to the given 16-bit address.
		 * 
		 * <p>This method blocks until a success or error response arrives or the 
		 * configured receive timeout expires.</p>
		 * 
		 * <p>The received timeout is configured using the {@code setReceiveTimeout}
		 * method and can be consulted with {@code getReceiveTimeout} method.</p>
		 * 
		 * <p>For non-blocking operations use the method 
		 * {@link #sendData(XBee16BitAddress, byte[])}.</p>
		 * 
		 * @param address The 16-bit address of the XBee that will receive the data.
		 * @param data Byte array containing data to be sent.
		 * 
		 * @throws InterfaceNotOpenException if the device is not open.
		 * @throws ArgumentNullException if {@code address == null} or 
		 *                              if {@code data == null}.
		 * @throws TimeoutException if there is a timeout sending the data.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see XBeeDevice#getReceiveTimeout()
		 * @see XBeeDevice#setReceiveTimeout(int)
		 * @see #sendData(RemoteXBeeDevice, byte[])
		 * @see #sendData(XBee64BitAddress, byte[])
		 * @see #sendDataAsync(RemoteXBeeDevice, byte[])
		 * @see #sendDataAsync(XBee16BitAddress, byte[])
		 * @see #sendDataAsync(XBee64BitAddress, byte[])
		 */
		public void SendData(XBee16BitAddress address, byte[] data)/*throws TimeoutException, XBeeException */{
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

			logger.InfoFormat(toString() + "Sending data to {0} >> {1}.", address, HexUtils.PrettyHexString(data));

			XBeePacket xbeePacket = new TX16Packet(getNextFrameID(), address, (byte)XBeeTransmitOptions.NONE, data);
			SendAndCheckXBeePacket(xbeePacket, false);
		}
	}
}