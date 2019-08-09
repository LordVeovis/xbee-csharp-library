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
using System.ComponentModel;
using Xamarin.Forms;

namespace RelayConsoleSample
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises a property changed event for the given property name.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void RaisePropertyChangedEvent(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, e);
			}
		}

		/// <summary>
		/// Shows a loading dialog with the given text.
		/// </summary>
		/// <param name="text">Text to display.</param>
		protected void ShowLoadingDialog(string text)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				UserDialogs.Instance.ShowLoading(text);
			});
		}

		/// <summary>
		/// Hides the loading dialog.
		/// </summary>
		protected void HideLoadingDialog()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				UserDialogs.Instance.HideLoading();
			});
		}

		/// <summary>
		/// Shows an error dialog with the given title and message.
		/// </summary>
		/// <param name="title">Error title.</param>
		/// <param name="message">Error message.</param>
		protected void ShowErrorDialog(string title, string message)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				UserDialogs.Instance.Alert(message, title);
			});
		}
	}
}
