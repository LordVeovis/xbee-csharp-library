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

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XBeeLibrary.Core.Models;

namespace RelayConsoleSample
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SendRelayMessagePage : PopupPage
	{
		// Variables.
		private BleDevice bleDevice;

		// Properties.
		public XBeeLocalInterface DestinationInterface { get; private set; } = XBeeLocalInterface.UNKNOWN;
		public string Data { get; private set; } = null;

		public SendRelayMessagePage(BleDevice bleDevice)
		{
			this.bleDevice = bleDevice;

			InitializeComponent();

			// Initialize the interface picker.
			interfacePicker.ItemsSource = new List<XBeeLocalInterface>
			{
				XBeeLocalInterface.SERIAL,
				XBeeLocalInterface.MICROPYTHON,
				XBeeLocalInterface.BLUETOOTH
			};
			interfacePicker.SelectedIndex = 0;
		}

		public void OnSendButtonClicked(object sender, System.EventArgs e)
		{
			DestinationInterface = (XBeeLocalInterface) interfacePicker.SelectedItem;
			Data = dataEntry.Text;
			
			ClosePopup();
		}

		public void OnCancelButtonClicked(object sender, System.EventArgs e)
		{
			ClosePopup();
		}

		protected override bool OnBackgroundClicked()
		{
			return true;
		}

		/// <summary>
		/// Closes the popup page.
		/// </summary>
		private async void ClosePopup()
		{
			await PopupNavigation.Instance.PopAsync();
		}
	}
}