using Common.Logging;
using Kveer.XBeeApi.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Connection.Serial
{
	public class NetSerialPort : AbstractSerialPort
	{
		SerialPort _serialPort;
		ILog _logger;

		public NetSerialPort(string port, int baudRate)
			: this(port, baudRate, DEFAULT_PORT_TIMEOUT)
		{
		}

		public NetSerialPort(string port, int baudRate, int receiveTimeout)
			: base(port, baudRate, receiveTimeout)
		{
			_logger = LogManager.GetLogger<NetSerialPort>();
		}

		public NetSerialPort(string port, SerialPortParameters parameters)
			: this(port, parameters, DEFAULT_PORT_TIMEOUT)
		{
		}

		public NetSerialPort(string port, SerialPortParameters parameters, int receiveTimeout)
			: base(port, parameters, receiveTimeout)
		{
		}

		public void Open()
		{
			if (_serialPort == null)
			{
				try
				{
					_serialPort = new SerialPort(this.port, this.baudRate);
				}
				catch (IOException ex)
				{
					throw new InvalidInterfaceException("No such port: " + port, ex);
				}
			}

			try
			{
				_serialPort.Open();

				if (parameters == null)
					parameters = new SerialPortParameters(baudRate, DEFAULT_DATA_BITS, DEFAULT_STOP_BITS, DEFAULT_PARITY, DEFAULT_FLOW_CONTROL);

				_serialPort.BaudRate = baudRate;
				_serialPort.DataBits = parameters.DataBits;
				_serialPort.StopBits = parameters.StopBits;
				_serialPort.Parity = parameters.Parity;

				_serialPort.Handshake = parameters.FlowControl;

				_serialPort.ReadTimeout = receiveTimeout;

				// Register serial port event listener to be notified when data is available.
				//serialPort.addEventListener(this);
				_serialPort.DataReceived += _serialPort_DataReceived;
			}
			catch (InvalidOperationException ex)
			{
				throw new InterfaceInUseException(string.Format("Port {0} is already in use by other application(s)", port), ex);
			}
		}

		void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				if (_serialPort.BytesToRead > 0)
				{
					Monitor.Pulse(this);
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message, ex);
			}
		}

		public void Close()
		{
			lock (_serialPort)
			{
				if (_serialPort != null)
				{
					try
					{
						_serialPort.Close();
					}
					catch (Exception) { }
				}
			}
		}

		public bool IsOpen
		{
			get { return _serialPort != null && _serialPort.IsOpen; }
		}

		public Stream GetInputStream()
		{
			return _serialPort.BaseStream;
		}

		public Stream GetOutputStream()
		{
			return _serialPort.BaseStream;
		}

		public void WriteData(byte[] data)
		{
			_serialPort.Write(data, 0, data.Length);
		}

		public void WriteData(byte[] data, int offset, int length)
		{
			_serialPort.Write(data, offset, length);
		}

		public int ReadData(byte[] data)
		{
			return _serialPort.Read(data, 0, data.Length);
		}

		public int ReadData(byte[] data, int offset, int length)
		{
			return _serialPort.Read(data, offset, length);
		}
	}
}
