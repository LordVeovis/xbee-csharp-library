using Kveer.XBeeApi.Connection;
using Kveer.XBeeApi.Connection.Serial;
using System;

namespace Kveer.XBeeApi
{
	/// <summary>
	/// Helper class used to create a serial port connection interface.
	/// </summary>
	public class XBee
	{
		/**
		 * Retrieves a serial port connection interface for the provided port with 
		 * the given baud rate.
		 * 
		 * @param port Serial port name.
		 * @param baudRate Serial port baud rate.
		 * 
		 * @return The serial port connection interface.
		 * 
		 * @throws ArgumentNullException if {@code port == null}.
		 * 
		 * @see #createConnectiontionInterface(String, SerialPortParameters)
		 * @see com.digi.xbee.api.connection.IConnectionInterface
		 */
		public static IConnectionInterface CreateConnectiontionInterface(string port, int baudRate)
		{
			IConnectionInterface connectionInterface = new MySerialPort(port, baudRate);
			return connectionInterface;
		}

		/**
		 * Retrieves a serial port connection interface for the provided port with 
		 * the given serial port parameters.
		 * 
		 * @param port Serial port name.
		 * @param serialPortParameters Serial port parameters.
		 * 
		 * @return The serial port connection interface.
		 * 
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code serialPortParameters == null}.
		 * 
		 * @see #createConnectiontionInterface(String, int)
		 * @see com.digi.xbee.api.connection.IConnectionInterface
		 * @see com.digi.xbee.api.connection.serial.SerialPortParameters
		 */
		public static IConnectionInterface CreateConnectiontionInterface(string port, SerialPortParameters serialPortParameters)
		{
			IConnectionInterface connectionInterface = new MySerialPort(port, serialPortParameters);
			return connectionInterface;
		}
	}
}