using Kveer.XBeeApi.Models;
using System;
namespace Kveer.XBeeApi
{
	/**
	 * This class represents a remote 802.15.4 device.
	 * 
	 * @see RemoteXBeeDevice
	 * @see RemoteDigiMeshDevice
	 * @see RemoteDigiPointDevice
	 * @see RemoteZigBeeDevice
	 */
	public class RemoteRaw802Device : RemoteXBeeDevice
	{

		/**
		 * Class constructor. Instantiates a new {@code RemoteRaw802Device} object 
		 * with the given local {@code Raw802Device} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local 802.15.4 device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote 802.15.4 device.
		 * @param addr64 The 64-bit address to identify this remote 802.15.4 device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteRaw802Device(Raw802Device localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteRaw802Device} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote 802.15.4 device.
		 * @param addr64 The 64-bit address to identify this remote 802.15.4 device.
		 * @param addr16 The 16-bit address to identify this remote 802.15.4 device. 
		 *               It might be {@code null}.
		 * @param id The node identifier of this remote 802.15.4 device. It might be 
		 *           {@code null}.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true} or  
		 *                                  if {@code localXBeeDevice.getXBeeProtocol() != XBeeProtocol.RAW_802_15_4}
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteRaw802Device(XBeeDevice localXBeeDevice, XBee64BitAddress addr64,
				XBee16BitAddress addr16, string id)
			: base(localXBeeDevice, addr64, addr16, id)
		{

			// Verify the local device has 802.15.4 protocol.
			if (localXBeeDevice.GetXBeeProtocol() != XBeeProtocol.RAW_802_15_4)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.RAW_802_15_4.GetDescription() + ".");
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteRaw802Device} object 
		 * with the given local {@code Raw802Device} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local 802.15.4 device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote 802.15.4 device.
		 * @param addr16 The 16-bit address to identify this remote 802.15.4 
		 *               device.
		 * 
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr16 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public RemoteRaw802Device(Raw802Device localXBeeDevice, XBee16BitAddress addr16)
			: base(localXBeeDevice, XBee64BitAddress.UNKNOWN_ADDRESS)
		{

			this.xbee16BitAddress = addr16;
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteRaw802Device} object 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local 802.15.4 device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote 802.15.4 device.
		 * @param addr16 The 16-bit address to identify this remote 802.15.4 
		 *               device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.getXBeeProtocol() != XBeeProtocol.RAW_802_15_4}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr16 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 */
		public RemoteRaw802Device(XBeeDevice localXBeeDevice, XBee16BitAddress addr16)
			: base(localXBeeDevice, XBee64BitAddress.UNKNOWN_ADDRESS)
		{

			// Verify the local device has 802.15.4 protocol.
			if (localXBeeDevice.GetXBeeProtocol() != XBeeProtocol.RAW_802_15_4)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.RAW_802_15_4.GetDescription() + ".");

			this.xbee16BitAddress = addr16;
		}

		/**
		 * Sets the XBee64BitAddress of this remote 802.15.4 device.
		 * 
		 * @param addr64 The 64-bit address to be set to the device.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public void set64BitAddress(XBee64BitAddress addr64)
		{
			this.xbee64BitAddress = addr64;
		}


		public override XBeeProtocol GetXBeeProtocol()
		{
			return XBeeProtocol.RAW_802_15_4;
		}
	}
}