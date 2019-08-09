/*
 * Copyright 2019, Digi International Inc.
 * Copyright 2014, 2015, Sébastien Rault.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using System;
using System.Text;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// This class represents an AT command used to read or set different properties of the XBee device.
	/// </summary>
	/// <remarks>AT commands can be sent directly to the connected device or to remote devices and may 
	/// have parameters. After executing an AT Command, an AT Response is received from the device.
	/// </remarks>
	/// <seealso cref="ATCommandResponse"/>
	public class ATCommand
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommand"/> class.
		/// </summary>
		/// <param name="command">The AT Command alias.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If the Length of <paramref name="command"/> is 
		/// not 2.</exception>
		public ATCommand(string command) : this(command, (byte[])null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommand"/> class.
		/// </summary>
		/// <remarks>If not <paramref name="parameter"/> is required, the constructor 
		/// <see cref="ATCommand(string)"/> is recommanded.</remarks>
		/// <param name="command">The AT Command alias.</param>
		/// <param name="parameter">The command parameter expressed as a string.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If the Length of <paramref name="command"/> is not 2.</exception>
		public ATCommand(string command, string parameter)
			: this(command, parameter == null ? null : Encoding.UTF8.GetBytes(parameter)) { }

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommand"/>.
		/// </summary>
		/// <remarks>If not <paramref name="parameter"/> is required, the constructor 
		/// <see cref="ATCommand(string)"/> is recommanded.</remarks>
		/// <param name="command">The AT Command alias.</param>
		/// <param name="parameter">The command parameter expressed as a byte array.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="command"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">If the Length of <paramref name="command"/> is not 2.</exception>
		public ATCommand(string command, byte[] parameter)
		{
			if (command == null)
				throw new ArgumentNullException("Command cannot be null.");
			if (command.Length != 2)
				throw new ArgumentException("Command lenght must be 2.");

			Command = command;
			Parameter = parameter;
		}

		// Properties.
		/// <summary>
		/// The AT command name.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// The AT command parameter value.
		/// </summary>
		public byte[] Parameter { get; set; }

		/// <summary>
		/// The AT command parameter in string format.
		/// </summary>
		public string ParameterString
		{
			get
			{
				if (Parameter == null)
					return null;

				return Encoding.UTF8.GetString(Parameter, 0, Parameter.Length);
			}
			set
			{
				Parameter = Encoding.UTF8.GetBytes(value);
			}
		}
	}
}