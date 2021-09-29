namespace IX15Configurator.Utils
{
    class RssiUtils
    {
        // Constants.
        public static readonly int WORST_RSSI = -127;

        private static readonly int RSSI_5 = -55;
        private static readonly int RSSI_4 = -68;
        private static readonly int RSSI_3 = -80;
        private static readonly int RSSI_2 = -95;

        private static readonly string IMG_RSSI_5 = "rssi_5";
        private static readonly string IMG_RSSI_4 = "rssi_4";
        private static readonly string IMG_RSSI_3 = "rssi_3";
        private static readonly string IMG_RSSI_2 = "rssi_2";
        private static readonly string IMG_RSSI_1 = "rssi_1";

        /// <summary>
        /// Returns the image corresponding to the given RSSI value.
        /// </summary>
        /// <param name="rssi">RSSI value.</param>
        /// <returns>The image corresponding to the given RSSI value.</returns>
        public static string GetRssiImage(int rssi)
        {
            if (rssi > RSSI_5)
                return IMG_RSSI_5;
            else if (rssi > RSSI_4)
                return IMG_RSSI_4;
            else if (rssi > RSSI_3)
                return IMG_RSSI_3;
            else if (rssi > RSSI_2)
                return IMG_RSSI_2;
            else
                return IMG_RSSI_1;
        }
    }
}
