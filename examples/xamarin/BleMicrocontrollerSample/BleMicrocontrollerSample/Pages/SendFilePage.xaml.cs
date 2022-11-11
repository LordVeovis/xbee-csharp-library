﻿/*
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

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BleMicrocontrollerSample
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SendFilePage : CustomContentPage
	{
		private SendFilePageViewModel sendFilePageViewModel;

		public SendFilePage(BleDevice device)
		{
			InitializeComponent();
			sendFilePageViewModel = new SendFilePageViewModel(device);
			BindingContext = sendFilePageViewModel;

			// Register the back button action.
			if (EnableBackButtonOverride)
			{
				CustomBackButtonAction = async () =>
				{
					// Ask the user if wants to close the connection.
					if (await DisplayAlert("Disconnect device", "Do you want to disconnect the XBee device?", "Yes", "No"))
						sendFilePageViewModel.DisconnectDevice();
				};
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			sendFilePageViewModel.RegisterEventHandler();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			sendFilePageViewModel.UnregisterEventHandler();
		}

		public void SendFileButtonClicked(object sender, System.EventArgs e)
		{
			sendFilePageViewModel.LoadFile();
		}
	}
}