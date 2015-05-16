using System;

namespace Kveer.XBeeApi.Connection.Serial
{
	/// <summary>
	/// Helper class used to store serial port information.
	/// </summary>
	public class SerialPortInfo
	{
		/// <summary>
		/// Gets the serial port name.
		/// </summary>
		public string PortName { get; private set; }

		/// <summary>
		/// Gets or sets the serial port description.
		/// </summary>
		public string PortDescription { get; set; }

		/// <summary>
		/// Initializes a new instance of class <see cref="SerialPortInfo"/>.
		/// </summary>
		/// <param name="portName">Name of the port.</param>
		public SerialPortInfo(string portName)
			: this(portName, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of class <see cref="SerialPortInfo"/>.
		/// </summary>
		/// <param name="portName">Name of the port.</param>
		/// <param name="portDescription">Description of the port.</param>
		public SerialPortInfo(string portName, string portDescription)
		{
			this.PortName = portName;
			this.PortDescription = portDescription;
		}
	}
}