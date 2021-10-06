/*
 * Copyright 2019-2021, Digi International Inc.
 * Copyright 2014, 2015, Sébastien Rault.
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

using System.Collections.Generic;
using XBeeLibrary.Core.Utils;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates the different hardware versions of the XBee devices.
	/// </summary>
	public enum HardwareVersionEnum
	{
		// Enumeration entries.
		X09_009 = 1,
		X09_019 = 2,
		XH9_009 = 3,
		XH9_019 = 4,
		X24_009 = 5,
		X24_019 = 6,
		X09_001 = 7,
		XH9_001 = 8,
		X08_004 = 9,
		XC09_009 = 0xa,
		XC09_038 = 0xb,
		X24_038 = 0xc,
		X09_009_TX = 0xd,
		X09_019_TX = 0xe,
		XH9_009_TX = 0xf,
		XH9_019_TX = 0x10,
		X09_001_TX = 0x11,
		XH9_001_TX = 0x12,
		XT09B_XXX = 0x13,
		XT09_XXX = 0x14,
		XC08_009 = 0x15,
		XC08_038 = 0x16,
		XB24_AXX_XX = 0x17,
		XBP24_AXX_XX = 0x18,
		XB24_BXIX_XXX = 0x19,
		XBP24_BXIX_XXX = 0x1a,
		XBP09_DXIX_XXX = 0x1b,
		XBP09_XCXX_XXX = 0x1c,
		XBP08_DXXX_XXX = 0x1d,
		XBP24B = 0x1e,
		XB24_WF = 0x1f,
		AMBER_MBUS = 0x20,
		XBP24C = 0x21,
		XB24C = 0x22,
		XSC_GEN3 = 0x23,
		SRD_868_GEN3 = 0x24,
		ABANDONATED = 0x25,
		SMT_900LP = 0x26,
		WIFI_ATHEROS = 0x27,
		SMT_WIFI_ATHEROS = 0x28,
		SMT_475LP = 0x29,
		XBEE_CELL_TH = 0x2a,
		XLR_MODULE = 0x2b,
		XB900HP_NZ = 0x2c,
		XBP24C_TH_DIP = 0x2d,
		XB24C_TH_DIP = 0x2e,
		XLR_BASEBOARD = 0x2f,
		XBP24C_S2C_SMT = 0x30,
		SX_PRO = 0x31,
		S2D_SMT_PRO = 0x32,
		S2D_SMT_REG = 0x33,
		S2D_TH_PRO = 0x34,
		S2D_TH_REG = 0x35,
		SX = 0x3E,
		XTR = 0x3F,
		CELLULAR_CAT1_LTE_VERIZON = 0x40,
		XBEE3_MICRO = 0x41,
		XBEE3_TH = 0x42,
		XBEE3_RESERVED = 0x43,
		CELLULAR_3G = 0x44,
		XB8X = 0x45,
		CELLULAR_LTE_VERIZON = 0x46,  // Abandoned
		CELLULAR_LTE_ATT = 0x47,
		CELLULAR_NBIOT_EUROPE = 0x48,  // Never released
		CELLULAR_3_CAT1_LTE_ATT = 0x49,
		CELLULAR_3_LTE_M_VERIZON = 0x4A,  // Abandoned
		CELLULAR_3_LTE_M_ATT = 0x4B,
		CELLULAR_3_LTE_M_ATT_TELIT = 0x4C,  // Never released
		CELLULAR_3_CAT1_LTE_VERIZON = 0x4D,
		CELLULAR_3_LTE_M_TELIT = 0x4E,
		XBEE3_DM_LR = 0x50,
		XBEE3_DM_LR_868 = 0x51,
		XBEE3_RR = 0x52,
		S2C_P5 = 0x53,
		CELLULAR_3_CAT1_GLOBAL = 0x54
	}

	public static class HardwareVersionEnumExtensions
	{
		static IDictionary<HardwareVersionEnum, string> lookupTable = new Dictionary<HardwareVersionEnum, string>();

		static HardwareVersionEnumExtensions()
		{
			lookupTable.Add(HardwareVersionEnum.X09_009, "X09-009");
			lookupTable.Add(HardwareVersionEnum.X09_019, "X09-019");
			lookupTable.Add(HardwareVersionEnum.XH9_009, "XH9-009");
			lookupTable.Add(HardwareVersionEnum.XH9_019, "XH9-019");
			lookupTable.Add(HardwareVersionEnum.X24_009, "X24-009");
			lookupTable.Add(HardwareVersionEnum.X24_019, "X24-019");
			lookupTable.Add(HardwareVersionEnum.X09_001, "X09-001");
			lookupTable.Add(HardwareVersionEnum.XH9_001, "XH9-001");
			lookupTable.Add(HardwareVersionEnum.X08_004, "X08-004");
			lookupTable.Add(HardwareVersionEnum.XC09_009, "XC09-009");
			lookupTable.Add(HardwareVersionEnum.XC09_038, "XC09-038");
			lookupTable.Add(HardwareVersionEnum.X24_038, "X24-038");
			lookupTable.Add(HardwareVersionEnum.X09_009_TX, "X09-009-TX");
			lookupTable.Add(HardwareVersionEnum.X09_019_TX, "X09-019-TX");
			lookupTable.Add(HardwareVersionEnum.XH9_009_TX, "XH9-009-TX");
			lookupTable.Add(HardwareVersionEnum.XH9_019_TX, "XH9-019-TX");
			lookupTable.Add(HardwareVersionEnum.X09_001_TX, "X09-001-TX");
			lookupTable.Add(HardwareVersionEnum.XH9_001_TX, "XH9-001-TX");
			lookupTable.Add(HardwareVersionEnum.XT09B_XXX, "XT09B-xxx (Attenuator version)");
			lookupTable.Add(HardwareVersionEnum.XT09_XXX, "XT09-xxx");
			lookupTable.Add(HardwareVersionEnum.XC08_009, "XC08-009");
			lookupTable.Add(HardwareVersionEnum.XC08_038, "XC08-038");
			lookupTable.Add(HardwareVersionEnum.XB24_AXX_XX, "XB24-Axx-xx");
			lookupTable.Add(HardwareVersionEnum.XBP24_AXX_XX, "XBP24-Axx-xx");
			lookupTable.Add(HardwareVersionEnum.XB24_BXIX_XXX, "XB24-BxIx-xxx and XB24-Z7xx-xxx");
			lookupTable.Add(HardwareVersionEnum.XBP24_BXIX_XXX, "XBP24-BxIx-xxx and XBP24-Z7xx-xxx");
			lookupTable.Add(HardwareVersionEnum.XBP09_DXIX_XXX, "XBP09-DxIx-xxx Digi Mesh");
			lookupTable.Add(HardwareVersionEnum.XBP09_XCXX_XXX, "XBP09-XCxx-xxx: S3 XSC Compatibility");
			lookupTable.Add(HardwareVersionEnum.XBP08_DXXX_XXX, "XBP08-Dxx-xxx 868MHz");
			lookupTable.Add(HardwareVersionEnum.XBP24B, "XBP24B: Low cost ZB PRO and PLUS S2B");
			lookupTable.Add(HardwareVersionEnum.XB24_WF, "XB24-WF: XBee 802.11 (Redpine module)");
			lookupTable.Add(HardwareVersionEnum.AMBER_MBUS, "??????: M-Bus module made by Amber");
			lookupTable.Add(HardwareVersionEnum.XBP24C, "XBP24C: XBee PRO SMT Ember 357 S2C PRO");
			lookupTable.Add(HardwareVersionEnum.XB24C, "XB24C: XBee SMT Ember 357 S2C");
			lookupTable.Add(HardwareVersionEnum.XSC_GEN3, "XSC_GEN3: XBP9 XSC 24 dBm");
			lookupTable.Add(HardwareVersionEnum.SRD_868_GEN3, "SDR_868_GEN3: XB8 12 dBm");
			lookupTable.Add(HardwareVersionEnum.ABANDONATED, "Abandonated");
			lookupTable.Add(HardwareVersionEnum.SMT_900LP, "900LP (SMT): 900LP on 'S8 HW'");
			lookupTable.Add(HardwareVersionEnum.WIFI_ATHEROS, "WiFi Atheros (TH-DIP) XB2S-WF");
			lookupTable.Add(HardwareVersionEnum.SMT_WIFI_ATHEROS, "WiFi Atheros (SMT) XB2B-WF");
			lookupTable.Add(HardwareVersionEnum.SMT_475LP, "475LP (SMT): Beta 475MHz");
			lookupTable.Add(HardwareVersionEnum.XBEE_CELL_TH, "XBee-Cell (TH): XBee Cellular");
			lookupTable.Add(HardwareVersionEnum.XLR_MODULE, "XLR Module");
			lookupTable.Add(HardwareVersionEnum.XB900HP_NZ, "XB900HP (New Zealand): XB9 NZ HW/SW");
			lookupTable.Add(HardwareVersionEnum.XBP24C_TH_DIP, "XBP24C (TH-DIP): XBee PRO DIP");
			lookupTable.Add(HardwareVersionEnum.XB24C_TH_DIP, "XB24C (TH-DIP): XBee DIP");
			lookupTable.Add(HardwareVersionEnum.XLR_BASEBOARD, "XLR Baseboard");
			lookupTable.Add(HardwareVersionEnum.XBP24C_S2C_SMT, "XBee PRO SMT");
			lookupTable.Add(HardwareVersionEnum.SX_PRO, "SX Pro");
			lookupTable.Add(HardwareVersionEnum.S2D_SMT_PRO, "XBP24D: S2D SMT PRO");
			lookupTable.Add(HardwareVersionEnum.S2D_SMT_REG, "XB24D: S2D SMT Reg");
			lookupTable.Add(HardwareVersionEnum.S2D_TH_PRO, "XBP24D: S2D TH PRO");
			lookupTable.Add(HardwareVersionEnum.S2D_TH_REG, "XB24D: S2D TH Reg");
			lookupTable.Add(HardwareVersionEnum.SX, "SX");
			lookupTable.Add(HardwareVersionEnum.XTR, "XTR");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_CAT1_LTE_VERIZON, "XBee Cellular Cat 1 LTE Verizon");
			lookupTable.Add(HardwareVersionEnum.XBEE3_MICRO, "XBee3 Micro/SMT");
			lookupTable.Add(HardwareVersionEnum.XBEE3_TH, "XBee3 TH");
			lookupTable.Add(HardwareVersionEnum.XBEE3_RESERVED, "XBee3 Reserved");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_3G, "XBee Cellular 3G");
			lookupTable.Add(HardwareVersionEnum.XB8X, "XB8X");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_LTE_VERIZON, "XBee Cellular LTE-M Verizon");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_LTE_ATT, "XBee Cellular LTE-M AT&T");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_NBIOT_EUROPE, "XBee Cellular NBIoT Europe");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_3_CAT1_LTE_ATT, "XBee3 Cellular Cat 1 LTE AT&T");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_3_LTE_M_VERIZON, "XBee3 Cellular LTE-M Verizon");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_3_LTE_M_ATT, "XBee3 Cellular LTE-M AT&T (u-blox)");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_3_LTE_M_ATT_TELIT, "XBee3 Cellular LTE-M AT&T (Telit)");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_3_CAT1_LTE_VERIZON, "XBee3 Cellular Cat 1 LTE Verizon");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_3_LTE_M_TELIT, "XBee 3 Cellular LTE - M / NB - IoT (Telit)");
			lookupTable.Add(HardwareVersionEnum.XBEE3_DM_LR, "XB3-DMLR");
			lookupTable.Add(HardwareVersionEnum.XBEE3_DM_LR_868, "XB3-DMLR868");
			lookupTable.Add(HardwareVersionEnum.XBEE3_RR, "XBee 3 Reduced RAM");
			lookupTable.Add(HardwareVersionEnum.S2C_P5, "S2C P5");
			lookupTable.Add(HardwareVersionEnum.CELLULAR_3_CAT1_GLOBAL, "XBee Cellular 3 Cat 1 Global");
		}

		/// <summary>
		/// Gets the Hardware version numeric value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The hardware version numeric value.</returns>
		public static int GetValue(this HardwareVersionEnum source)
		{
			return (int)source;
		}

		/// <summary>
		/// Gets the hardware version description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The hardware version description.</returns>
		public static string GetDescription(this HardwareVersionEnum source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the <see cref="HardwareVersionEnum"/> associated to the given numeric value.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">Numeric value of the <see cref="HardwareVersionEnum"/> to retrieve.</param>
		/// <returns>The <see cref="HardwareVersionEnum"/> associated to the given numeric value.</returns>
		public static HardwareVersionEnum Get(this HardwareVersionEnum source, int value)
		{
			return (HardwareVersionEnum)value;
		}

		/// <summary>
		/// Returns the <see cref="HardwareVersionEnum"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="HardwareVersionEnum"/> in string format.</returns>
		public static string ToDisplayString(this HardwareVersionEnum source)
		{
			return string.Format("{0}: {1}", HexUtils.ByteToHexString((byte)source.GetValue()), source.GetDescription());
		}
	}
}