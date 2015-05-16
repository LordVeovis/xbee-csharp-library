namespace Kveer.XBeeApi
{
	/**
	 * This class represents a DigiPoint Network.
	 *  
	 * <p>The network allows the discovery of remote devices in the same network 
	 * as the local one and stores them.</p>
	 * 
	 * @see DigiMeshNetwork
	 * @see Raw802Network
	 * @see XBeeNetwork
	 * @see ZigBeeNetwork
	 */
	public class DigiPointNetwork : XBeeNetwork
	{

		/**
		 * Instantiates a new DigiPoint Network object.
		 * 
		 * @param device Local DigiPoint device to get the network from.
		 * 
		 * @throws ArgumentNullException if {@code device == null}.
		 * 
		 * @see DigiPointDevice
		 */
		public DigiPointNetwork(DigiPointDevice device)
			: base(device)
		{
		}
	}
}