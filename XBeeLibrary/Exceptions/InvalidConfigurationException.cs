﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Exceptions
{
	/// <summary>
	/// This exception will be thrown when trying to open an interface with an invalid configuration. Usually happens when the XBee device is communicating through a serial port.
	/// </summary>
	public class InvalidConfigurationException : ConnectionException
	{
		private const string DEFAULT_MESSAGE = "The configuration used to open the interface is invalid.";

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
		/// </summary>
		public InvalidConfigurationException() : base(DEFAULT_MESSAGE) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		public InvalidConfigurationException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class with a specified error message and the exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">ThThe error message that explains the reason for this exception.</param>
		/// <param name="cause">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public InvalidConfigurationException(string message, Exception innerException) : base(message, innerException) { }

	}
}
