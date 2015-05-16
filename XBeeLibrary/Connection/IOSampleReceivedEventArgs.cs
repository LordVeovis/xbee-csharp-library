using Kveer.XBeeApi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Connection
{
	/// <summary>
	/// Provides data for IO sample received event.
	/// </summary>
	public class IOSampleReceivedEventArgs : EventArgs
	{
		public RemoteXBeeDevice RemoteDevice { get; private set; }

		public IOSample IOSample { get; private set; }

		public IOSampleReceivedEventArgs(RemoteXBeeDevice remoteDevice, IOSample ioSample)
		{
			RemoteDevice = remoteDevice;
			IOSample = ioSample;
		}
	}
}
