using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Text.RegularExpressions;
using XBeeLibrary.Xamarin;
using IX15Configurator.Utils;
using System.ComponentModel;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Text;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Models;
using System.Threading;
using System.Text.Json;
using System.Collections.Generic;
using IX15Configurator.Exceptions;
using XBeeLibrary.Core.Exceptions;

namespace IX15Configurator.Models
{
    /// <summary>
    /// Class that represents an IX15 device connected through Bluetooth Low Energy.
    /// </summary>
    public class IX15Device : INotifyPropertyChanged
    {
        // Constants.
        public static string MAC_REGEX = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
        public static string MAC_REPLACE = "$1:$2:$3:$4:$5:$6";

        private const string PREFIX_PWD = "pwd_";

        private const string CLI_SHOW_SYSTEM = "show system";
        private const string CLI_SHOW_NETWORK = "show network";

        private const int TIMEOUT_READ_DEVICE_INFO = 15;
        private const int TIMEOUT_DEFAULT = 5;

        public static readonly int INACTIVITY_TIME = 10000;

        // Variables.
        private int rssi = RssiUtils.WORST_RSSI;
        private int commandId = 0;

        private bool isActive = true;

        private readonly string BleMac;

        private DeviceSettings settings;

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
        /// Gets the RF MAC address of this device (empty if it is unknown).
        /// </summary>
        public string RfMac { get; private set; } = "";

        /// <summary>
        /// Gets the Bluetooth Low Energy MAC address of this device (empty 
        /// if it is running in iOS).
        /// </summary>
        public string BLEMac { get; private set; } = "";

        /// <summary>
        /// Gets the BLE name of this device.
        /// </summary>
        public string Name => String.IsNullOrEmpty(Device.Name) ? "<Unknown>" : Device.Name;

        /// <summary>
        /// Gets whether the device is connected or not.
        /// </summary>
        public bool IsConnected => Device.State == DeviceState.Connected;

        /// <summary>
        /// Gets and sets the RSSI of this device.
        /// </summary>
        public int Rssi
        {
            get { return rssi; }
            set
            {
                if (value < 0 && value != RssiUtils.WORST_RSSI)
                    rssi = value;

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
            get { return isActive; }
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
            set
            {
                XBeeDevice.SetBluetoothPassword(value);
            }
        }

        /// <summary>
        /// Indicates whether the device is connecting or not.
        /// </summary>
        public bool Connecting { get; set; } = false;


        /// <summary>
        /// The device settings.
        /// </summary>
        public DeviceSettings Settings
        {
            get { return settings; }
        }

        // Events.
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Class constructor. Instantiates a new <c>IX15Device</c> with the provided
        /// <c>IDevice</c>.
        /// </summary>
        /// <param name="device">Internal BLE device.</param>
        public IX15Device(IDevice device)
        {
            Device = device ?? throw new ArgumentNullException("The device cannot be null");
            Rssi = device.Rssi;
            BleMac = Regex.Replace(Device.Id.ToString().Split('-')[4], MAC_REGEX, MAC_REPLACE).ToUpper();
            RfMac = AppPreferences.GetRfMac(BleMac);
            if (Xamarin.Forms.Device.RuntimePlatform.Equals(Xamarin.Forms.Device.Android))
                BLEMac = BleMac;

            XBeeDevice = new XBeeBLEDevice(device, null);
            settings = new DeviceSettings(this);
        }

        /// <summary>
        /// Opens the connection with the IX15 device.
        /// </summary>
        public void Open()
        {
            XBeeDevice.Open();

            // Store the RF MAC address.
            if (string.IsNullOrEmpty(RfMac))
                AppPreferences.SetRfMac(BleMac, XBeeDevice.XBee64BitAddr.ToString());
        }

        /// <summary>
        /// Closes the connection with the IX15 device.
        /// </summary>
        public void Close()
        {
            XBeeDevice.Close();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            IX15Device dev = (IX15Device)obj;
            return Id == dev.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
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
            var tcs = new TaskCompletionSource<string>();

            new Task(async () =>
            {
                var password = await SecureStorage.GetAsync(PREFIX_PWD + BleMac);
                tcs.SetResult(password);
            }).Start();

            return tcs.Task;
        }

        /// <summary>
        /// Stores the password of the device.
        /// </summary>
        /// <param name="password">Password to store.</param>
        public async void SetStoredPassword(string password)
        {
            await SecureStorage.SetAsync(PREFIX_PWD + BleMac, password);
        }

        /// <summary>
        /// Clears the stored password of the device.
        /// </summary>
        public async void ClearStoredPassword()
        {
            var password = await GetStoredPassword();
            if (password != null)
                SecureStorage.Remove(PREFIX_PWD + BleMac);
        }

        /// <summary>
        /// Retrieves the IX15 device information.
        /// </summary>
        /// <returns>A <c>DeviceInformation</c> with the device information.</returns>
        /// <exception cref="CLIException">If there is any error reading the device information.</exception>
        public async Task<DeviceInformation> GetDeviceInfo()
        {
            string systemString = await ExecuteCLICommand(CLI_SHOW_SYSTEM, TIMEOUT_READ_DEVICE_INFO);
            string networkString = await ExecuteCLICommand(CLI_SHOW_NETWORK, TIMEOUT_READ_DEVICE_INFO);

            return DeviceInformation.Parse(systemString, networkString);
        }

        /// <summary>
        /// Executes a CLI command in the device.
        /// </summary>
        /// <param name="command">The CLI command to execute.</param>
        /// <returns>The command answer, <c>null</c> if error.</returns>
        /// <exception cref="CLIException">If there is any error executing the CLI command.</exception>
        public async Task<string> ExecuteCLICommand(String command)
        {
            return await ExecuteCLICommand(command, TIMEOUT_DEFAULT);
        }

        /// <summary>
        /// Executes a CLI command in the device.
        /// </summary>
        /// <param name="command">The CLI command to execute.</param>
        /// <param name="timeout">The CLI command timeout.</param>
        /// <returns>The command answer.</returns>
        /// <exception cref="CLIException">If there is any error executing the CLI command.</exception>
        public async Task<string> ExecuteCLICommand(String command, int timeout)
        {
            string commandAnswer = null;
            int receivedCount = 0;
            Dictionary<int, string> answerChunks = new Dictionary<int, string>();
            ManualResetEvent waitHandle = new ManualResetEvent(false);

            if (!XBeeDevice.IsOpen)
                throw new CLIException("Device is not open");

            void UserDataRelayReceived(object sender, UserDataRelayReceivedEventArgs e)
            {
                commandId += 1;
                UserDataRelayMessage message = e.UserDataRelayMessage;
                string messageString = Encoding.ASCII.GetString(message.Data, 0, message.Data.Length);
                Console.WriteLine("Received relay frame: " + messageString);
                try
                {
                    CLIRequest answer = JsonSerializer.Deserialize<CLIRequest>(messageString);
                    answerChunks.Add(answer.Index, answer.Data);
                    receivedCount += 1;
                    if (receivedCount == answer.Total)
                    {
                        // Sort the received answer chunks.
                        string encodedAnswer = "";
                        for (int i = 1; i <= answer.Total; i++)
                        {
                            encodedAnswer += answerChunks[i];
                        }
                        // Decode the answer.
                        byte[] data = Convert.FromBase64String(encodedAnswer);
                        commandAnswer = Encoding.ASCII.GetString(data);
                        waitHandle.Set();
                    }
                }
                catch (JsonException ex)
                {
                    throw new CLIException("Error parsing received JSON string: " + ex.Message);
                }
            }

            string request = CLIRequest.ComposeCLICommandRequest(commandId, command, timeout);
            XBeeDevice.UserDataRelayReceived += UserDataRelayReceived;
            await Task.Run(async () =>
            {
                try
                {
                    XBeeDevice.SendUserDataRelay(XBeeLocalInterface.SERIAL, Encoding.ASCII.GetBytes(request));
                    waitHandle.WaitOne(timeout * 1000);
                } catch (XBeeException ex2) {
                    XBeeDevice.UserDataRelayReceived -= UserDataRelayReceived;
                    throw new CLIException("Error sending CLI request: " + ex2.Message);
                }
            });

            XBeeDevice.UserDataRelayReceived -= UserDataRelayReceived;

            if (commandAnswer == null)
                throw new CLIException("Timeout waiting for CLI execution answer");

            return commandAnswer;
        }
    }
}
