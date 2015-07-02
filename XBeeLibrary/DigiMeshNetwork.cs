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
		/// <summary>
		/// Initializes a new instance of <see cref="DigiMeshNetwork"/>.
		/// </summary>
		/// <param name="device">A local DigiMesh device to get the network from.</param>
		/// <exception cref="ArgumentNullException">if <paramref name="device"/> is null.</exception>
		public DigiMeshNetwork(DigiMeshDevice device)
			: base(device)
		{
		}
	}
}