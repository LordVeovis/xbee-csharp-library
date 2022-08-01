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
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace InterfacesConfigurationSample.ViewModels
{
    public class DeviceViewModelBase : ViewModelBase
    {
        // Properties.
        protected BleDevice bleDevice;

        // Commands.
        /// <summary>
        /// Command used to disconnect the device.
        /// </summary>
        public ICommand DisconnectCommand { get; private set; }

        /// <summary>
        /// Class constructor. Instantiates a new <c>DeviceViewModelBase</c> object
        /// with the provided BLE device.
        /// <param name="bleDevice">BLE device used by this view model
        /// and others that inherit it.</param>
        /// </summary>
        public DeviceViewModelBase(BleDevice bleDevice) : base()
        {
            this.bleDevice = bleDevice;

            DisconnectCommand = new Command(DisconnectDevice);
        }

        /// <summary>
        /// Disconnects the BLE device.
        /// </summary>
        public async void DisconnectDevice()
        {
            if (bleDevice == null)
            {
                return;
            }

            await Task.Run(() =>
            {
                // Close the connection.
                bleDevice.Close();

                // Load the devices (root) page.
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.Navigation.PopToRootAsync();
                });
            });
        }
    }
}
