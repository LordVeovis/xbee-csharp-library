using Acr.UserDialogs;
using IX15Configurator.Pages;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace IX15Configurator.ViewModels
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
        /// Command used to open the About page.
        /// </summary>
        public ICommand AboutCommand { get; private set; }

        /// <summary>
        /// Opens the URL specified in the parameter.
        /// </summary>
        public ICommand OpenURLCommand { get; private set; }

        /// <summary>
        /// Class constructor. Instantiates a new <c>ViewModelBase</c> object.
        /// </summary>
        public ViewModelBase()
        {
            AboutCommand = new Command(OpenAboutPage);
            OpenURLCommand = new Command<string>(OpenURL);
        }

        /// <summary>
        /// Generates and raises a new event indicating that the provided 
        /// property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that has 
        /// changed.</param>
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
        /// Displays an alert with the given title and message.
        /// </summary>
        /// <param name="alertTitle">Title of the alert to display.</param>
        /// <param name="alertMessage">Message of the alert to display.</param>
        /// <param name="buttonText">Text to display in the button of the alert dialog.</param>
        protected void DisplayAlert(string alertTitle, string alertMessage, string buttonText)
        {
            Page currentPage = GetCurrentPage();
            if (currentPage == null)
                return;

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
            var tcs = new TaskCompletionSource<bool>();

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
        /// Opens the 'About' page.
        /// </summary>
        public async void OpenAboutPage()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AboutPage());
        }

        /// <summary>
        /// Opens the specified HTML page in the default WEB browser of the 
        /// phone.
        /// </summary>
        /// <param name="urlPage">The HTML page to open.</param>
        public async void OpenURL(string urlPage)
        {
            if (await Xamarin.Essentials.Launcher.CanOpenAsync(new Uri(urlPage)))
                await Xamarin.Essentials.Launcher.OpenAsync(new Uri(urlPage));
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
