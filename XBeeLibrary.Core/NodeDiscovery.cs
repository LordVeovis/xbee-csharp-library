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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.Common;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core
{
	 /// <summary>
	 /// Helper class used to perform a node discovery (<c>ND</c>) in the provided local XBee device.
	 /// </summary>
	 /// <remarks>This action requires an XBee connection and optionally a discover timeout. The node 
	 /// discovery works on all protocols and working modes returning as result a list of discovered 
	 /// XBee Devices.
	 /// 
	 /// The discovery process updates the network of the local device with the new discovered modules 
	 /// and refreshes the already existing references.</remarks>
	class NodeDiscovery
	{
		// Constants.
		private const string ND_COMMAND = "ND";

		public const long DEFAULT_TIMEOUT = 20000; // 20 seconds.

		// Variables.
		private static byte globalFrameID = 1;

		private AbstractXBeeDevice xbeeDevice;
		private AbstractXBeeDevice localDevice;

		private IList<RemoteXBeeDevice> deviceList;

		private bool discovering = false;
		private byte frameID;

		protected ILog logger;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="NodeDiscovery"/> object with the provided 
		/// parameters.
		/// </summary>
		/// <param name="xbeeDevice">XBee Device where to perform the node discovery.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="xbeeDevice"/> == null</c>.</exception>
		public NodeDiscovery(AbstractXBeeDevice xbeeDevice)
		{
			this.xbeeDevice = xbeeDevice ?? throw new ArgumentNullException("Local XBee device cannot be null.");
			if (xbeeDevice.IsRemote)
				localDevice = ((RemoteXBeeDevice) this.xbeeDevice).GetLocalXBeeDevice();
			else
				localDevice = xbeeDevice;

			frameID = globalFrameID;
			globalFrameID++;
			if (globalFrameID == 0xFF)
				globalFrameID = 1;

			logger = LogManager.GetLogger(GetType());
		}

		// Events.
		/// <summary>
		/// Represents the method that will handle the device discovered packet received event.
		/// </summary>
		public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered;

		/// <summary>
		/// Represents the method that will handle the discovery error event.
		/// </summary>
		public event EventHandler<DiscoveryErrorEventArgs> DiscoveryError;

		/// <summary>
		/// Represents the method that will handle the discovery finished event.
		/// </summary>
		public event EventHandler<DiscoveryFinishedEventArgs> DiscoveryFinished;

		// Properties.
		/// <summary>
		/// Indicates whether the discovery process is running.
		/// </summary>
		/// <see cref="StartDiscoveryProcess"/>
		/// <see cref="StopDiscoveryProcess"/>
		public bool IsRunning { get; private set; } = false;

		/// <summary>
		/// Discovers and reports the first remote XBee device that matches the supplied identifier.
		/// </summary>
		/// <remarks>This method blocks until the configured timeout in the device (NT) expires.</remarks>
		/// <param name="id">The identifier of the device to be discovered.</param>
		/// <returns>The discovered remote XBee device with the given identifier, <c>null</c> if the 
		/// timeout expires and the device was not found.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="XBeeException">If there is an error discovering the device.</exception>
		/// <seealso cref="DiscoverDevices(IList{string})"/>
		public RemoteXBeeDevice DiscoverDevice(string id)
		{
			// Check if the connection is open.
			if (!localDevice.IsOpen)
				throw new InterfaceNotOpenException();

			logger.DebugFormat("{0}ND for {1} device.", xbeeDevice.ToString(), id);

			IsRunning = true;
			discovering = true;

			PerformNodeDiscovery(id);

			XBeeNetwork network = localDevice.GetNetwork();
			RemoteXBeeDevice rDevice = null;

			if (deviceList != null && deviceList.Count > 0)
			{
				rDevice = deviceList[0];
				if (rDevice != null)
					rDevice = network.AddRemoteDevice(rDevice);
			}

			return rDevice;
		}

		/// <summary>
		/// Discovers and reports all remote XBee devices that match the supplied identifiers.
		/// </summary>
		/// <remarks>This method blocks until the configured timeout in the device (NT) expires.</remarks>
		/// <param name="ids">List which contains the identifiers of the devices to be discovered.</param>
		/// <returns>A list of the discovered remote XBee devices with the given identifiers.</returns>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="XBeeException">If there is an error discovering the device.</exception>
		/// <seealso cref="DiscoverDevice(string)"/>
		public List<RemoteXBeeDevice> DiscoverDevices(IList<string> ids)
		{
			// Check if the connection is open.
			if (!localDevice.IsOpen)
				throw new InterfaceNotOpenException();

			logger.DebugFormat("{0} ND for all '[{1}]' devices.", xbeeDevice.ToString(), string.Join(", ", ids));

			IsRunning = true;
			discovering = true;

			PerformNodeDiscovery(null);

			List<RemoteXBeeDevice> foundDevices = new List<RemoteXBeeDevice>(0);
			if (deviceList == null)
				return foundDevices;

			XBeeNetwork network = localDevice.GetNetwork();

			foreach (RemoteXBeeDevice d in deviceList)
			{
				string nID = d.NodeID;
				if (nID == null)
					continue;
				foreach (string id in ids)
				{
					if (nID.Equals(id))
					{
						RemoteXBeeDevice rDevice = network.AddRemoteDevice(d);
						if (rDevice != null && !foundDevices.Contains(rDevice))
							foundDevices.Add(rDevice);
					}
				}
			}

			return foundDevices;
		}

		/// <summary>
		/// Performs a node discover to search for XBee devices in the same network. 
		/// </summary>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <seealso cref="IsRunning"/>
		/// <seealso cref="StopDiscoveryProcess"/>
		public void StartDiscoveryProcess()
		{
			// Check if the connection is open.
			if (!localDevice.IsOpen)
				throw new InterfaceNotOpenException();

			IsRunning = true;
			discovering = true;

			Task discoveryTask = new Task(() =>
			{
				try
				{
					PerformNodeDiscovery(null);
				}
				catch (XBeeException e)
				{
					// Notify the listeners about the error and finish.
					NotifyDiscoveryFinished(e.Message);
				}
			});

			discoveryTask.Start();
		}

		/// <summary>
		/// Stops the disocovery process if it is running.
		/// </summary>
		/// <seealso cref="IsRunning"/>
		/// <seealso cref="StartDiscoveryProcess"/>
		public void StopDiscoveryProcess()
		{
			discovering = false;
		}

		/// <summary>
		/// Performs a node discover to search for XBee devices in the same network. 
		/// </summary>
		/// <remarks>This method blocks until the configured timeout expires.</remarks>
		/// <param name="id">The identifier of the device to be discovered, or <c>null</c> to discover 
		/// all devices in the network.</param>
		/// <exception cref="XBeeException">If there is an error sending the discovery command.</exception>
		private void PerformNodeDiscovery(string id)
		{
			try
			{
				DiscoverDevicesAPI(id);

				// Notify that the discovery finished without errors.
				NotifyDiscoveryFinished(null);
			}
			finally
			{
				IsRunning = false;
				discovering = false;
			}
		}

		/// <summary>
		/// Callback called after a discovery packet is received and the corresponding Packet Received event 
		/// has been fired.
		/// </summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="e">The Packet Received event.</param>
		/// <param name="id">The ID of the device to find.</param>
		public async void DiscoveryPacketReceived(object sender, PacketReceivedEventArgs e, string id)
		{
			if (!discovering)
				return;
			RemoteXBeeDevice rdevice = null;

			byte[] commandValue = GetRemoteDeviceData((XBeeAPIPacket)e.ReceivedPacket);

			rdevice = await ParseDiscoveryAPIData(commandValue, localDevice);

			// If a device with a specific id is being searched and it was 
			// already found, return it.
			if (id != null)
			{
				if (rdevice != null && id.Equals(rdevice.NodeID))
				{
					lock (deviceList)
					{
						if (deviceList != null)
						{
							if (!deviceList.Any(d => d.XBee64BitAddr == rdevice.XBee64BitAddr))
								deviceList.Add(rdevice);
						}
					}
					// If the local device is 802.15.4 wait until the 'end' command is received.
					if (xbeeDevice.XBeeProtocol != XBeeProtocol.RAW_802_15_4)
						discovering = false;
				}
			}
			else if (rdevice != null)
				NotifyDeviceDiscovered(rdevice);
		}

		public override string ToString()
		{
			return GetType().Name + " [" + xbeeDevice.ToString() + "] @" +
					GetHashCode().ToString("x");
		}

		/// <summary>
		/// Performs the device discovery in API1 or API2 (API Escaped) mode.
		/// </summary>
		/// <param name="id">The identifier of the device to be discovered, or <c>null</c> 
		/// to discover all devices in the network.</param>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="XBeeException">If there is an error sending the discovery command.</exception>
		private void DiscoverDevicesAPI(string id)
		{
			if (deviceList == null)
				deviceList = new List<RemoteXBeeDevice>();
			deviceList.Clear();

			logger.DebugFormat("{0}Start listening.", xbeeDevice.ToString());
			void handler(object sender, PacketReceivedEventArgs e) => DiscoveryPacketReceived(sender, e, id);
			localDevice.PacketReceived += handler;

			try
			{
				long deadLine = 0;
				var stopwatch = Stopwatch.StartNew();

				// In 802.15.4 devices, the discovery finishes when the 'end' command 
				// is received, so it's not necessary to calculate the timeout.
				// This also applies to S1B devices working in compatibility mode.
				bool is802Compatible = Is802Compatible();
				if (!is802Compatible)
					deadLine += CalculateTimeout();

				SendNodeDiscoverCommand(id);

				if (!is802Compatible)
				{
					// Wait for scan timeout.
					while (discovering)
					{
						if (stopwatch.ElapsedMilliseconds < deadLine)
							Task.Delay(100).Wait();
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
						Task.Delay(100).Wait();
					}
				}
			}
			finally
			{
				localDevice.PacketReceived -= handler;
				logger.DebugFormat("{0}Stop listening.", xbeeDevice.ToString());
			}
		}

		/// <summary>
		/// Calculates the maximum response time, in milliseconds, for network discovery responses.
		/// </summary>
		/// <returns>Maximum network discovery timeout.</returns>
		private long CalculateTimeout()
		{
			long timeout = -1;

			// Read the maximum discovery timeout (N?).
			try
			{
				var timeoutValue = xbeeDevice.GetParameter("N?");
				if (timeoutValue == null || timeoutValue.Length < 1)
					throw new ATCommandEmptyException("N?");
				timeout = ByteUtils.ByteArrayToLong(timeoutValue);
			}
			catch (XBeeException)
			{
				logger.DebugFormat("{0}Could not read the N? value.", xbeeDevice.ToString());
			}

			// If N? does not exist, read the NT parameter.
			if (timeout == -1)
			{
				// Read the device timeout (NT).
				try
				{
					var discoverTime = xbeeDevice.GetParameter("NT");
					if (discoverTime == null || discoverTime.Length < 1)
						throw new ATCommandEmptyException("NT");
					timeout = ByteUtils.ByteArrayToLong(discoverTime) * 100;
				}
				catch (XBeeException)
				{
					timeout = DEFAULT_TIMEOUT;
					string error = "Could not read the discovery timeout from the device (NT). "
							+ "The default timeout (" + DEFAULT_TIMEOUT + " ms.) will be used.";
					NotifyDiscoveryError(error);
				}

				// In DigiMesh/DigiPoint the network discovery timeout is NT + the 
				// network propagation time. It means that if the user sends an AT 
				// command just after NT ms, s/he will receive a timeout exception. 
				if (xbeeDevice.XBeeProtocol == XBeeProtocol.DIGI_MESH)
					timeout += 3000;
				else if (xbeeDevice.XBeeProtocol == XBeeProtocol.DIGI_POINT)
					timeout += 8000;
			}

			if (xbeeDevice.XBeeProtocol == XBeeProtocol.DIGI_MESH)
			{
				try
				{
					// If the module is 'Sleep support', wait another discovery cycle.
					var sleepMode = xbeeDevice.GetParameter("SM");
					if (sleepMode == null || sleepMode.Length < 1)
						throw new ATCommandEmptyException("SM");
					bool isSleepSupport = ByteUtils.ByteArrayToInt(sleepMode) == 7;
					if (isSleepSupport)
						timeout += timeout + (timeout / 10L);
				}
				catch (XBeeException)
				{
					logger.DebugFormat("{0}Could not determine if the module is 'Sleep Support'.", xbeeDevice.ToString());
				}
			}

			return timeout;
		}

		/// <summary>
		/// Returns a byte array with the remote device data to be parsed.
		/// </summary>
		/// <param name="packet">The API packet that contains the data.</param>
		/// <returns>A byte array with the data to be parsed.</returns>
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
				case APIFrameType.REMOTE_AT_COMMAND_RESPONSE:
					RemoteATCommandResponsePacket remoteATResponse = (RemoteATCommandResponsePacket)packet;
					// Check the frame ID.
					if (remoteATResponse.FrameID != frameID)
						return null;
					// Check the command.
					if (!remoteATResponse.Command.Equals(ND_COMMAND))
						return null;
					// Check if the 'end' command is received (empty response with OK status).
					if (remoteATResponse.CommandValue == null || remoteATResponse.CommandValue.Length == 0)
					{
						discovering = remoteATResponse.Status != ATCommandStatus.OK;
						return null;
					}

					logger.DebugFormat("{0}Received self response: {1}.", xbeeDevice.ToString(), packet);

					data = remoteATResponse.CommandValue;
					break;
				default:
					break;
			}

			return data;
		}

		/// <summary>
		/// Parses the given node discovery API data to create and return a remote 
		/// XBee Device.
		/// </summary>
		/// <param name="data">Byte array with the data to parse.</param>
		/// <param name="localDevice">The local device that received the remote XBee data.</param>
		/// <returns>The discovered XBee device.</returns>
		private async Task<RemoteXBeeDevice> ParseDiscoveryAPIData(byte[] data, AbstractXBeeDevice localDevice)
		{
			if (data == null)
				return null;

			RemoteXBeeDevice device = null;
			XBee16BitAddress addr16 = null;
			XBee64BitAddress addr64 = null;
			string id = null;
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


				switch (localDevice.XBeeProtocol)
				{
					case XBeeProtocol.ZIGBEE:
					case XBeeProtocol.DIGI_MESH:
					case XBeeProtocol.ZNET:
					case XBeeProtocol.DIGI_POINT:
					case XBeeProtocol.XLR:
					// TODO [XLR_DM] The next version of the XLR will add DigiMesh support.
					// For the moment only point-to-multipoint is supported in this kind of devices.
					case XBeeProtocol.XLR_DM:
					case XBeeProtocol.SX:
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
								xbeeDevice.ToString(), localDevice.XBeeProtocol.GetDescription(), addr16,
								addr64, id, parentAddress, HexUtils.ByteArrayToHexString(profileID),
								HexUtils.ByteArrayToHexString(manufacturerID));

						break;
					case XBeeProtocol.RAW_802_15_4:
						// Read strength signal byte.
						signalStrength = inputStream.ReadByte();
						// Read node identifier.
						id = ByteUtils.ReadString(inputStream);

						logger.DebugFormat("{0}Discovered {1} device: 16-bit[{2}], 64-bit[{3}], id[{4}], rssi[{5}].",
								xbeeDevice.ToString(), localDevice.XBeeProtocol.GetDescription(), addr16, addr64, id, signalStrength);

						break;
					case XBeeProtocol.UNKNOWN:
					default:
						logger.DebugFormat("{0}Discovered {1} device: 16-bit[{2}], 64-bit[{3}].",
								xbeeDevice.ToString(), localDevice.XBeeProtocol.GetDescription(), addr16, addr64);
						break;
				}

				// Create device and fill with parameters.
				switch (localDevice.XBeeProtocol)
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

		/// <summary>
		/// Sends the node discover (<c>ND</c>) command.
		/// </summary>
		/// <param name="id">The identifier of the device to be discovered, or <c>null</c> to 
		/// discover all devices in the network.</param>
		/// <exception cref="InterfaceNotOpenException">If this device connection is not open.</exception>
		/// <exception cref="XBeeException">If there is any other XBee related exception.</exception>
		private void SendNodeDiscoverCommand(string id)
		{
			if (id == null)
				id = "";

			if (xbeeDevice.IsRemote)
				localDevice.SendPacketAsync(new RemoteATCommandPacket(frameID, xbeeDevice.XBee64BitAddr,
					xbeeDevice.XBee16BitAddr, (byte)XBeeTransmitOptions.NONE, ND_COMMAND, id));
			else
				localDevice.SendPacketAsync(new ATCommandPacket(frameID, ND_COMMAND, id));
		}

		/// <summary>
		/// Notifies the given discovery listeners that a device was discovered.
		/// </summary>
		/// <param name="remoteDevice">The remote device discovered.</param>
		private void NotifyDeviceDiscovered(RemoteXBeeDevice remoteDevice)
		{
			logger.DebugFormat("{0}Device discovered: {1}.", xbeeDevice, remoteDevice);

			if (DeviceDiscovered == null)
			{
				lock (deviceList)
				{
					if (deviceList != null)
					{
						if (!deviceList.Any(d => d.XBee64BitAddr == remoteDevice.XBee64BitAddr))
							deviceList.Add(remoteDevice);
					}
				}
				return;
			}

			XBeeNetwork network = localDevice.GetNetwork();
			RemoteXBeeDevice addedDev = network.AddRemoteDevice(remoteDevice);
			if (addedDev != null)
			{
				try
				{
					lock (DeviceDiscovered)
					{
						var handler = DeviceDiscovered;
						if (handler != null)
						{
							var args = new DeviceDiscoveredEventArgs(addedDev);

							handler.GetInvocationList().AsParallel().ForAll((action) =>
							{
								action.DynamicInvoke(this, args);
							});
						}
					}
				}
				catch (Exception e)
				{
					logger.Error(e.Message, e);
				}
			}
			else
			{
				string error = "Error adding device '" + remoteDevice + "' to the network.";
				NotifyDiscoveryError(error);
			}
		}

		/// <summary>
		/// Notifies the given discovery listeners about the provided error.
		/// </summary>
		/// <param name="error">The error to notify.</param>
		private void NotifyDiscoveryError(string error)
		{
			logger.ErrorFormat("{0}Error discovering devices: {1}", xbeeDevice, error);

			if (DiscoveryError == null)
				return;

			try
			{
				lock (DiscoveryError)
				{
					var handler = DiscoveryError;
					if (handler != null)
					{
						var args = new DiscoveryErrorEventArgs(error);

						handler.GetInvocationList().AsParallel().ForAll((action) =>
						{
							action.DynamicInvoke(this, args);
						});
					}
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message, e);
			}
		}

		/// <summary>
		/// Notifies the given discovery listeners that the discovery process has finished.
		/// </summary>
		/// <param name="error">The error message, or <c>null</c> if the process finished 
		/// successfully.</param>
		private void NotifyDiscoveryFinished(string error)
		{
			if (error != null && error.Length > 0)
				logger.ErrorFormat("{0}Finished discovery: {1}", xbeeDevice, error);
			else
				logger.DebugFormat("{0}Finished discovery.", xbeeDevice);

			if (DiscoveryFinished == null)
				return;

			try
			{
				lock (DiscoveryFinished)
				{
					var handler = DiscoveryFinished;
					if (handler != null)
					{
						var args = new DiscoveryFinishedEventArgs(error);

						handler.GetInvocationList().AsParallel().ForAll((action) =>
						{
							action.DynamicInvoke(this, args);
						});
					}
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message, e);
			}
		}

		/// <summary>
		/// Checks whether the device performing the node discovery is a legacy 802.15.4 device or 
		/// a S1B device working compatibility mode.
		/// </summary>
		/// <returns><c>true</c> if the device performing the node discovery is a legacy 802.15.4 
		/// device or S1B in compatibility mode, <c>false</c> otherwise.</returns>
		private bool Is802Compatible()
		{
			if (xbeeDevice.XBeeProtocol != XBeeProtocol.RAW_802_15_4)
				return false;
			byte[] param = null;
			try
			{
				param = xbeeDevice.GetParameter("C8");
			}
			catch (Exception) { }
			if (param == null || param.Length < 1 || ((param[0] & 0x2) == 2))
				return true;
			return false;
		}
	}
}