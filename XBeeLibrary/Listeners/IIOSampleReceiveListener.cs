using Kveer.XBeeApi.IO;

namespace Kveer.XBeeApi.listeners
{
	/**
	 * This interface defines the required methods that an object should implement
	 * to behave as an IO Sample listener and be notified when IO samples are 
	 * received from a remote XBee device of the network.
	 */
	public interface IIOSampleReceiveListener
	{
		/**
		 * Called when an IO sample is received through the connection interface.
		 * 
		 * @param remoteDevice The remote XBee device that sent the sample.
		 * @param ioSample The received IO sample.
		 * 
		 * @see com.digi.xbee.api.RemoteXBeeDevice
		 * @see com.digi.xbee.api.io.IOSample
		 */
		void ioSampleReceived(RemoteXBeeDevice remoteDevice, IOSample ioSample);
	}
}