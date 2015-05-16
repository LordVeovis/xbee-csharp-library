using Common.Logging;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace Kveer.XBeeApi.Connection.Serial
{
	/// <summary>
	/// Class that provides common functionality to work with serial ports.
	/// </summary>
	public class MySerialPort : IConnectionInterface
	{
		// Constants.
		/**
		 * Default receive timeout: {@value} seconds.
		 * 
		 * <p>When the specified number of milliseconds have elapsed, read will 
		 * return immediately.</p>
		 */
		public const int DEFAULT_PORT_TIMEOUT = 10;

		/**
		 * Default number of data bits: {@value}.
		 */
		public const int DEFAULT_DATA_BITS = 8;

		/**
		 * Default number of stop bits: {@value}.
		 */
		public const StopBits DEFAULT_STOP_BITS = StopBits.One;

		/**
		 * Default parity: {@value} (None).
		 */
		public const Parity DEFAULT_PARITY = Parity.None;

		/**
		 * Default flow control: {@value} (None).
		 */
		public const Handshake DEFAULT_FLOW_CONTROL = Handshake.None;

		protected const int FLOW_CONTROL_HW = 3;

		protected const string PORT_ALIAS = "Serial Port";

		/// <summary>
		/// Gets the name of the serial port.
		/// </summary>
		public string port { get; protected set; }

		protected int baudRate;
		protected int receiveTimeout;

		protected SerialPortParameters parameters;

		private ILog _logger;

		/**
		 * Class constructor. Instantiates a new {@code AbstractSerialPort} object
		 * with the given parameters.
		 * 
		 * @param port COM port name to use.
		 * @param parameters Serial port connection parameters.
		 * 
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code parameters == null}.
		 * 
		 * @see #AbstractSerialPort(String, int)
		 * @see #AbstractSerialPort(String, int, int)
		 * @see #AbstractSerialPort(String, SerialPortParameters, int)
		 * @see SerialPortParameters
		 */
		public MySerialPort(String port, SerialPortParameters parameters)
			: this(port, parameters, DEFAULT_PORT_TIMEOUT)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code AbstractSerialPort} object 
		 * with the given parameters.
		 * 
		 * @param port COM port name to use.
		 * @param baudRate Serial connection baud rate, the rest of parameters will 
		 *                 be set by default.
		 * 
		 * @throws ArgumentNullException if {@code port == null}.
		 * 
		 * @see #DEFAULT_DATA_BITS
		 * @see #DEFAULT_FLOW_CONTROL
		 * @see #DEFAULT_PARITY
		 * @see #DEFAULT_STOP_BITS
		 * @see #DEFAULT_PORT_TIMEOUT
		 * @see #AbstractSerialPort(String, int, int)
		 * @see #AbstractSerialPort(String, SerialPortParameters)
		 * @see #AbstractSerialPort(String, SerialPortParameters, int)
		 */
		public MySerialPort(String port, int baudRate)
			: this(port, new SerialPortParameters(baudRate, DEFAULT_DATA_BITS, DEFAULT_STOP_BITS, DEFAULT_PARITY, DEFAULT_FLOW_CONTROL), DEFAULT_PORT_TIMEOUT)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code AbstractSerialPort} object
		 * with the given parameters.
		 * 
		 * @param port COM port name to use.
		 * @param baudRate Serial port baud rate, the rest of parameters will be 
		 *        set by default.
		 * @param receiveTimeout Receive timeout in milliseconds.
		 * 
		 * @throws ArgumentException if {@code receiveTimeout < 0}.
		 * @throws ArgumentNullException if {@code port == null}.
		 * 
		 * @see DEFAULT_DATA_BITS
		 * @see DEFAULT_FLOW_CONTROL
		 * @see DEFAULT_PARITY
		 * @see DEFAULT_STOP_BITS
		 * @see #AbstractSerialPort(String, int)
		 * @see #AbstractSerialPort(String, SerialPortParameters)
		 * @see #AbstractSerialPort(String, SerialPortParameters, int)
		 */
		public MySerialPort(String port, int baudRate, int receiveTimeout)
			: this(port, new SerialPortParameters(baudRate, DEFAULT_DATA_BITS, DEFAULT_STOP_BITS, DEFAULT_PARITY, DEFAULT_FLOW_CONTROL), receiveTimeout)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code AbstractSerialPort} object
		 * with the given parameters.
		 * 
		 * @param port COM port name to use.
		 * @param parameters Serial port connection parameters.
		 * @param receiveTimeout Serial connection receive timeout in milliseconds.
		 * 
		 * @throws ArgumentException if {@code receiveTimeout < 0}.
		 * @throws ArgumentNullException if {@code port == null} or
		 *                              if {@code parameters == null}.
		 *
		 * @see #AbstractSerialPort(String, int)
		 * @see #AbstractSerialPort(String, int, int)
		 * @see #AbstractSerialPort(String, SerialPortParameters)
		 * @see SerialPortParameters
		 */
		public MySerialPort(String port, SerialPortParameters parameters, int receiveTimeout)
		{
			Contract.Requires<ArgumentNullException>(port != null, "Serial port cannot be null");
			Contract.Requires<ArgumentNullException>(parameters != null, "SerialPortParameters cannot be null");
			Contract.Requires<ArgumentOutOfRangeException>(receiveTimeout >= 0, "Receive timeout cannot be less than 0");

			this.port = port;
			this.baudRate = parameters.BaudRate;
			this.receiveTimeout = receiveTimeout;
			this.parameters = parameters;
			this._logger = LogManager.GetLogger<MySerialPort>();

			SerialPort = new SerialPort(port, baudRate);
			SerialPort.DataBits = parameters.DataBits;
			SerialPort.StopBits = parameters.StopBits;
			SerialPort.Parity = parameters.Parity;

			SerialPort.Handshake = parameters.FlowControl;

			SerialPort.ReadTimeout = receiveTimeout;
			SerialPort.DataReceived += SerialPort_DataReceived;
		}

		void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				if (SerialPort.BytesToRead > 0)
				{
					Monitor.Pulse(this);
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message, ex);
			}
		}

		/**
		 * Sets the new parameters of the serial port.
		 * 
		 * @param baudRate The new value of baud rate.
		 * @param dataBits The new value of data bits.
		 * @param stopBits The new value of stop bits.
		 * @param parity The new value of parity.
		 * @param flowControl The new value of flow control.
		 * 
		 * @throws ConnectionException if any error occurs when setting the serial 
		 *                             port parameters
		 * @throws ArgumentException if {@code baudRate < 0} or
		 *                                  if {@code dataBits < 0} or
		 *                                  if {@code stopBits < 0} or
		 *                                  if {@code parity < 0} or
		 *                                  if {@code flowControl < 0}.
		 * @throws InvalidConfigurationException if the configuration is invalid.
		 * 
		 * @see #getPortParameters()
		 * @see #setPortParameters(SerialPortParameters)
		 */
		public void SetPortParameters(int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl) /*throws InvalidConfigurationException, ConnectionException*/ {
			SerialPortParameters parameters = new SerialPortParameters(baudRate, dataBits, stopBits, parity, flowControl);
			SetPortParameters(parameters);
		}

		/**
		 * Sets the new parameters of the serial port as 
		 * {@code SerialPortParameters}.
		 * 
		 * @param parameters The new serial port parameters.
		 * 
		 * @throws ConnectionException if any error occurs when setting the serial 
		 *                             port parameters.
		 * @throws InvalidConfigurationException if the configuration is invalid.
		 * @throws ArgumentNullException if {@code parameters == null}.
		 * 
		 * @see #getPortParameters()
		 * @see #setPortParameters(int, int, int, int, int)
		 * @see SerialPortParameters
		 */
		public void SetPortParameters(SerialPortParameters parameters) /*throws InvalidConfigurationException, ConnectionException*/ {
			Contract.Requires<ArgumentNullException>(parameters != null, "Serial port parameters cannot be null.");

			baudRate = parameters.BaudRate;
			this.parameters = parameters;
			if (SerialPort.IsOpen)
			{
				SerialPort.Close();
				SerialPort.Open();
			}
		}

		public override string ToString()
		{
			if (parameters != null)
			{
				String parity = "N";
				String flowControl = "N";
				if (parameters.Parity == Parity.Odd)
					parity = "O";
				else if (parameters.Parity == Parity.Even)
					parity = "E";
				else if (parameters.Parity == Parity.Mark)
					parity = "M";
				else if (parameters.Parity == Parity.Space)
					parity = "S";
				if (parameters.FlowControl == Handshake.RequestToSend)
					flowControl = "H";
				else if (parameters.FlowControl == Handshake.XOnXOff)
					flowControl = "S";
				return "[" + port + " - " + baudRate + "/" + parameters.DataBits +
						"/" + parity + "/" + parameters.StopBits + "/" + flowControl + "] ";
			}
			else
				return "[" + port + " - " + baudRate + "/8/N/1/N] ";
		}

		public SerialPort SerialPort { get; protected set; }
	}
}