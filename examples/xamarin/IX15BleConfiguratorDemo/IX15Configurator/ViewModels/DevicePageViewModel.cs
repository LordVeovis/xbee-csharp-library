using IX15Configurator.Exceptions;
using IX15Configurator.Models;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace IX15Configurator.ViewModels
{
    public class DevicePageViewModel : DeviceViewModelBase
    {
        // Constants.
        private const string TASK_READ_DEVICE_INFO = "Reading device information...";
        private const string TASK_WRITING_SETTINGS = "Writing settings...";

        private const string TITLE_ERROR_READ_SETTINGS = "Error reading settings";
        private const string TITLE_ERROR_READ_DEVICE_INFO = "Error reading device information";
        private const string TITLE_ERROR_WRITE_SETTINGS = "Error saving settings";

        private const string DESCRIPTION_READ_DEVICE_INFO = "There was an error reading the device information:\n\n{0}\n\nAre you sure the device is an IX15 device?";

        // Variables.
        private DeviceInformation deviceInfo;

        private DeviceSettings deviceSettings;

        private bool isBusy = false;

        // Properties.
        /// <summary>
        /// Device information.
        /// </summary>
        public DeviceInformation DeviceInformation
        {
            get { return deviceInfo; }
            set
            {
                deviceInfo = value;
                RaisePropertyChangedEvent(nameof(DeviceInformation));
            }
        }

        /// <summary>
        /// Device settings.
        /// </summary>
        public DeviceSettings DeviceSettings
        {
            get { return deviceSettings; }
        }

        /// <summary>
        /// Returns whether the window is busy or not.
        /// </summary>
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                RaisePropertyChangedEvent(nameof(IsBusy));
                RaisePropertyChangedEvent(nameof(CanReadSettings));
                RaisePropertyChangedEvent(nameof(CanSaveSettings));
            }
        }

        /// <summary>
        /// Returns whether settings can be read or not.
        /// </summary>
        public bool CanReadSettings
        {
            get { return !isBusy; }
        }

        /// <summary>
        /// Returns whether settings can be saved or not.
        /// </summary>
        public bool CanSaveSettings
        {
            get { return !isBusy && deviceSettings.AllSettingsAreValid && deviceSettings.AnySettingChanged; }
        }

        // Commands.
        /// <summary>
        /// Command used to refresh (read) the value of all the settings 
        /// associated to the view model.
        /// </summary>
        public ICommand ReadAllCommand { get; private set; }

        /// <summary>
        /// Command used to write the value of all the settings (that changed their 
        /// value) associated to the view model.
        /// </summary>
        public ICommand WriteAllCommand { get; private set; }

        /// <summary>
        /// Class constructor. Instantiates a new <c>DevicePageViewModel</c> object 
        /// with the provided parameters.
        /// </summary>
        /// <param name="ix15Device"></param>
        public DevicePageViewModel(IX15Device ix15Device) : base(ix15Device)
        {
            deviceSettings = ix15Device.Settings;

            ReadAllCommand = new Command(RefreshSettings);
            WriteAllCommand = new Command(WriteSettings);

            // Subscribe to each setting to detect when the value changes.
            foreach (AbstractSetting setting in deviceSettings.Settings)
                setting.PropertyChanged += SettingsValidationChanged;

            InitPage();
        }

        /// <summary>
        /// Initializes the IX15 device information page. Displays the 
        /// device information and fills the configuration categories list.
        /// </summary>
        private void InitPage()
        {
            RefreshSettings();
        }

        /// <summary>
        /// Callback notified whenever a validatable setting has changed.
        /// </summary>
        /// <param name="sender">The validatable setting.</param>
        /// <param name="e">The changed event args.</param>
        private void SettingsValidationChanged(object sender, PropertyChangedEventArgs e)
        {
            // Just notify that the 'CanSaveSettings' value has changed so that dependant observables can update accordingly.
            RaisePropertyChangedEvent(nameof(CanSaveSettings));
        }

        /// <summary>
        /// Reads the value of the settings displayed in the page associated 
        /// to the view model.
        /// </summary>
        public async void RefreshSettings()
        {
            await Task.Run(async () =>
            {
                IsBusy = true;
                ReplaceLoadingDialog(TASK_READ_DEVICE_INFO);
                try
                {
                    DeviceInformation = await ix15Device.GetDeviceInfo();
                    try
                    {
                        await deviceSettings.ReadAll();
                    }
                    catch (CLIException ex2)
                    {
                        DisplayAlert(TITLE_ERROR_READ_DEVICE_INFO, String.Format(DESCRIPTION_READ_DEVICE_INFO, ex2.Message));
                        DisconnectDevice();
                    }
                }
                catch (CLIException ex1)
                {
                    DisplayAlert(TITLE_ERROR_READ_SETTINGS, ex1.Message);
                }
                finally
                {
                    HideLoadingDialog();
                    IsBusy = false;
                }
            });
        }

        /// <summary>
        /// Writes the value of the settings displayed in the page associated 
        /// to the view model.
        /// </summary>
        public async void WriteSettings()
        {
            await Task.Run(async () =>
            {
                IsBusy = true;
                ReplaceLoadingDialog(TASK_WRITING_SETTINGS);
                try
                {
                    await deviceSettings.WriteAll();
                }
                catch (CLIException e)
                {
                    DisplayAlert(TITLE_ERROR_WRITE_SETTINGS, string.Format(e.Message));
                }
                finally
                {
                    HideLoadingDialog();
                    IsBusy = false;
                }
            });
        }
    }
}
