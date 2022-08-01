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
using InterfacesConfigurationSample.ViewModels;
using Xamarin.Forms.Xaml;

namespace InterfacesConfigurationSample.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DevicePage : CustomContentPage
    {
        // Constants.
        private const string QUESTION_DISCONNECT = "Are you sure you want to disconnect the device?";

        // Variables.
        private readonly BleDevice bleDevice;

        /// <summary>
        /// Class constructor. Instantiates a new <c>DevicePage</c> 
        /// object with the provided parameters.
        /// </summary>
        /// <param name="bleDevice">The BLE device to represent in the page.</param>
        public DevicePage(BleDevice bleDevice)
        {
            this.bleDevice = bleDevice;

            InitializeComponent();
            BindingContext = new DevicePageViewModel(bleDevice);

            // Register the back button action.
            if (EnableBackButtonOverride)
            {
                CustomBackButtonAction = async () =>
                {
                    if (await DisplayAlert(null, QUESTION_DISCONNECT, ViewModelBase.BUTTON_YES, ViewModelBase.BUTTON_NO))
                    {
                        (BindingContext as DevicePageViewModel).DisconnectDevice();
                    }
                };
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext == null)
            {
                return;
            }
            (BindingContext as DevicePageViewModel).Name = bleDevice.XBeeDevice.NodeID;
        }
    }
}