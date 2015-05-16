using Common.Logging;
using Kveer.XBeeApi;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.listeners;
using Kveer.XBeeApi.Listeners;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Packet;
using Kveer.XBeeApi.Packet.Common;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kveer.XBeeApi
{
	/**
	 * Helper class used to perform a node discovery ({@code ND}) in the provided 
	 * local XBee device. 
	 * 
	 * <p>This action requires an XBee connection and optionally a discover timeout.
	 * The node discovery works on all protocols and working modes returning as 
	 * result a list of discovered XBee Devices.</p> 
	 * 
	 * <p>The discovery process updates the network of the local device with the new
	 * discovered modules and refreshes the already existing references.</p>
	 */
	class NodeDiscovery
	{

		// Constants.
		private const string ND_COMMAND = "ND";

		public const long DEFAULT_TIMEOUT = 20000; // 20 seconds.

		// Variables.
		private static byte globalFrameID = 1;

		private XBeeDevice xbeeDevice;

		private IList<RemoteXBeeDevice> deviceList;

		private bool discovering = false;
		private bool running = false;

		private byte frameID;

		protected ILog logger;

		/**
		 * Instantiates a new {@code NodeDiscovery} object.
		 * 
		 * @param xbeeDevice XBee Device to perform the discovery operation.
		 * 
		 * @throws ArgumentNullException If {@code xbeeDevice == null}.
		 * 
		 * @see XBeeDevice
		 */
		public NodeDiscovery(XBeeDevice xbeeDevice)
		{
			if (xbeeDevice == null)
				throw new ArgumentNullException("Local XBee device cannot be null.");

			this.xbeeDevice = xbeeDevice;

			frameID = globalFrameID;
			globalFrameID++;
			if (globalFrameID == 0xFF)
				globalFrameID = 1;

			logger = LogManager.GetLogger(this.GetType());
		}

		/**
		 * Discovers and reports the first remote XBee device that matches the 
		 * supplied identifier.
		 * 
		 * <p>This method blocks until the device is discovered or the configured 
		 * timeout in the device (NT) expires.</p>
		 * 
		 * @param id The identifier of the device to be discovered.
		 * 
		 * @return The discovered remote XBee device with the given identifier, 
		 *         {@code null} if the timeout expires and the device was not found.
		 * 
		 * @throws InterfaceNotOpenException if the device is not open.
		 * @throws XBeeException if there is an error sending the discovery command.
		 * 
		 * @see #discoverDevices(List)
		 */
		public RemoteXBeeDevice discoverDevice(string id)/*throws XBeeException */{
			// Check if the connection is open.
			if (!xbeeDevice.IsOpen)
				throw new InterfaceNotOpenException();

			logger.DebugFormat("{0}ND for {1} device.", xbeeDevice.ToString(), id);

			running = true;
			discovering = true;

			performNodeDiscovery(null, id);

			XBeeNetwork network = xbeeDevice.GetNetwork();
			RemoteXBeeDevice rDevice = null;

			if (deviceList != null && deviceList.Count > 0)
			{
				rDevice = deviceList[0];
				if (rDevice != null)
					rDevice = network.addRemoteDevice(rDevice);
			}

			return rDevice;
		}

		/**
		 * Discovers and reports all remote XBee devices that match the supplied 
		 * identifiers.
		 * 
		 * <p>This method blocks until the configured timeout in the device (NT) 
		 * expires.</p>
		 * 
		 * @param ids List which contains the identifiers of the devices to be 
		 *            discovered.
		 * 
		 * @return A list of the discovered remote XBee devices with the given 
		 *         identifiers.
		 * 
		 * @throws InterfaceNotOpenException if the device is not open.
		 * @throws XBeeException if there is an error discovering the devices.
		 * 
		 * @see #discoverDevice(String)
		 */
		public List<RemoteXBeeDevice> discoverDevices(IList<String> ids)/*throws XBeeException */{
			// Check if the connection is open.
			if (!xbeeDevice.IsOpen)
				throw new InterfaceNotOpenException();

			logger.DebugFormat("{0}ND for all {1} devices.", xbeeDevice.ToString(), ids.ToString());

			running = true;
			discovering = true;

			performNodeDiscovery(null, null);

			List<RemoteXBeeDevice> foundDevices = new List<RemoteXBeeDevice>(0);
			if (deviceList == null)
				return foundDevices;

			XBeeNetwork network = xbeeDevice.GetNetwork();

			foreach (RemoteXBeeDevice d in deviceList)
			{
				String nID = d.GetNodeID();
				if (nID == null)
					continue;
				foreach (String id in ids)
				{
					if (nID.Equals(id))
					{
						RemoteXBeeDevice rDevice = network.addRemoteDevice(d);
						if (rDevice != null && !foundDevices.Contains(rDevice))
							foundDevices.Add(rDevice);
					}
				}
			}

			return foundDevices;
		}

		/**
		 * Performs a node discover to search for XBee devices in the same network. 
		 * 
		 * @param listeners Discovery listeners to be notified about process events.
		 * 
		 * @throws InterfaceNotOpenException if the device is not open.
		 * @throws ArgumentNullException if {@code listeners == null}.
		 * 
		 * @see #isRunning()
		 * @see #stopDiscoveryProcess()
		 */
		public void startDiscoveryProcess(IList<IDiscoveryListener> listeners)
		{
			// Check if the connection is open.
			if (!xbeeDevice.IsOpen)
				throw new InterfaceNotOpenException();
			if (listeners == null)
				throw new ArgumentNullException("Listeners list cannot be null.");

			running = true;
			discovering = true;

			Thread discoveryThread = new Thread(new ThreadStart(() =>
			{
				try
				{
					performNodeDiscovery(listeners, null);
				}
				catch (XBeeException e)
				{
					// Notify the listeners about the error and finish.
					notifyDiscoveryFinished(listeners, e.Message);
				}
			}));

			discoveryThread.Start();
		}

		/**
		 * Stops the discovery process if it is running.
		 * 
		 * @see #isRunning()
		 * @see #startDiscoveryProcess(List)
		 */
		public void stopDiscoveryProcess()
		{
			discovering = false;
		}

		/**
		 * Retrieves whether the discovery process is running.
		 * 
		 * @return {@code true} if the discovery process is running, {@code false} 
		 *         otherwise.
		 * 
		 * @see #startDiscoveryProcess(List)
		 * @see #stopDiscoveryProcess()
		 */
		public bool isRunning()
		{
			return running;
		}

		/**
		 * Performs a node discover to search for XBee devices in the same network. 
		 * 
		 * <p>This method blocks until the configured timeout expires.</p>
		 * 
		 * @param listeners Discovery listeners to be notified about process events.
		 * @param id The identifier of the device to be discovered, or {@code null}
		 *           to discover all devices in the network.
		 * 
		 * @throws XBeeException if there is an error sending the discovery command.
		 */
		private void performNodeDiscovery(IList<IDiscoveryListener> listeners, String id)/*throws XBeeException */{
			try
			{
				DiscoverDevicesAPI(listeners, id);

				// Notify that the discovery finished without errors.
				notifyDiscoveryFinished(listeners, null);
			}
			finally
			{
				running = false;
				discovering = false;
			}
		}

		class CustomPacketReceiveListener : IPacketReceiveListener
		{
			NodeDiscovery _node;
			string _id;
			IList<IDiscoveryListener> _listeners;

			public CustomPacketReceiveListener(NodeDiscovery node, IList<IDiscoveryListener> listeners, string id)
			{
				_node = node;
				_id = id;
				_listeners = listeners;
			}
			public async void PacketReceived(XBeePacket receivedPacket)
			{
				if (!_node.discovering)
					return;
				RemoteXBeeDevice rdevice = null;

				byte[] commandValue = _node.GetRemoteDeviceData((XBeeAPIPacket)receivedPacket);

				rdevice = await _node.ParseDiscoveryAPIData(commandValue, _node.xbeeDevice);

				// If a device with a specific id is being search and it is 
				// already found, return it.
				if (_id != null)
				{
					if (rdevice != null && _id.Equals(rdevice.GetNodeID()))
					{
						lock (_node.deviceList)
						{
							_node.deviceList.Add(rdevice);
						}
						// If the local device is 802.15.4 wait until the 'end' command is received.
						if (_node.xbeeDevice.GetXBeeProtocol() != XBeeProtocol.RAW_802_15_4)
							_node.discovering = false;
					}
				}
				else if (rdevice != null)
					_node.notifyDeviceDiscovered(_listeners, rdevice);
			}
		}

		/**
		 * Performs the device discovery in API1 or API2 (API Escaped) mode.
		 * 
		 * @param listeners Discovery listeners to be notified about process events.
		 * @param id The identifier of the device to be discovered, or {@code null}
		 *           to discover all devices in the network.
		 * 
		 * @throws XBeeException if there is an error sending the discovery command.
		 */
		private void DiscoverDevicesAPI(IList<IDiscoveryListener> listeners, string id)/*throws XBeeException */{
			if (deviceList == null)
				deviceList = new List<RemoteXBeeDevice>();
			deviceList.Clear();

			IPacketReceiveListener packetReceiveListener = new CustomPacketReceiveListener(this, listeners, id);

			logger.DebugFormat("{0}Start listening.", xbeeDevice.ToString());
			xbeeDevice.AddPacketListener(packetReceiveListener);

			try
			{
				long deadLine = 0;
				var stopwatch = Stopwatch.StartNew();

				// In 802.15.4 devices, the discovery finishes when the 'end' command 
				// is received, so it's not necessary to calculate the timeout.
				if (xbeeDevice.GetXBeeProtocol() != XBeeProtocol.RAW_802_15_4)
					deadLine += CalculateTimeout(listeners);

				sendNodeDiscoverCommand(id);

				if (xbeeDevice.GetXBeeProtocol() != XBeeProtocol.RAW_802_15_4)
				{
					// Wait for scan timeout.
					while (discovering)
					{
						if (stopwatch.ElapsedMilliseconds < deadLine)
							try
							{
								Thread.Sleep(100);
							}
							catch (ThreadInterruptedException ) { }
						else
						{
							stopwatch.Stop();
							discovering = false;
						}
					}
				}
				else
				{
					// Wait until the 'end' command is received.
					while (discovering)
					{
						try
						{
							Thread.Sleep(100);
						}
						catch (ThreadInterruptedException ) { }
					}
				}
			}
			finally
			{
				xbeeDevice.RemovePacketListener(packetReceiveListener);
				logger.DebugFormat("{0}Stop listening.", xbeeDevice.ToString());
			}
		}

		/**
		 * Calculates the maximum response time, in milliseconds, for network
		 * discovery responses.
		 * 
		 * @param listeners Discovery listeners to be notified about process events.
		 * 
		 * @return Maximum network discovery timeout.
		 */
		private long CalculateTimeout(IList<IDiscoveryListener> listeners)
		{
			long timeout = -1;

			// Read the maximum discovery timeout (N?).
			try
			{
				timeout = ByteUtils.ByteArrayToLong(xbeeDevice.GetParameter("N?"));
			}
			catch (XBeeException )
			{
				logger.DebugFormat("{0}Could not read the N? value.", xbeeDevice.ToString());
			}

			// If N? does not exist, read the NT parameter.
			if (timeout == -1)
			{
				// Read the device timeout (NT).
				try
				{
					timeout = ByteUtils.ByteArrayToLong(xbeeDevice.GetParameter("NT")) * 100;
				}
				catch (XBeeException )
				{
					timeout = DEFAULT_TIMEOUT;
					String error = "Could not read the discovery timeout from the device (NT). "
							+ "The default timeout (" + DEFAULT_TIMEOUT + " ms.) will be used.";
					notifyDiscoveryError(listeners, error);
				}

				// In DigiMesh/DigiPoint the network discovery timeout is NT + the 
				// network propagation time. It means that if the user sends an AT 
				// command just after NT ms, s/he will receive a timeout exception. 
				if (xbeeDevice.GetXBeeProtocol() == XBeeProtocol.DIGI_MESH)
				{
					timeout += 3000;
				}
				else if (xbeeDevice.GetXBeeProtocol() == XBeeProtocol.DIGI_POINT)
				{
					timeout += 8000;
				}
			}

			if (xbeeDevice.GetXBeeProtocol() == XBeeProtocol.DIGI_MESH)
			{
				try
				{
					// If the module is 'Sleep support', wait another discovery cycle.
					bool isSleepSupport = ByteUtils.ByteArrayToInt(xbeeDevice.GetParameter("SM")) == 7;
					if (isSleepSupport)
						timeout += timeout + (timeout / 10L);
				}
				catch (XBeeException )
				{
					logger.DebugFormat("{0}Could not determine if the module is 'Sleep Support'.", xbeeDevice.ToString());
				}
			}

			return timeout;
		}

		/**
		 * Returns a byte array with the remote device data to be parsed.
		 * 
		 * @param packet The API packet that contains the data.
		 * 
		 * @return A byte array with the data to be parsed.
		 */
		private byte[] GetRemoteDeviceData(XBeeAPIPacket packet)
		{
			byte[] data = null;

			logger.TraceFormat("{0}Received packet: {1}.", xbeeDevice.ToString(), packet);

			APIFrameType frameType = packet.FrameType;
			switch (frameType)
			{
				case APIFrameType.AT_COMMAND_RESPONSE:
					ATCommandResponsePacket atResponse = (ATCommandResponsePacket)packet;
					// Check the frame ID.
					if (atResponse.FrameID != frameID)
						return null;
					// Check the command.
					if (!atResponse.Command.Equals(ND_COMMAND))
						return null;
					// Check if the 'end' command is received (empty response with OK status).
					if (atResponse.CommandValue == null || atResponse.CommandValue.Length == 0)
					{
						discovering = atResponse.Status != ATCommandStatus.OK;
						return null;
					}

					logger.DebugFormat("{0}Received self response: {1}.", xbeeDevice.ToString(), packet);

					data = atResponse.CommandValue;
					break;
				default:
					break;
			}

			return data;
		}

		/**
		 * Parses the given node discovery API data to create and return a remote 
		 * XBee Device.
		 * 
		 * @param data Byte array with the data to parse.
		 * @param localDevice The local device that received the remote XBee data.
		 * 
		 * @return Discovered XBee device.
		 */
		private async Task<RemoteXBeeDevice> ParseDiscoveryAPIData(byte[] data, XBeeDevice localDevice)
		{
			if (data == null)
				return null;

			RemoteXBeeDevice device = null;
			XBee16BitAddress addr16 = null;
			XBee64BitAddress addr64 = null;
			String id = null;
			// TODO role of the device: coordinator, router, end device or unknown.
			//XBeeDeviceType role = XBeeDeviceType.UNKNOWN;
			int signalStrength = 0;
			byte[] profileID = null;
			byte[] manufacturerID = null;

			using (var inputStream = new MemoryStream(data))
			{
				// Read 16 bit address.
				addr16 = new XBee16BitAddress(await ByteUtils.ReadBytes(2, inputStream));
				// Read 64 bit address.
				addr64 = new XBee64BitAddress(await ByteUtils.ReadBytes(8, inputStream));


				switch (localDevice.GetXBeeProtocol())
				{
					case XBeeProtocol.ZIGBEE:
					case XBeeProtocol.DIGI_MESH:
					case XBeeProtocol.ZNET:
					case XBeeProtocol.DIGI_POINT:
					case XBeeProtocol.XLR:
					// TODO [XLR_DM] The next version of the XLR will add DigiMesh support.
					// For the moment only point-to-multipoint is supported in this kind of devices.
					case XBeeProtocol.XLR_DM:
						// Read node identifier.
						id = ByteUtils.ReadString(inputStream);
						// Read parent address.
						XBee16BitAddress parentAddress = new XBee16BitAddress(await ByteUtils.ReadBytes(2, inputStream));
						// TODO Read device type.
						//role = XBeeDeviceType.get(inputStream.read());
						// Consume status byte, it is not used yet.
						await ByteUtils.ReadBytes(1, inputStream);
						// Read profile ID.
						profileID = await ByteUtils.ReadBytes(2, inputStream);
						// Read manufacturer ID.
						manufacturerID = await ByteUtils.ReadBytes(2, inputStream);

						logger.DebugFormat("{0}Discovered {1} device: 16-bit[{2}], 64-bit[{3}], id[{4}], parent[{5}], profile[{6}], manufacturer[{7}].",
								xbeeDevice.ToString(), localDevice.GetXBeeProtocol().GetDescription(), addr16,
								addr64, id, parentAddress, HexUtils.ByteArrayToHexString(profileID),
								HexUtils.ByteArrayToHexString(manufacturerID));

						break;
					case XBeeProtocol.RAW_802_15_4:
						// Read strength signal byte.
						signalStrength = inputStream.ReadByte();
						// Read node identifier.
						id = ByteUtils.ReadString(inputStream);

						logger.DebugFormat("{0}Discovered {1} device: 16-bit[{2}], 64-bit[{3}], id[{4}], rssi[{5}].",
								xbeeDevice.ToString(), localDevice.GetXBeeProtocol().GetDescription(), addr16, addr64, id, signalStrength);

						break;
					case XBeeProtocol.UNKNOWN:
					default:
						logger.DebugFormat("{0}Discovered {1} device: 16-bit[{2}], 64-bit[{3}].",
								xbeeDevice.ToString(), localDevice.GetXBeeProtocol().GetDescription(), addr16, addr64);
						break;
				}

				// Create device and fill with parameters.
				switch (localDevice.GetXBeeProtocol())
				{
					case XBeeProtocol.ZIGBEE:
						device = new RemoteZigBeeDevice(localDevice, addr64, addr16, id/*, role*/);
						// TODO profileID and manufacturerID
						break;
					case XBeeProtocol.DIGI_MESH:
						device = new RemoteDigiMeshDevice(localDevice, addr64, id/*, role*/);
						// TODO profileID and manufacturerID
						break;
					case XBeeProtocol.DIGI_POINT:
						device = new RemoteDigiPointDevice(localDevice, addr64, id/*, role*/);
						// TODO profileID and manufacturerID
						break;
					case XBeeProtocol.RAW_802_15_4:
						device = new RemoteRaw802Device(localDevice, addr64, addr16, id/*, role*/);
						// TODO signalStrength
						break;
					default:
						device = new RemoteXBeeDevice(localDevice, addr64, addr16, id/*, role*/);
						break;
				}
			}

			return device;
		}

		/**
		 * Sends the node discover ({@code ND}) command.
		 * 
		 * @param id The identifier of the device to be discovered, or {@code null}
		 *           to discover all devices in the network.
		 * 
		 * @throws XBeeException if there is an error writing in the communication interface.
		 */
		private void sendNodeDiscoverCommand(String id)/*throws XBeeException */{
			if (id == null)
				xbeeDevice.SendPacketAsync(new ATCommandPacket(frameID, ND_COMMAND, ""));
			else
				xbeeDevice.SendPacketAsync(new ATCommandPacket(frameID, ND_COMMAND, id));
		}

		/**
		 * Notifies the given discovery listeners that a device was discovered.
		 * 
		 * @param listeners The discovery listeners to be notified.
		 * @param device The remote device discovered.
		 */
		private void notifyDeviceDiscovered(IList<IDiscoveryListener> listeners, RemoteXBeeDevice device)
		{
			if (listeners == null)
			{
				lock (deviceList)
				{
					deviceList.Add(device);
				}
				return;
			}

			XBeeNetwork network = xbeeDevice.GetNetwork();

			RemoteXBeeDevice addedDev = network.addRemoteDevice(device);
			if (addedDev != null)
			{
				foreach (IDiscoveryListener listener in listeners)
					listener.DeviceDiscovered(addedDev);
			}
			else
			{
				String error = "Error adding device '" + device + "' to the network.";
				notifyDiscoveryError(listeners, error);
			}
		}

		/**
		 * Notifies the given discovery listeners about the provided error.
		 * 
		 * @param listeners The discovery listeners to be notified.
		 * @param error The error to notify.
		 */
		private void notifyDiscoveryError(IList<IDiscoveryListener> listeners, String error)
		{
			logger.ErrorFormat("{0}Error discovering devices: {1}", xbeeDevice.ToString(), error);

			if (listeners == null)
				return;

			foreach (IDiscoveryListener listener in listeners)
				listener.DiscoveryError(error);
		}

		/**
		 * Notifies the given discovery listeners that the discovery process has 
		 * finished.
		 * 
		 * @param listeners The discovery listeners to be notified.
		 * @param error The error message, or {@code null} if the process finished 
		 *              successfully.
		 */
		private void notifyDiscoveryFinished(IList<IDiscoveryListener> listeners, String error)
		{
			if (error != null && error.Length > 0)
				logger.ErrorFormat("{0}Finished discovery: {1}", xbeeDevice.ToString(), error);
			else
				logger.DebugFormat("{0}Finished discovery.", xbeeDevice.ToString());

			if (listeners == null)
				return;

			foreach (IDiscoveryListener listener in listeners)
				listener.DiscoveryFinished(error);
		}

		/*
		 * (non-Javadoc)
		 * @see java.lang.Object#toString()
		 */
		//@Override
		public override string ToString()
		{
			return GetType().Name + " [" + xbeeDevice.ToString() + "] @" +
					GetHashCode().ToString("x");
		}
	}
}