using System.Text;
using System.Text.RegularExpressions;

namespace IX15Configurator.Models
{
    public class DeviceInformation
    {
        // Constants
        private const string UNKNOWN = "-";

        private const string PATTERN_FIRMWARE_VERSION = "^ Firmware Version.*:(.*)$";
        private const string PATTERN_ALT_FIRMWARE_VERSION = "^ Alt. Firmware Version.*:(.*)$";
        private const string PATTERN_BOOTLOADER_VERSION = "^ Bootloader Version.*:(.*)$";
        private const string PATTERN_HOST_NAME = "^ Hostname.*:(.*)$";
        private const string PATTERN_SERIAL_NUMBER = "^ Serial Number.*:(.*)$";
        private const string PATTERN_IP_ADDRESS = "^ eth.*IPv4.*up(.*)$";

        // Variables
        private string firmwareVersionActive;
        private string firmwareVersionInactive;
        private string bootloaderVersion;
        private string serialNumber;
        private string hostName;
        private string ipAddress;

        // Properties
        /// <summary>
        /// Firmware version of the active partition.
        /// </summary>
        public string FirmwareVersionActive
        {
            get { return firmwareVersionActive; }
        }

        /// <summary>
        /// Firmware version of the inactive partition.
        /// </summary>
        public string FirmwareVersionInactive
        {
            get { return firmwareVersionInactive; }
        }

        /// <summary>
        /// Bootloader version.
        /// </summary>
        public string BootloaderVersion
        {
            get { return bootloaderVersion; }
        }

        /// <summary>
        /// Bootloader version.
        /// </summary>
        public string SerialNumber
        {
            get { return serialNumber; }
        }

        /// <summary>
        /// Host name.
        /// </summary>
        public string HostName
        {
            get { return hostName; }
        }

        /// <summary>
        /// IPaddress.
        /// </summary>
        public string IPAddress
        {
            get { return ipAddress; }
        }

        /// <summary>
        /// Class constructor. Instantiates a new <c>DeviceInformation</c> with the given
        /// parameters.
        /// </summary>
        /// <param name="firmwareVersionActive">Active firmware version.</param>
        /// <param name="firmwareVersionInactive">Inactive firmware version.</param>
        /// <param name="bootloaderVersion">Bootloader version.</param>
        /// <param name="serialNumber">Serial number.</param>
        /// <param name="hostName">Host name.</param>
        /// <param name="ipAddress">IP address.</param>
        public DeviceInformation(string firmwareVersionActive, string firmwareVersionInactive,
            string bootloaderVersion, string serialNumber, string hostName, string ipAddress)
        {
            this.firmwareVersionActive = firmwareVersionActive;
            this.firmwareVersionInactive = firmwareVersionInactive;
            this.bootloaderVersion = bootloaderVersion;
            this.serialNumber = serialNumber;
            this.hostName = hostName;
            this.ipAddress = ipAddress;
        }

        /// <summary>
        /// Parses the given strings and returns a new <c>DeviceInformation</c>.
        /// </summary>
        /// <param name="systemString">The system string to parse and extract the device information.</param>
        /// <param name="networkString">The network string to parse and extract the device information.</param>
        /// <returns>A new <c>DeviceInformation</c> from the parsed strings.</returns>
        public static DeviceInformation Parse(string systemString, string networkString)
        {
            string firmwareVersionActive = UNKNOWN;
            string firmwareVersionInactive = UNKNOWN;
            string bootloaderVersion = UNKNOWN;
            string serialNumber = UNKNOWN;
            string hostName = UNKNOWN;
            string ipAddress = UNKNOWN;

            if (systemString != null)
            {
                // Extract Firmware Version.
                Match match = Regex.Match(systemString, PATTERN_FIRMWARE_VERSION, RegexOptions.Multiline);
                if (match.Success)
                    firmwareVersionActive = match.Groups[1].Value.Trim();

                // Extract Alternative Firmware Version.
                match = Regex.Match(systemString, PATTERN_ALT_FIRMWARE_VERSION, RegexOptions.Multiline);
                if (match.Success)
                    firmwareVersionInactive = match.Groups[1].Value.Trim();

                // Extract Bootloader Version.
                match = Regex.Match(systemString, PATTERN_BOOTLOADER_VERSION, RegexOptions.Multiline);
                if (match.Success)
                    bootloaderVersion = match.Groups[1].Value.Trim();

                // Extract Serial Number.
                match = Regex.Match(systemString, PATTERN_SERIAL_NUMBER, RegexOptions.Multiline);
                if (match.Success)
                    serialNumber = match.Groups[1].Value.Trim();

                // Extract Host Name.
                match = Regex.Match(systemString, PATTERN_HOST_NAME, RegexOptions.Multiline);
                if (match.Success)
                    hostName = match.Groups[1].Value.Trim();
            }

            if (networkString != null)
            {
                // Extract IP Address.
                Match match = Regex.Match(networkString, PATTERN_IP_ADDRESS, RegexOptions.Multiline);
                if (match.Success)
                    ipAddress = match.Groups[1].Value.Trim();
            }

            return new DeviceInformation(firmwareVersionActive, firmwareVersionInactive,
                bootloaderVersion, serialNumber, hostName, ipAddress);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("Device information:\n");
            stringBuilder.AppendLine("  - Firmware version: " + firmwareVersionActive);
            stringBuilder.AppendLine("  - Alt. firmware version: " + firmwareVersionInactive);
            stringBuilder.AppendLine("  - Bootloader version: " + bootloaderVersion);
            stringBuilder.AppendLine("  - Serial number: " + serialNumber);
            stringBuilder.AppendLine("  - Host name: " + hostName);
            stringBuilder.AppendLine("  - IP address: " + ipAddress);

            return stringBuilder.ToString();
        }
    }
}
