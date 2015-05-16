using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Kveer.XBeeApi.Models
{
	/// <summary>
	/// This class represents an XBee message containing the remote XBee device the message belongs to, the content (data) of the message and a flag indicating if the message is a broadcast message (was received or is being sent via broadcast). 
	/// </summary>
	/// <remarks>This class is used within the XBee C# Library to read data sent by remote devices.</remarks>
	public class XBeeMessage
	{
		/// <summary>
		/// Gets the remote XBee device this message is associated to.
		/// </summary>
		public RemoteXBeeDevice Device { get; private set; }

		/// <summary>
		/// Gets the data as byte array containing the data of the message.
		/// </summary>
		public byte[] Data { get; private set; }

		/// <summary>
		/// Gets whether or not the message was received via broadcast.
		/// </summary>
		public bool IsBroadcast { get; private set; }

		/// <summary>
		/// Initializes new instance of class <see cref="XBeeMessage"/>.
		/// </summary>
		/// <param name="remoteXBeeDevice">The remote XBee device the message belongs to.</param>
		/// <param name="data">Byte array containing the data of the message.</param>
		public XBeeMessage(RemoteXBeeDevice remoteXBeeDevice, byte[] data)
			: this(remoteXBeeDevice, data, false)
		{
		}

		/// <summary>
		/// Initializes new instance of class <see cref="XBeeMessage"/>.
		/// </summary>
		/// <param name="remoteXBeeDevice">The remote XBee device the message belongs to.</param>
		/// <param name="data">Byte array containing the data of the message.</param>
		/// <param name="isBroadcast">Indicates if the message was received via broadcast.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="remoteXBeeDevice"/> is null of <paramref name="data"/> is null.</exception>
		public XBeeMessage(RemoteXBeeDevice remoteXBeeDevice, byte[] data, bool isBroadcast)
		{
			Contract.Requires<ArgumentNullException>(remoteXBeeDevice != null, "Remote XBee device cannot be null.");
			Contract.Requires<ArgumentNullException>(data != null, "Data cannot be null.");

			this.Device = remoteXBeeDevice;
			this.Data = data;
			this.IsBroadcast = isBroadcast;
		}

		/// <summary>
		/// Gets the data of the message in string format.
		/// </summary>
		/// <returns>The data of the message in string format.</returns>
		public string DataString
		{
			get
			{
				return Encoding.UTF8.GetString(Data);
			}
		}
	}
}