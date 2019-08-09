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
using System.Threading.Tasks;
using Xamarin.Forms;
using XBeeLibrary.Core.Exceptions;
using XBeeLibrary.Core.Utils;

namespace BleConfigurationSample
{
	public class ConfigurationPageViewModel : ViewModelBase
	{
		// Variables.
		private string shValue;
		private string slValue;
		private string blValue;
		private string niValue;
		private string vrValue;
		private string hvValue;

		private int apValue;
		private int d9Value;

		// Properties.
		public BleDevice BleDevice { get; private set; }

		public string ShValue
		{
			get { return shValue; }
			set
			{
				shValue = value;
				RaisePropertyChangedEvent("ShValue");
			}
		}

		public string SlValue
		{
			get { return slValue; }
			set
			{
				slValue = value;
				RaisePropertyChangedEvent("SlValue");
			}
		}

		public string BlValue
		{
			get { return blValue; }
			set
			{
				blValue = value;
				RaisePropertyChangedEvent("BlValue");
			}
		}

		public string NiValue
		{
			get { return niValue; }
			set
			{
				niValue = value;
				RaisePropertyChangedEvent("NiValue");
			}
		}

		public int ApValue
		{
			get { return apValue; }
			set
			{
				apValue = value;
				RaisePropertyChangedEvent("ApValue");
			}
		}

		public int D9Value
		{
			get { return d9Value; }
			set
			{
				d9Value = value;
				RaisePropertyChangedEvent("D9Value");
			}
		}

		public string VrValue
		{
			get { return vrValue; }
			set
			{
				vrValue = value;
				RaisePropertyChangedEvent("VrValue");
			}
		}

		public string HvValue
		{
			get { return hvValue; }
			set
			{
				hvValue = value;
				RaisePropertyChangedEvent("HvValue");
			}
		}

		/// <summary>
		/// Class constructor. Instantiates a new <c>ConfigurationPageViewModel</c> object with the
		/// given Bluetooth device.
		/// </summary>
		/// <param name="device">Bluetooth device.</param>
		public ConfigurationPageViewModel(BleDevice device)
		{
			BleDevice = device;
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
		/// Reads all the settings and updates their values in the UI.
		/// </summary>
		public void ReadSettings()
		{
			StartOperation(true);
		}

		/// <summary>
		/// Writes all the settings with their new values.
		/// </summary>
		public void WriteSettings()
		{
			StartOperation(false);
		}

		/// <summary>
		/// Starts the read or write operation.
		/// </summary>
		/// <param name="read"><c>true</c> to start the read operation, <c>false</c> to start the
		/// write operation.</param>
		private void StartOperation(bool read)
		{
			// Show a progress dialog while performing the operation.
			ShowLoadingDialog(read ? "Reading settings..." : "Writing settings...");

			// The read/write process blocks the UI interface, so it must be done in a different thread.
			Task.Run(() =>
			{
				try
				{
					if (read)
					{
						// Read the values.
						ShValue = HexUtils.ByteArrayToHexString(BleDevice.XBeeDevice.GetParameter("SH"));
						SlValue = HexUtils.ByteArrayToHexString(BleDevice.XBeeDevice.GetParameter("SL"));
						BlValue = HexUtils.ByteArrayToHexString(BleDevice.XBeeDevice.GetParameter("BL"));
						NiValue = Encoding.Default.GetString(BleDevice.XBeeDevice.GetParameter("NI"));
						ApValue = int.Parse(HexUtils.ByteArrayToHexString(BleDevice.XBeeDevice.GetParameter("AP")));
						D9Value = int.Parse(HexUtils.ByteArrayToHexString(BleDevice.XBeeDevice.GetParameter("D9")));
						VrValue = HexUtils.ByteArrayToHexString(BleDevice.XBeeDevice.GetParameter("VR"));
						HvValue = HexUtils.ByteArrayToHexString(BleDevice.XBeeDevice.GetParameter("HV"));
					}
					else
					{
						// Write the values.
						BleDevice.XBeeDevice.SetParameter("NI", Encoding.Default.GetBytes(NiValue));
						BleDevice.XBeeDevice.SetParameter("AP", HexUtils.HexStringToByteArray(ApValue.ToString()));
						BleDevice.XBeeDevice.SetParameter("D9", HexUtils.HexStringToByteArray(D9Value.ToString()));
					}
				}
				catch (XBeeException e)
				{
					ShowErrorDialog("Error performing operation", e.Message);
				}
				// Close the dialog.
				HideLoadingDialog();
			});
		}
	}
}
