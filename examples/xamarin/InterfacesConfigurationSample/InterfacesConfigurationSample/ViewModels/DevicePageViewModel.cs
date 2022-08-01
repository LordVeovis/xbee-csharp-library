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

using System.Threading.Tasks;
using System.Collections.Generic;
using XBeeLibrary.Core.Utils;
using XBeeLibrary.Xamarin;
using InterfacesConfigurationSample.Models;

namespace InterfacesConfigurationSample.ViewModels
{
    public class DevicePageViewModel : DeviceViewModelBase
    {
        // Variables.
        private string name = "";
        private string macAddress = "";

        // Properties.
        /// <summary>
        /// Name of the BLE device associated to this view model.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChangedEvent("Name");
            }
        }

        /// <summary>
        /// MAC address of the BLE device associated to this view model.
        /// </summary>
        public string MacAddress
        {
            get => macAddress;
            set
            {
                macAddress = value;
                RaisePropertyChangedEvent("MacAddress");
            }
        }

        /// <summary>
        /// List of interfaces of the BLE device associated to this view model.
        /// </summary>
        public List<InterfaceViewModel> Interfaces { get; set; } = new List<InterfaceViewModel>();

        /// <summary>
        /// Class constructor. Instantiates a new <c>DevicePageViewModel</c> object 
        /// with the provided parameters.
        /// </summary>
        /// <param name="bleDevice"></param>
        public DevicePageViewModel(BleDevice bleDevice) : base(bleDevice)
        {
            InitPage();
        }

        /// <summary>
        /// Initializes the BLE device information page. Displays the 
        /// device information and fills the interfaces list.
        /// </summary>
        private void InitPage()
        {
            XBeeBLEDevice device = bleDevice.XBeeDevice;

            // Get basic information from the device.
            Name = device.NodeID;
            MacAddress = ParsingUtils.ByteArrayToHexString(device.XBee64BitAddr.Value);

            Task.Run(() =>
              {
                  // Fill interfaces view model list.
                  foreach (Interface iface in bleDevice.Interfaces)
                  {
                      Interfaces.Add(new InterfaceViewModel(bleDevice, iface));
                  }
              });
        }
    }
}
