using IX15Configurator.Exceptions;
using IX15Configurator.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IX15Configurator.Models
{
    public class DeviceSettings
    {
        // Constants
        private const string COMMAND_GET_SETTING = "config {0}";
        private const string COMMAND_SET_SETTING = "config {0} {1}";

        private const string SETTING_ENABLE_ADMIN_SHELL = "Enable Admin shell";
        private const string SETTING_SESSION_IDLE_TIME = "Session IDLE time";
        private const string SETTING_IP_MODE = "IP mode";
        private const string SETTING_DEFAULT_IP = "Default IP address";
        private const string SETTING_IP = "IP address";
        private const string SETTING_ENABLE_DHCP_SERVER = "Enable DHCP server";
        private const string SETTING_ENABLE_MODEM = "Enable modem";

        private const string SETTING_CMD_ENABLE_ADMIN_SHELL = "auth group admin acl shell enable";  // true/false
        private const string SETTING_CMD_SESSION_IDLE_TIME = "auth idle_timeout";  // 10m
        private const string SETTING_CMD_IP_MODE = "network interface eth ipv4 type";  // dhcp/static
        private const string SETTING_CMD_DEFAULT_IP = "network interface defaultip address";  // 192.168.2.1/24
        private const string SETTING_CMD_IP = "network interface eth ipv4 address";  // 192.168.2.100/24
        private const string SETTING_CMD_ENABLE_DHCP_SERVER = "network interface eth ipv4 dhcp_server enable";  // true/false
        private const string SETTING_CMD_ENABLE_MODEM = "network modem modem enable";  // true/false

        public const string VALUE_IP_MODE_STATIC = "static";
        public const string VALUE_IP_MODE_DHCP = "dhcp";

        private const string VALUE_DISPLAY_IP_MODE_STATIC = "Static";
        private const string VALUE_DISPLAY_IP_MODE_DHCP = "DHCP";

        private const string VALUE_DEFAULT_ENABLE_ADMIN_SHELL = "false";
        private const string VALUE_DEFAULT_SESSION_IDLE_TIME = "10m";
        private const string VALUE_DEFAULT_IP_MODE = VALUE_IP_MODE_STATIC;
        private const string VALUE_DEFAULT_DEFAULT_IP = "192.168.210.1/24";
        private const string VALUE_DEFAULT_IP = "192.168.2.1/24";
        private const string VALUE_DEFAULT_ENABLE_DHCP_SERVER = "true";
        private const string VALUE_DEFAULT_ENABLE_MODEM = "true";

        private const string ERROR_WRITE_SETTINGS = "Error saving settings:";
        private const string ERROR_READ_SETTINGS = "Error reading settings:";
        private const string ERROR_SETTING_FORMAT = " - {0}: {1}";

        private const int TIMEOUT_WRITE_SETTING = 7;
        private const int TIMEOUT_READ_SETTING = 7;

        public static Dictionary<string, string> IP_MODE_VALUES = new Dictionary<string, string>()
        {
            { VALUE_IP_MODE_STATIC, VALUE_DISPLAY_IP_MODE_STATIC},
            { VALUE_IP_MODE_DHCP,  VALUE_DISPLAY_IP_MODE_DHCP}
        };

        // Variables
        private AbstractSetting enableAdminShellSetting;
        private AbstractSetting idleSessionTimeSetting;
        private AbstractSetting ipModeSetting;
        private AbstractSetting ipSetting;
        private AbstractSetting defaultIPSetting;
        private AbstractSetting enableDHCPServerSetting;
        private AbstractSetting enableModemSetting;

        private List<AbstractSetting> settings;

        private IX15Device device;

        // Properties
        /// <summary>
        /// Gets the associated device.
        /// </summary>
        public IX15Device Device
        {
            get { return device; }
        }

        /// <summary>
        /// Gets and sets the value of the 'enable admin shell' setting.
        /// </summary>
        public AbstractSetting AdminShellEnabled
        {
            get { return enableAdminShellSetting; }
        }

        /// <summary>
        /// Gets and sets the value of the 'session idle time' setting.
        /// </summary>
        public AbstractSetting SessionIdleTime
        {
            get { return idleSessionTimeSetting; }
        }

        /// <summary>
        /// Gets and sets the value of the 'IP mode' setting.
        /// </summary>
        public AbstractSetting IPMode
        {
            get { return ipModeSetting; }
        }

        /// <summary>
        /// Gets and sets the value of the 'IP' setting.
        /// </summary>
        public AbstractSetting IP
        {
            get { return ipSetting; }
        }

        /// <summary>
        /// Gets and sets the value of the 'Default IP' setting.
        /// </summary>
        public AbstractSetting DefaultIP
        {
            get { return defaultIPSetting; }
        }

        /// <summary>
        /// Gets and sets the value of the 'enable DHCP server' setting.
        /// </summary>
        public AbstractSetting DHCPServerEnabled
        {
            get { return enableDHCPServerSetting; }
        }

        /// <summary>
        /// Gets and sets the value of the 'enable modem' setting.
        /// </summary>
        public AbstractSetting ModemEnabled
        {
            get { return enableModemSetting; }
        }

        /// <summary>
        /// The list of settings.
        /// </summary>
        public List<AbstractSetting> Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Returns whether all settings are valid or not.
        /// </summary>
        public bool AllSettingsAreValid
        {
            get
            {
                return Settings.TrueForAll(v => v.IsValid);
            }
        }

        /// <summary>
        /// Returns whether all settings are valid or not.
        /// </summary>
        public bool AnySettingChanged
        {
            get
            {
                foreach (AbstractSetting setting in settings)
                {
                    if (setting.HasChanged)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Class constructor. Instantiates a new <c>DeviceSettings</c> with the provided
        /// <c>IDevice</c>.
        /// </summary>
        /// <param name="device">IX15 that owns the settings.</param>
        public DeviceSettings(IX15Device device)
        {
            this.device = device;

            enableAdminShellSetting = new BooleanSetting(SETTING_ENABLE_ADMIN_SHELL, SETTING_CMD_ENABLE_ADMIN_SHELL, VALUE_DEFAULT_ENABLE_ADMIN_SHELL);
            idleSessionTimeSetting = new TextSetting(SETTING_SESSION_IDLE_TIME, SETTING_CMD_SESSION_IDLE_TIME, VALUE_DEFAULT_SESSION_IDLE_TIME, new TimeDurationValidator());
            ipModeSetting = new ComboSetting(SETTING_IP_MODE, SETTING_CMD_IP_MODE, VALUE_DEFAULT_IP_MODE, IP_MODE_VALUES);
            ipSetting = new TextSetting(SETTING_IP, SETTING_CMD_IP, VALUE_DEFAULT_IP, new IPValidator());
            defaultIPSetting = new TextSetting(SETTING_DEFAULT_IP, SETTING_CMD_DEFAULT_IP, VALUE_DEFAULT_DEFAULT_IP, new IPValidator());
            enableDHCPServerSetting = new BooleanSetting(SETTING_ENABLE_DHCP_SERVER, SETTING_CMD_ENABLE_DHCP_SERVER, VALUE_DEFAULT_ENABLE_DHCP_SERVER);
            enableModemSetting = new BooleanSetting(SETTING_ENABLE_MODEM, SETTING_CMD_ENABLE_MODEM, VALUE_DEFAULT_ENABLE_MODEM);

            settings = new List<AbstractSetting>()
            {
                enableAdminShellSetting,
                idleSessionTimeSetting,
                ipModeSetting,
                ipSetting,
                defaultIPSetting,
                enableDHCPServerSetting,
                enableModemSetting
            };
        }

        /// <summary>
        /// Reads all the settings.
        /// </summary>
        /// <exception cref="CLIException">If there is any error reading any setting.</exception>
        public async Task ReadAll()
        {
            List<string> errorValues = new List<string>() { ERROR_READ_SETTINGS };

            foreach (AbstractSetting setting in settings)
            {
                try
                {
                    setting.Value = (await device.ExecuteCLICommand(String.Format(COMMAND_GET_SETTING, setting.Command), TIMEOUT_READ_SETTING)).Trim();
                    setting.HasChanged = false;
                }
                catch (CLIException ex)
                {
                    errorValues.Add(String.Format(ERROR_SETTING_FORMAT, setting.Name, ex.Message));
                }
            }

            if (errorValues.Count > 1)
                throw new CLIException(String.Join("\n", errorValues.ToArray()));
        }

        /// <summary>
        /// Writes all the settings.
        /// </summary>
        /// <exception cref="CLIException">If there is any error writing any setting.</exception>
        public async Task WriteAll()
        {
            List<string> errorValues = new List<string>() { ERROR_WRITE_SETTINGS };

            foreach (AbstractSetting setting in settings)
            {
                try
                {
                    if (setting.HasChanged)
                    {
                        await WriteSetting(setting);
                        setting.HasChanged = false;
                    }
                }
                catch (CLIException ex)
                {
                    errorValues.Add(String.Format(ERROR_SETTING_FORMAT, setting.Name, ex.Message));
                }
            }

            if (errorValues.Count > 1)
                throw new CLIException(String.Join("\n", errorValues.ToArray()));
        }

        /// <summary>
        /// Writes the given setting.
        /// </summary>
        /// <param name="setting">The setting to write.</param>
        /// <exception cref="CLIException">If there is any error writing the setting.</exception>
        private async Task WriteSetting(AbstractSetting setting)
        {
            string answer = await device.ExecuteCLICommand(String.Format(COMMAND_SET_SETTING, setting.Command, setting.Value), TIMEOUT_WRITE_SETTING);
            if (answer.Contains("Validation error"))
                throw new CLIException("Invalid setting value");
        }
    }
}
