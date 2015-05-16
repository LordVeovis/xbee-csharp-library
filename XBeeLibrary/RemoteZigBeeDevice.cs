using Kveer.XBeeApi.Models;
using System;
namespace Kveer.XBeeApi
{
	/**
	 * This class represents a remote ZigBee device.
	 * 
	 * @see RemoteXBeeDevice
	 * @see RemoteDigiMeshDevice
	 * @see RemoteDigiPointDevice
	 * @see RemoteRaw802Device
	 */
	public class RemoteZigBeeDevice : RemoteXBeeDevice
	{
		/**
		 * Class constructor. Instantiates a new {@code RemoteZigBeeDevice} object 
		 * with the given local {@code ZigBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local ZigBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote ZigBee device.
		 * @param addr64 The 64-bit address to identify this remote ZigBee device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteZigBeeDevice(ZigBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteZigBeeDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote ZigBee device.
		 * @param addr64 The 64-bit address to identify this remote ZigBee device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true} or 
		 *                                  if {@code localXBeeDevice.getXBeeProtocol() != XBeeProtocol.ZIGBEE}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteZigBeeDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{

			// Verify the local device has ZigBee protocol.
			if (localXBeeDevice.GetXBeeProtocol() != XBeeProtocol.ZIGBEE)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.ZIGBEE.GetDescription() + ".");
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteZigBeeDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote ZigBee device.
		 * @param addr64 The 64-bit address to identify this remote ZigBee device.
		 * @param addr16 The 16-bit address to identify this remote ZigBee device. 
		 *               It might be {@code null}.
		 * @param ni The node identifier of this remote ZigBee device. It might be 
		 *           {@code null}.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true} or 
		 *                                  if {@code localXBeeDevice.getXBeeProtocol() != XBeeProtocol.ZIGBEE}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteZigBeeDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64,
				XBee16BitAddress addr16, string ni)
			: base(localXBeeDevice, addr64, addr16, ni)
		{

			// Verify the local device has ZigBee protocol.
			if (localXBeeDevice.GetXBeeProtocol() != XBeeProtocol.ZIGBEE)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.ZIGBEE.GetDescription() + ".");
		}

		public override XBeeProtocol GetXBeeProtocol()
		{
			return XBeeProtocol.ZIGBEE;
		}
	}
}