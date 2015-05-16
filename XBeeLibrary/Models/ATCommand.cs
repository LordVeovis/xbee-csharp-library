using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// This class represents an AT command used to read or set different properties of the XBee device.
	/// </summary>
	/// <remarks>AT commands can be sent directly to the connected device or to remote devices and may have parameters.
	/// After executing an AT Command, an AT Response is received from the device.
	/// </remarks>
	/// <seealso cref="ATCommandResponse"/>
	public class ATCommand
	{

		public string Command { get; private set; }

		public byte[] Parameter { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommand"/> class.
		/// </summary>
		/// <param name="command">The AT Command alias.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="code"/> is null.</exception>
		/// <exception cref="ArgumentException">if the Length of <paramref name="code"/> is not 2.</exception>
		public ATCommand(string command)
			: this(command, (byte[])null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ATCommand"/> class.
		/// </summary>
		/// <remarks>If not <paramref name="parameter"/> is required, the constructor <see cref="ATCommand(string)"/> is recommanded.</remarks>
		/// <param name="command">The AT Command alias.</param>
		/// <param name="parameter">The command parameter expressed as a string.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="code"/> is null.</exception>
		/// <exception cref="ArgumentException">if the Length of <paramref name="code"/> is not 2.</exception>
		public ATCommand(string command, string parameter)
			: this(command, parameter == null ? null : Encoding.UTF8.GetBytes(parameter))
		{
		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommand"/>.
		/// </summary>
		/// <remarks>If not <paramref name="parameter"/> is required, the constructor <see cref="ATCommand(string)"/> is recommanded.</remarks>
		/// <param name="command">The AT Command alias.</param>
		/// <param name="parameter">The command parameter expressed as a byte array.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="code"/> is null.</exception>
		/// <exception cref="ArgumentException">if the Length of <paramref name="code"/> is not 2.</exception>
		public ATCommand(string command, byte[] parameter)
		{
			Contract.Requires<ArgumentNullException>(command != null, "Command cannot be null.");
			Contract.Requires<ArgumentException>(command.Length != 2, "Command lenght must be 2.");

			this.Command = command;
			this.Parameter = parameter;
		}

		/// <summary>
		/// Gets or sets the AT command parameter in string format.
		/// </summary>
		public string ParameterString
		{
			get
			{
				if (Parameter == null)
					return null;

				return Encoding.UTF8.GetString(Parameter);
			}
			set
			{
				Parameter = Encoding.UTF8.GetBytes(value);
			}
		}
	}
}