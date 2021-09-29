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
using IX15Configurator.Models;
using IX15Configurator.Utils;
using XBeeLibrary.Core.Exceptions;
using IX15Configurator.Pages;
using Plugin.Permissions;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;
using IX15Configurator.Services;

namespace IX15Configurator.ViewModels
{
    public class DeviceListPageViewModel : ViewModelBase
    {
        // Constants.
        private const string ERROR_DISCONNECTED_TITLE = "Device disconnected";
        private const string ERROR_DISCONNECTED = "Connection lost with the device.";
        private const string ERROR_DEVICE_NOT_FOUND_TITLE = "Device not found";
        private const string ERROR_DEVICE_NOT_FOUND = "Could not find any device with MAC '{0}'. Ensure that your device is up.";
        private const string ERROR_CONNECT_TITLE = "Connect error";
        private const string ERROR_CONNECT = "Could not connect to the device > {0}";
        private const string ERROR_ALREADY_CONNECTED = "The selected device is already connected";

        private const string QUESTION_CLEAR_PASSWORDS = "Do you really want to clear the stored passwords?";

        private const string TASK_CONNECT = "Connecting to '{0}'...";

        private const string TEXT_SCAN = "Scan";
        private const string TEXT_STOP = "Stop";

        private const string TEXT_INFO_FILTER = "The list will only show Bluetooth devices whose names contain the entered filter text.";

        private const string STATUS_SCANNING = "Scanning";
        private const string STATUS_STOPPED = "Scan stopped";

        public const string PREFIX_PWD = "pwd_";

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
        private ObservableCollection<IX15Device> devices;
        private ObservableCollection<IX15Device> allDevices;
        private IX15Device selectedDevice;

        private string password;

        // Properties.
        /// <summary>
        /// Indicates whether the page is active or not.
        /// </summary>
        public bool ActivePage
        {
            get { return activePage; }
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
            get { return isEmpty; }
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
            get { return isScanning; }
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
            get { return isRefreshing; }
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
            get { return scanStatus; }
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
            get { return scanButtonText; }
            set
            {
                scanButtonText = value;
                RaisePropertyChangedEvent("ScanButtonText");
            }
        }

        /// <summary>
        /// List of filtered devices found over BLE.
        /// </summary>
        public ObservableCollection<IX15Device> Devices
        {
            get { return devices; }
            set
            {
                devices = value;
                RaisePropertyChangedEvent("Devices");
            }
        }

        /// <summary>
        /// The selected device to establish a BLE connection with.
        /// </summary>
        public IX15Device SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                selectedDevice = value;
                RaisePropertyChangedEvent("SelectedDevice");
                ConnectToDevice();
            }
        }

        /// <summary>
        /// The text to filter the device list.
        /// </summary>
        public string FilterText
        {
            get { return AppPreferences.GetSearchFilterText(); }
            set
            {
                AppPreferences.SetSearchFilterText(value);
                ApplyFilterOptions();
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

        /// <summary>
        /// Command used to show the filter info.
        /// </summary>
        public ICommand ShowFilterInfoCommand { get; private set; }

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
            ShowFilterInfoCommand = new Command(ShowFilterInfo);
        }

        private void InitPage()
        {
            devices = new ObservableCollection<IX15Device>();
            allDevices = new ObservableCollection<IX15Device>();

            // Access to local BLE adapter.
            adapter = CrossBluetoothLE.Current.Adapter;

            // Subscribe to device advertisements.
            adapter.DeviceAdvertised += (sender, e) =>
            {
                IX15Device advertisedDevice = new IX15Device(e.Device);
                // If the device is not discovered, add it to the list. Otherwise update its RSSI value.
                if (!allDevices.Contains(advertisedDevice))
                    AddDeviceToList(advertisedDevice);
                else
                    allDevices[allDevices.IndexOf(advertisedDevice)].Rssi = advertisedDevice.Rssi;
            };

            // Subscribe to device connection lost.
            adapter.DeviceConnectionLost += (sender, e) =>
            {
                if (selectedDevice == null || CrossBluetoothLE.Current.State != BluetoothState.On || selectedDevice.Connecting)
                    return;

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
                CheckLocation();

            // Timer task to re-start the scan process.
            Device.StartTimer(TimeSpan.FromMilliseconds(SCAN_INTERVAL), () =>
            {
                if (IsScanning)
                    StartScanning();

                return activePage;
            });

            // Timer task to check inactive devices.
            Device.StartTimer(TimeSpan.FromMilliseconds(IX15Device.INACTIVITY_TIME), () =>
            {
                if (IsScanning)
                {
                    foreach (IX15Device device in devices)
                        device.IsActive = DateTimeOffset.Now.ToUnixTimeMilliseconds() - device.LastUpdated < IX15Device.INACTIVITY_TIME;
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
        /// Connects to the BLE device with the provided MAC address.
        /// </summary>
        /// <param name="qrAddress">The MAC address of the BLE device to 
        /// connect to.</param>
        private void ConnectQR(string qrAddress)
        {
            IX15Device foundDevice = null;
            foreach (IX15Device bleDevice in devices)
            {
                if (bleDevice.BLEMac == qrAddress)
                {
                    foundDevice = bleDevice;
                    break;
                }
            }

            if (foundDevice != null)
            {
                selectedDevice = foundDevice;
                ConnectToDevice();
            }
            else
            {
                DisplayAlert(ERROR_DEVICE_NOT_FOUND_TITLE, string.Format(ERROR_DEVICE_NOT_FOUND, qrAddress));
            }
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
            var tcs = new TaskCompletionSource<string>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                PasswordPage passwordPage = new PasswordPage(authFailed);
                await PopupNavigation.Instance.PushAsync(passwordPage);
                passwordPage.Disappearing += (object sender, EventArgs e) =>
                {
                    string pwd = passwordPage.Password;
                    // If password is null, the popup was cancelled.
                    if (pwd != null)
                        storePassword = passwordPage.StorePassword;
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
                return;

            StopScanning();

            if (selectedDevice.IsConnected)
                DisplayAlert(ERROR_CONNECT_TITLE, ERROR_ALREADY_CONNECTED);
            else
            {
                selectedDevice.Connecting = true;
                await Task.Run(async () =>
                {
                    ShowLoadingDialog(string.Format(TASK_CONNECT, selectedDevice.Name));

                    try
                    {
                        // Open the connection with the device. Try to connect up to 3 times.
                        var retries = OPEN_RETRIES;
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
                                    throw e;
                            }
                            await Task.Delay(1000);
                        }

                        // Store the device's password.
                        if (storePassword)
                            selectedDevice.SetStoredPassword(password);

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
                            return;

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
                            selectedDevice.Connecting = false;
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
            var state = await GetBluetoothState();
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
                        selectedDevice.Close();
                    // Ask the user to turn bluetooth on.
                    ShowBluetoothDisabledAlert();

                    // If this was the last page, clear the list. Otherwise go to this page.
                    if (Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault() is DeviceListPage)
                        ClearDevicesList();
                    else
                        Application.Current.MainPage.Navigation.PopToRootAsync();
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
            var tcs = new TaskCompletionSource<BluetoothState>();

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
                return;

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
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationPermission>();
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
                StartScanning();
        }

        /// <summary>
        /// Clears the stored passwords.
        /// </summary>
        private async void ClearPasswordsControl()
        {
            bool result = await DisplayQuestion(null, QUESTION_CLEAR_PASSWORDS);
            if (result)
                SecureStorage.RemoveAll();
        }

        /// <summary>
        /// Clears the list of discovered devices.
        /// </summary>
        public void ClearDevicesList()
        {
            allDevices.Clear();
            devices.Clear();
            IsEmpty = true;
        }

        /// <summary>
        /// Adds a new device to the list of discovered devices.
        /// </summary>
        /// <param name="deviceToAdd"></param>
        private void AddDeviceToList(IX15Device deviceToAdd)
        {
            allDevices.Add(deviceToAdd);

            // Check if device can be added to the list of filtered devices.
            if (deviceToAdd.Name.ToLower().Contains(AppPreferences.GetSearchFilterText().ToLower()))
            {
                // Add device to list.
                devices.Add(deviceToAdd);
                // Update the refresh status.
                if (IsRefreshing)
                    IsRefreshing = false;
            }

            IsEmpty = devices.Count == 0;
        }

        /// <summary>
        /// Applies the filtering options to the found devices list.
        /// </summary>
        public void ApplyFilterOptions()
        {
            devices.Clear();

            foreach (var Device in allDevices)
            {
                if (Device.Name.ToLower().Contains(AppPreferences.GetSearchFilterText().ToLower()))
                {
                    // Add device to list.
                    devices.Add(Device);
                    // Update the refresh status.
                    if (IsRefreshing)
                        IsRefreshing = false;
                }
            }

            IsEmpty = devices.Count == 0;
        }

        /// <summary>
        /// Prepares the connection with the given device.
        /// </summary>
        /// <param name="device">BLE device to connect to.</param>
        public async void PrepareConnection(IX15Device device)
        {
            // Check if the device's password is stored.
            password = await device.GetStoredPassword();

            if (password == null)
            {
                password = await AskForPassword(false);

                // If the user cancelled the dialog, return.
                if (password == null)
                    return;
            }

            device.Password = password;

            ActivePage = false;

            // Save the selected device and connect to it.
            SelectedDevice = device;
        }

        /// <summary>
        /// Shows the search filter information.
        /// </summary>
        private void ShowFilterInfo()
        {
            DisplayAlert(null, TEXT_INFO_FILTER);
        }
    }
}
