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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Events.Relay;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.IO;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Core.Packet;
using XBeeLibrary.Core.Packet.Cellular;
using XBeeLibrary.Core.Packet.Common;
using XBeeLibrary.Core.Packet.IP;
using XBeeLibrary.Core.Packet.Raw;
using XBeeLibrary.Core.Packet.Relay;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Connection
{
	/// <summary>
	/// Thread that constantly reads data from an input stream.
	/// </summary>
	/// <remarks>Depending on the XBee operating mode, read data is notified as is to the subscribed 
	/// listeners or is parsed to a packet using the packet parser and then notified to subscribed 
	/// listeners.</remarks>
	public class DataReader
	{
		// Constants.
		private const int ALL_FRAME_IDS = 99999;
		private const int MAXIMUM_PARALLEL_LISTENER_THREADS = 20;

		// Variables.
		private IConnectionInterface connectionInterface;

		private volatile OperatingMode mode;

		// It is required to keep a list of CustomPacketReceiveEventHandler objects. Each of those 
		// contains a Packet Received event handler and a frame ID associated with it. This means 
		// that each handler registered here will only respond to events associated with the correct 
		// Frame ID. When it is 99999 (ALL_FRAME_IDS), all the packets will be handled.
		private List<CustomPacketReceivedEventHandler> packetReceivedHandlers = new List<CustomPacketReceivedEventHandler>();

		private ILog logger;

		private XBeePacketParser parser;
		private AbstractXBeeDevice xbeeDevice;

		private Task task;

		/// <summary>
		/// Class constructor. Instantiates a new <see cref="DataReader"/> object for the given connection 
		/// interface using the given XBee operating mode and XBee device.
		/// </summary>
		/// <param name="connectionInterface">Connection interface to read data from.</param>
		/// <param name="mode">XBee operating mode.</param>
		/// <param name="xbeeDevice">Reference to the XBee device containing this <see cref="DataReader"/> 
		/// object.</param>
		/// <exception cref="ArgumentNullException">If <c><paramref name="connectionInterface"/> == null</c>.</exception>
		/// <seealso cref="IConnectionInterface"/>
		/// <seealso cref="XBeeDevice"/>
		/// <seealso cref="OperatingMode"/>
		public DataReader(IConnectionInterface connectionInterface, OperatingMode mode, AbstractXBeeDevice xbeeDevice)
		{
			this.connectionInterface = connectionInterface ?? throw new ArgumentNullException("Connection interface cannot be null.");
			this.mode = mode;
			this.xbeeDevice = xbeeDevice;
			logger = LogManager.GetLogger<DataReader>();
			parser = new XBeePacketParser();
			XBeePacketsQueue = new XBeePacketsQueue();

			// Create the task.
			task = new Task(() => { Run(); }, TaskCreationOptions.LongRunning);
		}

		// Events.
		/// <summary>
		/// Represents the method that will handle the data received event.
		/// </summary>
		/// <seealso cref="DataReceivedEventArgs"/>
		public event EventHandler<DataReceivedEventArgs> DataReceived;

		/// <summary>
		/// Represents the method that will handle the packet received event.
		/// </summary>
		/// <seealso cref="PacketReceivedEventArgs"/>
		public event EventHandler<PacketReceivedEventArgs> XBeePacketReceived;

		/// <summary>
		/// Represents the method that will handle the IO sample packet received event.
		/// </summary>
		/// <seealso cref="IOSampleReceivedEventArgs"/>
		public event EventHandler<IOSampleReceivedEventArgs> IOSampleReceived;

		/// <summary>
		/// Represents the method that will handle the Modem status event.
		/// </summary>
		/// <seealso cref="ModemStatusReceivedEventArgs"/>
		public event EventHandler<ModemStatusReceivedEventArgs> ModemStatusReceived;

		/// <summary>
		/// Represents the method that will handle the explicit data received event.
		/// </summary>
		/// <seealso cref="ExplicitDataReceivedEventArgs"/>
		public event EventHandler<ExplicitDataReceivedEventArgs> ExplicitDataReceived;

		/// <summary>
		/// Represents the method that will handle the User Data Relay received event.
		/// </summary>
		/// <seealso cref="UserDataRelayReceivedEventArgs"/>
		public event EventHandler<UserDataRelayReceivedEventArgs> UserDataRelayReceived;

		/// <summary>
		/// Represents the method that will handle the Bluetooth data received event.
		/// </summary>
		/// <seealso cref="BluetoothDataReceivedEventArgs"/>
		public event EventHandler<BluetoothDataReceivedEventArgs> BluetoothDataReceived;

		/// <summary>
		/// Represents the method that will handle the MicroPython data received event.
		/// </summary>
		/// <seealso cref="MicroPythonDataReceivedEventArgs"/>
		public event EventHandler<MicroPythonDataReceivedEventArgs> MicroPythonDataReceived;

		/// <summary>
		/// Represents the method that will handle the serial data received event.
		/// </summary>
		/// <seealso cref="SerialDataReceivedEventArgs"/>
		public event EventHandler<SerialDataReceivedEventArgs> SerialDataReceived;

		/// <summary>
		/// Represents the method that will handle the SMS received event.
		/// </summary>
		/// <seealso cref="SMSReceivedEventArgs"/>
		public event EventHandler<SMSReceivedEventArgs> SMSReceived;

		/// <summary>
		/// Represents the method that will handle the IP data received event.
		/// </summary>
		/// <seealso cref="IPDataReceivedEventArgs"/>
		public event EventHandler<IPDataReceivedEventArgs> IPDataReceived;

		// Properties.
		/// <summary>
		/// Indicates whether the data reader is running or not.
		/// </summary>
		public bool IsRunning { get; private set; } = false;

		/// <summary>
		/// Returns the queue of read XBee packets.
		/// </summary>
		/// <returns>The queue of read XBee packets.</returns>
		/// <seealso cref="Models.XBeePacketsQueue"/>
		public XBeePacketsQueue XBeePacketsQueue { get; }

		/// <summary>
		/// Sets the XBee operating mode of this data reader.
		/// </summary>
		/// <param name="mode">The new xBee operating mode.</param>
		/// <seealso cref="OperatingMode"/>
		public void SetXBeeReaderMode(OperatingMode mode)
		{
			this.mode = mode;
		}

		/// <summary>
		/// Returns the remote XBee device from where the given package was sent from.
		/// </summary>
		/// <remarks>This is for internal use only.
		/// 
		/// If the package does not contain information about the source, this method returns <c>null</c> 
		/// (for example, <see cref="ModemStatusPacket"/>).
		/// 
		/// First the device that sent the provided package is looked in the network of the local 
		/// XBee device. If the remote device is not in the network, it is automatically added only 
		/// if the packet contains information about the origin of the package.</remarks>
		/// <param name="apiPacket">The packet sent from the remote device.</param>
		/// <returns>The remote XBee device that sends the given packet. It may be <c>null</c> if the 
		/// packet is not a known frame (<see cref="APIFrameType"/>) or if it does not contain information 
		/// of the source device.</returns>
		/// <exception cref="ArgumentNullException">If <c><paramref name="apiPacket"/> == null</c>.</exception>
		/// <exception cref="XBeeException">If any error occurred while adding the device to the 
		/// network.</exception>
		public RemoteXBeeDevice GetRemoteXBeeDeviceFromPacket(XBeeAPIPacket apiPacket)
		{
			if (apiPacket == null)
				throw new ArgumentNullException("XBee API packet cannot be null.");
			
			APIFrameType apiType = apiPacket.FrameType;

			RemoteXBeeDevice remoteDevice = null;
			XBee64BitAddress addr64 = null;
			XBee16BitAddress addr16 = null;

			XBeeNetwork network = xbeeDevice.GetNetwork();

			switch (apiType)
			{
				case APIFrameType.RECEIVE_PACKET:
					ReceivePacket receivePacket = (ReceivePacket)apiPacket;
					addr64 = receivePacket.SourceAddress64;
					addr16 = receivePacket.SourceAddress16;
					if (!addr64.Equals(XBee64BitAddress.UNKNOWN_ADDRESS))
						remoteDevice = network.GetDevice(addr64);
					else if (!addr16.Equals(XBee16BitAddress.UNKNOWN_ADDRESS))
						remoteDevice = network.GetDevice(addr16);
					break;
				case APIFrameType.RX_64:
					RX64Packet rx64Packet = (RX64Packet)apiPacket;
					addr64 = rx64Packet.SourceAddress64;
					remoteDevice = network.GetDevice(addr64);
					break;
				case APIFrameType.RX_16:
					RX16Packet rx16Packet = (RX16Packet)apiPacket;
					addr64 = XBee64BitAddress.UNKNOWN_ADDRESS;
					addr16 = rx16Packet.SourceAddress16;
					remoteDevice = network.GetDevice(addr16);
					break;
				case APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR:
					IODataSampleRxIndicatorPacket ioSamplePacket = (IODataSampleRxIndicatorPacket)apiPacket;
					addr64 = ioSamplePacket.SourceAddress64;
					addr16 = ioSamplePacket.SourceAddress16;
					remoteDevice = network.GetDevice(addr64);
					break;
				case APIFrameType.RX_IO_64:
					RX64IOPacket rx64IOPacket = (RX64IOPacket)apiPacket;
					addr64 = rx64IOPacket.SourceAddress64;
					remoteDevice = network.GetDevice(addr64);
					break;
				case APIFrameType.RX_IO_16:
					RX16IOPacket rx16IOPacket = (RX16IOPacket)apiPacket;
					addr64 = XBee64BitAddress.UNKNOWN_ADDRESS;
					addr16 = rx16IOPacket.SourceAddress16;
					remoteDevice = network.GetDevice(addr16);
					break;
				case APIFrameType.EXPLICIT_RX_INDICATOR:
					ExplicitRxIndicatorPacket explicitDataPacket = (ExplicitRxIndicatorPacket)apiPacket;
					addr64 = explicitDataPacket.SourceAddress64;
					addr16 = explicitDataPacket.SourceAddress16;
					remoteDevice = network.GetDevice(addr64);
					break;
				default:
					// Rest of the types are considered not to contain information 
					// about the origin of the packet.
					return remoteDevice;
			}

			// If the origin is not in the network, add it.
			if (remoteDevice == null)
			{
				remoteDevice = CreateRemoteXBeeDevice(addr64, addr16, null);
				if (!addr64.Equals(XBee64BitAddress.UNKNOWN_ADDRESS) || !addr16.Equals(XBee16BitAddress.UNKNOWN_ADDRESS))
					network.AddRemoteDevice(remoteDevice);
			}

			return remoteDevice;
		}

		/// <summary>
		/// Stops the data reader thread.
		/// </summary>
		public void StopReader()
		{
			IsRunning = false;
			lock (connectionInterface)
			{
				Monitor.Pulse(connectionInterface);
			}
			logger.Debug(connectionInterface.ToString() + "Data reader stopped.");
		}

		/// <summary>
		/// Adds the given <see cref="CustomPacketReceivedEventHandler"/> to the list of event 
		/// handlers that will be notified whenever a packet associated to the frame ID of 
		/// <paramref name="handler"/> is received.
		/// </summary>
		/// <param name="handler">The handler to be notified when a packet is received.</param>
		/// <seealso cref="RemovePacketReceivedHandler(EventHandler{PacketReceivedEventArgs})"/>
		/// <seealso cref="CustomPacketReceivedEventHandler"/>
		/// <seealso cref="PacketReceivedEventArgs"/>
		internal void AddPacketReceivedHandler(EventHandler<PacketReceivedEventArgs> handler)
		{
			lock (packetReceivedHandlers)
			{
				packetReceivedHandlers.Add(new CustomPacketReceivedEventHandler(handler));
				XBeePacketReceived += handler;
			}
		}

		/// <summary>
		/// Adds the given <see cref="CustomPacketReceivedEventHandler"/> to the list of event handlers 
		/// that will be notified whenever a packet associated to the frame ID of 
		/// <paramref name="handler"/> is received.
		/// </summary>
		/// <param name="handler">The handler to be notified when a packet is received.</param>
		/// <param name="frameId">The frame ID associated with the handler.</param>
		/// <seealso cref="RemovePacketReceivedHandler(EventHandler{PacketReceivedEventArgs})"/>
		/// <seealso cref="CustomPacketReceivedEventHandler"/>
		/// <seealso cref="PacketReceivedEventArgs"/>
		internal void AddPacketReceivedHandler(EventHandler<PacketReceivedEventArgs> handler, int frameId)
		{
			lock (packetReceivedHandlers)
			{
				packetReceivedHandlers.Add(new CustomPacketReceivedEventHandler(handler, frameId));
				XBeePacketReceived += handler;
			}
		}

		/// <summary>
		/// Removes the given <see cref="CustomPacketReceivedEventHandler"/> from the list of packet 
		/// received event handlers.
		/// </summary>
		/// <param name="handler">The handler to be removed.</param>
		/// <seealso cref="AddPacketReceivedHandler(EventHandler{PacketReceivedEventArgs})"/>
		/// <seealso cref="CustomPacketReceivedEventHandler"/>
		/// <seealso cref="PacketReceivedEventArgs"/>
		internal void RemovePacketReceivedHandler(EventHandler<PacketReceivedEventArgs> handler)
		{
			lock (packetReceivedHandlers)
			{
				// Search the handler in the list.
				foreach (CustomPacketReceivedEventHandler customHandler in packetReceivedHandlers)
				{
					if (customHandler.Handler.Equals(handler))
					{
						packetReceivedHandlers.Remove(customHandler);
						XBeePacketReceived -= handler;
						return;
					}
				}
			}
		}

		internal void Start()
		{
			task.Start();
		}

		private void Run()
		{
			logger.Debug(connectionInterface.ToString() + "Data reader started.");
			IsRunning = true;
			// Clear the list of read packets.
			XBeePacketsQueue.ClearQueue();
			try
			{
				lock (connectionInterface)
				{
					Monitor.Wait(connectionInterface);
				}
				while (IsRunning)
				{
					if (!IsRunning)
						break;
					if (connectionInterface.Stream != null)
					{
						switch (mode)
						{
							case OperatingMode.AT:
								break;
							case OperatingMode.API:
							case OperatingMode.API_ESCAPE:
								int headerByte = connectionInterface.Stream.ReadByte();
								// If it is packet header parse the packet, if not discard this byte and continue.
								if (headerByte == SpecialByte.HEADER_BYTE.GetValue())
								{
									try
									{
										XBeePacket packet = parser.ParsePacket(connectionInterface.Stream, mode);
										PacketReceived(packet);
									}
									catch (InvalidPacketException e)
									{
										logger.Error("Error parsing the API packet.", e);
									}
								}
								break;
							default:
								break;
						}
					}
					else if (connectionInterface.Stream == null)
						break;
					if (connectionInterface.Stream == null)
						break;
					else if (connectionInterface.Stream.Available > 0)
						continue;
					lock (connectionInterface)
					{
						Monitor.Wait(connectionInterface);
					}
				}
			}
			catch (IOException e)
			{
				logger.Error("Error reading from input stream.", e);
			}
			finally
			{
				if (IsRunning)
				{
					IsRunning = false;
					if (connectionInterface.IsOpen)
						connectionInterface.Close();
				}
			}
		}

		/// <summary>
		/// Dispatches the received XBee packet to the corresponding event handler(s).
		/// </summary>
		/// <param name="packet">The received XBee packet to be dispatched to the corresponding event 
		/// handlers.</param>
		/// <seealso cref="XBeeAPIPacket"/>
		/// <seealso cref="XBeePacket"/>
		private void PacketReceived(XBeePacket packet)
		{
			// Add the packet to the packets queue.
			XBeePacketsQueue.AddPacket(packet);
			// Notify that a packet has been received to the corresponding event handlers.
			NotifyPacketReceived(packet);

			// Check if the packet is an API packet.
			if (!(packet is XBeeAPIPacket))
				return;

			// Get the API packet type.
			XBeeAPIPacket apiPacket = (XBeeAPIPacket)packet;
			APIFrameType apiType = apiPacket.FrameType;

			try
			{
				// Obtain the remote device from the packet.
				RemoteXBeeDevice remoteDevice = GetRemoteXBeeDeviceFromPacket(apiPacket);
				byte[] data = null;

				switch (apiType)
				{
					case APIFrameType.RECEIVE_PACKET:
						ReceivePacket receivePacket = (ReceivePacket)apiPacket;
						data = receivePacket.RFData;
						NotifyDataReceived(new XBeeMessage(remoteDevice, data, apiPacket.IsBroadcast));
						break;
					case APIFrameType.RX_64:
						RX64Packet rx64Packet = (RX64Packet)apiPacket;
						data = rx64Packet.RFData;
						NotifyDataReceived(new XBeeMessage(remoteDevice, data, apiPacket.IsBroadcast));
						break;
					case APIFrameType.RX_16:
						RX16Packet rx16Packet = (RX16Packet)apiPacket;
						data = rx16Packet.RFData;
						NotifyDataReceived(new XBeeMessage(remoteDevice, data, apiPacket.IsBroadcast));
						break;
					case APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR:
						IODataSampleRxIndicatorPacket ioSamplePacket = (IODataSampleRxIndicatorPacket)apiPacket;
						NotifyIOSampleReceived(remoteDevice, ioSamplePacket.IOSample);
						break;
					case APIFrameType.RX_IO_64:
						RX64IOPacket rx64IOPacket = (RX64IOPacket)apiPacket;
						NotifyIOSampleReceived(remoteDevice, rx64IOPacket.IoSample);
						break;
					case APIFrameType.RX_IO_16:
						RX16IOPacket rx16IOPacket = (RX16IOPacket)apiPacket;
						NotifyIOSampleReceived(remoteDevice, rx16IOPacket.IoSample);
						break;
					case APIFrameType.MODEM_STATUS:
						ModemStatusPacket modemStatusPacket = (ModemStatusPacket)apiPacket;
						NotifyModemStatusReceived(modemStatusPacket.Status);
						break;
					case APIFrameType.EXPLICIT_RX_INDICATOR:
						ExplicitRxIndicatorPacket explicitDataPacket = (ExplicitRxIndicatorPacket)apiPacket;
						byte sourceEndpoint = explicitDataPacket.SourceEndpoint;
						byte destEndpoint = explicitDataPacket.DestEndpoint;
						byte[] clusterID = explicitDataPacket.ClusterID;
						byte[] profileID = explicitDataPacket.ProfileID;
						data = explicitDataPacket.RFData;
						// If this is an explicit packet for data transmissions in the Digi profile, 
						// notify also the data event handler and add a Receive packet to the queue.
						if (sourceEndpoint == ExplicitRxIndicatorPacket.DATA_ENDPOINT &&
								destEndpoint == ExplicitRxIndicatorPacket.DATA_ENDPOINT &&
								clusterID.SequenceEqual(ExplicitRxIndicatorPacket.DATA_CLUSTER) &&
								profileID.SequenceEqual(ExplicitRxIndicatorPacket.DIGI_PROFILE))
						{
							NotifyDataReceived(new XBeeMessage(remoteDevice, data, apiPacket.IsBroadcast));
							XBeePacketsQueue.AddPacket(new ReceivePacket(explicitDataPacket.SourceAddress64,
									explicitDataPacket.SourceAddress16,
									explicitDataPacket.ReceiveOptions,
									explicitDataPacket.RFData));
						}
						NotifyExplicitDataReceived(new ExplicitXBeeMessage(remoteDevice, sourceEndpoint, destEndpoint, clusterID,
							profileID, data, explicitDataPacket.IsBroadcast));
						break;
					case APIFrameType.USER_DATA_RELAY_OUTPUT:
						UserDataRelayOutputPacket relayPacket = (UserDataRelayOutputPacket)apiPacket;
						NotifyUserDataRelayReceived(new UserDataRelayMessage(relayPacket.SourceInterface, relayPacket.Data));
						break;
					case APIFrameType.RX_IPV4:
						RXIPv4Packet rxIPv4Packet = (RXIPv4Packet)apiPacket;
						NotifyIPDataReceived(new IPMessage(
								rxIPv4Packet.SourceAddress,
								rxIPv4Packet.SourcePort,
								rxIPv4Packet.DestPort,
								rxIPv4Packet.Protocol,
								rxIPv4Packet.Data));
						break;
					case APIFrameType.RX_SMS:
						RXSMSPacket rxSMSPacket = (RXSMSPacket)apiPacket;
						NotifySMSReceived(new SMSMessage(rxSMSPacket.PhoneNumber, rxSMSPacket.Data));
						break;
					default:
						break;
				}

			}
			catch (XBeeException e)
			{
				logger.Error(e.Message, e);
			}
		}

		/// <summary>
		/// Creates a new remote XBee device with the provided 64-bit address, 16-bit address, node 
		/// identifier and the XBee device that is using this data reader as the connection interface 
		/// for the remote device.
		/// </summary>
		/// <remarks>The new XBee device will be a <see cref="RemoteDigiMeshDevice"/>, 
		/// a <see cref="RemoteDigiPointDevice"/>, a <see cref="RemoteRaw802Device"/> or a 
		/// <see cref="RemoteZigBeeDevice"/> depending on the protocol of the local XBee device. If 
		/// the protocol cannot be determined or is unknown a <see cref="RemoteXBeeDevice"/> will 
		/// be created instead.</remarks>
		/// <param name="addr64">The 64-bit address of the new remote device. It cannot be <c>null</c>.</param>
		/// <param name="addr16">The 16-bit address of the new remote device. It may be <c>null</c>.</param>
		/// <param name="ni">The node identifier of the new remote device. It may be <c>null</c>.</param>
		/// <returns>A new remote XBee device with the given parameters.</returns>
		private RemoteXBeeDevice CreateRemoteXBeeDevice(XBee64BitAddress addr64,
				XBee16BitAddress addr16, string ni)
		{
			RemoteXBeeDevice device = null;

			switch (xbeeDevice.XBeeProtocol)
			{
				case XBeeProtocol.ZIGBEE:
					device = new RemoteZigBeeDevice(xbeeDevice, addr64, addr16, ni);
					break;
				case XBeeProtocol.DIGI_MESH:
					device = new RemoteDigiMeshDevice(xbeeDevice, addr64, ni);
					break;
				case XBeeProtocol.DIGI_POINT:
					device = new RemoteDigiPointDevice(xbeeDevice, addr64, ni);
					break;
				case XBeeProtocol.RAW_802_15_4:
					device = new RemoteRaw802Device(xbeeDevice, addr64, addr16, ni);
					break;
				default:
					device = new RemoteXBeeDevice(xbeeDevice, addr64, addr16, ni);
					break;
			}

			return device;
		}

		/// <summary>
		/// Notifies subscribed data receive listeners that a new XBee data packet has been received 
		/// in form of an <see cref="XBeeMessage"/>.
		/// </summary>
		/// <param name="xbeeMessage">The XBee message to be sent to subscribed XBee data listeners.</param>
		/// <seealso cref="XBeeMessage"/>
		private void NotifyDataReceived(XBeeMessage xbeeMessage)
		{
			if (DataReceived == null)
				return;

			if (xbeeMessage.IsBroadcast)
				logger.InfoFormat(connectionInterface.ToString() + "Broadcast data received from {0} >> {1}.", 
					xbeeMessage.Device.XBee64BitAddr, HexUtils.PrettyHexString(xbeeMessage.Data));
			else
				logger.InfoFormat(connectionInterface.ToString() + "Data received from {0} >> {1}.", 
					xbeeMessage.Device.XBee64BitAddr, HexUtils.PrettyHexString(xbeeMessage.Data));

			try
			{
				lock (DataReceived)
				{
					var handler = DataReceived;
					if (handler != null)
					{
						var args = new DataReceivedEventArgs(xbeeMessage);

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
		/// Notifies subscribed XBee packet event handlers that a new XBee Packet has been received.
		/// </summary>
		/// <param name="packet">The received XBee packet.</param>
		/// <seealso cref="XBeeAPIPacket"/>
		/// <seealso cref="XBeePacket"/>
		private void NotifyPacketReceived(XBeePacket packet)
		{
			logger.DebugFormat(connectionInterface.ToString() + "Packet received: \n{0}", packet.ToPrettyString());

			try
			{
				lock (packetReceivedHandlers)
				{
					var args = new PacketReceivedEventArgs(packet);
					XBeeAPIPacket apiPacket = (XBeeAPIPacket)packet;
					List<CustomPacketReceivedEventHandler> handlersToRemove = new List<CustomPacketReceivedEventHandler>();

					// Need to go over the list of Packet received handlers to 
					// verify which ones need to be notified of the received packet.
					foreach (var packetHandler in packetReceivedHandlers.ToList())
					{
						if (packetHandler.Handler != null)
						{
							if (packetHandler.FrameId == ALL_FRAME_IDS)
								packetHandler.Handler.DynamicInvoke(this, args);
							else if (apiPacket.NeedsAPIFrameID 
								&& apiPacket.FrameID == packetHandler.FrameId)
							{
								packetHandler.Handler.DynamicInvoke(this, args);
								handlersToRemove.Add(packetHandler);
							}
						}
					}
					foreach (CustomPacketReceivedEventHandler handlerToRemove in handlersToRemove)
						RemovePacketReceivedHandler(handlerToRemove.Handler);
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message, e);
			}
		}

		/// <summary>
		/// Notifies subscribed IO sample listeners that a new IO sample packet has been received.
		/// </summary>
		/// <param name="remoteDevice">The remote XBee device that sent the sample.</param>
		/// <param name="ioSample">The received IO sample.</param>
		/// <seealso cref="RemoteXBeeDevice"/>
		/// <seealso cref="IOSample"/>
		private void NotifyIOSampleReceived(RemoteXBeeDevice remoteDevice, IOSample ioSample)
		{
			logger.Debug(connectionInterface.ToString() + " IO sample received.");

			if (IOSampleReceived == null)
				return;

			try
			{
				lock (IOSampleReceived)
				{
					var handler = IOSampleReceived;
					if (handler != null)
					{
						var args = new IOSampleReceivedEventArgs(remoteDevice, ioSample);

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
		/// Notifies subscribed Modem Status listeners that a Modem Status event packet has been received.
		/// </summary>
		/// <param name="modemStatusEvent">The Modem Status event.</param>
		/// <seealso cref="ModemStatusEvent"/>
		private void NotifyModemStatusReceived(ModemStatusEvent modemStatusEvent)
		{
			logger.Debug(connectionInterface.ToString() + "Modem Status event received.");

			try
			{
				lock (ModemStatusReceived)
				{
					var handler = ModemStatusReceived;
					if (handler != null)
					{
						var args = new ModemStatusReceivedEventArgs(modemStatusEvent);

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
		/// Notifies subscribed explicit data receive listeners that a new XBee explicit data packet has been 
		/// received in form of an <see cref="ExplicitXBeeMessage"/>.
		/// </summary>
		/// <param name="explicitXBeeMessage">The XBee message to be sent to subscribed XBee data listeners.</param>
		/// <seealso cref="ExplicitXBeeMessage"/>
		private void NotifyExplicitDataReceived(ExplicitXBeeMessage explicitXBeeMessage)
		{
			if (ExplicitDataReceived == null)
				return;

			if (explicitXBeeMessage.IsBroadcast)
			{
				logger.InfoFormat(connectionInterface.ToString() + "Broadcast explicit data received from {0} >> {1}.",
					explicitXBeeMessage.Device.XBee64BitAddr, HexUtils.PrettyHexString(explicitXBeeMessage.Data));
			}
			else
			{
				logger.InfoFormat(connectionInterface.ToString() + "Explicit data received from {0} >> {1}.",
					explicitXBeeMessage.Device.XBee64BitAddr, HexUtils.PrettyHexString(explicitXBeeMessage.Data));
			}

			try
			{
				lock (ExplicitDataReceived)
				{
					var handler = ExplicitDataReceived;
					if (handler != null)
					{
						var args = new ExplicitDataReceivedEventArgs(explicitXBeeMessage);

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
		/// Notifies subscribed User Data Relay receive listeners that a new packet has been received in 
		/// form of an <see cref="UserDataRelayMessage"/>.
		/// </summary>
		/// <param name="userDataRelayMessage">The User Data Relay message.</param>
		/// <seealso cref="UserDataRelayMessage"/>
		private void NotifyUserDataRelayReceived(UserDataRelayMessage userDataRelayMessage)
		{
			logger.InfoFormat(connectionInterface.ToString() + " User Data Relay received from interface {0} >> {1}.",
					userDataRelayMessage.SourceInterface.GetDescription(), HexUtils.PrettyHexString(userDataRelayMessage.Data));

			// Notify generic event callbacks.
			if (UserDataRelayReceived != null)
			{
				try
				{
					lock (UserDataRelayReceived)
					{
						var handler = UserDataRelayReceived;
						if (handler != null)
						{
							var args = new UserDataRelayReceivedEventArgs(userDataRelayMessage);

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

			// Notify specific event callbacks depending on the interface.
			try
			{
				switch (userDataRelayMessage.SourceInterface)
				{
					case XBeeLocalInterface.BLUETOOTH:
						if (BluetoothDataReceived != null)
						{
							lock (BluetoothDataReceived)
							{
								var handler = BluetoothDataReceived;
								if (handler != null)
								{
									var args = new BluetoothDataReceivedEventArgs(userDataRelayMessage.Data);

									handler.GetInvocationList().AsParallel().ForAll((action) =>
									{
										action.DynamicInvoke(this, args);
									});
								}
							}
						}
						break;
					case XBeeLocalInterface.MICROPYTHON:
						if (MicroPythonDataReceived != null)
						{
							lock (MicroPythonDataReceived)
							{
								var handler = MicroPythonDataReceived;
								if (handler != null)
								{
									var args = new MicroPythonDataReceivedEventArgs(userDataRelayMessage.Data);

									handler.GetInvocationList().AsParallel().ForAll((action) =>
									{
										action.DynamicInvoke(this, args);
									});
								}
							}
						}
						break;
					case XBeeLocalInterface.SERIAL:
						if (SerialDataReceived != null)
						{
							lock (SerialDataReceived)
							{
								var handler = SerialDataReceived;
								if (handler != null)
								{
									var args = new SerialDataReceivedEventArgs(userDataRelayMessage.Data);

									handler.GetInvocationList().AsParallel().ForAll((action) =>
									{
										action.DynamicInvoke(this, args);
									});
								}
							}
						}
						break;
					default:
						break;
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message, e);
			}
		}

		/// <summary>
		/// Notifies subscribed SMS receive listeners that a new XBee SMS packet has been received 
		/// in form of an <see cref="SMSMessage"/>.
		/// </summary>
		/// <param name="smsMessage">The message to be sent to subscribed data listeners.</param>
		/// <seealso cref="SMSMessage"/>
		private void NotifySMSReceived(SMSMessage smsMessage)
		{
			logger.InfoFormat(connectionInterface.ToString() + "SMS received from {0} >> {1}.",
				smsMessage.PhoneNumber, smsMessage.Data);

			try
			{
				lock (SMSReceived)
				{
					var handler = SMSReceived;
					if (handler != null)
					{
						var args = new SMSReceivedEventArgs(smsMessage);

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
		/// Notifies subscribed IP data receive listeners that a new XBee IP packet has been received 
		/// in form of an <see cref="IPMessage"/>.
		/// </summary>
		/// <param name="ipMessage">The message to be sent to subscribed data listeners.</param>
		/// <seealso cref="IPMessage"/>
		private void NotifyIPDataReceived(IPMessage ipMessage)
		{
			logger.InfoFormat(connectionInterface.ToString() + " IP message received from {0} >> {1}.",
				ipMessage.IPAddress, ipMessage.DataString);

			try
			{
				lock (IPDataReceived)
				{
					var handler = IPDataReceived;
					if (handler != null)
					{
						var args = new IPDataReceivedEventArgs(ipMessage);

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
		/// Internal class to group a Packet Received Event Handler and a Frame ID. By default,if no Frame ID is 
		/// specified <see cref="ALL_FRAME_IDS"/> is assumed (handler associated to all incoming packets).
		/// </summary>
		internal class CustomPacketReceivedEventHandler
		{
			/// <summary>
			/// Class Constructor. Instantiates a new <see cref="CustomPacketReceivedEventHandler"/> 
			/// object for the given <paramref name="handler"/> and associates it with all incoming packets.
			/// </summary>
			/// <param name="handler">The packet received event handler.</param>
			public CustomPacketReceivedEventHandler(EventHandler<PacketReceivedEventArgs> handler)
			{
				Handler = handler;
			}

			/// <summary>
			/// Class Constructor. Instantiates a new <see cref="CustomPacketReceivedEventHandler"/> 
			/// object for the given <paramref name="handler"/> and associates it with the 
			/// given <paramref name="frameId"/>.
			/// </summary>
			/// <param name="handler">The packet received event handler.</param>
			/// <param name="frameId">The frame ID.</param>
			public CustomPacketReceivedEventHandler(EventHandler<PacketReceivedEventArgs> handler, int frameId)
			{
				Handler = handler;
				FrameId = frameId;
			}

			/// <summary>
			/// The Packet Received handler to associate to a frame ID.
			/// </summary>
			public EventHandler<PacketReceivedEventArgs> Handler { get; private set; }

			/// <summary>
			/// The Frame ID to associate to a Packet Receive handler. Default value of <see cref="ALL_FRAME_IDS"/> 
			/// to associate the handler to all incoming packets.
			/// </summary>
			public int FrameId { get; private set; } = ALL_FRAME_IDS;
		}
	}
}