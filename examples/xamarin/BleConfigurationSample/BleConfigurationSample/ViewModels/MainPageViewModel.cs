/*
 * Copyright 2019, Digi International Inc.
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

using Acr.UserDialogs;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using XBeeLibrary.Core.Exceptions;

namespace BleConfigurationSample
{
	public class MainPageViewModel : ViewModelBase
	{
		// Variables.
		private IAdapter adapter;
		private ObservableCollection<BleDevice> devices;

		private bool isScanning = false;

		// Properties.
		public ObservableCollection<BleDevice> Devices
		{
			get { return devices; }
			set
			{
				devices = value;
				RaisePropertyChangedEvent("Devices");
			}
		}

		public BleDevice SelectedDevice { get; set; }

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
		/// Class constructor. Instantiates a new <c>MainPageViewModel</c> object.
		/// </summary>
		public MainPageViewModel()
		{
			devices = new ObservableCollection<BleDevice>();

			// Initialize Bluetooth stuff.
			adapter = CrossBluetoothLE.Current.Adapter;

			// Subscribe to device advertisements.
			adapter.DeviceAdvertised += (object sender, DeviceEventArgs e) =>
			{
				BleDevice advertisedDevice = new BleDevice(e.Device);
				// If the device is not discovered, add it to the list.
				if (!Devices.Contains(advertisedDevice))
					Devices.Add(advertisedDevice);
			};

			// Subscribe to device connection lost.
			adapter.DeviceConnectionLost += (object sender, DeviceErrorEventArgs e) =>
			{
				if (SelectedDevice == null || CrossBluetoothLE.Current.State != BluetoothState.On)
					return;

				// Close the connection with the device.
				SelectedDevice.XBeeDevice.Close();

				// Return to main page and prompt the disconnection error.
				Device.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.Navigation.PopToRootAsync();
					ShowErrorDialog("Device disconnected", "Bluetooth connection lost with the device.");
				});
			};
		}

		/// <summary>
		/// Starts the Bluetooth scan process.
		/// </summary>
		public void StartScan()
		{
			if (IsScanning)
				return;

			IsScanning = true;
			Devices.Clear();
			RestartScanning();

			// Timer task to re-start the scan process every 10 seconds.
			Device.StartTimer(TimeSpan.FromSeconds(10), () =>
			{
				RestartScanning();
				return IsScanning;
			});
		}

		/// <summary>
		/// Re-starts the Bluetooth scan process.
		/// </summary>
		private async void RestartScanning()
		{
			if (IsScanning)
			{
				await adapter.StopScanningForDevicesAsync();
				await adapter.StartScanningForDevicesAsync();
			}
		}

		/// <summary>
		/// Stops the Bluetooth scan process.
		/// </summary>
		public async void StopScan()
		{
			IsScanning = false;

			if (adapter.IsScanning)
				await adapter.StopScanningForDevicesAsync();
		}

		/// <summary>
		/// Asks the user for the Bluetooth password and tries to connect to the selected device.
		/// </summary>
		/// <param name="authFailed"><c>true</c> if the first authentication attempt failed,
		/// <c>false</c> otherwise.</param>
		public void AskForPassword(bool authFailed)
		{
			UserDialogs.Instance.Prompt(new PromptConfig
			{
				Title = "Enter Bluetooth password",
				// If the first authentication attempt failed, show an error message.
				Message = authFailed ? "Invalid password, please try again" : null,
				InputType = InputType.Password,
				OkText = "OK",
				CancelText = "Cancel",
				OnAction = args =>
				{
					if (args.Ok)
					{
						// Set the device password.
						SelectedDevice.XBeeDevice.SetBluetoothPassword(args.Value);
						// Connect to the selected device.
						ConnectToDevice();
					}
					else
					{
						// Restart the scan.
						StartScan();
					}
				}
			});
		}

		/// <summary>
		/// Connects to the selected device.
		/// </summary>
		private void ConnectToDevice()
		{
			// The connection process blocks the UI interface, so it must be done in a different thread.
			Task.Run(() =>
			{
				// Show a progress dialog while connecting to the device.
				ShowLoadingDialog("Connecting to device...");
				try
				{
					// Open the connection with the device.
					SelectedDevice.XBeeDevice.Open();

					// If the open method did not throw an exception, the connection is open.
					Device.BeginInvokeOnMainThread(() =>
					{
						// Load the Configuration page.
						Application.Current.MainPage.Navigation.PushAsync(new ConfigurationPage(SelectedDevice));
					});
				}
				catch (BluetoothAuthenticationException)
				{
					// There was a problem in the Bluetooth authentication process, so ask for the password again.
					HideLoadingDialog();
					AskForPassword(true);
				}
				catch (XBeeException e)
				{
					HideLoadingDialog();
					ShowErrorDialog("Error connecting to device", e.Message);
				}
				finally
				{
					HideLoadingDialog();
				}
			});
		}
	}
}
