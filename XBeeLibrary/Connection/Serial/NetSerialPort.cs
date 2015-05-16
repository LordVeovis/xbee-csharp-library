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

		public override void Open()
		{
			if (SerialPort == null)
			{
				try
				{
					SerialPort = new SerialPort(this.port, this.baudRate);
				}
				catch (IOException ex)
				{
					throw new InvalidInterfaceException("No such port: " + port, ex);
				}
			}

			try
			{
				SerialPort.Open();

				if (parameters == null)
					parameters = new SerialPortParameters(baudRate, DEFAULT_DATA_BITS, DEFAULT_STOP_BITS, DEFAULT_PARITY, DEFAULT_FLOW_CONTROL);

				SerialPort.BaudRate = baudRate;
				SerialPort.DataBits = parameters.DataBits;
				SerialPort.StopBits = parameters.StopBits;
				SerialPort.Parity = parameters.Parity;

				SerialPort.Handshake = parameters.FlowControl;

				SerialPort.ReadTimeout = receiveTimeout;

				// Register serial port event listener to be notified when data is available.
				//serialPort.addEventListener(this);
				SerialPort.DataReceived += _serialPort_DataReceived;
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

		public override void Close()
		{
			lock (SerialPort)
			{
				if (SerialPort != null)
				{
					try
					{
						SerialPort.Close();
					}
					catch (Exception) { }
				}
			}
		}

		public override Stream GetInputStream()
		{
			return SerialPort.BaseStream;
		}

		public override Stream GetOutputStream()
		{
			return SerialPort.BaseStream;
		}

		public override bool IsCD
		{
			get { return SerialPort.CDHolding; }
		}

		public override bool IsCTS
		{
			get { return SerialPort.CtsHolding; }
		}

		public override bool IsDSR
		{
			get { return SerialPort.DsrHolding; }
		}

		public override int ReadTimeout
		{
			get
			{
				return SerialPort.ReadTimeout;
			}
			set
			{
				SerialPort.ReadTimeout = value;
			}
		}

		public override void SetDTR(bool state)
		{
			SerialPort.DtrEnable = state;
		}

		public override void SetRTS(bool state)
		{
			SerialPort.RtsEnable = state;
		}

		public override void SetBreak(bool enabled)
		{
			SerialPort.BreakState = enabled;
		}

		public override void SendBreak(int duration)
		{
			var oldState = SerialPort.BreakState;
			SerialPort.BreakState = true;
			Task.Delay(duration);
			SerialPort.BreakState = oldState;
		}
	}
}
