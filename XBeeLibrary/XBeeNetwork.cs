using Common.Logging;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.Listeners;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Kveer.XBeeApi
{
	/**
	 * This class represents an XBee Network.
	 *  
	 * <p>The network allows the discovery of remote devices in the same network 
	 * as the local one and stores them.</p>
	 */
	public class XBeeNetwork
	{

		// Variables.

		private XBeeDevice localDevice;

		private ConcurrentDictionary<XBee64BitAddress, RemoteXBeeDevice> remotesBy64BitAddr;
		private ConcurrentDictionary<XBee16BitAddress, RemoteXBeeDevice> remotesBy16BitAddr;

		private List<IDiscoveryListener> discoveryListeners = new List<IDiscoveryListener>();

		private NodeDiscovery nodeDiscovery;

		protected ILog logger;

		/**
		 * Instantiates a new {@code XBeeNetwork} object.
		 * 
		 * @param device Local XBee device to get the network from.
		 * 
		 * @throws ArgumentNullException if {@code device == null}.
		 * 
		 * @see XBeeDevice
		 */
		public XBeeNetwork(XBeeDevice device)
		{
			if (device == null)
				throw new ArgumentNullException("Local XBee device cannot be null.");

			localDevice = device;
			remotesBy64BitAddr = new ConcurrentDictionary<XBee64BitAddress, RemoteXBeeDevice>();
			remotesBy16BitAddr = new ConcurrentDictionary<XBee16BitAddress, RemoteXBeeDevice>();
			nodeDiscovery = new NodeDiscovery(localDevice);

			logger = LogManager.GetLogger(this.GetType());
		}

		/**
		 * Discovers and reports the first remote XBee device that matches the 
		 * supplied identifier.
		 * 
		 * <p>This method blocks until the device is discovered or the configured 
		 * timeout expires. To configure the discovery timeout, use the method
		 * {@link #setDiscoveryTimeout(long)}.</p>
		 * 
		 * <p>To configure the discovery options, use the 
		 * {@link #setDiscoveryOptions(Set)} method.</p> 
		 * 
		 * @param id The identifier of the device to be discovered.
		 * 
		 * @return The discovered remote XBee device with the given identifier, 
		 *         {@code null} if the timeout expires and the device was not found.
		 * 
		 * @throws ArgumentException if {@code id.length() == 0}.
		 * @throws InterfaceNotOpenException if the device is not open.
		 * @throws ArgumentNullException if {@code id == null}.
		 * @throws XBeeException if there is an error discovering the device.
		 * 
		 * @see #discoverDevices(List)
		 * @see #getDevice(String)
		 * @see RemoteXBeeDevice
		 */
		public RemoteXBeeDevice discoverDevice(String id)/*throws XBeeException */{
			if (id == null)
				throw new ArgumentNullException("Device identifier cannot be null.");
			if (id.Length == 0)
				throw new ArgumentException("Device identifier cannot be an empty string.");

			logger.DebugFormat("{0}Discovering '{1}' device.", localDevice.ToString(), id);

			return nodeDiscovery.discoverDevice(id);
		}

		/**
		 * Discovers and reports all remote XBee devices that match the supplied 
		 * identifiers.
		 * 
		 * <p>This method blocks until the configured timeout expires. To configure 
		 * the discovery timeout, use the method {@link #setDiscoveryTimeout(long)}.
		 * </p>
		 * 
		 * <p>To configure the discovery options, use the 
		 * {@link #setDiscoveryOptions(Set)} method.</p> 
		 * 
		 * @param ids List which contains the identifiers of the devices to be 
		 *            discovered.
		 * 
		 * @return A list of the discovered remote XBee devices with the given 
		 *         identifiers.
		 * 
		 * @throws ArgumentException if {@code ids.size() == 0}.
		 * @throws InterfaceNotOpenException if the device is not open.
		 * @throws ArgumentNullException if {@code ids == null}.
		 * @throws XBeeException if there is an error discovering the devices.
		 * 
		 * @see #discoverDevice(String)
		 * @see RemoteXBeeDevice
		 */
		public List<RemoteXBeeDevice> discoverDevices(IList<String> ids)/*throws XBeeException */{
			if (ids == null)
				throw new ArgumentNullException("List of device identifiers cannot be null.");
			if (ids.Count == 0)
				throw new ArgumentException("List of device identifiers cannot be empty.");

			logger.DebugFormat("{0}Discovering all '{1}' devices.", localDevice.ToString(), ids.ToString());

			return nodeDiscovery.discoverDevices(ids);
		}

		/**
		 * Adds the given discovery listener to the list of listeners to be notified 
		 * when the discovery process is running.
		 * 
		 * <p>If the listener has already been included, this method does nothing.
		 * </p>
		 * 
		 * @param listener Listener to be notified when the discovery process is
		 *                 running.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}.
		 * 
		 * @see com.digi.xbee.api.listeners.IDiscoveryListener
		 * @see #removeDiscoveryListener(IDiscoveryListener)
		 */
		public void addDiscoveryListener(IDiscoveryListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("Listener cannot be null.");

			lock (discoveryListeners)
			{
				if (!discoveryListeners.Contains(listener))
					discoveryListeners.Add(listener);
			}
		}

		/**
		 * Removes the given discovery listener from the list of discovery 
		 * listeners.
		 * 
		 * <p>If the listener is not included in the list, this method does nothing.
		 * </p>
		 * 
		 * @param listener Discovery listener to remove.
		 * 
		 * @throws ArgumentNullException if {@code listener == null}.
		 * 
		 * @see com.digi.xbee.api.listeners.IDiscoveryListener
		 * @see #addDiscoveryListener(IDiscoveryListener)
		 */
		public void removeDiscoveryListener(IDiscoveryListener listener)
		{
			if (listener == null)
				throw new ArgumentNullException("Listener cannot be null.");

			lock (discoveryListeners)
			{
				if (discoveryListeners.Contains(listener))
					discoveryListeners.Remove(listener);
			}
		}

		/**
		 * Starts the discovery process with the configured timeout and options.
		 * 
		 * <p>To be notified every time an XBee device is discovered, add a
		 * {@code IDiscoveryListener} using the 
		 * {@link #addDiscoveryListener(IDiscoveryListener)} method before starting
		 * the discovery process.</p>
		 * 
		 * <p>To configure the discovery timeout, use the 
		 * {@link #setDiscoveryTimeout(long)} method.</p>
		 * 
		 * <p>To configure the discovery options, use the 
		 * {@link #setDiscoveryOptions(Set)} method.</p> 
		 * 
		 * @throws IllegalStateException if the discovery process is already running.
		 * @throws InterfaceNotOpenException if the device is not open.
		 * 
		 * @see #addDiscoveryListener(IDiscoveryListener)
		 * @see #stopDiscoveryProcess()
		 */
		public void startDiscoveryProcess()
		{
			if (isDiscoveryRunning())
				throw new InvalidOperationException("The discovery process is already running.");

			lock (discoveryListeners)
			{
				nodeDiscovery.startDiscoveryProcess(discoveryListeners);
			}
		}

		/**
		 * Stops the discovery process if it is running.
		 * 
		 * <p>Note that DigiMesh/DigiPoint devices are blocked until the discovery
		 * time configured (NT parameter) has elapsed, so if you try to get/set
		 * any parameter during the discovery process you will receive a timeout 
		 * exception.</p>
		 * 
		 * @see #isDiscoveryRunning()
		 * @see #removeDiscoveryListener(IDiscoveryListener)
		 * @see #startDiscoveryProcess()
		 */
		public void stopDiscoveryProcess()
		{
			nodeDiscovery.stopDiscoveryProcess();
		}

		/**
		 * Retrieves whether the discovery process is running or not.
		 * 
		 * @return {@code true} if the discovery process is running, {@code false} 
		 *         otherwise.
		 * 
		 * @see #startDiscoveryProcess()
		 * @see #stopDiscoveryProcess()
		 */
		public bool isDiscoveryRunning()
		{
			return nodeDiscovery.isRunning();
		}

		/**
		 * Configures the discovery timeout ({@code NT} parameter) with the given 
		 * value.
		 * 
		 * <p>Note that in some protocols, the discovery process may take longer
		 * than the value set in this method due to the network propagation time.
		 * </p>
		 * 
		 * @param timeout New discovery timeout in milliseconds.
		 * 
		 * @throws TimeoutException if there is a timeout setting the discovery
		 *                          timeout.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #setDiscoveryOptions(Set)
		 */
		public void setDiscoveryTimeout(long timeout)/*throws TimeoutException, XBeeException */{
			if (timeout <= 0)
				throw new ArgumentException("Timeout must be bigger than 0.");

			localDevice.SetParameter("NT", ByteUtils.LongToByteArray(timeout / 100));
		}

		/**
		 * Configures the discovery options ({@code NO} parameter) with the given 
		 * value.
		 * 
		 * @param options New discovery options.
		 * 
		 * @throws TimeoutException if there is a timeout setting the discovery
		 *                          options.
		 * @throws XBeeException if there is any other XBee related exception.
		 * 
		 * @see #setDiscoveryTimeout(long)
		 * @see DiscoveryOptions
		 */
		public void setDiscoveryOptions(ISet<DiscoveryOptions> options)/*throws TimeoutException, XBeeException */{
			if (options == null)
				throw new ArgumentNullException("Options cannot be null.");

			int value = DiscoveryOptions.APPEND_DD.CalculateDiscoveryValue(localDevice.GetXBeeProtocol(), options);
			localDevice.SetParameter("NO", ByteUtils.IntToByteArray(value));
		}

		/**
		 * Returns all remote devices already contained in the network.
		 * 
		 * <p>Note that this method <b>does not perform a discovery</b>, only 
		 * returns the devices that have been previously discovered.</p>
		 * 
		 * @return A list with all XBee devices in the network.
		 * 
		 * @see #getDevices(String)
		 * @see RemoteXBeeDevice
		 */
		public IList<RemoteXBeeDevice> getDevices()
		{
			//var nodes = new List<RemoteXBeeDevice>();
			//nodes.addAll(remotesBy64BitAddr.Values);
			//nodes.addAll(remotesBy16BitAddr.Values);
			//return nodes;
			return remotesBy64BitAddr.Values.Union(remotesBy16BitAddr.Values).ToList();
		}

		/**
		 * Returns all remote devices that match the supplied identifier.
		 * 
		 * <p>Note that this method <b>does not perform a discovery</b>, only 
		 * returns the devices that have been previously discovered.</p>
		 * 
		 * @param id The identifier of the devices to be retrieved.
		 * 
		 * @return A list of the remote XBee devices contained in the network with 
		 *         the given identifier.
		 * 
		 * @throws ArgumentException if {@code id.length() == 0}.
		 * @throws ArgumentNullException if {@code id == null}.
		 * 
		 * @see #getDevice(String)
		 * @see RemoteXBeeDevice
		 */
		public List<RemoteXBeeDevice> getDevices(String id)
		{
			if (id == null)
				throw new ArgumentNullException("Device identifier cannot be null.");
			if (id.Length == 0)
				throw new ArgumentException("Device identifier cannot be an empty string.");

			List<RemoteXBeeDevice> devices = new List<RemoteXBeeDevice>();

			// Look in the 64-bit map.
			foreach (RemoteXBeeDevice remote in remotesBy64BitAddr.Values)
			{
				if (remote.GetNodeID().Equals(id))
					devices.Add(remote);
			}
			// Look in the 16-bit map.
			foreach (RemoteXBeeDevice remote in remotesBy16BitAddr.Values)
			{
				if (remote.GetNodeID().Equals(id))
					devices.Add(remote);
			}
			// Return the list.
			return devices;
		}

		/**
		 * Returns the first remote device that matches the supplied identifier.
		 * 
		 * <p>Note that this method <b>does not perform a discovery</b>, only 
		 * returns the device that has been previously discovered.</p>
		 * 
		 * @param id The identifier of the device to be retrieved.
		 * 
		 * @return The remote XBee device contained in the network with the given 
		 *         identifier, {@code null} if the network does not contain any 
		 *         device with that Node ID.
		 * 
		 * @throws ArgumentException if {@code id.length() == 0}.
		 * @throws ArgumentNullException if {@code id == null}.
		 * 
		 * @see #discoverDevice(String)
		 * @see #getDevices(String)
		 * @see RemoteXBeeDevice
		 */
		public RemoteXBeeDevice getDevice(String id)
		{
			if (id == null)
				throw new ArgumentNullException("Device identifier cannot be null.");
			if (id.Length == 0)
				throw new ArgumentException("Device identifier cannot be an empty string.");

			// Look in the 64-bit map.
			foreach (RemoteXBeeDevice remote in remotesBy64BitAddr.Values)
			{
				if (remote.GetNodeID().Equals(id))
					return remote;
			}
			// Look in the 16-bit map.
			foreach (RemoteXBeeDevice remote in remotesBy16BitAddr.Values)
			{
				if (remote.GetNodeID().Equals(id))
					return remote;
			}
			// The given ID is not in the network.
			return null;
		}

		/**
		 * Returns the remote device already contained in the network whose 64-bit 
		 * address matches the given one.
		 * 
		 * <p>Note that this method <b>does not perform a discovery</b>, only 
		 * returns the device that has been previously discovered.</p>
		 * 
		 * @param address The 64-bit address of the device to be retrieved.
		 * 
		 * @return The remote device in the network or {@code null} if it is not 
		 *         found.
		 * 
		 * @throws ArgumentException if {@code address.equals(XBee64BitAddress.UNKNOWN_ADDRESS)}.
		 * @throws ArgumentNullException if {@code address == null}.
		 */
		public RemoteXBeeDevice getDevice(XBee64BitAddress address)
		{
			if (address == null)
				throw new ArgumentNullException("64-bit address cannot be null.");
			if (address.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
				throw new ArgumentNullException("64-bit address cannot be unknown.");

			logger.DebugFormat("{0}Getting device '{1}' from network.", localDevice.ToString(), address);

			return remotesBy64BitAddr[address];
		}

		/**
		 * Returns the remote device already contained in the network whose 16-bit 
		 * address matches the given one.
		 * 
		 * <p>Note that this method <b>does not perform a discovery</b>, only 
		 * returns the device that has been previously discovered.</p>
		 * 
		 * @param address The 16-bit address of the device to be retrieved.
		 * 
		 * @return The remote device in the network or {@code null} if it is not 
		 *         found.
		 * 
		 * @throws ArgumentException if {@code address.equals(XBee16BitAddress.UNKNOWN_ADDRESS)}.
		 * @throws ArgumentNullException if {@code address == null}.
		 * @throws OperationNotSupportedException if the protocol of the local XBee device is DigiMesh or Point-to-Multipoint.
		 */
		public RemoteXBeeDevice getDevice(XBee16BitAddress address)/*throws OperationNotSupportedException */{
			if (localDevice.GetXBeeProtocol() == XBeeProtocol.DIGI_MESH)
				throw new OperationNotSupportedException("DigiMesh protocol does not support 16-bit addressing.");
			if (localDevice.GetXBeeProtocol() == XBeeProtocol.DIGI_POINT)
				throw new OperationNotSupportedException("Point-to-Multipoint protocol does not support 16-bit addressing.");
			if (address == null)
				throw new ArgumentNullException("16-bit address cannot be null.");
			if (address.Equals(XBee16BitAddress.UNKNOWN_ADDRESS))
				throw new ArgumentNullException("16-bit address cannot be unknown.");

			logger.DebugFormat("{0}Getting device '{1}' from network.", localDevice.ToString(), address);

			// The preference order is: 
			//    1.- Look in the 64-bit map 
			//    2.- Then in the 16-bit map.
			// This should be maintained in the 'addRemoteDevice' method.

			RemoteXBeeDevice devInNetwork = null;

			// Look in the 64-bit map.
			ICollection<RemoteXBeeDevice> devices = remotesBy64BitAddr.Values;
			foreach (RemoteXBeeDevice d in devices)
			{
				XBee16BitAddress a = get16BitAddress(d);
				if (a != null && a.Equals(address))
				{
					devInNetwork = d;
					break;
				}
			}

			// Look in the 16-bit map.
			if (devInNetwork == null)
				devInNetwork = remotesBy16BitAddr[address];

			return devInNetwork;
		}

		/**
		 * Adds the given remote device to the network. 
		 * 
		 * <p>Notice that this operation does not join the remote XBee device to the
		 * network; it just tells the network that it contains that device. However, 
		 * the device has only been added to the device list, and may not be 
		 * physically in the same network.</p>
		 * 
		 * <p>The way of adding a device to the network is based on the 64-bit 
		 * address. If it is not configured:</p>
		 * 
		 * <ul>
		 * <li>For 802.15.4 and ZigBee devices, it will use the 16-bit address.</li>
		 * <li>For the rest will return {@code false} as the result of the addition.</li>
		 * </ul>
		 * 
		 * @param remoteDevice The remote device to be added to the network.
		 * 
		 * @return The remote XBee Device instance in the network, {@code null} if
		 *         the device could not be successfully added.
		 * 
		 * @throws ArgumentNullException if {@code RemoteDevice == null}.
		 * 
		 * @see #addRemoteDevices(List)
		 * @see #removeRemoteDevice(RemoteXBeeDevice)
		 * @see RemoteXBeeDevice
		 */
		public RemoteXBeeDevice addRemoteDevice(RemoteXBeeDevice remoteDevice)
		{
			if (remoteDevice == null)
				throw new ArgumentNullException("Remote device cannot be null.");

			logger.DebugFormat("{0}Adding device '{1}' to network.", localDevice.ToString(), remoteDevice.ToString());

			RemoteXBeeDevice devInNetwork = null;
			XBee64BitAddress addr64 = remoteDevice.Get64BitAddress();
			XBee16BitAddress addr16 = get16BitAddress(remoteDevice);

			// Check if the device has 64-bit address.
			if (addr64 != null && !addr64.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
			{
				// The device has 64-bit address, so look in the 64-bit map.
				devInNetwork = remotesBy64BitAddr[addr64];
				if (devInNetwork != null)
				{
					// The device exists in the 64-bit map, so update the reference and return it.
					logger.DebugFormat("{0}Existing device '{1}' in network.", localDevice.ToString(), devInNetwork.ToString());
					devInNetwork.UpdateDeviceDataFrom(remoteDevice);
					return devInNetwork;
				}
				else
				{
					// The device does not exist in the 64-bit map, so check its 16-bit address.
					if (addr16 != null && !addr16.Equals(XBee16BitAddress.UNKNOWN_ADDRESS))
					{
						// The device has 16-bit address, so look in the 16-bit map.
						devInNetwork = remotesBy16BitAddr[addr16];
						if (devInNetwork != null)
						{
							// The device exists in the 16-bit map, so remove it and add it to the 64-bit map.
							logger.DebugFormat("{0}Existing device '{1}' in network.", localDevice.ToString(), devInNetwork.ToString());
							remotesBy16BitAddr.TryRemove(addr16, out devInNetwork);
							devInNetwork.UpdateDeviceDataFrom(remoteDevice);
							remotesBy64BitAddr.AddOrUpdate(addr64, devInNetwork, (k, v) => devInNetwork);
							return devInNetwork;
						}
						else
						{
							// The device does not exist in the 16-bit map, so add it to the 64-bit map.
							remotesBy64BitAddr.AddOrUpdate(addr64, remoteDevice, (k, v) => remoteDevice);
							return remoteDevice;
						}
					}
					else
					{
						// The device has not 16-bit address, so add it to the 64-bit map.
						remotesBy64BitAddr.AddOrUpdate(addr64, remoteDevice, (k, v) => remoteDevice);
						return remoteDevice;
					}
				}
			}

			// If the device has not 64-bit address, check if it has 16-bit address.
			if (addr16 != null && !addr16.Equals(XBee16BitAddress.UNKNOWN_ADDRESS))
			{
				// The device has 16-bit address, so look in the 64-bit map.
				ICollection<RemoteXBeeDevice> devices = remotesBy64BitAddr.Values;
				foreach (RemoteXBeeDevice d in devices)
				{
					XBee16BitAddress a = get16BitAddress(d);
					if (a != null && a.Equals(addr16))
					{
						devInNetwork = d;
						break;
					}
				}
				// Check if the device exists in the 64-bit map.
				if (devInNetwork != null)
				{
					// The device exists in the 64-bit map, so update the reference and return it.
					logger.DebugFormat("{0}Existing device '{1}' in network.", localDevice.ToString(), devInNetwork.ToString());
					devInNetwork.UpdateDeviceDataFrom(remoteDevice);
					return devInNetwork;
				}
				else
				{
					// The device does not exist in the 64-bit map, so look in the 16-bit map.
					devInNetwork = remotesBy16BitAddr[addr16];
					if (devInNetwork != null)
					{
						// The device exists in the 16-bit map, so update the reference and return it.
						logger.DebugFormat("{0}Existing device '{1}' in network.", localDevice.ToString(), devInNetwork.ToString());
						devInNetwork.UpdateDeviceDataFrom(remoteDevice);
						return devInNetwork;
					}
					else
					{
						// The device does not exist in the 16-bit map, so add it.
						remotesBy16BitAddr.AddOrUpdate(addr16, remoteDevice, (k, v) => remoteDevice);
						return remoteDevice;
					}
				}
			}

			// If the device does not contain a valid address, return null.
			logger.ErrorFormat("{0}Remote device '{1}' cannot be added: 64-bit and 16-bit addresses must be specified.",
					localDevice.ToString(), remoteDevice.ToString());
			return null;
		}

		/**
		 * Adds the given list of remote devices to the network.
		 * 
		 * <p>Notice that this operation does not join the remote XBee devices to 
		 * the network; it just tells the network that it contains those devices. 
		 * However, the devices have only been added to the device list, and may 
		 * not be physically in the same network.</p>
		 * 
		 * <p>The way of adding a device to the network is based on the 64-bit 
		 * address. If it is not configured:</p>
		 * 
		 * <ul>
		 * <li>For 802.15.4 and ZigBee devices, the 16-bit address will be used instead.</li>
		 * <li>For the rest will return {@code false} as the result of the addition.</li>
		 * </ul>
		 * 
		 * @param list The list of remote devices to be added to the network.
		 * 
		 * @return A list with the successfully added devices to the network.
		 * 
		 * @throws ArgumentNullException if {@code list == null}.
		 * 
		 * @see #addRemoteDevice(RemoteXBeeDevice)
		 * @see RemoteXBeeDevice
		 */
		public List<RemoteXBeeDevice> addRemoteDevices(IList<RemoteXBeeDevice> list)
		{
			if (list == null)
				throw new ArgumentNullException("The list of remote devices cannot be null.");

			List<RemoteXBeeDevice> addedList = new List<RemoteXBeeDevice>(list.Count);

			if (list.Count == 0)
				return addedList;

			logger.DebugFormat("{0}Adding '{1}' devices to network.", localDevice.ToString(), list.Count);

			for (int i = 0; i < list.Count; i++)
			{
				RemoteXBeeDevice d = addRemoteDevice(list[i]);
				if (d != null)
					addedList.Add(d);
			}

			return addedList;
		}

		/**
		 * Removes the given remote XBee device from the network.
		 * 
		 * <p>Notice that this operation does not remove the remote XBee device 
		 * from the actual XBee network; it just tells the network object that it 
		 * will no longer contain that device. However, next time a discovery is 
		 * performed, it could be added again automatically.</p>
		 * 
		 * <p>This method will check for a device that matches the 64-bit address 
		 * of the provided one, if found, that device will be removed from the 
		 * corresponding list. In case the 64-bit address is not defined, it will 
		 * use the 16-bit address for DigiMesh and ZigBee devices.</p>
		 * 
		 * @param remoteDevice The remote device to be removed from the network.
		 * 
		 * @throws ArgumentNullException if {@code RemoteDevice == null}.
		 * 
		 * @see #addRemoteDevice(RemoteXBeeDevice)
		 * @see #clearDeviceList()
		 * @see RemoteXBeeDevice
		 */
		public void removeRemoteDevice(RemoteXBeeDevice remoteDevice)
		{
			if (remoteDevice == null)
				throw new ArgumentNullException("Remote device cannot be null.");

			RemoteXBeeDevice devInNetwork = null;

			// Look in the 64-bit map.
			XBee64BitAddress addr64 = remoteDevice.Get64BitAddress();
			if (addr64 != null && !addr64.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
			{
				if (remotesBy64BitAddr.TryRemove(addr64, out devInNetwork))
					return;
			}

			// If not found, look in the 16-bit map.
			XBee16BitAddress addr16 = get16BitAddress(remoteDevice);
			if (addr16 != null && !addr16.Equals(XBee16BitAddress.UNKNOWN_ADDRESS))
			{

				// The preference order is: 
				//    1.- Look in the 64-bit map 
				//    2.- Then in the 16-bit map.
				// This should be maintained in the 'getDeviceBy16BitAddress' method.

				// Look for the 16-bit address in the 64-bit map.
				ICollection<RemoteXBeeDevice> devices = remotesBy64BitAddr.Values;
				foreach (RemoteXBeeDevice d in devices)
				{
					XBee16BitAddress a = get16BitAddress(d);
					if (a != null && a.Equals(addr16))
					{
						RemoteXBeeDevice r;
						remotesBy64BitAddr.TryRemove(d.Get64BitAddress(), out r);
						return;
					}
				}

				// If not found, look for the 16-bit address in the 16-bit map. 
				// Remove the device.
				if (remotesBy16BitAddr.TryRemove(addr16, out devInNetwork))
					return;
			}

			// If the device does not contain a valid address log an error.
			if ((addr64 == null || addr64.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
					&& (addr16 == null || addr16.Equals(XBee16BitAddress.UNKNOWN_ADDRESS)))
				logger.ErrorFormat("{0}Remote device '{1}' cannot be removed: 64-bit and 16-bit addresses must be specified.",
						localDevice.ToString(), remoteDevice.ToString());
		}

		/**
		 * Removes all the devices from this network. 
		 * 
		 * <p>The network will be empty after this call returns.</p>
		 * 
		 * <p>Notice that this does not imply removing the XBee devices from the 
		 * actual XBee network; it just tells the object that the list should be 
		 * empty now. Next time a discovery is performed, the list could be filled 
		 * with the remote XBee devices found.</p>
		 * 
		 * @see #removeRemoteDevice(RemoteXBeeDevice)
		 */
		public void clearDeviceList()
		{
			logger.DebugFormat("{0}Clearing the network.", localDevice.ToString());
			remotesBy64BitAddr.Clear();
			remotesBy64BitAddr.Clear();
		}

		/**
		 * Returns the number of devices already discovered in this network.
		 * 
		 * @return The number of devices already discovered in this network.
		 */
		public int getNumberOfDevices()
		{
			return remotesBy64BitAddr.Count + remotesBy16BitAddr.Count;
		}

		/**
		 * Retrieves the 16-bit address of the given remote device.
		 * 
		 * @param device The remote device to get the 16-bit address.
		 * 
		 * @return The 16-bit address of the device, {@code null} if it does not
		 *         contain a valid one.
		 */
		private XBee16BitAddress get16BitAddress(RemoteXBeeDevice device)
		{
			if (device == null)
				return null;

			XBee16BitAddress address = null;

			switch (device.GetXBeeProtocol())
			{
				case XBeeProtocol.RAW_802_15_4:
					address = ((RemoteRaw802Device)device).Get16BitAddress();
					break;
				case XBeeProtocol.ZIGBEE:
					address = ((RemoteZigBeeDevice)device).Get16BitAddress();
					break;
				default:
					// TODO should we allow this operation for general remote devices?
					address = device.Get16BitAddress();
					break;
			}

			return address;
		}

		public override string ToString()
		{
			return string.Format("{0} [{1}] @{2}", GetType().Name, localDevice.ToString(),
					GetHashCode().ToString("x"));
		}
	}
}