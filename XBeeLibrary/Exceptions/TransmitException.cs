using Kveer.XBeeApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when receiving a transmit status different than <code>XBeeTransmitStatus.SUCCESS</code> after sending an XBee API packet.
	/// </summary>
	public class TransmitException : CommunicationException
	{
		private const string DEFAULT_MESSAGE = "There was a problem transmitting the XBee API packet.";

		/// <summary>
		/// Gets the <see cref="XBeeTransmitStatus"/> of the exception containing information about the transmission.
		/// </summary>
		public XBeeTransmitStatus TransmitStatus { get; private set; }

		public string TransmitStatusMessage { get { return TransmitStatus.GetDescription(); } }

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class.
		/// </summary>
		/// <param name="transmitStatus">The status of the transmission.</param>
		public TransmitException(XBeeTransmitStatus transmitStatus)
			: base(DEFAULT_MESSAGE)
		{
			TransmitStatus = transmitStatus;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="transmitStatus">The status of the transmission.</param>
		public TransmitException(string message, XBeeTransmitStatus transmitStatus)
			: base(message)
		{
			TransmitStatus = transmitStatus;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		/// <param name="transmitStatus">The status of the transmission.</param>
		public TransmitException(string message, Exception innerException, XBeeTransmitStatus transmitStatus)
			: base(message, innerException)
		{
			TransmitStatus = transmitStatus;
		}

		public override string Message
		{
			get
			{
				return string.Format("{0} > {1}", base.Message, TransmitStatus.GetDescription());
			}
		}

	}
}
