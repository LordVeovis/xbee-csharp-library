namespace Kveer.XBeeApi.Listeners
{
	/**
	 * Interface defining the required methods that an object should implement to be 
	 * notified about device discovery events. 
	 */
	public interface IDiscoveryListener
	{
		/**
		 * Notifies that a remote device was discovered in the network.
		 * 
		 * @param discoveredDevice The discovered remote device.
		 * 
		 * @see com.digi.xbee.api.RemoteXBeeDevice
		 */
		void DeviceDiscovered(RemoteXBeeDevice discoveredDevice);

		/**
		 * Notifies that an error occurred during the discovery process.
		 * 
		 * <p>This method is only called when an error occurs but does not cause 
		 * the process to finish.</p>
		 * 
		 * @param error The error message.
		 */
		void DiscoveryError(string error);

		/**
		 * Notifies that the discovery process has finished.
		 * 
		 * @param error The error message, or {@code null} if the process finished 
		 *              successfully.
		 */
		void DiscoveryFinished(string error);
	}
}