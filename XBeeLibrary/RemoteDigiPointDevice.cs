using Kveer.XBeeApi.Models;
using System;
namespace Kveer.XBeeApi
{
	/**
	 * This class represents a remote DigiPoint device.
	 * 
	 * @see RemoteXBeeDevice
	 * @see RemoteDigiMeshDevice
	 * @see RemoteRaw802Device
	 * @see RemoteZigBeeDevice
	 */
	public class RemoteDigiPointDevice : RemoteXBeeDevice
	{

		/**
		 * Class constructor. Instantiates a new {@code RemoteDigiPointDevice} object 
		 * with the given local {@code DigiPointDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local point-to-multipoint device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote point-to-multipoint device.
		 * @param addr64 The 64-bit address to identify this remote point-to-multipoint 
		 *               device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteDigiPointDevice(DigiPointDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteDigiPointDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote point-to-multipoint device.
		 * @param addr64 The 64-bit address to identify this remote point-to-multipoint 
		 *               device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true} or 
		 *                                  if {@code localXBeeDevice.getXBeeProtocol() != XBeeProtocol.DIGI_POINT}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteDigiPointDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{

			// Verify the local device has point-to-multipoint protocol.
			if (localXBeeDevice.XBeeProtocol != XBeeProtocol.DIGI_POINT)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.DIGI_POINT.GetDescription() + ".");
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteDigiPointDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote point-to-multipoint device.
		 * @param addr64 The 64-bit address to identify this remote point-to-multipoint 
		 *               device.
		 * @param id The node identifier of this remote point-to-multipoint device. 
		 *           It might be {@code null}.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true} or 
		 *                                  if {@code localXBeeDevice.getXBeeProtocol() != XBeeProtocol.DIGI_POINT}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteDigiPointDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64, string id)
			: base(localXBeeDevice, addr64, null, id)
		{

			// Verify the local device has point-to-multipoint protocol.
			if (localXBeeDevice.XBeeProtocol != XBeeProtocol.DIGI_POINT)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.DIGI_POINT.GetDescription() + ".");
		}

		public override XBeeProtocol XBeeProtocol
		{
			get
			{
				return XBeeProtocol.DIGI_POINT;
			}
		}
	}
}