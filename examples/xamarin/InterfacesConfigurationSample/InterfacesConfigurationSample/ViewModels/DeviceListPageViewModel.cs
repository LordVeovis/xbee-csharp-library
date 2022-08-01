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

using InterfacesConfigurationSample.Models;
using InterfacesConfigurationSample.Pages;
using InterfacesConfigurationSample.Services;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;
using Plugin.Permissions;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XBeeLibrary.Core.Exceptions;

namespace InterfacesConfigurationSample.ViewModels
{
    public class DeviceListPageViewModel : ViewModelBase
    {
        // Constants.
        private const string ERROR_DISCONNECTED_TITLE = "Device disconnected";
        private const string ERROR_DISCONNECTED = "Connection lost with the device.";
        private const string ERROR_CONNECT_TITLE = "Connect error";
        private const string ERROR_CONNECT = "Could not connect to the device > {0}";
        private const string ERROR_ALREADY_CONNECTED = "The selected device is already connected";

        private const string QUESTION_CLEAR_PASSWORDS = "Do you really want to clear the stored passwords?";

        private const string TASK_CONNECT = "Connecting to '{0}'...";

        private const string TEXT_SCAN = "Scan";
        private const string TEXT_STOP = "Stop";

        private const string STATUS_SCANNING = "Scanning";
        private const string STATUS_STOPPED = "Scan stopped";

        private const int SCAN_INTERVAL = 10000;

        private const int OPEN_RETRIES = 3;
        private const int MIN_ANDROID_VERSION_FOR_LOCATION = 6;

        // Variables.
        private bool isBluetoothChecked = false;
        private bool activePage = true;
        private bool storePassword = false;

        private bool isEmpty;
        private bool isScanning;
        private bool isRefreshing;

        private string scanStatus;
        private string scanButtonText;

        private IAdapter adapter;
        private ObservableCollection<BleDevice> devices;
        private BleDevice selectedDevice;

        private string password;

        // Properties.
        /// <summary>
        /// Indicates whether the page is active or not.
        /// </summary>
        public bool ActivePage
        {
            get => activePage;
            set
            {
                activePage = value;
                RaisePropertyChangedEvent("ActivePage");
            }
        }

        /// <summary>
        /// Indicates whether the list of devices is empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get => isEmpty;
            set
            {
                isEmpty = value;
                RaisePropertyChangedEvent("IsEmpty");
            }
        }

        /// <summary>
        /// Indicates whether the scan process is running or not.
        /// </summary>
        public bool IsScanning
        {
            get => isScanning;
            set
            {
                isScanning = value;
                RaisePropertyChangedEvent("IsScanning");
            }
        }

        /// <summary>
        /// Indicates whether the list of devices is being refreshed or not.
        /// </summary>
        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                RaisePropertyChangedEvent("IsRefreshing");
            }
        }

        /// <summary>
        /// Scan status label.
        /// </summary>
        public string ScanStatus
        {
            get => scanStatus;
            set
            {
                scanStatus = value;
                RaisePropertyChangedEvent("ScanStatus");
            }
        }

        /// <summary>
        /// Scan status button text.
        /// </summary>
        public string ScanButtonText
        {
            get => scanButtonText;
            set
            {
                scanButtonText = value;
                RaisePropertyChangedEvent("ScanButtonText");
            }
        }

        /// <summary>
        /// List of devices found over BLE.
        /// </summary>
        public ObservableCollection<BleDevice> Devices
        {
            get => devices;
            set
            {
                devices = value;
                RaisePropertyChangedEvent("Devices");
            }
        }

        /// <summary>
        /// The selected device to establish a BLE connection with.
        /// </summary>
        public BleDevice SelectedDevice
        {
            get => selectedDevice;
            set
            {
                selectedDevice = value;
                RaisePropertyChangedEvent("SelectedDevice");
                ConnectToDevice();
            }
        }

        // Commands.
        /// <summary>
        /// Command used to toggle the scan process.
        /// </summary>
        public ICommand ToggleScan { get; private set; }

        /// <summary>
        /// Command used to refresh the list of discovered devices.
        /// </summary>
        public ICommand RefreshList { get; private set; }

        /// <summary>
        /// Command used to clear the stored passwords.
        /// </summary>
        public ICommand ClearPasswords { get; private set; }

        // Methods.
        /// <summary>
        /// Class constructor. Instantiates a new <c>DeviceListPageViewModel</c> object 
        /// with the provided parameters.
        /// </summary>
        public DeviceListPageViewModel() : base()
        {
            InitPage();

            // Define behavior of Scan button.
            ToggleScan = new Command(ToggleScanControl);
            RefreshList = new Command(RefreshListControl);
            ClearPasswords = new Command(ClearPasswordsControl);
        }

        private void InitPage()
        {
            devices = new ObservableCollection<BleDevice>();

            // Access to local BLE adapter.
            adapter = CrossBluetoothLE.Current.Adapter;

            // Subscribe to device advertisements.
            adapter.DeviceAdvertised += (sender, e) =>
            {
                BleDevice advertisedDevice = new BleDevice(e.Device);
                // If the device is not discovered, add it to the list. Otherwise update its RSSI value.
                if (!devices.Contains(advertisedDevice))
                {
                    AddDeviceToList(advertisedDevice);
                }
                else
                {
                    devices[devices.IndexOf(advertisedDevice)].Rssi = advertisedDevice.Rssi;
                }
            };

            // Subscribe to device connection lost.
            adapter.DeviceConnectionLost += (sender, e) =>
            {
                if (selectedDevice == null || CrossBluetoothLE.Current.State != BluetoothState.On || selectedDevice.Connecting)
                {
                    return;
                }

                // Close the connection with the device.
                selectedDevice.Close();

                // Return to main page and prompt the disconnection error.
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.Navigation.PopToRootAsync();
                    DisplayAlert(ERROR_DISCONNECTED_TITLE, ERROR_DISCONNECTED);
                });
            };
        }

        public void StartContinuousScan()
        {
            // Clear the list of BLE devices.
            ClearDevicesList();

            StartScanning();

            // Timer task to check the bluetooth state.
            Device.StartTimer(TimeSpan.FromSeconds(0), () =>
            {
                if (!isBluetoothChecked)
                {
                    CheckBluetooth();
                    isBluetoothChecked = true;
                }
                return false;
            });

            // Check whether location is enabled or not.
            // This is required for Android devices with higher version than 6 (Marshmallow).
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                CheckLocation();
            }

            // Timer task to re-start the scan process.
            Device.StartTimer(TimeSpan.FromMilliseconds(SCAN_INTERVAL), () =>
            {
                if (IsScanning)
                {
                    StartScanning();
                }

                return activePage;
            });

            // Timer task to check inactive devices.
            Device.StartTimer(TimeSpan.FromMilliseconds(BleDevice.INACTIVITY_TIME), () =>
            {
                if (IsScanning)
                {
                    foreach (BleDevice device in devices)
                    {
                        device.IsActive = DateTimeOffset.Now.ToUnixTimeMilliseconds() - device.LastUpdated < BleDevice.INACTIVITY_TIME;
                    }
                }
                return activePage;
            });
        }

        /// <summary>
        /// Starts the scan process to look for BLE devices.
        /// </summary>
        private async void StartScanning()
        {
            if (adapter.IsScanning)
            {
                Debug.WriteLine("adapter.StopScanningForDevicesAsync()");
                await adapter.StopScanningForDevicesAsync();
            }

            ScanStatus = STATUS_SCANNING;
            ScanButtonText = TEXT_STOP;
            IsScanning = true;

            Debug.WriteLine("adapter.StartScanningForDevicesAsync()");
            await adapter.StartScanningForDevicesAsync();
        }

        /// <summary>
        /// Stops the BLE scanning process.
        /// </summary>
        private void StopScanning()
        {
            new Task(async () =>
            {
                if (adapter.IsScanning)
                {
                    Debug.WriteLine("Still scanning, stopping the scan");
                    await adapter.StopScanningForDevicesAsync();
                }
            }).Start();

            ScanStatus = STATUS_STOPPED;
            ScanButtonText = TEXT_SCAN;
            IsScanning = false;
        }

        /// <summary>
        /// Shows a popup asking the user for the password and returns it.
        /// </summary>
        /// <param name="authFailed">Whether the first authentication attempt
        /// failed or not.</param>
        /// <returns>The password entered by the user or <c>null</c> if the
        /// dialog was cancelled.</returns>
        private Task<string> AskForPassword(bool authFailed)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                PasswordPage passwordPage = new PasswordPage(authFailed);
                await PopupNavigation.Instance.PushAsync(passwordPage);
                passwordPage.Disappearing += (object sender, EventArgs e) =>
                {
                    string pwd = passwordPage.Password;
                    // If password is null, the popup was cancelled.
                    if (pwd != null)
                    {
                        storePassword = passwordPage.StorePassword;
                    }

                    tcs.SetResult(pwd);
                };
            });

            return tcs.Task;
        }

        /// <summary>
        /// Connects to the selected BLE device.
        /// </summary>
        private async void ConnectToDevice()
        {
            if (selectedDevice == null)
            {
                return;
            }

            StopScanning();

            if (selectedDevice.IsConnected)
            {
                DisplayAlert(ERROR_CONNECT_TITLE, ERROR_ALREADY_CONNECTED);
            }
            else
            {
                selectedDevice.Connecting = true;
                await Task.Run(async () =>
                {
                    ShowLoadingDialog(string.Format(TASK_CONNECT, selectedDevice.Name));

                    try
                    {
                        // Open the connection with the device. Try to connect up to 3 times.
                        int retries = OPEN_RETRIES;
                        while (!selectedDevice.XBeeDevice.IsOpen && retries > 0)
                        {
                            retries--;
                            try
                            {
                                selectedDevice.Open();
                            }
                            catch (Exception e)
                            {
                                // If the number of retries is 0 or there was a problem with 
                                // the Bluetooth authentication, raise the exception.
                                if (e is BluetoothAuthenticationException || retries == 0)
                                {
                                    throw e;
                                }
                            }
                            await Task.Delay(1000);
                        }

                        // Store the device's password.
                        if (storePassword)
                        {
                            selectedDevice.SetStoredPassword(password);
                        }

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            // Load device information page.
                            await Application.Current.MainPage.Navigation.PushAsync(new DevicePage(selectedDevice));
                        });
                    }
                    catch (BluetoothAuthenticationException)
                    {
                        HideLoadingDialog();

                        selectedDevice.Close();

                        password = await AskForPassword(true);

                        // If the user cancelled the dialog, return.
                        if (password == null)
                        {
                            return;
                        }

                        selectedDevice.Password = password;

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ConnectToDevice();
                        });
                    }
                    catch (Exception ex)
                    {
                        DisplayAlert(ERROR_CONNECT_TITLE, string.Format(ERROR_CONNECT, ex.Message));

                        // Close the connection. It was established and a problem occurred when reading settings.
                        selectedDevice.Close();
                        SelectedDevice = null;
                    }
                    finally
                    {
                        HideLoadingDialog();
                        if (selectedDevice != null)
                        {
                            selectedDevice.Connecting = false;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Checks whether the bluetooth adapter is on or off. In case it is
        /// off, shows a dialog asking the user to turn it on.
        /// </summary>
        private async void CheckBluetooth()
        {
            BluetoothState state = await GetBluetoothState();
            if (state == BluetoothState.Off)
            {
                ShowBluetoothDisabledAlert();
            }

            // Listen for changes in the bluetooth adapter.
            CrossBluetoothLE.Current.StateChanged += (o, e) =>
            {
                if (e.NewState == BluetoothState.Off)
                {
                    // Close the connection with the device.
                    if (selectedDevice != null)
                    {
                        selectedDevice.Close();
                    }
                    // Ask the user to turn bluetooth on.
                    ShowBluetoothDisabledAlert();

                    // If this was the last page, clear the list. Otherwise go to this page.
                    if (Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault() is DeviceListPage)
                    {
                        ClearDevicesList();
                    }
                    else
                    {
                        Application.Current.MainPage.Navigation.PopToRootAsync();
                    }
                    // If there is a loading dialog, close it.
                    HideLoadingDialog();
                }
            };
        }

        /// <summary>
        /// Returns the current bluetooth state.
        /// </summary>
        /// <returns>BluetoothState</returns>
        private Task<BluetoothState> GetBluetoothState()
        {
            IBluetoothLE ble = CrossBluetoothLE.Current;
            TaskCompletionSource<BluetoothState> tcs = new TaskCompletionSource<BluetoothState>();

            // In some cases the bluetooth state is unknown the first time it is read.
            if (ble.State == BluetoothState.Unknown)
            {
                // Listen for changes in the bluetooth adapter.
                EventHandler<BluetoothStateChangedArgs> handler = null;
                handler = (o, e) =>
                {
                    CrossBluetoothLE.Current.StateChanged -= handler;
                    tcs.SetResult(e.NewState);
                };
                CrossBluetoothLE.Current.StateChanged += handler;
            }
            else
            {
                tcs.SetResult(ble.State);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Shows an alert asking the user to turn bluetooth on.
        /// </summary>
        private void ShowBluetoothDisabledAlert()
        {
            DisplayAlert(ERROR_BLUETOOTH_DISABLED_TITLE, ERROR_BLUETOOTH_DISABLED, BUTTON_CLOSE);
        }

        /// <summary>
        /// Checks whether the location is enabled or not. In case it is
        /// not, it shows a dialog asking the user to turn it on.
        /// </summary>
        private async void CheckLocation()
        {
            if (DeviceInfo.Version.Major < MIN_ANDROID_VERSION_FOR_LOCATION)
            {
                return;
            }

            if (DependencyService.Get<IGPSDependencyService>().IsGPSEnabled())
            {
                await RequestLocationPermission();
            }
            else
            {
                await Task.Run(async () =>
                {
                    // This await is needed. Otherwise some sort of collision appears
                    // with the automatic FTP firmware verification.
                    await Task.Delay(100);
                    ShowLocationDisabledAlert();
                });
            }
        }

        /// <summary>
        /// Shows an alert asking the user to turn location on.
        /// </summary>
        private void ShowLocationDisabledAlert()
        {
            DisplayAlert(ERROR_LOCATION_DISABLED_TITLE, ERROR_LOCATION_DISABLED, BUTTON_CLOSE);
        }

        /// <summary>
        /// Requests location permision.
        /// </summary>
        private async Task RequestLocationPermission()
        {
            try
            {
                PermissionStatus status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationPermission>();
                if (status != PermissionStatus.Granted)
                {
                    status = await CrossPermissions.Current.RequestPermissionAsync<LocationPermission>();
                }

                if (status != PermissionStatus.Granted)
                {
                    DisplayAlert(ERROR_LOCATION_PERMISSION_TITLE, ERROR_LOCATION_PERMISSION);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(ERROR_CHECK_PERMISSION_TITLE, string.Format(ERROR_CHECK_PERMISSION, ex.Message));
            }
        }

        /// <summary>
        /// Toggles the scan process.
        /// </summary>
        public void ToggleScanControl()
        {
            if (IsScanning)
            {
                StopScanning();
                IsRefreshing = false;
            }
            else
            {
                ClearDevicesList();
                StartScanning();
            }
        }

        /// <summary>
        /// Refreshes the list of the discovered devices.
        /// </summary>
        public void RefreshListControl()
        {
            // As the scan is done every 10 seconds while scanning, the refresh process
            // only requires for the list to be cleared, then, as devices will keep
            // coming it will create the illusion of the list being refreshed.
            IsRefreshing = true;
            ClearDevicesList();

            // If the scan is stopped, start it.
            if (!IsScanning)
            {
                StartScanning();
            }
        }

        /// <summary>
        /// Clears the stored passwords.
        /// </summary>
        private async void ClearPasswordsControl()
        {
            bool result = await DisplayQuestion(null, QUESTION_CLEAR_PASSWORDS);
            if (result)
            {
                SecureStorage.RemoveAll();
            }
        }

        /// <summary>
        /// Clears the list of discovered devices.
        /// </summary>
        public void ClearDevicesList()
        {
            devices.Clear();
            IsEmpty = true;
        }

        /// <summary>
        /// Adds a new device to the list of discovered devices.
        /// </summary>
        /// <param name="deviceToAdd"></param>
        private void AddDeviceToList(BleDevice deviceToAdd)
        {
            // Add device to list.
            devices.Add(deviceToAdd);
            // Update the refresh status.
            if (IsRefreshing)
            {
                IsRefreshing = false;
            }

            IsEmpty = devices.Count == 0;
        }

        /// <summary>
        /// Prepares the connection with the given device.
        /// </summary>
        /// <param name="device">BLE device to connect to.</param>
        public async void PrepareConnection(BleDevice device)
        {
            // Check if the device's password is stored.
            password = await device.GetStoredPassword();

            if (password == null)
            {
                password = await AskForPassword(false);

                // If the user cancelled the dialog, return.
                if (password == null)
                {
                    return;
                }
            }

            device.Password = password;

            ActivePage = false;

            // Save the selected device and connect to it.
            SelectedDevice = device;
        }
    }
}
