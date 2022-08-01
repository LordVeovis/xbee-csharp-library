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

using InterfacesConfigurationSample.Exceptions;
using InterfacesConfigurationSample.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace InterfacesConfigurationSample.ViewModels
{
    public class SettingsPageViewModel : DeviceViewModelBase
    {
        // Constants.
        private const string TASK_READ_SETTINGS = "Reading settings...";
        private const string TASK_WRITING_SETTINGS = "Writing settings...";

        private const string TITLE_ERROR_READ_SETTINGS = "Error reading settings";
        private const string TITLE_ERROR_WRITE_SETTINGS = "Error saving settings";

        // Variables.
        private readonly Interface iface;

        private ObservableCollection<AbstractSetting> settings = new ObservableCollection<AbstractSetting>();

        private bool isBusy = false;
        private bool areSettingsRefreshing = false;

        // Properties
        /// <summary>
        /// Name of the interface that contains the settings to configure.
        /// </summary>
        public string InterfaceName { get; private set; }

        /// <summary>
        /// Indicates whether the settings have been initialized (read for 
        /// first time) or not.
        /// </summary>
        public bool SettingsInitialized { get; private set; } = false;

        /// <summary>
        /// Indicates whether the list of settings is being refreshed or not.
        /// </summary>
        public bool AreSettingsRefreshing
        {
            get => areSettingsRefreshing;
            set
            {
                areSettingsRefreshing = value;
                RaisePropertyChangedEvent("AreSettingsRefreshing");
            }
        }

        /// <summary>
		/// List of setting contained in the interface.
		/// </summary>
		public ObservableCollection<AbstractSetting> Settings
        {
            get => settings;
            set
            {
                settings = value;
                RaisePropertyChangedEvent("Settings");
            }
        }

        /// <summary>
        /// Returns whether the window is busy or not.
        /// </summary>
        public bool IsBusy
        {
            get => isBusy;
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
        public bool CanReadSettings => !isBusy;

        /// <summary>
        /// Returns whether settings can be saved or not.
        /// </summary>
        public bool CanSaveSettings => !isBusy && iface.AllSettingsAreValid && iface.AnySettingChanged;

        // Commands.
        /// <summary>
        /// Command used to refresh (read) the value of all the settings 
        /// associated to the view model.
        /// </summary>
        public ICommand ReadAllCommand { get; private set; }

        /// <summary>
        /// Command used to write the value of all the settings associated to the view model.
        /// </summary>
        public ICommand WriteAllCommand { get; private set; }

        /// <summary>
        /// Class constructor. Instantiates a new <c>SettingsPageViewModel</c> object 
        /// with the provided parameters.
        /// </summary>
        /// <param name="bleDevice"></param>
        /// <param name="iface"></param>
        public SettingsPageViewModel(BleDevice bleDevice, Interface iface) : base(bleDevice)
        {
            Settings = settings;
            InterfaceName = iface.Name;

            this.iface = iface;

            ReadAllCommand = new Command(RefreshSettings);
            WriteAllCommand = new Command(WriteSettings);

            InitPage();
        }

        /// <summary>
        /// Initializes the IX15 device information page. Displays the 
        /// device information and fills the configuration categories list.
        /// </summary>
        private void InitPage()
        {
            // Add property changed callback to each setting.
            foreach (AbstractSetting setting in iface.Settings)
            {
                setting.PropertyChanged += SettingsValidationChanged;
                settings.Add(setting);
            }
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
                ReplaceLoadingDialog(TASK_READ_SETTINGS);

                // Hide pull-to-refresh spinner from the beginning (we have the loading dialog).
                AreSettingsRefreshing = false;

                try
                {
                    await iface.ReadSettings();
                    SettingsInitialized = true;
                }
                catch (CommunicationException ex1)
                {
                    DisplayAlert(TITLE_ERROR_READ_SETTINGS, ex1.Message);
                }
                finally
                {
                    HideLoadingDialog();
                    // Keep pull-to-refresh spinner hidden (override built-in functionality).
                    AreSettingsRefreshing = false;
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
                    await iface.WriteSettings();
                }
                catch (CommunicationException e)
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
