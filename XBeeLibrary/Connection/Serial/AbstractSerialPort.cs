using Common.Logging;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace Kveer.XBeeApi.Connection.Serial
{
	/// <summary>
	/// Abstract class that provides common functionality to work with serial ports.
	/// </summary>
	public abstract class AbstractSerialPort : IConnectionInterface
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

		protected bool connectionOpen = false;

		private ILog logger;

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
		protected AbstractSerialPort(String port, SerialPortParameters parameters)
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
		protected AbstractSerialPort(String port, int baudRate)
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
		protected AbstractSerialPort(String port, int baudRate, int receiveTimeout)
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
		protected AbstractSerialPort(String port, SerialPortParameters parameters, int receiveTimeout)
		{
			Contract.Requires<ArgumentNullException>(port != null, "Serial port cannot be null");
			Contract.Requires<ArgumentNullException>(parameters != null, "SerialPortParameters cannot be null");
			Contract.Requires<ArgumentOutOfRangeException>(receiveTimeout >= 0, "Receive timeout cannot be less than 0");

			this.port = port;
			this.baudRate = parameters.BaudRate;
			this.receiveTimeout = receiveTimeout;
			this.parameters = parameters;
			this.logger = LogManager.GetLogger<AbstractSerialPort>();
		}

		public bool IsOpen
		{
			get
			{
				return connectionOpen;
			}
		}


		/**
		 * Sets the state of the DTR.
		 * 
		 * @param state {@code true} to set the line status high, {@code false} to 
		 *              set it low.
		 * 
		 * @see #isCD()
		 * @see #isCTS()
		 * @see #isDSR()
		 * @see #setRTS(bool)
		 */
		public abstract void SetDTR(bool state);

		/**
		 * Sets the state of the RTS line.
		 * 
		 * @param state {@code true} to set the line status high, {@code false} to 
		 *              set it low.
		 * 
		 * @see #isCD()
		 * @see #isCTS()
		 * @see #isDSR()
		 * @see #setDTR(bool)
		 */
		public abstract void SetRTS(bool state);

		/**
		 * Returns the state of the CTS line.
		 * 
		 * @return {@code true} if the line is high, {@code false} otherwise.
		 * 
		 * @see #isCD()
		 * @see #isDSR()
		 * @see #setDTR(bool)
		 * @see #setRTS(bool)
		 */
		public abstract bool IsCTS { get; }

		/**
		 * Returns the state of the DSR line.
		 * 
		 * @return {@code true} if the line is high, {@code false} otherwise.
		 * 
		 * @see #isCD()
		 * @see #isCTS()
		 * @see #setDTR(bool)
		 * @see #setRTS(bool)
		 */
		public abstract bool IsDSR { get; }

		/**
		 * Returns the state of the CD line.
		 * 
		 * @return {@code true} if the line is high, {@code false} otherwise.
		 * 
		 * @see #isCTS()
		 * @see #isDSR()
		 * @see #setDTR(bool)
		 * @see #setRTS(bool)
		 */
		public abstract bool IsCD { get; }

		/**
		 * Returns whether or not the port's flow control is configured in 
		 * hardware mode.
		 *  
		 * @return {@code true} if the flow control is hardware, {@code false} 
		 *         otherwise.
		 * 
		 * @see #getPortParameters()
		 * @see #setPortParameters(SerialPortParameters)
		 * @see #setPortParameters(int, int, int, int, int)
		 */
		public bool IsHardwareFlowControl
		{
			get
			{
				return parameters.FlowControl == Handshake.RequestToSend;
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
			if (IsOpen)
			{
				Close();
				Open();
			}
		}

		/**
		 * Enables or disables the break line.
		 * 
		 * @param enabled {@code true} to enable the Break line, {@code false} to 
		 *                disable it.
		 * 
		 * @see #sendBreak(int)
		 */
		public abstract void SetBreak(bool enabled);

		/**
		 * Sends a break signal to the serial port with the given duration
		 * (in milliseconds).
		 * 
		 * @param duration Duration of the break signal in milliseconds.
		 * 
		 * @see #setBreak(bool)
		 */
		public abstract void SendBreak(int duration);

		/// <summary>
		/// Gets or sets the read timeout of the serial port (in milliseconds).
		/// </summary>
		public abstract int ReadTimeout { get; set; }

		/**
		 * Purges the serial port removing all the data from the input stream.
		 * 
		 * @see #flush()
		 */
		public void Purge()
		{
			if (GetInputStream() != null)
			{
				try
				{
					byte[] availableBytes = new byte[GetInputStream().available()];
					if (GetInputStream().available() > 0)
						GetInputStream().Read(availableBytes, 0, GetInputStream().available());
				}
				catch (IOException e)
				{
					logger.Error(e.Message, e);
				}
			}
		}

		/**
		 * Flushes the available data of the output stream.
		 * 
		 * @see #purge()
		 */
		public void Flush()
		{
			if (GetOutputStream() != null)
			{
				try
				{
					GetOutputStream().Flush();
				}
				catch (IOException e)
				{
					logger.Error(e.Message, e);
				}
			}
		}

		public void WriteData(byte[] data) /*throws IOException*/ {
			if (data == null)
				throw new ArgumentNullException("Data to be sent cannot be null.");

			if (GetOutputStream() != null)
			{
				// Writing data in ports without any device connected and configured with 
				// hardware flow-control causes the majority of serial libraries to hang.

				// Before writing any data, check if the port is configured with hardware 
				// flow-control and, if so, try to write the data up to 3 times verifying 
				// that the CTS line is high (there is a device connected to the other side 
				// ready to receive data).
				if (IsHardwareFlowControl)
				{
					int tries = 0;
					while (tries < 3 && !IsCTS)
					{
						try
						{
							Thread.Sleep(100);
						}
						catch (ThreadInterruptedException) { }
						tries += 1;
					}
					if (IsCTS)
					{
						GetOutputStream().Write(data, 0, data.Length);
						GetOutputStream().Flush();
					}
				}
				else
				{
					GetOutputStream().Write(data, 0, data.Length);
					GetOutputStream().Flush();
				}
			}
		}

		public void WriteData(byte[] data, int offset, int Length) /*throws IOException*/ {
			if (data == null)
				throw new ArgumentNullException("Data to be sent cannot be null.");
			if (offset < 0)
				throw new ArgumentOutOfRangeException("Offset cannot be less than 0.");
			if (Length < 1)
				throw new ArgumentOutOfRangeException("Length cannot be less than 0.");
			if (offset >= data.Length)
				throw new ArgumentOutOfRangeException("Offset must be less than the data Length.");
			if (offset + Length > data.Length)
				throw new ArgumentOutOfRangeException("Offset + Length cannot be great than the data Length.");

			if (GetOutputStream() != null)
			{
				// Writing data in ports without any device connected and configured with 
				// hardware flow-control causes the majority of serial libraries to hang.

				// Before writing any data, check if the port is configured with hardware 
				// flow-control and, if so, try to write the data up to 3 times verifying 
				// that the CTS line is high (there is a device connected to the other side 
				// ready to receive data).
				if (IsHardwareFlowControl)
				{
					int tries = 0;
					while (tries < 3 && !IsCTS)
					{
						try
						{
							Thread.Sleep(100);
						}
						catch (ThreadInterruptedException) { }
						tries += 1;
					}
					if (IsCTS)
					{
						GetOutputStream().Write(data, offset, Length);
						GetOutputStream().Flush();
					}
				}
				else
				{
					GetOutputStream().Write(data, offset, Length);
					GetOutputStream().Flush();
				}
			}
		}

		public int ReadData(byte[] data) /*throws IOException*/ {
			if (data == null)
				throw new ArgumentNullException("Buffer cannot be null.");

			int readBytes = 0;
			if (GetInputStream() != null)
				readBytes = GetInputStream().Read(data, 0, data.Length);
			return readBytes;
		}

		public int ReadData(byte[] data, int offset, int Length) /*throws IOException */{
			if (data == null)
				throw new ArgumentNullException("Buffer cannot be null.");
			if (offset < 0)
				throw new ArgumentOutOfRangeException("Offset cannot be less than 0.");
			if (Length < 1)
				throw new ArgumentOutOfRangeException("Length cannot be less than 0.");
			if (offset >= data.Length)
				throw new ArgumentOutOfRangeException("Offset must be less than the buffer Length.");
			if (offset + Length > data.Length)
				throw new ArgumentOutOfRangeException("Offset + Length cannot be great than the buffer Length.");

			int readBytes = 0;
			if (GetInputStream() != null)
				readBytes = GetInputStream().Read(data, offset, Length);
			return readBytes;
		}

		/**
		 * Returns the XBee serial port parameters.
		 * 
		 * @return The XBee serial port parameters.
		 * 
		 * @see #setPortParameters(SerialPortParameters)
		 * @see #setPortParameters(int, int, int, int, int)
		 * @see SerialPortParameters
		 */
		public SerialPortParameters GetPortParameters()
		{
			if (parameters != null)
				return parameters;
			return new SerialPortParameters(baudRate, DEFAULT_DATA_BITS,
					DEFAULT_STOP_BITS, DEFAULT_PARITY, DEFAULT_FLOW_CONTROL);
		}

		/**
		 * Returns the serial port receive timeout (in milliseconds).
		 * 
		 * @return The serial port receive timeout in milliseconds.
		 * 
		 * @see #getReadTimeout()
		 * @see #setReadTimeout(int)
		 */
		public int GetReceiveTimeout()
		{
			return receiveTimeout;
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
				if (parameters.FlowControl ==  Handshake.RequestToSend)
					flowControl = "H";
				else if (parameters.FlowControl ==  Handshake.XOnXOff)
					flowControl = "S";
				return "[" + port + " - " + baudRate + "/" + parameters.DataBits +
						"/" + parity + "/" + parameters.StopBits + "/" + flowControl + "] ";
			}
			else
				return "[" + port + " - " + baudRate + "/8/N/1/N] ";
		}

		public abstract void Open();

		public abstract void Close();

		public abstract Stream GetInputStream();

		public abstract Stream GetOutputStream();
	}
}