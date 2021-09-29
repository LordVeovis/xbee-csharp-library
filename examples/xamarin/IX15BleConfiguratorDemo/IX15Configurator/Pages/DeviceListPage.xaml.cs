using IX15Configurator.Models;
using IX15Configurator.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IX15Configurator.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceListPage : CustomContentPage
    {
        // Constants.
        private const string ACTION_REMOVE_PASSWORD = "Remove password";

        // Variables.
        // TODO: Add back when Scan QR functionality feature goes into the app.
        // private ZXingScannerPage scanPage;

        private DeviceListPageViewModel deviceListPageViewModel;

        /// <summary>
        /// Class constructor. Instantiates a new <c>DeviceListPage</c> object.
        /// </summary>
        public DeviceListPage()
        {
            InitializeComponent();
            deviceListPageViewModel = new DeviceListPageViewModel();
            BindingContext = deviceListPageViewModel;

            // TODO: Add back when Scan QR functionality feature goes into the app.
            //btScanQR.Clicked += async delegate
            //{
            //	scanPage = new ZXingScannerPage();
            //	scanPage.OnScanResult += (result) =>
            //	{
            //		scanPage.IsScanning = false;
            //		Device.BeginInvokeOnMainThread(() =>
            //		{
            //			Navigation.PopAsync();
            //			ConnectQR(result.Text);
            //		});
            //	};

            //	StartScanning();
            //	await Navigation.PushAsync(scanPage);
            //};
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            deviceListPageViewModel.ActivePage = true;

            deviceListPageViewModel.StartContinuousScan();

            // TODO: Remove this part of code when
            // https://github.com/xamarin/Xamarin.Forms/issues/2118 is fixed.
            // This reads the list of ToolItems, clears the list and regenerates it,
            // so the first time this page is loaded, the Toolbar Items are shown.
            if (Device.RuntimePlatform == Device.Android)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    var items = ToolbarItems.ToList();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ToolbarItems.Clear();
                        foreach (var item in items)
                            ToolbarItems.Add(item);
                    });
                });
            }

            // Apply scan filtering preferences.
            (BindingContext as DeviceListPageViewModel).ApplyFilterOptions();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            deviceListPageViewModel.ActivePage = false;
        }

        /// <summary>
        /// Event executed when a BLE device is selected from the list.
        /// </summary>
        /// <param name="sender">The object that generated the event.</param>
        /// <param name="e">The event arguments.</param>
        public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            IX15Device selectedDevice = e.SelectedItem as IX15Device;

            // Clear selection.
            ((ListView)sender).SelectedItem = null;

            if (selectedDevice == null || !selectedDevice.IsActive)
                return;

            // Prepare the connection.
            deviceListPageViewModel.PrepareConnection(selectedDevice);
        }

        /// <summary>
        /// Event executed when the binding context of a device list item 
        /// changes.
        /// </summary>
        /// <param name="sender">The object that generated the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();

            if (BindingContext == null)
                return;

            var deviceViewCell = ((ViewCell)sender);
            var device = deviceViewCell.BindingContext as IX15Device;
            if (device == null)
                return;

            // Clear the context actions of the device ViewCell.
            deviceViewCell.ContextActions.Clear();

            // If the device has a password stored, create the context action to remove it.
            if (await device.GetStoredPassword() != null)
            {
                deviceViewCell.ContextActions.Add(new MenuItem()
                {
                    Text = ACTION_REMOVE_PASSWORD,
                    Command = new Command(() =>
                    {
                        // Remove the password of the device.
                        device.ClearStoredPassword();
                        // As the device doesn't have password now, clear the context actions of the ViewCell.
                        deviceViewCell.ContextActions.Clear();
                    })
                });
            }
        }
    }
}