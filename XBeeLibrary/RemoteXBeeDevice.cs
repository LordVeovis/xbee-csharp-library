using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.Models;
using System;
using System.IO;
namespace Kveer.XBeeApi
{

	/**
	 * This class represents a remote XBee device.
	 * 
	 * @see RemoteDigiMeshDevice
	 * @see RemoteDigiPointDevice
	 * @see RemoteRaw802Device
	 * @see RemoteZigBeeDevice
	 */
	public class RemoteXBeeDevice : AbstractXBeeDevice
	{

		/**
		 * Class constructor. Instantiates a new {@code RemoteXBeeDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote XBee device.
		 * @param addr64 The 64-bit address to identify this remote XBee device.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteXBeeDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64)
			: base(localXBeeDevice, addr64)
		{
		}

		/**
		 * Class constructor. Instantiates a new {@code RemoteXBeeDevice} object 
		 * with the given local {@code XBeeDevice} which contains the connection 
		 * interface to be used.
		 * 
		 * @param localXBeeDevice The local XBee device that will behave as 
		 *                        connection interface to communicate with this 
		 *                        remote XBee device.
		 * @param addr64 The 64-bit address to identify this remote XBee device.
		 * @param addr16 The 16-bit address to identify this remote XBee device. It 
		 *               might be {@code null}.
		 * @param ni The node identifier of this remote XBee device. It might be 
		 *           {@code null}.
		 * 
		 * @throws ArgumentException if {@code localXBeeDevice.isRemote() == true}.
		 * @throws ArgumentNullException if {@code localXBeeDevice == null} or
		 *                              if {@code addr64 == null}.
		 * 
		 * @see com.digi.xbee.api.models.XBee16BitAddress
		 * @see com.digi.xbee.api.models.XBee64BitAddress
		 */
		public RemoteXBeeDevice(XBeeDevice localXBeeDevice, XBee64BitAddress addr64,
				XBee16BitAddress addr16, string ni)
			: base(localXBeeDevice, addr64, addr16, ni)
		{
		}

		/**
		 * Always returns {@code true}, since it is a remote device.
		 * 
		 * @return {@code true} always.
		 */
		public override bool IsRemote
		{
			get
			{
				return true;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.digi.xbee.api.AbstractXBeeDevice#reset()
		 */
		//@Override
		public override void reset()/*throws TimeoutException, XBeeException */{
			// Check connection.
			if (!connectionInterface.IsOpen)
				throw new InterfaceNotOpenException();

			logger.InfoFormat(ToString() + "Resetting the remote module ({0})...", get64BitAddress());

			ATCommandResponse response = null;
			try
			{
				response = SendATCommand(new ATCommand("FR"));
			}
			catch (IOException e)
			{
				throw new XBeeException("Error writing in the communication interface.", e);
			}
			catch (Kveer.XBeeApi.Exceptions.TimeoutException e)
			{
				// Remote 802.15.4 devices do not respond to the AT command.
				if (localXBeeDevice.getXBeeProtocol() == XBeeProtocol.RAW_802_15_4)
					return;
				else
					throw e;
			}

			// Check if AT Command response is valid.
			checkATCommandResponseIsValid(response);
		}

		public override string ToString()
		{
			String id = getNodeID();
			if (id == null)
				id = "";
			return string.Format("{0} - {1}", get64BitAddress(), getNodeID());
		}
	}
}