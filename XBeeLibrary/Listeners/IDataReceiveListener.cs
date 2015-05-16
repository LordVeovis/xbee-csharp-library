using Kveer.XBeeApi.Models;
namespace Kveer.XBeeApi.listeners
{
	/**
	 * This interface defines the required methods that should be implemented to 
	 * behave as a data listener and be notified when new data is received from a 
	 * remote XBee device of the network.
	 */
	public interface IDataReceiveListener
	{
		/**
		 * Called when data is received from a remote node of the network.
		 * 
		 * @param xbeeMessage An {@code XBeeMessage} object containing the data,
		 *                    the address of the {@code RemoteXBeeDevice} that sent
		 *                    the data and a flag indicating whether the data was 
		 *                    sent via broadcast or not.
		 * 
		 * @see com.digi.xbee.api.models.XBeeMessage
		 * @see com.digi.xbee.api.RemoteXBeeDevice
		 */
		void DataReceived(XBeeMessage xbeeMessage);
	}
}