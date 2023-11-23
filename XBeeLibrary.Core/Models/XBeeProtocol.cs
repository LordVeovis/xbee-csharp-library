/*
 * Copyright 2019-2023, Digi International Inc.
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace XBeeLibrary.Core.Models
{
	/// <summary>
	/// Enumerates the available XBee protocols. The XBee protocol is determined by the combination 
	/// of hardware and firmware of an XBee device.
	/// </summary>
	public enum XBeeProtocol
	{
		// Enumeration entries.
		ZIGBEE = 0,
		RAW_802_15_4 = 1,
		XBEE_WIFI = 2,
		DIGI_MESH = 3,
		XCITE = 4,
		XTEND = 5,
		XTEND_DM = 6,
		SMART_ENERGY = 7,
		DIGI_POINT = 8,
		ZNET = 9,
		XC = 10,
		XLR = 11,
		XLR_DM = 12,
		SX = 13,
		XLR_MODULE = 14,
		CELLULAR = 15,
		CELLULAR_NBIOT = 16,
		THREAD = 17,
		BLU = 18,
		UNKNOWN = 99
	}

	public static class XBeeProtocolExtensions
	{
		static IDictionary<XBeeProtocol, string> lookupTable = new Dictionary<XBeeProtocol, string>();

		static XBeeProtocolExtensions()
		{
			lookupTable.Add(XBeeProtocol.ZIGBEE, "ZigBee");
			lookupTable.Add(XBeeProtocol.RAW_802_15_4, "802.15.4");
			lookupTable.Add(XBeeProtocol.XBEE_WIFI, "Wi-Fi");
			lookupTable.Add(XBeeProtocol.DIGI_MESH, "DigiMesh");
			lookupTable.Add(XBeeProtocol.XCITE, "XCite");
			lookupTable.Add(XBeeProtocol.XTEND, "XTend (Legacy)");
			lookupTable.Add(XBeeProtocol.XTEND_DM, "XTend (DigiMesh)");
			lookupTable.Add(XBeeProtocol.SMART_ENERGY, "Smart Energy");
			lookupTable.Add(XBeeProtocol.DIGI_POINT, "Point-to-multipoint");
			lookupTable.Add(XBeeProtocol.ZNET, "ZNet 2.5");
			lookupTable.Add(XBeeProtocol.XC, "XSC");
			lookupTable.Add(XBeeProtocol.XLR, "XLR");
			lookupTable.Add(XBeeProtocol.XLR_DM, "XLR"); // TODO [XLR_DM] XLR device with DigiMesh support.
			lookupTable.Add(XBeeProtocol.SX, "XBee SX");
			lookupTable.Add(XBeeProtocol.XLR_MODULE, "XLR Module");
			lookupTable.Add(XBeeProtocol.CELLULAR, "Cellular");
			lookupTable.Add(XBeeProtocol.CELLULAR_NBIOT, "Cellular NB-IoT");
			lookupTable.Add(XBeeProtocol.THREAD, "Thread");
			lookupTable.Add(XBeeProtocol.BLU, "Bluetooth");
			lookupTable.Add(XBeeProtocol.UNKNOWN, "Unknown");
		}

		/// <summary>
		/// Gets the XBee protocol ID.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>XBee protocol ID.</returns>
		public static int GetID(this XBeeProtocol source)
		{
			return (int)source;
		}

		/// <summary>
		/// Gets the XBee protocol description.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>XBee protocol description.</returns>
		public static string GetDescription(this XBeeProtocol source)
		{
			return lookupTable[source];
		}

		/// <summary>
		/// Gets the <see cref="XBeeProtocol"/> associated to the given ID.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="id">The ID of the <see cref="XBeeProtocol"/> to retrieve.</param>
		/// <returns>The <see cref="XBeeProtocol"/> associated to the given ID.</returns>
		public static XBeeProtocol Get(this Format source, int id)
		{
			var values = Enum.GetValues(typeof(XBeeProtocol));

			if (values.OfType<int>().Contains(id))
				return (XBeeProtocol)id;

			return XBeeProtocol.UNKNOWN;
		}

		/// <summary>
		/// Determines the XBee protocol based on the given Hardware and firmware versions.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="hardwareVersion">The hardware version of the protocol to determine.</param>
		/// <param name="firmwareVersion">The firmware version of the protocol to determine.</param>
		/// <returns>The XBee protocol corresponding to the given hardware and firmware versions.</returns>
		public static XBeeProtocol DetermineProtocol(this XBeeProtocol source, HardwareVersion hardwareVersion, string firmwareVersion)
		{
			if (hardwareVersion == null || firmwareVersion == null || hardwareVersion.Value < 0x09)
			{
				return XBeeProtocol.UNKNOWN;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XC09_009.GetValue()
					|| hardwareVersion.Value == HardwareVersionEnum.XC09_038.GetValue())
			{
				return XBeeProtocol.XCITE;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XT09_XXX.GetValue())
			{
				if ((firmwareVersion.Length == 4 && firmwareVersion.StartsWith("8"))
						|| (firmwareVersion.Length == 5 && firmwareVersion[1] == '8'))
					return XBeeProtocol.XTEND_DM;
				return XBeeProtocol.XTEND;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XB24_AXX_XX.GetValue()
					|| hardwareVersion.Value == HardwareVersionEnum.XBP24_AXX_XX.GetValue())
			{
				if ((firmwareVersion.Length == 4 && firmwareVersion.StartsWith("8")))
					return XBeeProtocol.DIGI_MESH;
				return XBeeProtocol.RAW_802_15_4;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XB24_BXIX_XXX.GetValue()
					|| hardwareVersion.Value == HardwareVersionEnum.XBP24_BXIX_XXX.GetValue())
			{
				if ((firmwareVersion.Length == 4 && firmwareVersion.StartsWith("1") && firmwareVersion.EndsWith("20"))
						|| (firmwareVersion.Length == 4 && firmwareVersion.StartsWith("2")))
					return XBeeProtocol.ZIGBEE;
				else if (firmwareVersion.Length == 4 && firmwareVersion.StartsWith("3"))
					return XBeeProtocol.SMART_ENERGY;
				return XBeeProtocol.ZNET;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBP09_DXIX_XXX.GetValue())
			{
				if ((firmwareVersion.Length == 4 && firmwareVersion.StartsWith("8")
						|| (firmwareVersion.Length == 4 && firmwareVersion[1] == '8'))
						|| (firmwareVersion.Length == 5 && firmwareVersion[1] == '8'))
					return XBeeProtocol.DIGI_MESH;
				return XBeeProtocol.DIGI_POINT;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBP09_XCXX_XXX.GetValue())
			{
				return XBeeProtocol.XC;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBP08_DXXX_XXX.GetValue())
			{
				return XBeeProtocol.DIGI_POINT;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBP24B.GetValue())
			{
				if (firmwareVersion.Length == 4 && firmwareVersion.StartsWith("3"))
					return XBeeProtocol.SMART_ENERGY;
				return XBeeProtocol.ZIGBEE;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XB24_WF.GetValue()
						|| hardwareVersion.Value == HardwareVersionEnum.WIFI_ATHEROS.GetValue()
						|| hardwareVersion.Value == HardwareVersionEnum.SMT_WIFI_ATHEROS.GetValue())
			{
				return XBeeProtocol.XBEE_WIFI;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBP24C.GetValue()
						|| hardwareVersion.Value == HardwareVersionEnum.XB24C.GetValue())
			{
				if (firmwareVersion.Length == 4 && firmwareVersion.StartsWith("5"))
					return XBeeProtocol.SMART_ENERGY;
				return XBeeProtocol.ZIGBEE;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XSC_GEN3.GetValue()
						|| hardwareVersion.Value == HardwareVersionEnum.SRD_868_GEN3.GetValue())
			{
				if (firmwareVersion.Length == 4 && firmwareVersion.StartsWith("8"))
					return XBeeProtocol.DIGI_MESH;
				else if (firmwareVersion.Length == 4 && firmwareVersion.StartsWith("1"))
					return XBeeProtocol.DIGI_POINT;
				return XBeeProtocol.XC;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBEE_CELL_TH.GetValue())
			{
				return XBeeProtocol.UNKNOWN;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XLR_MODULE.GetValue())
			{
				// This is for the old version of the XLR we have (K60), and it is 
				// reporting the firmware of the module (8001), this will change in 
				// future (after K64 integration) reporting the hardware and firmware
				// version of the baseboard (see the case HardwareVersionEnum.XLR_BASEBOARD).
				// TODO maybe this should be removed in future, since this case will never be released.
				return XBeeProtocol.XLR;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XLR_BASEBOARD.GetValue())
			{
				// XLR devices with K64 will report the baseboard hardware version, 
				// and also firmware version (the one we have here is 1002, but this value
				// is not being reported since is an old K60 version, the module fw version
				// is reported instead).

				// TODO [XLR_DM] The next version of the XLR will add DigiMesh support should be added.
				// Probably this XLR_DM and XLR will depend on the firmware version.

				return XBeeProtocol.XLR;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XB900HP_NZ.GetValue())
			{
				return XBeeProtocol.DIGI_POINT;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBP24C_TH_DIP.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XB24C_TH_DIP.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBP24C_S2C_SMT.GetValue())
			{
				if (firmwareVersion.Length == 4 && (firmwareVersion.StartsWith("5") || firmwareVersion.StartsWith("6")))
					return XBeeProtocol.SMART_ENERGY;
				else if (firmwareVersion.StartsWith("2"))
					return XBeeProtocol.RAW_802_15_4;
				else if (firmwareVersion.StartsWith("9"))
					return XBeeProtocol.DIGI_MESH;
				return XBeeProtocol.ZIGBEE;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.SX_PRO.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.SX.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XTR.GetValue())
			{
				if (firmwareVersion.StartsWith("2"))
					return XBeeProtocol.XTEND;
				else if (firmwareVersion.StartsWith("8"))
					return XBeeProtocol.XTEND_DM;
				return XBeeProtocol.SX;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.S2D_SMT_PRO.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.S2D_SMT_REG.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.S2D_TH_PRO.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.S2D_TH_REG.GetValue())
			{
				if (firmwareVersion.StartsWith("8"))
					return XBeeProtocol.THREAD;
				return XBeeProtocol.ZIGBEE;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.CELLULAR_CAT1_LTE_VERIZON.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3G.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_LTE_VERIZON.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_LTE_ATT.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_NBIOT_EUROPE.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_CAT1_LTE_ATT.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_LTE_M_VERIZON.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_LTE_M_ATT.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_LTE_M_ATT_TELIT.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_CAT1_LTE_VERIZON.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_LTE_M_TELIT.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_CAT1_GLOBAL.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_CAT1_NA.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_LTE_M_LOW_POWER.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_CAT4_GLOBAL.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.CELLULAR_3_CAT4_NA.GetValue())
			{
				return XBeeProtocol.CELLULAR;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBEE3_MICRO.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBEE3_TH.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBEE3_RESERVED.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBEE3_RR.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBEE_RR_TH.GetValue())
			{
				if (firmwareVersion.StartsWith("2"))
					return XBeeProtocol.RAW_802_15_4;
				else if (firmwareVersion.StartsWith("3"))
					return XBeeProtocol.DIGI_MESH;
				return XBeeProtocol.ZIGBEE;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XB8X.GetValue())
			{
				return XBeeProtocol.DIGI_MESH;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBEE3_DM_LR.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBEE3_DM_LR_868.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBEE_XR_900_TH.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBEE_XR_868_TH.GetValue())
			{
				return XBeeProtocol.DIGI_MESH;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.S2C_P5.GetValue())
			{
				return XBeeProtocol.ZIGBEE;
			}
			else if (hardwareVersion.Value == HardwareVersionEnum.XBEE3_BLU_MICRO_SMT.GetValue()
				|| hardwareVersion.Value == HardwareVersionEnum.XBEE3_BLU_TH.GetValue())
			{
				return XBeeProtocol.BLU;
			}

			return XBeeProtocol.ZIGBEE;
		}

		/// <summary>
		/// Returns the <see cref="XBeeProtocol"/> in string format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The <see cref="XBeeProtocol"/> in string format.</returns>
		public static string ToDisplayString(this XBeeProtocol source)
		{
			return lookupTable[source];
		}
	}
}