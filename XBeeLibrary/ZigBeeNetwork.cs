namespace Kveer.XBeeApi
{
	/**
	 * This class represents a ZigBee Network.
	 *  
	 * <p>The network allows the discovery of remote devices in the same network 
	 * as the local one and stores them.</p>
	 * 
	 * @see DigiMeshNetwork
	 * @see DigiPointNetwork
	 * @see Raw802Network
	 * @see XBeeNetwork
	 */
	public class ZigBeeNetwork : XBeeNetwork
	{

		/**
		 * Instantiates a new ZigBee Network object.
		 * 
		 * @param device Local ZigBee device to get the network from.
		 * 
		 * @throws ArgumentNullException if {@code device == null}.
		 * 
		 * @see ZigBeeDevice
		 */
		public ZigBeeNetwork(ZigBeeDevice device)
			: base(device)
		{
		}
	}
}