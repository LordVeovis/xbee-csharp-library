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

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using XBeeLibrary.Core.Events.Relay;

namespace BleMicroPythonSample
{
	public class TemperaturePageViewModel : ViewModelBase
	{
		// Constants.
		private static readonly string SEPARATOR = "@@@";
		private static readonly string MSG_ACK = "OK";

		private static readonly int ACK_TIMEOUT = 5000;

		private static readonly Color COLOR_DEFAULT = Color.Default;
		private static readonly Color COLOR_GREEN = Color.FromHex("84C361");

		// Variables.
		private bool ackReceived = false;

		private readonly object ackLock = new object();

		private string tempValue;
		private string humValue;

		private Color valuesColor;

		// Properties.
		public BleDevice BleDevice { get; private set; }

		public bool IsRunning { get; set; }

		public string TemperatureValue
		{
			get { return tempValue; }
			set
			{
				tempValue = value;
				RaisePropertyChangedEvent("TemperatureValue");
			}
		}

		public string HumidityValue
		{
			get { return humValue; }
			set
			{
				humValue = value;
				RaisePropertyChangedEvent("HumidityValue");
			}
		}

		public Color ValuesColor
		{
			get { return valuesColor; }
			set
			{
				valuesColor = value;
				RaisePropertyChangedEvent("ValuesColor");
			}
		}

		/// <summary>
		/// Class constructor. Instantiates a new <c>TemperaturePageViewModel</c> object with the
		/// given Bluetooth device.
		/// </summary>
		/// <param name="device">Bluetooth device.</param>
		public TemperaturePageViewModel(BleDevice device)
		{
			BleDevice = device;
			IsRunning = false;

			TemperatureValue = "-";
			HumidityValue = "-";
			ValuesColor = COLOR_DEFAULT;
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
		/// Registers the event handler to be notified when new data from the MicroPython interface
		/// is received.
		/// </summary>
		public void RegisterEventHandler()
		{
			BleDevice.XBeeDevice.MicroPythonDataReceived += MicroPythonDataReceived;
		}

		/// <summary>
		/// Unregisters the event handler to be notified when new data from the MicroPython
		/// interface is received.
		/// </summary>
		public void UnregisterEventHandler()
		{
			BleDevice.XBeeDevice.MicroPythonDataReceived -= MicroPythonDataReceived;
		}

		/// <summary>
		/// MicroPython data received event handler.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args.</param>
		private void MicroPythonDataReceived(object sender, MicroPythonDataReceivedEventArgs e)
		{
			string data = Encoding.Default.GetString(e.Data);

			// If the response is "OK", notify the lock to continue the process.
			if (data.Equals(MSG_ACK))
			{
				ackReceived = true;
				lock (ackLock)
				{
					Monitor.Pulse(ackLock);
				}
			}
			else
			{
				// If the process is stopped, do nothing.
				if (!IsRunning)
					return;

				// Get the temperature and humidity from the received data.
				string[] dataArray = data.Split(SEPARATOR);
				if (dataArray.Length != 2)
					return;

				// Update the values of the temperature and humidity.
				TemperatureValue = dataArray[0];
				HumidityValue = dataArray[1];

				// Make the texts blink for a short time.
				ValuesColor = COLOR_GREEN;
				Task.Delay(200).Wait();
				ValuesColor = COLOR_DEFAULT;
			}
		}

		/// <summary>
		/// Starts the reading process of the temperature and humidity.
		/// </summary>
		/// <param name="rate">Refresh rate.</param>
		/// <returns></returns>
		public Task<bool> StartProcess(int rate)
		{
			return StartStopProcess(true, rate);
		}

		/// <summary>
		/// Stops the reading process of the temperature and humidity.
		/// </summary>
		/// <returns></returns>
		public Task<bool> StopProcess()
		{
			return StartStopProcess(false, -1);
		}

		/// <summary>
		/// Starts or stops the reading process of the temperature and humidity.
		/// </summary>
		/// <param name="start"><c>true</c> to start the process, <c>false</c> to stop it.</param>
		/// <param name="rate">Refresh rate.</param>
		/// <returns></returns>
		private Task<bool> StartStopProcess(bool start, int rate)
		{
			var tcs = new TaskCompletionSource<bool>();

			IsRunning = start;
			string data = start ? ("ON" + SEPARATOR + rate) : "OFF";
			Task.Run(() =>
			{
				// Send a message to the MicroPython interface with the action and refresh time.
				bool ackReceived = SendDataAndWaitResponse(Encoding.Default.GetBytes(data));
				tcs.SetResult(ackReceived);
			});

			return tcs.Task;
		}

		/// <summary>
		/// Sends the given data and waits for an ACK response during the configured timeout.
		/// </summary>
		/// <param name="data">Data to send.</param>
		/// <returns><c>true</c> if the ACK was received, <c>false</c> otherwise.</returns>
		/// <exception cref="XBeeException">If there is any problem sending the data.</exception>
		private bool SendDataAndWaitResponse(byte[] data)
		{
			ackReceived = false;
			// Send the data.
			BleDevice.XBeeDevice.SendMicroPythonData(data);
			// Wait until the ACK is received.
			lock (ackLock)
			{
				Monitor.Wait(ackLock, ACK_TIMEOUT);
			}
			// If the ACK was not received, show an error.
			if (!ackReceived)
			{
				ShowErrorDialog("Response not received", "Could not communicate with MicroPython. Please ensure you have the appropriate application running on it.");
				return false;
			}

			return true;
		}
	}
}
