using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when trying to open the port/communication interface but it is already in use by other applications.
	/// </summary>
	public class InterfaceInUseException : ConnectionException
	{
		private const string DEFAULT_MESSAGE = "The connection interface is already in use by other application(s).";

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceInUseException"/> class.
		/// </summary>
		public InterfaceInUseException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceInUseException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public InterfaceInUseException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceInUseException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public InterfaceInUseException(string message, Exception innerException) : base(message, innerException) { }

	}
}
