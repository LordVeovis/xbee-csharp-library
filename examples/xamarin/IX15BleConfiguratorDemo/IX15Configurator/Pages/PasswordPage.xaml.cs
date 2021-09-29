using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;

namespace IX15Configurator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PasswordPage : PopupPage
    {
        // Properties.
        /// <summary>
        /// Returns the password entered by the user or <c>null</c> if the
        /// popup was cancelled.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Returns whether the password should be stored or not.
        /// </summary>
        public bool StorePassword
        {
            get
            {
                return rembemerSwitch.IsToggled;
            }
        }

        /// <summary>
        /// Class consturctor. Instantiates a new <c>PasswordPage</c> with
        /// the given argument.
        /// </summary>
        /// <param name="authFailed">Whether the first authentication attempt
        /// failed or not.</param>
        public PasswordPage(bool authFailed)
        {
            InitializeComponent();

            authFailedLabel.IsVisible = authFailed;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            passwordEntry.Focus();
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        /// <summary>
        /// Called when the OK buttons is pressed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOkPressed(object sender, EventArgs e)
        {
            Password = passwordEntry.Text;
            ClosePopup();
        }

        /// <summary>
        /// Called when the Cancel button is pressed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnCancelPressed(object sender, EventArgs e)
        {
            Password = null;
            ClosePopup();
        }

        /// <summary>
        /// Closes the popup.
        /// </summary>
        private async void ClosePopup()
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}