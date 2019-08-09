/*
 * Copyright 2019, Digi International Inc.
 * Copyright 2014, 2015, Sébastien Rault.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core
{
	/// <summary>
	/// This class represents an XBee Network.
	/// </summary>
	/// <remarks>The network allows the discovery of remote devices in the same
	/// network as the local one and stores them.</remarks>
	public class XBeeNetwork
	{
		// Constants.
		public static readonly int MAX_SCAN_COUNTER = 1000;

		// Variables.
		private AbstractXBeeDevice localDevice;

		private ConcurrentDictionary<XBee64BitAddress, RemoteXBeeDevice> remotesBy64BitAddr;
		private ConcurrentDictionary<XBee16BitAddress, RemoteXBeeDevice> remotesBy16BitAddr;

		protected ILog logger;

		private NodeDiscovery nodeDiscovery;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="XBeeNetwork"/> object with the provided 
		/// parameters.
		/// </summary>
		/// <param name="device">The local XBee device to get the network from.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="device"/> == null</c>.</exception>
		internal XBeeNetwork(AbstractXBeeDevice device)
		{
			localDevice = device ?? throw new ArgumentNullException("Local XBee device cannot be null.");

			remotesBy64BitAddr = new ConcurrentDictionary<XBee64BitAddress, RemoteXBeeDevice>();
			remotesBy16BitAddr = new ConcurrentDictionary<XBee16BitAddress, RemoteXBeeDevice>();
			nodeDiscovery = new NodeDiscovery(localDevice);

			PanId = "?";
			Channel = "?";

			logger = LogManager.GetLogger(GetType());
		}

		// Events.
		/// <summary>
		/// Represents the method that will handle the device discovered event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="DeviceDiscoveredEventArgs"/>
		public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (nodeDiscovery != null)
					nodeDiscovery.DeviceDiscovered += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (nodeDiscovery != null)
					nodeDiscovery.DeviceDiscovered -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the discovery error event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="DiscoveryErrorEventArgs"/>
		public event EventHandler<DiscoveryErrorEventArgs> DiscoveryError
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (nodeDiscovery != null)
					nodeDiscovery.DiscoveryError += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (nodeDiscovery != null)
					nodeDiscovery.DiscoveryError -= value;
			}
		}

		/// <summary>
		/// Represents the method that will handle the discovery finished event.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the event handler is <c>null</c>.</exception>
		/// <seealso cref="DiscoveryFinishedEventArgs"/>
		public event EventHandler<DiscoveryFinishedEventArgs> DiscoveryFinished
		{
			add
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (nodeDiscovery != null)
					nodeDiscovery.DiscoveryFinished += value;
			}
			remove
			{
				if (value == null)
					throw new ArgumentNullException("Event handler cannot be null.");
				if (nodeDiscovery != null)
					nodeDiscovery.DiscoveryFinished -= value;
			}
		}

		// Properties.
		/// <summary>
		/// Indicates whether the discovery process is running or not.
		/// </summary>
		/// <returns><c>true</c> if the discovery process is running, <c>false</c> otherwise.</returns>
		public bool IsDiscoveryRunning => nodeDiscovery.IsRunning;

		/// <summary>
		/// The local XBee device, to which the network belongs.
		/// </summary>
		public XBeeDevice LocalDevice { get; private set; }

		/// <summary>
		/// The PAN ID of the network.
		/// </summary>
		public string PanId { get; private set; }

		/// <summary>
		/// The operating channel of the network.
		/// </summary>
		public string Channel { get; private set; }

		/// <summary>
		/// Discovers and reports the first remote XBee device that matches the supplied identifier.
		/// </summary>
		/// <remarks>This method blocks until the device is discovered or the configured timeout expires. 
		/// To configure the discovery timeout, use the <see cref="SetDiscoveryTimeout(long)"/> method.
		/// 
		/// To configure the discovery options, use the <see cref="SetDiscoveryOptions(ISet{DiscoveryOptions})"/> 
		/// method.</remarks>
		/// <param name="id">The identifier of the device to be discovered.</param>
		/// <returns>The discovered remote XBee device with the given identifier, <c>null</c> if the 
		/// timeout expires and the device was not found.</returns>
		/// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="id"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="XBeeException">If there is an error discovering the device.</exception>
		public RemoteXBeeDevice DiscoverDevice(string id)
		{
			if (id == null)
				throw new ArgumentNullException("Device identifier cannot be null.");
			if (id.Length == 0)
				throw new ArgumentException("Device identifier cannot be an empty string.");

			logger.DebugFormat("{0}Discovering '{1}' device.", localDevice.ToString(), id);

			return nodeDiscovery.DiscoverDevice(id);
		}

		/// <summary>
		/// Discovers and reports all remote XBee devices that match the supplied identifiers.
		/// </summary>
		/// <remarks>This method blocks until the configured timeout expires. To configure the discovery 
		/// timeout, use the method <see cref="SetDiscoveryTimeout(long)"/>.
		/// 
		/// To configure the discovery options, use the
		/// <see cref="SetDiscoveryOptions(ISet{DiscoveryOptions})"/> method.</remarks>
		/// <param name="ids">List which contains the identifiers of the devices to be discovered</param>
		/// <returns>A list of the discovered remote XBee devices with the given identifiers.</returns>
		/// <exception cref="ArgumentException">If <paramref name="ids"/> is empty.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="ids"/> == null</c>.</exception>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="XBeeException">If there is an error discovering the devices.</exception>
		public List<RemoteXBeeDevice> DiscoverDevices(IList<string> ids)
		{
			if (ids == null)
				throw new ArgumentNullException("List of device identifiers cannot be null.");
			if (ids.Count == 0)
				throw new ArgumentException("List of device identifiers cannot be empty.");

			logger.DebugFormat("{0}Discovering all '[{1}]' devices.", localDevice.ToString(), string.Join(", ", ids));

			return nodeDiscovery.DiscoverDevices(ids);
		}

		/// <summary>
		/// Starts the discovery process with the configured timeout and options.
		/// </summary>
		/// <remarks>To be notified every time an XBee device is discovered, add a <see cref="DeviceDiscovered"/> 
		/// event handler before starting the discovery process.
		/// 
		/// To configure the discovery timeout, use the <see cref="SetDiscoveryTimeout(long)"/> method.
		/// 
		/// To configure the discovery options, use the 
		/// <see cref="SetDiscoveryOptions(ISet{DiscoveryOptions})"/> method.</remarks>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="InvalidOperationException">If the discovery process is already running.</exception>
		public void StartNodeDiscoveryProcess()
		{
			if (IsDiscoveryRunning)
				throw new InvalidOperationException("The discovery process is already running.");

			nodeDiscovery.StartDiscoveryProcess();
		}

		/// <summary>
		/// Stops the discovery process if it is running.
		/// </summary>
		/// <remarks>Note that DigiMesh/DigiPoint devices are blocked until the discovery time configured 
		/// (NT parameter) has elapsed, so if you try to get/set any parameter during the discovery process 
		/// you will receive a timeout exception.</remarks>
		public void StopNodeDiscoveryProcess()
		{
			nodeDiscovery.StopDiscoveryProcess();
		}

		/// <summary>
		/// Configures the discovery timeout (<c>NT</c> parameter) with the given value.
		/// </summary>
		/// <param name="timeout">New discovery timeout in milliseconds.</param>
		/// <exception cref="ArgumentException">If <paramref name="timeout"/> is 0 or lesser.</exception>
		/// <exception cref="XBeeException">If there is any error setting the timeout.</exception>
		public void SetDiscoveryTimeout(long timeout)
		{
			if (timeout <= 0)
				throw new ArgumentException("Timeout must be bigger than 0.");

			localDevice.SetParameter("NT", ByteUtils.LongToByteArray(timeout / 100));
		}

		/// <summary>
		/// Configures the discovery options (<c>NO</c> parameter) with the given value.
		/// </summary>
		/// <param name="options">New discovery options.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="options"/> == null</c>.</exception>
		/// <exception cref="XBeeException">If there is any error setting the options.</exception>
		/// <seealso cref="DiscoveryOptions"/>
		public void SetDiscoveryOptions(ISet<DiscoveryOptions> options)
		{
			if (options == null)
				throw new ArgumentNullException("Options cannot be null.");

			int value = DiscoveryOptions.APPEND_DD.CalculateDiscoveryValue(localDevice.XBeeProtocol, options);
			localDevice.SetParameter("NO", ByteUtils.IntToByteArray(value));
		}

		/// <summary>
		/// Returns all remote devices already contained in the network.
		/// </summary>
		/// <remarks>Note that this method does not perform a discovery, only returns the devices that have 
		/// been previously discovered.</remarks>
		/// <returns>A list with all XBee devices in the network.</returns>
		public IList<RemoteXBeeDevice> GetDevices()
		{
			return remotesBy64BitAddr.Values.Union(remotesBy16BitAddr.Values).ToList();
		}

		/// <summary>
		/// Returns all remote devices that match the supplied identifier.
		/// </summary>
		/// <remarks>Note that this method does not perform a discovery, only returns the devices that have 
		/// been previously discovered.</remarks>
		/// <param name="id">The identifier of the devices to be retrieved.</param>
		/// <returns>A list of the remote XBee devices contained in the network with the given identifier.</returns>
		/// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="id"/> == null</c>.</exception>
		public List<RemoteXBeeDevice> GetDevices(string id)
		{
			if (id == null)
				throw new ArgumentNullException("Device identifier cannot be null.");
			if (id.Length == 0)
				throw new ArgumentException("Device identifier cannot be an empty string.");

			List<RemoteXBeeDevice> devices = new List<RemoteXBeeDevice>();

			// Look in the 64-bit map.
			foreach (RemoteXBeeDevice remote in remotesBy64BitAddr.Values)
			{
				if (string.Equals(remote.NodeID, id))
					devices.Add(remote);
			}
			// Look in the 16-bit map.
			foreach (RemoteXBeeDevice remote in remotesBy16BitAddr.Values)
			{
				if (remote.NodeID == id)
					devices.Add(remote);
			}
			// Return the list.
			return devices;
		}

		/// <summary>
		/// Returns the first remote device that matches the supplied identifier.
		/// </summary>
		/// <remarks>Note that this method does not perform a discovery, only returns the device that has 
		/// been previously discovered.</remarks>
		/// <param name="id">The identifier of the device to be retrieved.</param>
		/// <returns>The remote XBee device contained in the network with the given identifier, <c>null</c> 
		/// if the network does not contain any device with that Node ID.</returns>
		/// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="id"/> == null</c>.</exception>
		public RemoteXBeeDevice GetDevice(string id)
		{
			if (id == null)
				throw new ArgumentNullException("Device identifier cannot be null.");
			if (id.Length == 0)
				throw new ArgumentException("Device identifier cannot be an empty string.");

			// Look in the 64-bit map.
			var remote64 = remotesBy64BitAddr.Values.SingleOrDefault(r => r.NodeID == id);
			if (remote64 != null)
				return remote64;

			// Look in the 16-bit map.
			var remote16 = remotesBy16BitAddr.Values.SingleOrDefault(r => r.NodeID == id);
			if (remote16 != null)
				return remote16;

			// The given ID is not in the network.
			return null;
		}

		/// <summary>
		/// Returns the remote device already contained in the network whose 64-bit address matches 
		/// the given one.
		/// </summary>
		/// <remarks>Note that this method does not perform a discovery, only returns the device that 
		/// has been previously discovered.</remarks>
		/// <param name="address">The 64-bit address of the device to be retrieved.</param>
		/// <returns>The remote device in the network or <c>null</c> if it is not found.</returns>
		/// <exception cref="ArgumentException">If <paramref name="address"/> is 
		/// <see cref="XBee64BitAddress.UNKNOWN_ADDRESS"/>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c>.</exception>
		public RemoteXBeeDevice GetDevice(XBee64BitAddress address)
		{
			if (address == null)
				throw new ArgumentNullException("64-bit address cannot be null.");
			if (address.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
				throw new ArgumentException("64-bit address cannot be unknown.");

			logger.DebugFormat("{0}Getting device '{1}' from network.", localDevice.ToString(), address);

			remotesBy64BitAddr.TryGetValue(address, out RemoteXBeeDevice result);

			return result;
		}

		/// <summary>
		/// Returns the remote device already contained in the network whose 16-bit address matches the 
		/// given one.
		/// </summary>
		/// <remarks>Note that this method does not perform a discovery, only returns the device that has 
		/// been previously discovered.</remarks>
		/// <param name="address">The 16-bit address of the device to be retrieved.</param>
		/// <returns>The remote device in the network or <c>null</c> if it is not found.</returns>
		/// <exception cref="ArgumentException">If <paramref name="address"/> is 
		/// <see cref="XBee16BitAddress.UNKNOWN_ADDRESS"/>.</exception>
		/// <exception cref="ArgumentNullException">If <c><paramref name="address"/> == null</c>.</exception>
		/// <exception cref="OperationNotSupportedException">If the protocol of <c>localDevice</c> is 
		/// <see cref="XBeeProtocol.DIGI_MESH"/> or if it is <see cref="XBeeProtocol.DIGI_POINT"/></exception>
		public RemoteXBeeDevice GetDevice(XBee16BitAddress address)
		{
			if (localDevice.XBeeProtocol == XBeeProtocol.DIGI_MESH)
				throw new OperationNotSupportedException("DigiMesh protocol does not support 16-bit addressing.");
			if (localDevice.XBeeProtocol == XBeeProtocol.DIGI_POINT)
				throw new OperationNotSupportedException("Point-to-Multipoint protocol does not support 16-bit addressing.");
			if (address == null)
				throw new ArgumentNullException("16-bit address cannot be null.");
			if (address.Equals(XBee16BitAddress.UNKNOWN_ADDRESS))
				throw new ArgumentException("16-bit address cannot be unknown.");

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
				XBee16BitAddress a = Get16BitAddress(d);
				if (a != null && a.Equals(address))
				{
					devInNetwork = d;
					break;
				}
			}

			// Look in the 16-bit map.
			if (devInNetwork == null)
				remotesBy16BitAddr.TryGetValue(address, out devInNetwork);

			return devInNetwork;
		}

		/// <summary>
		/// Returns the protocol of the Network.
		/// </summary>
		/// <returns>The protocol of the Network.</returns>
		public XBeeProtocol GetProtocol()
		{
			return localDevice.XBeeProtocol;
		}

		/// <summary>
		/// Adds the given remote device to the network.
		/// </summary>
		/// <remarks>Notice that this operation does not join the remote XBee device to the network; it 
		/// just tells the network that it contains that device. However, the device has only been added 
		/// to the device list, and may not be physically in the same network.
		/// 
		/// The way of adding a device to the network is based on the 64-bit address. If it is not 
		/// configured:
		/// <list type="bullet">
		/// <item><description>For 802.15.4 and ZigBee devices, it will use the 16-bit address.</description></item>
		/// <item><description>For the rest will return <c>true</c> as the result of the addition.</description></item>
		/// </list></remarks>
		/// <param name="remoteDevice">The remote device to be added to the network.</param>
		/// <returns>The remote XBee Device instance in the network, <c>null</c> if the device could not be 
		/// successfully added.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteDevice"/> == null</c>.</exception>
		public RemoteXBeeDevice AddRemoteDevice(RemoteXBeeDevice remoteDevice)
		{
			if (remoteDevice == null)
				throw new ArgumentNullException("Remote device cannot be null.");

			logger.DebugFormat("{0}Adding device '{1}' to network.", localDevice.ToString(), remoteDevice.ToString());

			RemoteXBeeDevice devInNetwork = null;
			XBee64BitAddress addr64 = remoteDevice.XBee64BitAddr;
			XBee16BitAddress addr16 = Get16BitAddress(remoteDevice);

			// Check if the device has 64-bit address.
			if (addr64 != null && !addr64.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
			{
				// The device has 64-bit address, so look in the 64-bit map.
				if (remotesBy64BitAddr.TryGetValue(addr64, out devInNetwork))
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
						if (remotesBy16BitAddr.TryGetValue(addr16, out devInNetwork))
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
					XBee16BitAddress a = Get16BitAddress(d);
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
					if (remotesBy16BitAddr.TryGetValue(addr16, out devInNetwork))
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

		/// <summary>
		/// Adds the given list of remote devices to the network.
		/// </summary>
		/// <remarks>Notice that this operation does not join the remote XBee devices to the network; it 
		/// just tells the network that it contains those devices. However, the devices have only been 
		/// added to the device list, and may not be physically in the same network.
		/// 
		/// The way of adding a device to the network is based on the 64-bit address. If it is not 
		/// configured:
		/// <list type="bullet">
		/// <item><description>For 802.15.4 and ZigBee devices, the 16-bit address will be used instead.</description></item>
		/// <item><description>For the rest will return <c>true</c> as the result of the addition.</description></item>
		/// </list></remarks>
		/// <param name="list">The list of remote devices to be added to the network.</param>
		/// <returns>A list with the successfully added devices to the network.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="list"/> == null</c>.</exception>
		public List<RemoteXBeeDevice> AddRemoteDevices(IList<RemoteXBeeDevice> list)
		{
			if (list == null)
				throw new ArgumentNullException("The list of remote devices cannot be null.");

			List<RemoteXBeeDevice> addedList = new List<RemoteXBeeDevice>(list.Count);

			if (list.Count == 0)
				return addedList;

			logger.DebugFormat("{0}Adding '{1}' devices to network.", localDevice.ToString(), list.Count);

			for (int i = 0; i < list.Count; i++)
			{
				RemoteXBeeDevice d = AddRemoteDevice(list[i]);
				if (d != null)
					addedList.Add(d);
			}

			return addedList;
		}

		/// <summary>
		/// Removes the given remote XBee device from the network.
		/// </summary>
		/// <remarks>Notice that this operation does not remove the remote XBee device from the actual XBee 
		/// network; it just tells the network object that it will no longer contain that device. However, 
		/// next time a discovery is performed, it could be added again automatically.
		/// 
		/// This method will check for a device that matches the 64-bit address of the provided one, if 
		/// found, that device will be removed from the corresponding list. In case the 64-bit address is 
		/// not defined, it will use the 16-bit address for DigiMesh and ZigBee devices.</remarks>
		/// <param name="remoteDevice">The remote device to be removed from the network.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="remoteDevice"/> == null</c>.</exception>
		public void RemoveRemoteDevice(RemoteXBeeDevice remoteDevice)
		{
			if (remoteDevice == null)
				throw new ArgumentNullException("Remote device cannot be null.");

			RemoteXBeeDevice devInNetwork = null;

			// Look in the 64-bit map.
			XBee64BitAddress addr64 = remoteDevice.XBee64BitAddr;
			if (addr64 != null && !addr64.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
			{
				if (remotesBy64BitAddr.TryRemove(addr64, out devInNetwork))
					return;
			}

			// If not found, look in the 16-bit map.
			XBee16BitAddress addr16 = Get16BitAddress(remoteDevice);
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
					XBee16BitAddress a = Get16BitAddress(d);
					if (a != null && a.Equals(addr16))
					{
						remotesBy64BitAddr.TryRemove(d.XBee64BitAddr, out RemoteXBeeDevice r);
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

		/// <summary>
		/// Removes all the devices from this network.
		/// </summary>
		/// <remarks>The network will be empty after this call returns.
		/// 
		/// Notice that this does not imply removing the XBee devices from the actual XBee network; it 
		/// just tells the object that the list should be empty now. Next time a discovery is performed, 
		/// the list could be filled with the remote XBee devices found.</remarks>
		public void ClearDeviceList()
		{
			logger.DebugFormat("{0}Clearing the network.", localDevice.ToString());
			remotesBy64BitAddr.Clear();
			remotesBy16BitAddr.Clear();
		}

		/// <summary>
		/// Returns the number of devices already discovered in the network.
		/// </summary>
		/// <returns>The number of devices already discovered in the network.</returns>
		public int GetNumberOfDevices()
		{
			return remotesBy64BitAddr.Count + remotesBy16BitAddr.Count;
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns>A string representation of this object.</returns>
		public override string ToString()
		{
			return string.Format("{0} [{1}] @{2}", GetType().Name, localDevice.ToString(),
					GetHashCode().ToString("x"));
		}

		/// <summary>
		/// Returns the 16-bit address of the given remote device.
		/// </summary>
		/// <param name="device">The remote device to get its 16-bit address.</param>
		/// <returns>The 16-bit address of the device, <c>null</c> if it does not contain a valid one.</returns>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="XBee16BitAddress"/>
		private XBee16BitAddress Get16BitAddress(RemoteXBeeDevice device)
		{
			if (device == null)
				return null;

			XBee16BitAddress address = null;

			switch (device.XBeeProtocol)
			{
				case XBeeProtocol.RAW_802_15_4:
					address = ((RemoteRaw802Device)device).XBee16BitAddr;
					break;
				case XBeeProtocol.ZIGBEE:
					address = ((RemoteZigBeeDevice)device).XBee16BitAddr;
					break;
				default:
					// TODO should we allow this operation for general remote devices?
					address = device.XBee16BitAddr;
					break;
			}

			return address;
		}
	}
}