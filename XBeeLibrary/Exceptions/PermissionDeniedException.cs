using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when the user does not have the appropriate access to the connection interface. Usually happens when the XBee device is communicating through a serial port.
	/// </summary>
	public class PermissionDeniedException : ConnectionException
	{
		private const string DEFAULT_MESSAGE = "You don't have the required permissions to access the connection interface.";

		/// <summary>
		/// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
		/// </summary>
		public PermissionDeniedException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="PermissionDeniedException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public PermissionDeniedException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="PermissionDeniedException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public PermissionDeniedException(string message, Exception innerException) : base(message, innerException) { }

	}
}
