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

using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using XBeeLibrary.Core.Events;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Models;

namespace RelayConsoleSample
{
	public class RelayConsolePageViewModel : ViewModelBase
	{
		// Variables.
		private ObservableCollection<string> receivedMessages;

		// Properties.
		public BleDevice BleDevice { get; private set; }

		public ObservableCollection<string> ReceivedMessages
		{
			get { return receivedMessages; }
			set
			{
				receivedMessages = value;
				RaisePropertyChangedEvent("ReceivedMessages");
			}
		}

		// Commands.
		public ICommand ClearLogCommand { get; private set; }

		/// <summary>
		/// Class constructor. Instantiates a new <c>RelayConsolePageViewModel</c> object with the
		/// given Bluetooth device.
		/// </summary>
		/// <param name="device">Bluetooth device.</param>
		public RelayConsolePageViewModel(BleDevice device)
		{
			BleDevice = device;
			ReceivedMessages = new ObservableCollection<string>();

			ClearLogCommand = new Command(ClearLog);
		}

		/// <summary>
		/// Closes the connection with the device and goes to the previous page.
		/// </summary>
		public async void DisconnectDevice()
		{
			await Task.Run(() =>
			{
				// Close the connection.
				BleDevice.XBeeDevice.Close();

				// Go to the root page.
				Device.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.Navigation.PopToRootAsync();
				});
			});
		}

		/// <summary>
		/// Registers the event handler to be notified when new data from other XBee interface
		/// is received.
		/// </summary>
		public void RegisterEventHandler()
		{
			BleDevice.XBeeDevice.UserDataRelayReceived += UserDataRelayReceived;
		}

		/// <summary>
		/// Unregisters the event handler to be notified when new data from other XBee interface
		/// interface is received.
		/// </summary>
		public void UnregisterEventHandler()
		{
			BleDevice.XBeeDevice.UserDataRelayReceived -= UserDataRelayReceived;
		}

		/// <summary>
		/// Opens a popup page to send a User Data Relay message to other XBee interfaces.
		/// </summary>
		public async void SendRelayMessage()
		{
			SendRelayMessagePage page = new SendRelayMessagePage(BleDevice);
			await PopupNavigation.Instance.PushAsync(page);
			page.Disappearing += (object sender, EventArgs e) =>
			{
				if (page.DestinationInterface != XBeeLocalInterface.UNKNOWN)
				{
					Task.Run(() =>
					{
						try
						{
							// Send the User Data Relay message.
							BleDevice.XBeeDevice.SendUserDataRelay(page.DestinationInterface, Encoding.Default.GetBytes(page.Data));
							UserDialogs.Instance.Toast("Message sent successfully");
						}
						catch (XBeeException ex)
						{
							UserDialogs.Instance.Toast(string.Format("Error sending the message: {0}", ex.Message));
						}
					});
				}
			};
		}

		/// <summary>
		/// User Data Relay received event handler.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void UserDataRelayReceived(object sender, UserDataRelayReceivedEventArgs e)
		{
			UserDataRelayMessage message = e.UserDataRelayMessage;
			// Add the message to the list.
			ReceivedMessages.Add(string.Format("[{0}] {1}", message.SourceInterface.GetDescription(), Encoding.Default.GetString(message.Data)));
		}

		/// <summary>
		/// Clears the log of received messages.
		/// </summary>
		private void ClearLog()
		{
			ReceivedMessages.Clear();
		}
	}
}
