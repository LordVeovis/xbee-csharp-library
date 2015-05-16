using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when performing synchronous operations and the configured time expires.
	/// </summary>
	public class TimeoutException : CommunicationException
	{
		private const string DEFAULT_MESSAGE = "There was a timeout while executing the requested operation.";

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class.
		/// </summary>
		public TimeoutException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public TimeoutException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public TimeoutException(string message, Exception innerException) : base(message, innerException) { }

	}
}
