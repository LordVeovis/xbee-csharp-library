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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace InterfacesConfigurationSample.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        // Constants.
        public const string ERROR_BLUETOOTH_DISABLED_TITLE = "Bluetooth disabled";
        public const string ERROR_BLUETOOTH_DISABLED = "Bluetooth is required to work with the BLE devices, please turn it on.";
        public const string ERROR_LOCATION_DISABLED_TITLE = "Location disabled";
        public const string ERROR_LOCATION_DISABLED = "Location is required to work with BLE devices, please turn it on.";
        public const string ERROR_LOCATION_PERMISSION_TITLE = "Permission not granted";
        public const string ERROR_LOCATION_PERMISSION = "Location permission was not granted, application might not work as expected.";
        public const string ERROR_CHECK_PERMISSION_TITLE = "Error checking permissions";
        public const string ERROR_CHECK_PERMISSION = "There was an error checking the location permission: {0}";
        public const string BUTTON_OK = "OK";
        public const string BUTTON_NO = "No";
        public const string BUTTON_YES = "Yes";
        public const string BUTTON_CLOSE = "Close";

        // Variables.
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
        /// Returns the current page being displayed.
        /// </summary>
        /// <returns>The current <c>Page</c> being displayed.</returns>
        protected Page GetCurrentPage()
        {
            return Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
        }

        /// <summary>
        /// Displays an alert with the given title and message.
        /// </summary>
        /// <param name="alertTitle">Title of the alert to display.</param>
        /// <param name="alertMessage">Message of the alert to display.</param>
        protected void DisplayAlert(string alertTitle, string alertMessage)
        {
            DisplayAlert(alertTitle, alertMessage, BUTTON_OK);
        }

        /// <summary>
        /// Displays an alert with the given title, message and button text.
        /// </summary>
        /// <param name="alertTitle">Title of the alert to display.</param>
        /// <param name="alertMessage">Message of the alert to display.</param>
        /// <param name="buttonText">Text to display in the button of the alert dialog.</param>
        protected void DisplayAlert(string alertTitle, string alertMessage, string buttonText)
        {
            Page currentPage = GetCurrentPage();
            if (currentPage == null)
            {
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                currentPage.DisplayAlert(alertTitle, alertMessage, buttonText);
            });
        }

        /// <summary>
        /// Displays a question alert with the given title and message.
        /// </summary>
        /// <param name="questionTitle">Title of the question to display.</param>
        /// <param name="questionMessage">Message of the question to display.</param>
        protected Task<bool> DisplayQuestion(string questionTitle, string questionMessage)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            Page currentPage = GetCurrentPage();
            if (currentPage == null)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                bool result = await currentPage.DisplayAlert(questionTitle, questionMessage, BUTTON_YES, BUTTON_NO);
                tcs.SetResult(result);
            });

            return tcs.Task;
        }

        /// <summary>
        /// Displays a toast with the given configuration.
        /// </summary>
        /// <param name="toastConfig">Toast configuration.</param>
        protected void DisplayToast(ToastConfig toastConfig)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.Toast(toastConfig);
            });
        }

        /// <summary>
        /// Shows a loading dialog with the given text.
        /// </summary>
        /// <param name="text">Loading text.</param>
        public void ShowLoadingDialog(string text)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.ShowLoading(text);
            });
        }

        /// <summary>
        /// Hides the loading dialog (if any).
        /// </summary>
        public void HideLoadingDialog()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.HideLoading();
            });
        }

        /// <summary>
        /// Replaces the existing loading dialog by a new one with the given text.
        /// </summary>
        /// <param name="text">Loading text.</param>
        public void ReplaceLoadingDialog(string text)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.HideLoading();

                UserDialogs.Instance.ShowLoading(text);
            });
        }
    }
}
