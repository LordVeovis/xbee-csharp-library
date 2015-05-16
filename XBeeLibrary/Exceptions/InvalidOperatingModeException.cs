using Kveer.XBeeApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when performing any action with the XBee device and its operating mode is different than {@link OperatingMode#API} and {@link OperatingMode#API_ESCAPE}.
	/// </summary>
	public class InvalidOperatingModeException : CommunicationException
	{
		private const string DEFAULT_MESSAGE = "The operating mode of the XBee device is not supported by the library.";

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidOperatingModeException"/> class.
		/// </summary>
		public InvalidOperatingModeException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidOperatingModeException"/> class with the specified operating <paramref name="mode"/>.
		/// </summary>
		/// <param name="mode">The unsupported operating mode.</param>
		public InvalidOperatingModeException(OperatingMode mode) : base("Unsupported operating mode: " + mode) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidOperatingModeException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for this exception.</param>
		public InvalidOperatingModeException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidOperatingModeException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public InvalidOperatingModeException(string message, Exception innerException) : base(message, innerException) { }

	}
}
