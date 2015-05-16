using Kveer.XBeeApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when any problem related to the communication with the XBee device occurs.
	/// </summary>
	public class ATCommandException : CommunicationException
	{
		const string DEFAULT_MESSAGE = "There was a problem sending the AT command packet.";

		/// <summary>
		/// Gets the <see cref="ATCommandStatus"/> of the exception containing information about the AT command response.
		/// </summary>
		public ATCommandStatus CommandStatus { get; private set; }

		/// <summary>
		/// Gets the text containing the status of the AT command response.
		/// </summary>
		public string CommandStatusMessage { get { return CommandStatus.GetDescription(); } }

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandException"/> class.
		/// </summary>
		public ATCommandException(ATCommandStatus atCommandStatus) : base(DEFAULT_MESSAGE) {
			this.CommandStatus = atCommandStatus;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="atCommandStatus">The status of the AT command response.</param>
		public ATCommandException(string message, ATCommandStatus atCommandStatus)
			: base(message)
		{
			this.CommandStatus = atCommandStatus;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommandException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		/// <param name="atCommandStatus">The status of the AT command response.</param>
		public ATCommandException(string message, Exception innerException, ATCommandStatus atCommandStatus) : base(message, innerException) {
			this.CommandStatus = atCommandStatus;
		}

		public override string Message
		{
			get
			{
				return string.Format("{0} > {1}", base.Message, this.CommandStatusMessage);
			}
		}

	}
}
