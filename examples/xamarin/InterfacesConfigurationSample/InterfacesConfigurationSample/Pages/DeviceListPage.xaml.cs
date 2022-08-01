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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InterfacesConfigurationSample.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceListPage : CustomContentPage
    {
        // Constants.
        private const string ACTION_REMOVE_PASSWORD = "Remove password";

        // Variables.
        private readonly DeviceListPageViewModel deviceListPageViewModel;

        /// <summary>
        /// Class constructor. Instantiates a new <c>DeviceListPage</c> object.
        /// </summary>
        public DeviceListPage()
        {
            InitializeComponent();
            deviceListPageViewModel = new DeviceListPageViewModel();
            BindingContext = deviceListPageViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            deviceListPageViewModel.ActivePage = true;

            deviceListPageViewModel.StartContinuousScan();

            // TODO: Remove this part of code when
            // https://github.com/xamarin/Xamarin.Forms/issues/2118 is fixed.
            // This reads the list of ToolItems, clears the list and regenerates it,
            // so the first time this page is loaded, the Toolbar Items are shown.
            if (Device.RuntimePlatform == Device.Android)
            {
                Task.Run(async () =>
                  {
                      await Task.Delay(500);
                      List<ToolbarItem> items = ToolbarItems.ToList();
                      Device.BeginInvokeOnMainThread(() =>
                      {
                          ToolbarItems.Clear();
                          foreach (ToolbarItem item in items)
                          {
                              ToolbarItems.Add(item);
                          }
                      });
                  });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            deviceListPageViewModel.ActivePage = false;
        }

        /// <summary>
        /// Event executed when a BLE device is selected from the list.
        /// </summary>
        /// <param name="sender">The object that generated the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            BleDevice selectedDevice = e.SelectedItem as BleDevice;

            // Clear selection.
            ((ListView)sender).SelectedItem = null;

            if (selectedDevice == null || !selectedDevice.IsActive)
            {
                return;
            }

            // Prepare the connection.
            deviceListPageViewModel.PrepareConnection(selectedDevice);
        }

        /// <summary>
        /// Event executed when the binding context of a device list item 
        /// changes.
        /// </summary>
        /// <param name="sender">The object that generated the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();

            if (BindingContext == null)
            {
                return;
            }

            ViewCell deviceViewCell = (ViewCell)sender;
            BleDevice device = deviceViewCell.BindingContext as BleDevice;
            if (device == null)
            {
                return;
            }

            // Clear the context actions of the device ViewCell.
            deviceViewCell.ContextActions.Clear();

            // If the device has a password stored, create the context action to remove it.
            if (await device.GetStoredPassword() != null)
            {
                deviceViewCell.ContextActions.Add(new MenuItem()
                {
                    Text = ACTION_REMOVE_PASSWORD,
                    Command = new Command(() =>
                    {
                        // Remove the password of the device.
                        device.ClearStoredPassword();
                        // As the device doesn't have password now, clear the context actions of the ViewCell.
                        deviceViewCell.ContextActions.Clear();
                    })
                });
            }
        }
    }
}