using Kveer.XBeeApi.Models;

namespace Kveer.XBeeApi.listeners
{
	/**
	 * This interface defines the required methods that an object should implement
	 * to behave as a modem status listener and be notified when modem status events 
	 * are received from the radio.
	 */
	public interface IModemStatusReceiveListener
	{
		/**
		 * Called when a modem status event from the radio is received.
		 * 
		 * @param modemStatusEvent The modem status event that was received.
		 * 
		 * @see com.digi.xbee.api.models.ModemStatusEvent
		 */
		void modemStatusEventReceived(ModemStatusEvent modemStatusEvent);
	}
}