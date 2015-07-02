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
		/// <summary>
		/// Initializes a new instance of <see cref="DigiPointNetwork"/>.
		/// </summary>
		/// <param name="device">A local DigiPoint device to get the network from.</param>
		/// <exception cref="ArgumentNullException">if <see cref="device"/> is null.</exception>
		public DigiPointNetwork(DigiPointDevice device)
			: base(device)
		{
		}
	}
}