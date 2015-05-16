using System;
using System.Diagnostics.Contracts;
using System.IO.Ports;

namespace Kveer.XBeeApi.Connection.Serial
{
	/**
	 * Helper class used to store serial connection parameters information.
	 * 
	 * <p>Parameters are stored as public variables so that they can be accessed
	 * and read from any class.</p>
	 */
	public sealed class SerialPortParameters : IEquatable<SerialPortParameters>
	{

		// Constants.
		private const int HASH_SEED = 23;

		// Variables.
		public int BaudRate { get; set; }
		public int DataBits { get; set; }
		public StopBits StopBits { get; set; }
		public Parity Parity { get; set; }
		public Handshake FlowControl { get; set; }

		/**
		 * Class constructor. Instances a new {@code SerialPortParameters} object
		 * with the given parameters.
		 * 
		 * @param baudrate Serial connection baud rate,
		 * @param dataBits Serial connection data bits.
		 * @param stopBits Serial connection stop bits.
		 * @param parity Serial connection parity.
		 * @param flowControl Serial connection flow control.
		 * 
		 * @throws ArgumentException if {@code baudrate < 0} or
		 *                                  if {@code dataBits < 0} or
		 *                                  if {@code stopBits < 0} or
		 *                                  if {@code parity < 0} or
		 *                                  if {@code flowControl < 0}.
		 */
		public SerialPortParameters(int baudrate, int dataBits, StopBits stopBits, Parity parity, Handshake flowControl)
		{
			Contract.Requires<ArgumentOutOfRangeException>(baudrate >= 0, "Baudrate cannot be less than 0.");
			Contract.Requires<ArgumentOutOfRangeException>(dataBits >= 0, "Number of data bits cannot be less than 0.");

			this.BaudRate = baudrate;
			this.DataBits = dataBits;
			this.StopBits = stopBits;
			this.Parity = parity;
			this.FlowControl = flowControl;
		}

		public override bool Equals(object obj)
		{
			var other = obj as SerialPortParameters;
			if (other != null)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(SerialPortParameters other)
		{
			return other != null
				&& other.BaudRate == BaudRate
				&& other.DataBits == DataBits
				&& other.StopBits == StopBits
				&& other.Parity == Parity
				&& other.FlowControl == FlowControl;
		}

		public override int GetHashCode()
		{
			int hash = HASH_SEED;
			hash = hash * (hash + BaudRate);
			hash = hash * (hash + DataBits);
			hash = hash * (hash + (int)StopBits);
			hash = hash * (hash + (int)Parity);
			hash = hash * (hash + (int)FlowControl);
			return hash;
		}

		public override string ToString()
		{
			return string.Format("Baud Rate: {0}, Data Bits: {1}, Stop Bits: {2}, Parity: {3}, Flow Control: {4}",
				BaudRate,
				DataBits,
				StopBits,
				Parity,
				FlowControl);
		}
	}
}