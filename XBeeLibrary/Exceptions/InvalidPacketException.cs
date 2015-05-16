using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when there is an error parsing an API packet from the input stream.
	/// </summary>
	public class InvalidPacketException : CommunicationException
	{
		private const string DEFAULT_MESSAGE = "The XBee API packet is not properly formed.";

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidPacketException"/> class.
		/// </summary>
		public InvalidPacketException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidPacketException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public InvalidPacketException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidPacketException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public InvalidPacketException(string message, Exception innerException) : base(message, innerException) { }

	}
}
