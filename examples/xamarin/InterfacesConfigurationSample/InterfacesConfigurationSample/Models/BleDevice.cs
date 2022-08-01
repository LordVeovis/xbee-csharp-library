/*
 * Copyright 2022, Digi International Inc.
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

using InterfacesConfigurationSample.Exceptions;
using InterfacesConfigurationSample.Utils;
using InterfacesConfigurationSample.Utils.Validators;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;
using XBeeLibrary.Xamarin;

namespace InterfacesConfigurationSample.Models
{
    /// <summary>
    /// Class that represents a BLE device connected through Bluetooth Low Energy.
    /// </summary>
    public class BleDevice : INotifyPropertyChanged
    {
        // Constants.
        public static string MAC_REGEX = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
        public static string MAC_REPLACE = "$1:$2:$3:$4:$5:$6";

        private const string PREFIX_PWD = "pwd_";

        public static readonly int INACTIVITY_TIME = 10000;

        private static readonly int TIMEOUT_DEFAULT = 5;

        // Variables.
        private int rssi = RssiUtils.WORST_RSSI;

        private bool isActive = true;

        private readonly string BleMac;

        private readonly List<Interface> interfaces = new List<Interface>();

        // Properties.
        /// <summary>
        /// Gets the internal BLE device object associated to this device.
        /// </summary>
        public IDevice Device { get; private set; }

        /// <summary>
        /// Gets the XBee device object associated to this device.
        /// </summary>
        public XBeeBLEDevice XBeeDevice { get; private set; }

        /// <summary>
        /// Gets the BLE ID of this device.
        /// </summary>
        public Guid Id => Device.Id;

        /// <summary>
        /// Gets the Bluetooth Low Energy MAC address of this device (empty 
        /// if it is running in iOS).
        /// </summary>
        public string BLEMac { get; private set; } = "";

        /// <summary>
        /// Gets the BLE name of this device.
        /// </summary>
        public string Name => string.IsNullOrEmpty(Device.Name) ? "<Unknown>" : Device.Name;

        /// <summary>
        /// Gets whether the device is connected or not.
        /// </summary>
        public bool IsConnected => Device.State == DeviceState.Connected;

        /// <summary>
        /// Gets and sets the RSSI of this device.
        /// </summary>
        public int Rssi
        {
            get => rssi;
            set
            {
                if (value < 0 && value != RssiUtils.WORST_RSSI)
                {
                    rssi = value;
                }

                LastUpdated = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                IsActive = true;

                RaisePropertyChangedEvent("Rssi");
                RaisePropertyChangedEvent("RssiImage");
            }
        }

        /// <summary>
        /// Gets the image corresponding with the current RSSI value.
        /// </summary>
        public string RssiImage => RssiUtils.GetRssiImage(rssi);

        /// <summary>
        /// Gets the time when this device was last updated.
        /// </summary>
        public long LastUpdated { get; private set; }

        /// <summary>
        /// Gets and sets whether this device is active or not.
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                RaisePropertyChangedEvent("IsActive");
            }
        }

        /// <summary>
        /// Sets the password of the bluetooth device.
        /// </summary>
        public string Password
        {
            set => XBeeDevice.SetBluetoothPassword(value);
        }

        /// <summary>
        /// Indicates whether the device is connecting or not.
        /// </summary>
        public bool Connecting { get; set; } = false;

        /// <summary>
		/// List of interfaces of the device.
		/// </summary>
		public List<Interface> Interfaces => interfaces;

        // Events.
        public event PropertyChangedEventHandler PropertyChanged;

        // Methods
        /// <summary>
        /// Class constructor. Instantiates a new <c>BleDevice</c> with the provided
        /// <c>IDevice</c>.
        /// </summary>
        /// <param name="device">Internal BLE device.</param>
        public BleDevice(IDevice device)
        {
            Device = device ?? throw new ArgumentNullException("Device cannot be null");

            Rssi = device.Rssi;
            BleMac = Regex.Replace(Device.Id.ToString().Split('-')[4], MAC_REGEX, MAC_REPLACE).ToUpper();
            if (Xamarin.Forms.Device.RuntimePlatform.Equals(Xamarin.Forms.Device.Android))
            {
                BLEMac = BleMac;
            }

            XBeeDevice = new XBeeBLEDevice(device, null);

            InitializeInterfaces();
        }

        /// <summary>
        /// Initializes the device configurable interfaces.
        /// </summary>
        private void InitializeInterfaces()
        {
            GenerateNetworkInterface("Ethernet");
            GenerateNetworkInterface("WiFi");
        }

        /// <summary>
        /// Generates a new network interface.
        /// </summary>
        /// <param name="interfaceName">Name of the interface.</param>
        private void GenerateNetworkInterface(string interfaceName)
        {
            List<AbstractSetting> settings = new List<AbstractSetting>
            {
                new BooleanSetting("Enabled", "False"),
                new TextSetting("MAC", "00:00:00:00:00:00", new MACValidator()),
                new ComboSetting("Type", "static", new Dictionary<string, string>() { { "static", "Static" }, { "dhcp", "DHCP" } }),
                new TextSetting("IP", "0.0.0.0", new IPValidator()),
                new TextSetting("Netmask", "0.0.0.0", new IPValidator()),
                new TextSetting("Gateway", "0.0.0.0", new IPValidator()),
                new TextSetting("DNS", "0.0.0.0", new IPValidator())
            };
            interfaces.Add(new Interface(interfaceName, settings, this));
        }

        /// <summary>
        /// Opens the connection with the IX15 device.
        /// </summary>
        public void Open()
        {
            XBeeDevice.Open();
        }

        /// <summary>
        /// Closes the connection with the IX15 device.
        /// </summary>
        public void Close()
        {
            XBeeDevice.Close();
        }

        /// <summary>
        /// Generates and raises a new event indicating that the provided 
        /// property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that has 
        /// changed.</param>
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Returns the stored password of the device.
        /// </summary>
        /// <returns>Stored password of the device.</returns>
        public Task<string> GetStoredPassword()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            new Task(async () =>
            {
                string password = await SecureStorage.GetAsync(PREFIX_PWD + BleMac);
                tcs.SetResult(password);
            }).Start();

            return tcs.Task;
        }

        /// <summary>
        /// Stores the password of the device.
        /// </summary>
        /// <param name="password">Password to store.</param>
        public async Task SetStoredPassword(string password)
        {
            await SecureStorage.SetAsync(PREFIX_PWD + BleMac, password);
        }

        /// <summary>
        /// Clears the stored password of the device.
        /// </summary>
        public async Task ClearStoredPassword()
        {
            string password = await GetStoredPassword();
            if (password != null)
            {
                SecureStorage.Remove(PREFIX_PWD + BleMac);
            }
        }

        /// <summary>
        /// Sends the given data to the device.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>The execution answer.</returns>
        /// <exception cref="CommunicationException">If there is any error sending the data.</exception>
        public async Task<string> SendData(string data)
        {
            return await SendData(data, TIMEOUT_DEFAULT);
        }

        /// <summary>
        /// Sends the given data to the device.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>The execution answer.</returns>
        /// <exception cref="CommunicationException">If there is any error sending the data.</exception>
        public async Task<string> SendData(byte[] data)
        {
            return await SendData(data, TIMEOUT_DEFAULT);
        }

        /// <summary>
        /// Sends the given data to the device.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="timeout">The execution timeout.</param>
        /// <returns>The execution answer.</returns>
        /// <exception cref="CommunicationException">If there is any error sending the data.</exception>
        public async Task<string> SendData(string data, int timeout)
        {
            // Convert data to byte array.
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            // Send the data.
            return await SendData(byteData, timeout);
        }

        /// <summary>
        /// Sends the given data to the device.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="timeout">The execution timeout.</param>
        /// <returns>The execution answer.</returns>
        /// <exception cref="CommunicationException">If there is any error sending the data.</exception>
        public async Task<string> SendData(byte[] data, int timeout)
        {
            string answer = null;
            ManualResetEvent waitHandle = new ManualResetEvent(false);

            if (!XBeeDevice.IsOpen)
            {
                throw new Exceptions.CommunicationException("Device is not open");
            }

            void UserDataRelayReceived(object sender, UserDataRelayReceivedEventArgs e)
            {
                UserDataRelayMessage message = e.UserDataRelayMessage;
                // Extract JSON string
                answer = Encoding.ASCII.GetString(message.Data, 0, message.Data.Length);
                Console.WriteLine("Received data: " + answer);
                // Notify wait handle.
                waitHandle.Set();
            }

            // Register data received callback.
            XBeeDevice.UserDataRelayReceived += UserDataRelayReceived;
            // Send the data.
            await Task.Run(() =>
            {
                try
                {
                    XBeeDevice.SendUserDataRelay(XBeeLocalInterface.SERIAL, data);
                    // Wait for answer.
                    waitHandle.WaitOne(timeout * 1000);
                }
                catch (XBeeException ex2)
                {
                    XBeeDevice.UserDataRelayReceived -= UserDataRelayReceived;
                    throw new Exceptions.CommunicationException("Error sending data: " + ex2.Message);
                }
            });
            // Remove data received callback.
            XBeeDevice.UserDataRelayReceived -= UserDataRelayReceived;

            return answer ?? throw new Exceptions.CommunicationException("Timeout waiting for execution answer");
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            BleDevice dev = (BleDevice)obj;
            return Id == dev.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
