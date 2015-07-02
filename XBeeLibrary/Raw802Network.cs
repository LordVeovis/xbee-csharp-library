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
		/// <summary>
		/// Initializes a new instance of <see cref="Raw802Network"/> to represents a 802.15.4 network.
		/// </summary>
		/// <param name="device">A local 802.15.4 device to get the network from.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="device"/> is null.</exception>
		public Raw802Network(Raw802Device device)
			: base(device)
		{
		}
	}
}