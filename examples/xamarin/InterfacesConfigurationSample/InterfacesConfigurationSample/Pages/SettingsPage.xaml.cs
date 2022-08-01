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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InterfacesConfigurationSample.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : CustomContentPage
    {
        // Constants.
        private const string QUESTION_DISCONNECT = "Are you sure you want to disconnect the BLE device?";

        // Variables.
        private readonly SettingsPageViewModel viewModel;

        private readonly Interface iface;

        private readonly BleDevice bleDevice;

        /// <summary>
        /// Class constructor. Instantiates a new <c>DevicePage</c> 
        /// object with the provided parameters.
        /// </summary>
		/// <param name="bleDevice">The BLE device to synchronize settings with.</param>
		/// <param name="iface">The interface that contains the settings in the page.</param>
		/// <param name="viewModel">The settings page view model associated to this page.</param>
        public SettingsPage(BleDevice bleDevice, Interface iface, SettingsPageViewModel viewModel)
        {
            this.iface = iface;
            this.viewModel = viewModel;
            this.bleDevice = bleDevice;

            InitializeComponent();
            BindingContext = viewModel;

            // Register the back button action.
            if (EnableBackButtonOverride)
            {
                CustomBackButtonAction = () =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopAsync(true);
                    });
                };
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        /// <summary>
        /// Initializes (refresh) the settings of the page if they have not 
        /// been initialized before.
        /// </summary>
        public void InitSettings()
        {
            if (!viewModel.SettingsInitialized)
            {
                viewModel.RefreshSettings();
            }
        }
    }
}