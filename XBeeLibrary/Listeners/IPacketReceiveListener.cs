using Kveer.XBeeApi.Packet;

namespace Kveer.XBeeApi.listeners
{
	/// <summary>
	/// This interface defines the required methods that an object should implement to behave as a packet listener and be notified when new packets are received from a remote XBee device of the network.
	/// </summary>
	public interface IPacketReceiveListener
	{
		/// <summary>
		/// Called when an XBee packet is received through the connection interface.
		/// </summary>
		/// <param name="receivedPacket">The received XBee packet.</param>
		/// <seealso cref="XBeeAPIPacket"/>
		void PacketReceived(XBeePacket receivedPacket);
	}
}