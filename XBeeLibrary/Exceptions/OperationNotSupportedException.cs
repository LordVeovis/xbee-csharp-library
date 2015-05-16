using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when the operation performed is not supported by the XBee device.
	/// </summary>
	public class OperationNotSupportedException : XBeeDeviceException
	{
		private const string DEFAULT_MESSAGE = "The requested operation is not supported by either the connection interface or the XBee device.";

		/// <summary>
		/// Initializes a new instance of the <see cref="OperationNotSupportedException"/> class.
		/// </summary>
		public OperationNotSupportedException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="OperationNotSupportedException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public OperationNotSupportedException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="OperationNotSupportedException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public OperationNotSupportedException(string message, Exception innerException) : base(message, innerException) { }

	}
}
