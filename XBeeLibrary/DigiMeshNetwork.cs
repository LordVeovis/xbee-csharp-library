namespace Kveer.XBeeApi
{

	/**
	 * This class represents a DigiMesh Network.
	 *  
	 * <p>The network allows the discovery of remote devices in the same network 
	 * as the local one and stores them.</p>
	 * 
	 * @see DigiPointNetwork
	 * @see Raw802Network
	 * @see XBeeNetwork
	 * @see ZigBeeNetwork
	 */
	public class DigiMeshNetwork : XBeeNetwork
	{

		/**
		 * Instantiates a new DigiMesh Network object.
		 * 
		 * @param device Local DigiMesh device to get the network from.
		 * 
		 * @throws ArgumentNullException if {@code device == null}.
		 * 
		 * @see DigiMeshDevice
		 */
		public DigiMeshNetwork(DigiMeshDevice device)
			: base(device)
		{
		}
	}
}