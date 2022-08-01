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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InterfacesConfigurationSample.Models
{
    public class Interface
    {
        // Constants.
        private const string ERROR_WRITE_SETTINGS = "Error writing settings:";
        private const string ERROR_READ_SETTINGS = "Error reading settings:";
        private const string ERROR_SETTING_FORMAT = " - {0}: {1}";

        // Variables.
        private readonly BleDevice bleDevice;

        // Properties.
        /// <summary>
        /// The name of the interface.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The list of settings.
        /// </summary>
        public List<AbstractSetting> Settings { get; }

        /// <summary>
        /// Returns whether all settings are valid or not.
        /// </summary>
        public bool AllSettingsAreValid => Settings.TrueForAll(v => v.IsValid);

        /// <summary>
        /// Returns whether any setting has changed or not.
        /// </summary>
        public bool AnySettingChanged
        {
            get
            {
                foreach (AbstractSetting setting in Settings)
                {
                    if (setting.HasChanged)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // Methods.
        /// Class constructor. Instantiates a new <c>Interface</c> with the
        /// given parameters.
        /// </summary>
        /// <param name="name">The interface name.</param>
        public Interface(string name, List<AbstractSetting> settings, BleDevice bleDevice)
        {
            this.bleDevice = bleDevice;
            Name = name;
            Settings = settings;
        }

        /// <summary>
        /// Reads all the settings from the interface.
        /// </summary>
        /// <exception cref="CommunicationException">If there is any error reading the settings.</exception>
        public async Task ReadSettings()
        {
            List<string> errorValues = new List<string>() { ERROR_READ_SETTINGS };

            foreach (AbstractSetting setting in Settings)
            {
                try
                {
                    await ReadSetting(setting);
                }
                catch (CommunicationException ex)
                {
                    errorValues.Add(string.Format(ERROR_SETTING_FORMAT, setting.Name, ex.Message));
                }
            }

            if (errorValues.Count > 1)
            {
                throw new CommunicationException(string.Join("\n", errorValues.ToArray()));
            }
        }

        /// <summary>
        /// Reads the given setting.
        /// </summary>
        /// <param name="setting">The setting to read.</param>
        /// <exception cref="CommunicationException">If there is any error reading the setting.</exception>
        private async Task ReadSetting(AbstractSetting setting)
        {
            string answer = await bleDevice.SendData(GenerateReadSettingJson(setting));
            ProcessAnswer(answer);
            setting.Value = JObject.Parse(answer).GetValue("value").ToString();
            setting.HasChanged = false;
        }

        /// <summary>
        /// Writes all the settings from the interface.
        /// </summary>
        /// <exception cref="CommunicationException">If there is any error writing the settings.</exception>
        public async Task WriteSettings()
        {
            List<string> errorValues = new List<string>() { ERROR_WRITE_SETTINGS };

            foreach (AbstractSetting setting in Settings)
            {
                try
                {
                    if (setting.HasChanged)
                    {
                        await WriteSetting(setting);
                    }
                }
                catch (CommunicationException ex)
                {
                    errorValues.Add(string.Format(ERROR_SETTING_FORMAT, setting.Name, ex.Message));
                }
            }

            if (errorValues.Count > 1)
            {
                throw new CommunicationException(string.Join("\n", errorValues.ToArray()));
            }
        }

        /// <summary>
        /// Writes the given setting.
        /// </summary>
        /// <param name="setting">The setting to write.</param>
        /// <exception cref="CommunicationException">If there is any error writing the setting.</exception>
        private async Task WriteSetting(AbstractSetting setting)
        {
            string answer = await bleDevice.SendData(GenerateWriteSettingJson(setting));
            ProcessAnswer(answer);
            setting.HasChanged = false;
        }

        /// <summary>
        /// Processes the given device answer.
        /// </summary>
        /// <param name="answer"></param>
        private void ProcessAnswer(string answer)
        {
            try
            {
                JObject jsonObject = JObject.Parse(answer);
                if (!jsonObject.ContainsKey("status"))
                {
                    throw new CommunicationException("Invalid response from device: " + answer);
                }
                if (!jsonObject.GetValue("status").ToString().Equals("ok"))
                {
                    throw new CommunicationException(jsonObject.GetValue("desc").ToString());
                }

            }
            catch (JsonReaderException)
            {
                throw new CommunicationException("Invalid response from device: " + answer);
            }
        }

        /// <summary>
        /// Generates the read setting JSON command for the given setting.
        /// </summary>
        /// <param name="setting">The setting to generate the read command for.</param>
        /// <returns>The read settings JSON command.</returns>
        private string GenerateReadSettingJson(AbstractSetting setting)
        {
            JObject jsonObject = new JObject
            {
                { "operation", "read" },
                { "interface", Name },
                { "setting", setting.Name }
            };
            return jsonObject.ToString();
        }

        /// <summary>
        /// Generates the write setting JSON command.
        /// </summary>
        /// <param name="setting">The setting to generate the write command for.</param>
        /// <returns>The write settings JSON command.</returns>
        private string GenerateWriteSettingJson(AbstractSetting setting)
        {
            JObject jsonObject = new JObject
            {
                { "operation", "write" },
                { "interface", Name },
                { "setting", setting.Name },
                { "value", setting.Value }
            };
            return jsonObject.ToString();
        }
    }
}
