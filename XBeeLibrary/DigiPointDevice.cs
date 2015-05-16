using Kveer.XBeeApi.Connection;
using Kveer.XBeeApi.Connection.Serial;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.Models;
using System;
using System.IO.Ports;
namespace Kveer.XBeeApi
{
	/**
	 * This class represents a local DigiPoint device.
	 * 
	 * @see XBeeDevice
	 * @see DigiMeshDevice
	 * @see Raw802Device
	 * @see ZigBeeDevice
	 */
	public class DigiPointDevice : XBeeDevice
	{

		/**
		 * Class constructor. Instantiates a new {@code DigiPointDevice} object in the 
		 * given port name and baud rate.
		 * 
		 * @param port Serial port name where point-to-multipoint device is attached to.
		 * @param baudRate Serial port baud rate to communicate with the device. 
		 *                 Other connection parameters will be set as default (8 
		 *                 data bits, 1 stop bit, no parity, no flow control).
		 * 
		 * @throws ArgumentException if {@code baudRate < 0}.
		 * @throws ArgumentNullException if {@code port == null}.
		 */
		public DigiPointDevice(String port, int baudRate)
			: this(XBee.CreateConnectiontionInterface(port, baudRate))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code DigiPointDevice} object in the 
		 * given serial port name and settings.
		 * 
		 * @param port Serial port name where point-to-multipoint device is attached to.
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
		public DigiPointDevice(String port, int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl)
			: this(port, new SerialPortParameters(baudRate, dataBits, stopBits, parity, flowControl))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code DigiPointDevice} object in the 
		 * given serial port name and parameters.
		 * 
		 * @param port Serial port name where point-to-multipoint device is attached to.
		 * @param serialPortParameters Object containing the serial port parameters.
		 * 
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code serialPortParameters == null}.
		 * 
		 * @see SerialPortParameters
		 */
		public DigiPointDevice(String port, SerialPortParameters serialPortParameters)
			: this(XBee.CreateConnectiontionInterface(port, serialPortParameters))
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code DigiPointDevice} object with the 
		 * given connection interface.
		 * 
		 * @param connectionInterface The connection interface with the physical 
		 *                            point-to-multipoint device.
		 * 
		 * @throws ArgumentNullException if {@code connectionInterface == null}
		 * 
		 * @see IConnectionInterface
		 */
		public DigiPointDevice(IConnectionInterface connectionInterface)
			: base(connectionInterface)
		{
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.XBeeDevice#open()
		 */
		//@Override
		public void open()/*throws XBeeException */{
			base.Open();
			if (IsRemote)
				return;
			if (xbeeProtocol != XBeeProtocol.DIGI_POINT)
				throw new XBeeDeviceException("XBee device is not a " + GetXBeeProtocol().GetDescription() + " device, it is a " + xbeeProtocol.GetDescription() + " device.");
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
				network = new DigiPointNetwork(this);
			return network;
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.XBeeDevice#getXBeeProtocol()
		 */
		//@Override
		public override XBeeProtocol GetXBeeProtocol()
		{
			return XBeeProtocol.DIGI_POINT;
		}
	}
}