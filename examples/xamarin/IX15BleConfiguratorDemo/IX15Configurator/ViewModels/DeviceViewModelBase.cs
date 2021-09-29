using IX15Configurator.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace IX15Configurator.ViewModels
{
    public class DeviceViewModelBase : ViewModelBase
    {
        // Properties.
        protected IX15Device ix15Device;

        // Commands.
        /// <summary>
        /// Command used to disconnect the device.
        /// </summary>
        public ICommand DisconnectCommand { get; private set; }

        /// <summary>
        /// Class constructor. Instantiates a new <c>ViewModelBase</c> object
        /// with the provided IX15 device.
        /// <param name="ix15Device">IX15 device used by this view model
        /// and others that inherit it.</param>
        /// </summary>
        public DeviceViewModelBase(IX15Device ix15Device) : base()
        {
            this.ix15Device = ix15Device;

            DisconnectCommand = new Command(DisconnectDevice);
        }

        /// <summary>
        /// Disconnects the BLE device.
        /// </summary>
        public async void DisconnectDevice()
        {
            if (ix15Device == null)
                return;

            await Task.Run(() =>
            {
                // Close the connection.
                ix15Device.Close();

                // Load the devices (root) page.
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.Navigation.PopToRootAsync();
                });
            });
        }
    }
}
