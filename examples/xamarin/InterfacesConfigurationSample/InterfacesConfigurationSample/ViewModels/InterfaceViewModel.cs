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

using Acr.UserDialogs;
using InterfacesConfigurationSample.Models;
using InterfacesConfigurationSample.Pages;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace InterfacesConfigurationSample.ViewModels
{
    public class InterfaceViewModel : DeviceViewModelBase
    {
        // Constants.
        private const string TASK_GENERATING_DATA = "Generating settings page...";

        // Commands.
        /// <summary>
        /// Command that opens the interface in a new page.
        /// </summary>
        public ICommand OpenInterface { get; private set; }

        // Variables.
        private Interface iface;

        private string ifaceName;

        private SettingsPageViewModel viewModel;

        private SettingsPage settingsPage;

        // Properties.
        /// <summary>
        /// Interface associated to this view model.
        /// </summary>
        public Interface Interface
        {
            get => iface;
            set
            {
                iface = value;
                RaisePropertyChangedEvent("Interface");
            }
        }

        /// <summary>
        /// Name of the interface.
        /// </summary>
        public string InterfaceName
        {
            get => ifaceName;
            set
            {
                ifaceName = value;
                RaisePropertyChangedEvent("InterfaceName");
            }
        }

        /// <summary>
        /// Class constructor. Instantiates a new <c>InterfaceViewModel</c> 
        /// object with the provided parameters.
        /// </summary>
        /// <param name="bleDevice">BLE device used to configure the 
        /// settings contained in the interface associated to this view 
        /// model.</param>
        /// <param name="iface">The interface associated to this view 
        /// model.</param>
        public InterfaceViewModel(BleDevice bleDevice, Interface iface) : base(bleDevice)
        {
            InterfaceName = iface.Name;
            Interface = iface;

            OpenInterface = new Command(OpenInterfaceCommand);
        }

        /// <summary>
        /// Opens the interface associated to this view model in a new page.
        /// </summary>
        private async void OpenInterfaceCommand()
        {
            if (Interface == null)
            {
                return;
            }

            // Load the page using a loading dialog.
            using (UserDialogs.Instance.Loading(TASK_GENERATING_DATA))
            {
                await Task.Run(() =>
                {
                    // Instantiate the settings page view model.
                    if (viewModel == null)
                    {
                        viewModel = new SettingsPageViewModel(bleDevice, iface);
                    }
                    // Instantiate the settings page.
                    if (settingsPage == null)
                    {
                        settingsPage = new SettingsPage(bleDevice, iface, viewModel);
                    }
                });
                // Load the interface page.
                await Application.Current.MainPage.Navigation.PushAsync(settingsPage);
            }

            // Read/Initialize interface page settings.
            if (settingsPage != null)
            {
                settingsPage.InitSettings();
            }
        }
    }
}
