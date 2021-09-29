using System;
using Xamarin.Essentials;

namespace IX15Configurator.Utils
{
    class AppPreferences
    {
        // Application preferences.
        public const string RF_MAC = "RfMac";

        // Search filtering.
        public const string SEARCH_FILTER_TEXT = "SearchFilterText";

        // Default preferences values.
        public const string DEFAULT_SEARCH_FILTER_TEXT = "IX15";
        public const string DEFAULT_RF_MAC = "";

        // Event handlers.
        public static event EventHandler SearchFilterChanged = delegate { };

        /// <summary>
        /// Returns the value of the search filter text preference.
        /// </summary>
        /// <returns>The value of the search filter text preference.</returns>
        public static string GetSearchFilterText()
        {
            return Preferences.Get(SEARCH_FILTER_TEXT, DEFAULT_SEARCH_FILTER_TEXT);
        }

        /// <summary>
        /// Sets the value of the search filter text preference.
        /// </summary>
        /// <param name="filterText">The new value of the search filter text 
        /// preference.</param>
        public static void SetSearchFilterText(string filterText)
        {
            Preferences.Set(SEARCH_FILTER_TEXT, filterText);
            // Send an event to notify the change.
            SearchFilterChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Returns the RF MAC address associated to the given BLE MAC.
        /// </summary>
        /// <param name="bleMac">BLE MAC address.</param>
        /// <returns>The RF MAC address or empty if it is not stored.</returns>
        public static string GetRfMac(string bleMac)
        {
            return Preferences.Get(RF_MAC + bleMac, DEFAULT_RF_MAC);
        }

        /// <summary>
        /// Sets the RF MAC address associated to the given BLE MAC.
        /// </summary>
        /// <param name="bleMac">BLE MAC address.</param>
        /// <param name="rfMac">RF MAC address.</param>
        public static void SetRfMac(string bleMac, string rfMac)
        {
            Preferences.Set(RF_MAC + bleMac, rfMac);
        }

        /// <summary>
        /// Clears (removes) all the preferences and their values.
        /// </summary>
        public static void ClearPreferences()
        {
            Preferences.Clear();
        }
    }
}
