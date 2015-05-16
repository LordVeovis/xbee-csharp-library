namespace Kveer.XBeeApi
{
	/**
	 * This class represents an 802.15.4 Network.
	 *  
	 * <p>The network allows the discovery of remote devices in the same network 
	 * as the local one and stores them.</p>
	 * 
	 * @see DigiMeshNetwork
	 * @see DigiPointNetwork
	 * @see XBeeNetwork
	 * @see ZigBeeNetwork
	 */
	public class Raw802Network : XBeeNetwork
	{

		/**
		 * Instantiates a new 802.15.4 Network object.
		 * 
		 * @param device Local 802.15.4 device to get the network from.
		 * 
		 * @throws ArgumentNullException if {@code device == null}.
		 * 
		 * @see Raw802Device
		 */
		public Raw802Network(Raw802Device device)
			: base(device)
		{
		}
	}
}