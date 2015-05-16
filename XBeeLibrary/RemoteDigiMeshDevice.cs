using Kveer.XBeeApi.Models;
using System;
namespace Kveer.XBeeApi
{
	/**
	 * This class represents a remote DigiMesh device.
	 * 
	 * @see RemoteXBeeDevice
	 * @see RemoteDigiPointDevice
	 * @see RemoteRaw802Device
	 * @see RemoteZigBeeDevice
	 */
	public class RemoteDigiMeshDevice : RemoteXBeeDevice
	{

		/**
		 * Class constructor. Instantiates a new {@code RemoteDigiMeshDevice} object 
		 * with the given local {@code DigiMeshDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local DigiMesh device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote DigiMesh device.
		 * @param addr64 The 64-bit address to identify this remote DigiMesh device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteDigiMeshDevice(DigiMeshDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteDigiMeshDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote DigiMesh device.
		 * @param addr64 The 64-bit address to identify this remote DigiMesh device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true} or 
		 *                                  if {@code localXBeeDevice.getXBeeProtocol() != XBeeProtocol.DIGI_MESH}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteDigiMeshDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{

			// Verify the local device has DigiMesh protocol.
			if (localXBeeDevice.GetXBeeProtocol() != XBeeProtocol.DIGI_MESH)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.DIGI_MESH.GetDescription() + ".");
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteDigiMeshDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote DigiMesh device.
		 * @param addr64 The 64-bit address to identify this remote DigiMesh device.
		 * @param id The node identifier of this remote DigiMesh device. It might 
		 *           be {@code null}.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true} or 
		 *                                  if {@code localXBeeDevice.getXBeeProtocol() != XBeeProtocol.DIGI_MESH}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteDigiMeshDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64, string id)
			: base(localXBeeDevice, addr64, null, id)
		{

			// Verify the local device has DigiMesh protocol.
			if (localXBeeDevice.GetXBeeProtocol() != XBeeProtocol.DIGI_MESH)
				throw new ArgumentException("The protocol of the local XBee device is not " + XBeeProtocol.DIGI_MESH.GetDescription() + ".");
		}

		public override XBeeProtocol GetXBeeProtocol()
		{
			return XBeeProtocol.DIGI_MESH;
		}
	}
}