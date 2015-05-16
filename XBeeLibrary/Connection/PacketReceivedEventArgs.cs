using Kveer.XBeeApi.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.Connection
{
	public class PacketReceivedEventArgs : EventArgs
	{
		public XBeePacket ReceivedPacket { get; private set; }

		public PacketReceivedEventArgs(XBeePacket packet)
		{
			ReceivedPacket = packet;
		}
	}
}
