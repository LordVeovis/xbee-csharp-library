using IX15Configurator.Models;
using IX15Configurator.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace IX15Configurator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DevicePage : CustomContentPage
    {
        // Constants.
        private const string QUESTION_DISCONNECT = "Are you sure you want to disconnect the IX15 device?";

        // Variables.
        private IX15Device ix15Device;

        /// <summary>
        /// Class constructor. Instantiates a new <c>DevicePage</c> 
        /// object with the provided parameters.
        /// </summary>
        /// <param name="ix15Device">The IX15 device to represent in the 
        /// page.</param>
        public DevicePage(IX15Device ix15Device)
        {
            this.ix15Device = ix15Device;

            InitializeComponent();
            BindingContext = new DevicePageViewModel(ix15Device);

            // Register the back button action.
            if (EnableBackButtonOverride)
            {
                CustomBackButtonAction = async () =>
                {
                    if (await DisplayAlert(null, QUESTION_DISCONNECT, ViewModelBase.BUTTON_YES, ViewModelBase.BUTTON_NO))
                        (BindingContext as DevicePageViewModel).DisconnectDevice();
                };
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}