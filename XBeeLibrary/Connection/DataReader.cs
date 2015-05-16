using Common.Logging;
using Kveer.XBeeApi.Connection;
using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.listeners;
using Kveer.XBeeApi.Models;
using Kveer.XBeeApi.Packet.Common;
using Kveer.XBeeApi.Packet;
using Kveer.XBeeApi.Packet.Raw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Kveer.XBeeApi.Utils;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Kveer.XBeeApi.IO;

namespace Kveer.XBeeApi.Connection
{
	/// <summary>
	/// Thread that constantly reads data from an input stream.
	/// </summary>
	/// <remarks>Depending on the XBee operating mode, read data is notified as is to the subscribed listeners or is parsed to a packet using the packet parser and then notified to subscribed listeners.</remarks>
	public class DataReader /*: Thread*/
	{
		// Constants.
		private const int ALL_FRAME_IDS = 99999;
		private const int MAXIMUM_PARALLEL_LISTENER_THREADS = 20;

		// Variables.
		private bool running = false;

		private IConnectionInterface connectionInterface;

		private volatile OperatingMode mode;

		private IList<IDataReceiveListener> dataReceiveListeners = new List<IDataReceiveListener>();
		// The packetReceiveListeners requires to be a HashMap with an associated integer. The integer is used to determine 
		// the frame ID of the packet that should be received. When it is 99999 (ALL_FRAME_IDS), all the packets will be handled.
		private IDictionary<IPacketReceiveListener, int> packetReceiveListeners = new Dictionary<IPacketReceiveListener, int>();
		private IList<IIOSampleReceiveListener> ioSampleReceiveListeners = new List<IIOSampleReceiveListener>();
		private IList<IModemStatusReceiveListener> modemStatusListeners = new List<IModemStatusReceiveListener>();

		private ILog logger;

		private XBeePacketParser parser;

		private XBeePacketsQueue xbeePacketsQueue;

		private XBeeDevice xbeeDevice;

		/**
		 * Class constructor. Instantiates a new {@code DataReader} object for the 
		 * given connection interface using the given XBee operating mode and XBee
		 * device.
		 * 
		 * @param connectionInterface Connection interface to read data from.
		 * @param mode XBee operating mode.
		 * @param xbeeDevice Reference to the XBee device containing this 
		 *                   {@code DataReader} object.
		 * 
		 * @throws ArgumentNullException if {@code connectionInterface == null} or
		 *                                 {@code mode == null}.
		 * 
		 * @see IConnectionInterface
		 * @see com.digi.xbee.api.XBeeDevice
		 * @see com.digi.xbee.api.models.OperatingMode
		 */
		public DataReader(IConnectionInterface connectionInterface, OperatingMode mode, XBeeDevice xbeeDevice)
		{
			if (connectionInterface == null)
				throw new ArgumentNullException("Connection interface cannot be null.");
			if (mode == null)
				throw new ArgumentNullException("Operating mode cannot be null.");

			this.connectionInterface = connectionInterface;
			this.mode = mode;
			this.xbeeDevice = xbeeDevice;
			this.logger = LogManager.GetLogger<DataReader>();
			parser = new XBeePacketParser();
			xbeePacketsQueue = new XBeePacketsQueue();
		}

		Thread _thread;
		internal void start()
		{
			_thread = new Thread(new ThreadStart(() => Run()));
			_thread.Start();
		}

		/**
		 * Sets the XBee operating mode of this data reader.
		 * 
		 * @param mode New XBee operating mode.
		 * 
		 * @throws ArgumentNullException if {@code mode == null}.
		 * 
		 * @see com.digi.xbee.api.models.OperatingMode
		 */
		public void SetXBeeReaderMode(OperatingMode mode)
		{
			if (mode == null)
				throw new ArgumentNullException("Operating mode cannot be null.");

			this.mode = mode;
		}

		/**
		 * Adds the given data receive listener to the list of listeners that will 
		 * be notified when XBee data packets are received.
		 * 
		 * <p>If the listener has been already added, this method does nothing.</p>
		 * 
		 * @param listener Listener to be notified when new XBee data packets are 
		 *                 received.
		 * 
		 * @see #removeDataReceiveListener(IDataReceiveListener)
		 * @see com.digi.xbee.api.listeners.IDataReceiveListener
		 */
		public void AddDataReceiveListener(IDataReceiveListener listener)
		{
			lock (dataReceiveListeners)
			{
				if (!dataReceiveListeners.Contains(listener))
					dataReceiveListeners.Add(listener);
			}
		}

		/**
		 * Removes the given data receive listener from the list of data receive 
		 * listeners.
		 * 
		 * <p>If the listener is not included in the list, this method does nothing.
		 * </p>
		 * 
		 * @param listener Data receive listener to be remove from the list.
		 * 
		 * @see #addDataReceiveListener(IDataReceiveListener)
		 * @see com.digi.xbee.api.listeners.IDataReceiveListener
		 */
		public void RemoveDataReceiveListener(IDataReceiveListener listener)
		{
			lock (dataReceiveListeners)
			{
				if (dataReceiveListeners.Contains(listener))
					dataReceiveListeners.Remove(listener);
			}
		}

		/**
		 * Adds the given packet receive listener to the list of listeners that will
		 * be notified when any XBee packet is received.
		 * 
		 * <p>If the listener has been already added, this method does nothing.</p>
		 * 
		 * @param listener Listener to be notified when any XBee packet is received.
		 * 
		 * @see #addPacketReceiveListener(IPacketReceiveListener, int)
		 * @see #removePacketReceiveListener(IPacketReceiveListener)
		 * @see com.digi.xbee.api.listeners.IPacketReceiveListener
		 */
		public void AddPacketReceiveListener(IPacketReceiveListener listener)
		{
			AddPacketReceiveListener(listener, ALL_FRAME_IDS);
		}

		/**
		 * Adds the given packet receive listener to the list of listeners that will
		 * be notified when an XBee packet with the given frame ID is received.
		 * 
		 * <p>If the listener has been already added, this method does nothing.</p>
		 * 
		 * @param listener Listener to be notified when an XBee packet with the
		 *                 provided frame ID is received.
		 * @param frameID Frame ID for which this listener should be notified and 
		 *                removed after.
		 *                Using {@link #ALL_FRAME_IDS} this listener will be 
		 *                notified always and will be removed only by user request.
		 * 
		 * @see #addPacketReceiveListener(IPacketReceiveListener)
		 * @see #removePacketReceiveListener(IPacketReceiveListener)
		 * @see com.digi.xbee.api.listeners.IPacketReceiveListener
		 */
		public void AddPacketReceiveListener(IPacketReceiveListener listener, int frameID)
		{
			lock (packetReceiveListeners)
			{
				if (!packetReceiveListeners.ContainsKey(listener))
					packetReceiveListeners.Add(listener, frameID);
			}
		}

		/**
		 * Removes the given packet receive listener from the list of XBee packet 
		 * receive listeners.
		 * 
		 * <p>If the listener is not included in the list, this method does nothing.
		 * </p>
		 * 
		 * @param listener Packet receive listener to remove from the list.
		 * 
		 * @see #addPacketReceiveListener(IPacketReceiveListener)
		 * @see #addPacketReceiveListener(IPacketReceiveListener, int)
		 * @see com.digi.xbee.api.listeners.IPacketReceiveListener
		 */
		public void RemovePacketReceiveListener(IPacketReceiveListener listener)
		{
			lock (packetReceiveListeners)
			{
				if (packetReceiveListeners.ContainsKey(listener))
					packetReceiveListeners.Remove(listener);
			}
		}

		/**
		 * Adds the given IO sample receive listener to the list of listeners that 
		 * will be notified when an IO sample packet is received.
		 * 
		 * <p>If the listener has been already added, this method does nothing.</p>
		 * 
		 * @param listener Listener to be notified when new IO sample packets are 
		 *                 received.
		 * 
		 * @see #removeIOSampleReceiveListener(IIOSampleReceiveListener)
		 * @see com.digi.xbee.api.listeners.IIOSampleReceiveListener
		 */
		public void AddIOSampleReceiveListener(IIOSampleReceiveListener listener)
		{
			lock (ioSampleReceiveListeners)
			{
				if (!ioSampleReceiveListeners.Contains(listener))
					ioSampleReceiveListeners.Add(listener);
			}
		}

		/**
		 * Removes the given IO sample receive listener from the list of IO sample 
		 * receive listeners.
		 * 
		 * <p>If the listener is not included in the list, this method does nothing.
		 * </p>
		 * 
		 * @param listener IO sample receive listener to remove from the list.
		 * 
		 * @see #addIOSampleReceiveListener(IIOSampleReceiveListener)
		 * @see com.digi.xbee.api.listeners.IIOSampleReceiveListener
		 */
		public void RemoveIOSampleReceiveListener(IIOSampleReceiveListener listener)
		{
			lock (ioSampleReceiveListeners)
			{
				if (ioSampleReceiveListeners.Contains(listener))
					ioSampleReceiveListeners.Remove(listener);
			}
		}

		/**
		 * Adds the given Modem Status receive listener to the list of listeners 
		 * that will be notified when a modem status packet is received.
		 * 
		 * <p>If the listener has been already added, this method does nothing.</p>
		 * 
		 * @param listener Listener to be notified when new modem status packets are
		 *                 received.
		 * 
		 * @see #removeModemStatusReceiveListener(IModemStatusReceiveListener)
		 * @see com.digi.xbee.api.listeners.IModemStatusReceiveListener
		 */
		public void AddModemStatusReceiveListener(IModemStatusReceiveListener listener)
		{
			lock (modemStatusListeners)
			{
				if (!modemStatusListeners.Contains(listener))
					modemStatusListeners.Add(listener);
			}
		}

		/**
		 * Removes the given Modem Status receive listener from the list of Modem 
		 * Status receive listeners.
		 * 
		 * <p>If the listener is not included in the list, this method does nothing.
		 * </p>
		 * 
		 * @param listener Modem Status receive listener to remove from the list.
		 * 
		 * @see #addModemStatusReceiveListener(IModemStatusReceiveListener)
		 * @see com.digi.xbee.api.listeners.IModemStatusReceiveListener
		 */
		public void RemoveModemStatusReceiveListener(IModemStatusReceiveListener listener)
		{
			lock (modemStatusListeners)
			{
				if (modemStatusListeners.Contains(listener))
					modemStatusListeners.Remove(listener);
			}
		}

		/*
		 * (non-Javadoc)
		 * @see java.lang.Thread#run()
		 */
		//@Override
		public void Run()
		{
			logger.Debug(connectionInterface.ToString() + "Data reader started.");
			running = true;
			// Clear the list of read packets.
			xbeePacketsQueue.ClearQueue();
			try
			{
				lock (connectionInterface)
				{
					Monitor.Wait(connectionInterface);
				}
				while (running)
				{
					if (!running)
						break;
					if (connectionInterface.SerialPort != null)
					{
						switch (mode)
						{
							case OperatingMode.AT:
								break;
							case OperatingMode.API:
							case OperatingMode.API_ESCAPE:
								int headerByte = connectionInterface.SerialPort.ReadByte();
								// If it is packet header parse the packet, if not discard this byte and continue.
								if (headerByte == SpecialByte.HEADER_BYTE.GetValue())
								{
									try
									{
										XBeePacket packet = parser.ParsePacket(connectionInterface.SerialPort, mode);
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
					else if (connectionInterface.SerialPort == null)
						break;
					if (connectionInterface.SerialPort == null)
						break;
					else if (connectionInterface.SerialPort.BytesToRead > 0)
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
			catch (ThreadInterruptedException e)
			{
				logger.Error(e.Message, e);
			}
			//catch (IllegalStateException e)
			//{
			//	logger.Error(e.Message, e);
			//}
			finally
			{
				if (running)
				{
					running = false;
					if (connectionInterface.IsOpen)
						connectionInterface.Close();
				}
			}
		}

		/**
		 * Dispatches the received XBee packet to the corresponding listener(s).
		 * 
		 * @param packet The received XBee packet to be dispatched to the 
		 *               corresponding listeners.
		 * 
		 * @see com.digi.xbee.api.packet.XBeeAPIPacket
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		private void PacketReceived(XBeePacket packet)
		{
			// Add the packet to the packets queue.
			xbeePacketsQueue.AddPacket(packet);
			// Notify that a packet has been received to the corresponding listeners.
			notifyPacketReceived(packet);

			// Check if the packet is an API packet.
			if (!(packet is XBeeAPIPacket))
				return;

			// Get the API packet type.
			XBeeAPIPacket apiPacket = (XBeeAPIPacket)packet;
			APIFrameType apiType = apiPacket.FrameType;
			if (apiType == null)
				return;

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
						notifyDataReceived(new XBeeMessage(remoteDevice, data, apiPacket.IsBroadcast));
						break;
					case APIFrameType.RX_64:
						RX64Packet rx64Packet = (RX64Packet)apiPacket;
						data = rx64Packet.RFData;
						notifyDataReceived(new XBeeMessage(remoteDevice, data, apiPacket.IsBroadcast));
						break;
					case APIFrameType.RX_16:
						RX16Packet rx16Packet = (RX16Packet)apiPacket;
						data = rx16Packet.RFData;
						notifyDataReceived(new XBeeMessage(remoteDevice, data, apiPacket.IsBroadcast));
						break;
					case APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR:
						IODataSampleRxIndicatorPacket ioSamplePacket = (IODataSampleRxIndicatorPacket)apiPacket;
						NotifyIOSampleReceived(remoteDevice, ioSamplePacket.IOSample);
						break;
					case APIFrameType.RX_IO_64:
						RX64IOPacket rx64IOPacket = (RX64IOPacket)apiPacket;
						NotifyIOSampleReceived(remoteDevice, rx64IOPacket.getIOSample());
						break;
					case APIFrameType.RX_IO_16:
						RX16IOPacket rx16IOPacket = (RX16IOPacket)apiPacket;
						NotifyIOSampleReceived(remoteDevice, rx16IOPacket.IOSample);
						break;
					case APIFrameType.MODEM_STATUS:
						ModemStatusPacket modemStatusPacket = (ModemStatusPacket)apiPacket;
						NotifyModemStatusReceived(modemStatusPacket.Status);
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

		/**
		 * Returns the remote XBee device from where the given package was sent 
		 * from.
		 * 
		 * <p><b>This is for internal use only.</b></p>
		 * 
		 * <p>If the package does not contain information about the source, this 
		 * method returns {@code null} (for example, {@code ModemStatusPacket}).</p>
		 * 
		 * <p>First the device that sent the provided package is looked in the 
		 * network of the local XBee device. If the remote device is not in the 
		 * network, it is automatically added only if the packet contains 
		 * information about the origin of the package.</p>
		 * 
		 * @param packet The packet sent from the remote device.
		 * 
		 * @return The remote XBee device that sends the given packet. It may be 
		 *         {@code null} if the packet is not a known frame (see 
		 *         {@link APIFrameType}) or if it does not contain information of 
		 *         the source device.
		 * 
		 * @throws ArgumentNullException if {@code packet == null}
		 * @throws XBeeException if any error occur while adding the device to the 
		 *                       network.
		 */
		public RemoteXBeeDevice GetRemoteXBeeDeviceFromPacket(XBeeAPIPacket packet) /*throws XBeeException*/ {
			if (packet == null)
				throw new ArgumentNullException("XBee API packet cannot be null.");

			XBeeAPIPacket apiPacket = (XBeeAPIPacket)packet;
			APIFrameType apiType = apiPacket.FrameType;
			if (apiType == null)
				return null;

			RemoteXBeeDevice remoteDevice = null;
			XBee64BitAddress addr64 = null;
			XBee16BitAddress addr16 = null;

			XBeeNetwork network = xbeeDevice.GetNetwork();

			switch (apiType)
			{
				case APIFrameType.RECEIVE_PACKET:
					ReceivePacket receivePacket = (ReceivePacket)apiPacket;
					addr64 = receivePacket.get64bitSourceAddress();
					addr16 = receivePacket.get16bitSourceAddress();
					remoteDevice = network.getDevice(addr64);
					break;
				case APIFrameType.RX_64:
					RX64Packet rx64Packet = (RX64Packet)apiPacket;
					addr64 = rx64Packet.SourceAddress64;
					remoteDevice = network.getDevice(addr64);
					break;
				case APIFrameType.RX_16:
					RX16Packet rx16Packet = (RX16Packet)apiPacket;
					addr64 = XBee64BitAddress.UNKNOWN_ADDRESS;
					addr16 = rx16Packet.Get16bitSourceAddress();
					remoteDevice = network.getDevice(addr16);
					break;
				case APIFrameType.IO_DATA_SAMPLE_RX_INDICATOR:
					IODataSampleRxIndicatorPacket ioSamplePacket = (IODataSampleRxIndicatorPacket)apiPacket;
					addr64 = ioSamplePacket.get64bitSourceAddress();
					addr16 = ioSamplePacket.get16bitSourceAddress();
					remoteDevice = network.getDevice(addr64);
					break;
				case APIFrameType.RX_IO_64:
					RX64IOPacket rx64IOPacket = (RX64IOPacket)apiPacket;
					addr64 = rx64IOPacket.SourceAddress64;
					remoteDevice = network.getDevice(addr64);
					break;
				case APIFrameType.RX_IO_16:
					RX16IOPacket rx16IOPacket = (RX16IOPacket)apiPacket;
					addr64 = XBee64BitAddress.UNKNOWN_ADDRESS;
					addr16 = rx16IOPacket.get16bitSourceAddress();
					remoteDevice = network.getDevice(addr16);
					break;
				default:
					// Rest of the types are considered not to contain information 
					// about the origin of the packet.
					return remoteDevice;
			}

			// If the origin is not in the network, add it.
			if (remoteDevice == null)
			{
				remoteDevice = createRemoteXBeeDevice(addr64, addr16, null);
				network.addRemoteDevice(remoteDevice);
			}

			return remoteDevice;
		}

		/**
		 * Creates a new remote XBee device with the provided 64-bit address, 
		 * 16-bit address, node identifier and the XBee device that is using this 
		 * data reader as the connection interface for the remote device.
		 * 
		 * The new XBee device will be a {@code RemoteDigiMeshDevice}, 
		 * a {@code RemoteDigiPointDevice}, a {@code RemoteRaw802Device} or a 
		 * {@code RemoteZigBeeDevice} depending on the protocol of the local XBee 
		 * device. If the protocol cannot be determined or is unknown a 
		 * {@code RemoteXBeeDevice} will be created instead.
		 * 
		 * @param addr64 The 64-bit address of the new remote device. It cannot be 
		 *               {@code null}.
		 * @param addr16 The 16-bit address of the new remote device. It may be 
		 *               {@code null}.
		 * @param ni The node identifier of the new remote device. It may be 
		 *           {@code null}.
		 * 
		 * @return a new remote XBee device with the given parameters.
		 */
		private RemoteXBeeDevice createRemoteXBeeDevice(XBee64BitAddress addr64,
				XBee16BitAddress addr16, String ni)
		{
			RemoteXBeeDevice device = null;

			switch (xbeeDevice.getXBeeProtocol())
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

		/**
		 * Notifies subscribed data receive listeners that a new XBee data packet 
		 * has been received in form of an {@code XBeeMessage}.
		 *
		 * @param xbeeMessage The XBee message to be sent to subscribed XBee data
		 *                    listeners.
		 * 
		 * @see com.digi.xbee.api.models.XBeeMessage
		 */
		private void notifyDataReceived(XBeeMessage xbeeMessage)
		{
			if (xbeeMessage.IsBroadcast)
				logger.InfoFormat(connectionInterface.ToString() +
						"Broadcast data received from {0} >> {1}.", xbeeMessage.Device.get64BitAddress(), HexUtils.PrettyHexString(xbeeMessage.Data));
			else
				logger.InfoFormat(connectionInterface.ToString() +
						"Data received from {0} >> {1}.", xbeeMessage.Device.get64BitAddress(), HexUtils.PrettyHexString(xbeeMessage.Data));

			try
			{
				lock (dataReceiveListeners)
				{
					var action = new Action<IDataReceiveListener, XBeeMessage>((listener, message) =>
					{
						lock (listener)
						{
							listener.DataReceived(message);
						}
					});
					var maxThreads = Math.Min(MAXIMUM_PARALLEL_LISTENER_THREADS, dataReceiveListeners.Count);

					var tasks = new ConcurrentBag<Task>();
					//ScheduledExecutorService executor = Executors.newScheduledThreadPool();
					foreach (IDataReceiveListener listener in dataReceiveListeners)
					{
						tasks.Add(Task.Factory.StartNew((state) =>
						{
							var listener2 = (IDataReceiveListener)((object[])state)[0];
							var message = (XBeeMessage)((object[])state)[1];

							action(listener2, message);
						}, new object[] { listener, xbeeMessage }));


					}
					Task.WaitAll(tasks.ToArray());

					//executor.shutdown();
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message, e);
			}
		}

		/**
		 * Notifies subscribed XBee packet listeners that a new XBee packet has 
		 * been received.
		 *
		 * @param packet The received XBee packet.
		 * 
		 * @see com.digi.xbee.api.packet.XBeeAPIPacket
		 * @see com.digi.xbee.api.packet.XBeePacket
		 */
		private void notifyPacketReceived(XBeePacket packet)
		{
			logger.DebugFormat(connectionInterface.ToString() + "Packet received: \n{0}", packet.ToPrettyString());

			try
			{
				lock (packetReceiveListeners)
				{
					var removeListeners = new List<IPacketReceiveListener>();
					var runTask = new Action<IPacketReceiveListener>((listener) =>
					{
						lock (listener)
						{
							if (packetReceiveListeners[listener] == ALL_FRAME_IDS)
								listener.PacketReceived(packet);
							else if (((XBeeAPIPacket)packet).NeedsAPIFrameID &&
									((XBeeAPIPacket)packet).FrameID == packetReceiveListeners[listener])
							{
								listener.PacketReceived(packet);
								removeListeners.Add(listener);
							}
						}
					});

					//ScheduledExecutorService executor = Executors.newScheduledThreadPool(Math.Min(MAXIMUM_PARALLEL_LISTENER_THREADS, 
					//packetReceiveListeners.Count));
					var tasks = new ConcurrentBag<Task>();
					foreach (IPacketReceiveListener listener in packetReceiveListeners.Keys)
					{
						tasks.Add(Task.Factory.StartNew((state) =>
						{
							runTask((IPacketReceiveListener)state);
						}, listener));
						//	executor.execute(new Runnable() {
						//		/*
						//		 * (non-Javadoc)
						//		 * @see java.lang.Runnable#run()
						//		 */
						//		//@Override
						//		public void run() {
						//			// Synchronize the listener so it is not called 
						//			// twice. That is, let the listener to finish its job.
						//			lock (packetReceiveListeners) {
						//				lock (listener) {
						//					if (packetReceiveListeners[listener] == ALL_FRAME_IDS)
						//						listener.packetReceived(packet);
						//					else if (((XBeeAPIPacket)packet).needsAPIFrameID() && 
						//							((XBeeAPIPacket)packet).getFrameID() == packetReceiveListeners[listener]) {
						//						listener.packetReceived(packet);
						//						removeListeners.add(listener);
						//					}
						//				}
						//			}
						//		}
						//	});
					}
					Task.WaitAll(tasks.ToArray());
					//executor.shutdown();
					// Remove required listeners.
					foreach (IPacketReceiveListener listener in removeListeners)
						packetReceiveListeners.Remove(listener);
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message, e);
			}
		}

		/**
		 * Notifies subscribed IO sample listeners that a new IO sample packet has
		 * been received.
		 *
		 * @param ioSample The received IO sample.
		 * @param remoteDevice The remote XBee device that sent the sample.
		 * 
		 * @see com.digi.xbee.api.RemoteXBeeDevice
		 * @see com.digi.xbee.api.io.IOSample
		 */
		private void NotifyIOSampleReceived(RemoteXBeeDevice remoteDevice, IOSample ioSample)
		{
			logger.Debug(connectionInterface.ToString() + "IO sample received.");

			try
			{
				lock (ioSampleReceiveListeners)
				{
					var runTask = new Action<IIOSampleReceiveListener>(listener =>
					{
						lock (listener)
						{
							listener.ioSampleReceived(remoteDevice, ioSample);
						}
					});
					//ScheduledExecutorService executor = Executors.newScheduledThreadPool(Math.Min(MAXIMUM_PARALLEL_LISTENER_THREADS, 
					//ioSampleReceiveListeners.Count));
					var tasks = new ConcurrentBag<Task>();
					foreach (IIOSampleReceiveListener listener in ioSampleReceiveListeners)
					{
						tasks.Add(Task.Factory.StartNew((state) =>
						{
							runTask((IIOSampleReceiveListener)state);
						}, listener));
						//executor.execute(new Runnable() {
						//	/*
						//	 * (non-Javadoc)
						//	 * @see java.lang.Runnable#run()
						//	 */
						//	//@Override
						//	public void run() {
						//		// Synchronize the listener so it is not called 
						//		// twice. That is, let the listener to finish its job.
						//		lock (listener) {
						//			listener.ioSampleReceived(remoteDevice, ioSample);
						//		}
						//	}
						//});
					}
					Task.WaitAll(tasks.ToArray());
					//executor.shutdown();
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message, e);
			}
		}

		/**
		 * Notifies subscribed Modem Status listeners that a Modem Status event 
		 * packet has been received.
		 *
		 * @param modemStatusEvent The Modem Status event.
		 * 
		 * @see com.digi.xbee.api.models.ModemStatusEvent
		 */
		private void NotifyModemStatusReceived(ModemStatusEvent modemStatusEvent)
		{
			logger.Debug(connectionInterface.ToString() + "Modem Status event received.");

			try
			{
				lock (modemStatusListeners)
				{
					var runTask = new Action<IModemStatusReceiveListener>(listener =>
					{
						lock (listener)
						{
							listener.modemStatusEventReceived(modemStatusEvent);
						}
					});
					//ScheduledExecutorService executor = Executors.newScheduledThreadPool(Math.Min(MAXIMUM_PARALLEL_LISTENER_THREADS, 
					//		modemStatusListeners.size()));
					foreach (IModemStatusReceiveListener listener in modemStatusListeners)
					{
						Task.Factory.StartNew((state) =>
							runTask((IModemStatusReceiveListener)state), listener);
						//executor.execute(new Runnable() {
						//	/*
						//	 * (non-Javadoc)
						//	 * @see java.lang.Runnable#run()
						//	 */
						//	//@Override
						//	public void run() {
						//		// Synchronize the listener so it is not called 
						//		// twice. That is, let the listener to finish its job.
						//		lock (listener) {
						//			listener.modemStatusEventReceived(modemStatusEvent);
						//		}
						//	}
						//});
					}
					//executor.shutdown();
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message, e);
			}
		}

		/**
		 * Returns whether this Data reader is running or not.
		 * 
		 * @return {@code true} if the Data reader is running, {@code false} 
		 *         otherwise.
		 * 
		 * @see #stopReader()
		 */
		public bool IsRunning
		{
			get
			{
				return running;
			}
		}

		/**
		 * Stops the Data reader thread.
		 * 
		 * @see #isRunning()
		 */
		public void StopReader()
		{
			running = false;
			lock (connectionInterface)
			{
				Monitor.Pulse(connectionInterface);
			}
			logger.Debug(connectionInterface.ToString() + "Data reader stopped.");
		}

		/**
		 * Returns the queue of read XBee packets.
		 * 
		 * @return The queue of read XBee packets.
		 * 
		 * @see com.digi.xbee.api.models.XBeePacketsQueue
		 */
		public XBeePacketsQueue XBeePacketsQueue
		{
			get
			{
				return xbeePacketsQueue;
			}
		}
	}
}