using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when trying to open a non-existing interface.
	/// </summary>
	public class InvalidInterfaceException : ConnectionException
	{
		private const string DEFAULT_MESSAGE = "The connection interface you are trying to access is invalid or does not exist.";

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidInterfaceException"/> class.
		/// </summary>
		public InvalidInterfaceException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidInterfaceException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public InvalidInterfaceException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidInterfaceException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public InvalidInterfaceException(string message, Exception innerException) : base(message, innerException) { }

	}
}
