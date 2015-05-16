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
	public class CommunicationException : XBeeException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CommunicationException"/> class.
		/// </summary>
		public CommunicationException() : base() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="CommunicationException"/> class with the exception that is the cause of this exception.
		/// </summary>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public CommunicationException(Exception innerException) : base(null, innerException) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="CommunicationException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public CommunicationException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="CommunicationException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public CommunicationException(string message, Exception innerException) : base(message, innerException) { }

	}
}
