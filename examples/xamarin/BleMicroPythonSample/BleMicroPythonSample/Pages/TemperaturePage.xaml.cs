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

namespace BleMicroPythonSample
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TemperaturePage : CustomContentPage
	{
		private TemperaturePageViewModel temperaturePageViewModel;

		public TemperaturePage(BleDevice device)
		{
			InitializeComponent();
			temperaturePageViewModel = new TemperaturePageViewModel(device);
			BindingContext = temperaturePageViewModel;

			// Initialize the refresh rate picker.
			List<string> rates = new List<string>
			{
				"1 second",
				"2 seconds",
				"5 seconds",
				"10 seconds",
				"30 seconds",
				"60 seconds"
			};
			ratePicker.ItemsSource = rates;
			ratePicker.SelectedIndex = 2;

			// Register the back button action.
			if (EnableBackButtonOverride)
			{
				CustomBackButtonAction = async () =>
				{
					// Ask the user if wants to close the connection.
					if (await DisplayAlert("Disconnect device", "Do you want to disconnect the XBee device?", "Yes", "No"))
						temperaturePageViewModel.DisconnectDevice();
				};
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			temperaturePageViewModel.RegisterEventHandler();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			temperaturePageViewModel.UnregisterEventHandler();
		}

		public async void StartButtonClicked(object sender, System.EventArgs e)
		{
			bool newState = !temperaturePageViewModel.IsRunning;

			// Change the text of the button.
			startButton.Text = newState ? "Stop" : "Start";
			// Enable or disable the rate picker.
			ratePicker.IsEnabled = !newState;
			// Start or stop the process.
			bool success = newState ? await temperaturePageViewModel.StartProcess(int.Parse(((string)ratePicker.SelectedItem).Split(" ")[0]))
				: await temperaturePageViewModel.StopProcess();
			// If the process has started or stopped successfully, change the opacity of the grid.
			if (success)
				dataGrid.Opacity = newState ? 1 : 0.2;
		}
	}
}