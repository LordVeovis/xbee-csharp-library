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

using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BleConfigurationSample
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConfigurationPage : CustomContentPage
	{
		private ConfigurationPageViewModel configurationPageViewModel;

		public ConfigurationPage(BleDevice device)
		{
			InitializeComponent();
			configurationPageViewModel = new ConfigurationPageViewModel(device);
			BindingContext = configurationPageViewModel;

			// Initialize the AP values picker.
			List<string> apValues = new List<string>
			{
				"0 Transparent Mode",
				"1 API Mode Without Escapes",
				"2 API Mode With Escapes",
				"3 N/A",
				"4 MicroPython REPL",
				"5 Bypass Mode"
			};
			apPicker.ItemsSource = apValues;

			// Initialize the D9 values picker.
			List<string> d9Values = new List<string>
			{
				"0 Disabled",
				"1 Awake/Asleep Indicator",
				"2 N/A",
				"3 Digital Input",
				"4 Digital Output, Low",
				"5 Digital Output, High"
			};
			d9Picker.ItemsSource = d9Values;

			// Register the back button action.
			if (EnableBackButtonOverride)
			{
				CustomBackButtonAction = async () =>
				{
					// Ask the user if wants to close the connection.
					if (await DisplayAlert("Disconnect device", "Do you want to disconnect the XBee device?", "Yes", "No"))
						configurationPageViewModel.DisconnectDevice();
				};
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
		}

		public void ReadButtonClicked(object sender, System.EventArgs e)
		{
			configurationPageViewModel.ReadSettings();
		}

		public void WriteButtonClicked(object sender, System.EventArgs e)
		{
			configurationPageViewModel.WriteSettings();
		}
	}
}