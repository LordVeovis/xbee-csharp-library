using System.Collections.Generic;
using XBeeLibrary.Core.Utils;

namespace IX15Configurator.Models
{
    /// <summary>
    /// Enumerates the different application colors.
    /// </summary>
    public enum AppColor
    {
        COLOR_GRAY = 0x0,
        COLOR_GRAY_DARK = 0x1,
        COLOR_GRAY_LIGHT = 0x2,
        COLOR_GRAY_MEDIUM = 0x3,
        COLOR_GREEN_DIGI = 0x4,
        COLOR_BLUE_DARK = 0x5,
        COLOR_RED = 0x6,
        COLOR_RED_DARK = 0x7,
        COLOR_RED_LIGHT = 0x8,
        COLOR_RED_MEDIUM = 0x9
    }

    public static class AppColorExtensions
    {
        static IDictionary<AppColor, string> lookupTable = new Dictionary<AppColor, string>();

        static AppColorExtensions()
        {
            lookupTable.Add(AppColor.COLOR_GRAY, "#CCCCCC");
            lookupTable.Add(AppColor.COLOR_GRAY_DARK, "#717474");
            lookupTable.Add(AppColor.COLOR_GRAY_LIGHT, "#EEEEEE");
            lookupTable.Add(AppColor.COLOR_GRAY_MEDIUM, "#DDDDDD");
            lookupTable.Add(AppColor.COLOR_GREEN_DIGI, "#84C361");
            lookupTable.Add(AppColor.COLOR_BLUE_DARK, "#3577B6");
            lookupTable.Add(AppColor.COLOR_RED, "#FF9999");
            lookupTable.Add(AppColor.COLOR_RED_DARK, "#CC0000");
            lookupTable.Add(AppColor.COLOR_RED_LIGHT, "#FFCCCC");
            lookupTable.Add(AppColor.COLOR_RED_MEDIUM, "#FFAAAA");
        }

        /// <summary>
        /// Gets the application color.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>The application color numeric value.</returns>
        public static int GetValue(this AppColor source)
        {
            return (int)source;
        }

        /// <summary>
        /// Gets the application color code.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>The application color code.</returns>
        public static string GetCode(this AppColor source)
        {
            return lookupTable[source];
        }

        /// <summary>
        /// Gets the <see cref="AppColor"/> associated to the given 
        /// numeric value.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value">Numeric value of the <see cref="AppColor"/> 
        /// to get.</param>
        /// <returns>The <see cref="AppColor"/> associated to the given 
        /// numeric value.</returns>
        public static AppColor Get(this AppColor dumb, int value)
        {
            return (AppColor)value;
        }

        public static string ToDisplayString(this AppColor source)
        {
            return string.Format("{0}: {1}", HexUtils.ByteToHexString((byte)source.GetValue()), source.GetCode());
        }
    }
}
