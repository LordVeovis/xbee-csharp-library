using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Kveer.XBeeApi.Models
{

	/// <summary>
	/// This class represents the response of an AT Command sent by the connected XBee device or by a remote device after executing an AT Command.
	/// </summary>
	/// <remarks>Among the executed command, this object contains the response data and the command status.</remarks>
	/// <seealso cref="ATCommand"/>
	/// <seealso cref="ATCommandStatus"/>
	public class ATCommandResponse
	{
		/// <summary>
		/// Gets the AT command that generated the response.
		/// </summary>
		/// <seealso cref="ATCommand"/>
		public ATCommand Command { get; private set; }

		public ATCommandStatus Status { get; private set; }

		/// <summary>
		/// Gets the AT command response data in byte array format if any.
		/// </summary>
		public byte[] Response { get; private set; }

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommandResponse"/>.
		/// </summary>
		/// <param name="command">The <see cref="ATCommand"/> that generated the response.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="command"/> is null or <paramref name="status"/> is null.</exception>
		/// <seealso cref="ATCommand"/>
		public ATCommandResponse(ATCommand command)
			: this(command, null, ATCommandStatus.OK)
		{
		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommandResponse"/>.
		/// </summary>
		/// <param name="command">The <see cref="ATCommand"/> that generated the response.</param>
		/// <param name="status">The <see cref="ATCommandStatus"/> containing the response status.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="command"/> is null or <paramref name="status"/> is null.</exception>
		/// <seealso cref="ATCommand"/>
		/// <seealso cref="ATCommandStatus"/>
		public ATCommandResponse(ATCommand command, ATCommandStatus status)
			: this(command, null, status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommandResponse"/>.
		/// </summary>
		/// <param name="command">The <see cref="ATCommand"/> that generated the response.</param>
		/// <param name="response">The command response in byte array format.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="command"/> is null or <paramref name="status"/> is null.</exception>
		/// <seealso cref="ATCommand"/>
		public ATCommandResponse(ATCommand command, byte[] response)
			: this(command, response, ATCommandStatus.OK)
		{
		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="ATCommandResponse"/>.
		/// </summary>
		/// <param name="command">The <see cref="ATCommand"/> that generated the response.</param>
		/// <param name="response">The command response in byte array format.</param>
		/// <param name="status">The <see cref="ATCommandStatus"/> containing the response status.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="command"/> is null or <paramref name="status"/> is null.</exception>
		/// <seealso cref="ATCommand"/>
		/// <seealso cref="ATCommandStatus"/>
		public ATCommandResponse(ATCommand command, byte[] response, ATCommandStatus status)
		{
			Contract.Requires<ArgumentNullException>(command != null, "Command cannot be null.");
			Contract.Requires<ArgumentNullException>(status != null, "Status cannot be null.");

			Command = command;
			Response = response;
			Status = status;
		}

		/// <summary>
		/// Gest the AT command response data as string if any.
		/// </summary>
		public string ResponseString
		{
			get
			{
				if (Response == null)
					return null;

				return Encoding.UTF8.GetString(Response);
			}
		}
	}
}