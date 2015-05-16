using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// Generic XBee API exception. This class and its subclasses indicate conditions that an application might want to catch. This exception can be thrown when any problem related to the XBee device occurs.
	/// </summary>
	public class XBeeException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XBeeException"/> class.
		/// </summary>
		public XBeeException() : base() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="XBeeException"/> class with the exception that is the cause of this exception.
		/// </summary>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public XBeeException(Exception innerException) : base(null, innerException) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="XBeeException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public XBeeException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="XBeeException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public XBeeException(string message, Exception innerException) : base(message, innerException) { }
	}
}
