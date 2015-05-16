using Kveer.XBeeApi.Exceptions;
using Kveer.XBeeApi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kveer.XBeeApi.IO
{
	/// <summary>
	/// This class represents an IO Data Sample. The sample is built using the the constructor. The sample contains an analog and digital mask indicating which IO lines are configured with that functionality.
	/// Depending on the protocol the XBee device is executing, the digital and analog masks are retrieved in separated bytes (2 bytes for the digital mask and 1 for the analog mask) or merged (digital and analog masks are contained in the same 2 bytes).
	/// 
	/// 802.15.4 Protocol
	/// 
	/// Digital and analog channels masks
	/// ------------------------------------------------------------------
	/// Indicates which digital and ADC IO lines are configured in the module. Each bit corresponds to one digital or ADC IO line on the module:
	/// 
	/// bit 0 =  DIO0 1
	/// bit 1 =  DIO1 0
	/// bit 2 =  DIO2 0
	/// bit 3 =  DIO3 1
	/// bit 4 =  DIO4 0
	/// bit 5 =  DIO5 1
	/// bit 6 =  DIO6 0
	/// bit 7 =  DIO7 0
	/// bit 8 =  DIO8 0
	/// bit 9 =  AD0 0
	/// bit 10 = AD1 1
	/// bit 11 = AD2 1
	/// bit 12 = AD3 0
	/// bit 13 = AD4 0
	/// bit 14 = AD5 0
	/// bit 15 = N/A 0
	/// 
	/// Example: mask of {@code 0x0C29} means DIO0, DIO3, DIO5, AD1 and AD2 enabled.
	/// 0 0 0 0 1 1 0 0 0 0 1 0 1 0 0 1
	/// 
	/// Other Protocols
	/// 
	/// Digital Channel Mask
	/// ------------------------------------------------------------------
	/// Indicates which digital IO lines are configured in the module. Each bit corresponds to one digital IO line on the module:
	/// 
	/// bit 0 =  DIO0/AD0
	/// bit 1 =  DIO1/AD1
	/// bit 2 =  DIO2/AD2
	/// bit 3 =  DIO3/AD3
	/// bit 4 =  DIO4/AD4
	/// bit 5 =  DIO5/AD5/ASSOC
	/// bit 6 =  DIO6/RTS
	/// bit 7 =  DIO7/CTS
	/// bit 8 =  DIO8/DTR/SLEEP_RQ
	/// bit 9 =  DIO9/ON_SLEEP
	/// bit 10 = DIO10/PWM0/RSSI
	/// bit 11 = DIO11/PWM1
	/// bit 12 = DIO12/CD
	/// bit 13 = DIO13
	/// bit 14 = DIO14
	/// bit 15 = N/A
	/// 
	/// Example: mask of {@code 0x040B} means DIO0, DIO1, DIO2, DIO3 and DIO10 enabled.
	/// 0 0 0 0 0 1 0 0 0 0 0 0 1 0 1 1
	/// 
	/// Analog Channel Mask
	/// -----------------------------------------------------------------------
	/// Indicates which lines are configured as ADC. Each bit in the analog channel mask corresponds to one ADC line on the module.
	/// 
	/// bit 0 = AD0/DIO0
	/// bit 1 = AD1/DIO1
	/// bit 2 = AD2/DIO2
	/// bit 3 = AD3/DIO3
	/// bit 4 = AD4/DIO4
	/// bit 5 = AD5/DIO5/ASSOC
	/// bit 6 = N/A
	/// bit 7 = Supply Voltage Value
	/// 
	/// Example: mask of {@code 0x03} means AD0, and AD1 enabled.
	/// 0 0 0 0 0 0 1 1
	/// </summary>
	public class IOSample
	{	// Variables.
		private readonly byte[] ioSamplePayload;

		/// <summary>
		/// Gets the HSB of the digital mask.
		/// </summary>
		/// <seealso cref="DigitalLSBMask"/>
		/// <seealso cref="DigitalMask"/>
		public int DigitalHSBMask { get; private set; }

		/// <summary>
		/// Gets the LSB of the digital mask.
		/// </summary>
		/// <seealso cref="DigitalHSBMask"/>
		/// <seealso cref="DigitalMask"/>
		public int DigitalLSBMask { get; private set; }

		/// <summary>
		/// Gets the combined (HSB + LSB) digital mask.
		/// </summary>
		/// <seealso cref="DigitalLSBMask"/>
		/// <seealso cref="DigitalHSBMask"/>
		public int DigitalMask { get; private set; }
		public int AnalogMask { get; private set; }
		private int digitalHSBValues;
		private int digitalLSBValues;
		private int digitalValues;
		private int powerSupplyVoltage;

		private readonly IDictionary<IOLine, int> analogValuesMap = new Dictionary<IOLine, int>();
		private readonly IDictionary<IOLine, IOValue> digitalValuesMap = new Dictionary<IOLine, IOValue>();

		/// <summary>
		/// Instantiates a new object of type <see cref="IOSample"/> with the given IO sample payload.
		/// </summary>
		/// <param name="ioSamplePayload">The payload corresponding to an IO sample.</param>
		/// <exception cref="ArgumentException">if length of <paramref name="ioSamplePayload"/> is lower than 5.</exception>
		/// <exception cref="ArgumentNullException">if <paramref name="ioSamplePayload"/> is null.</exception>
		public IOSample(byte[] ioSamplePayload)
		{
			Contract.Requires<ArgumentNullException>(ioSamplePayload != null, "IO sample payload cannot be null.");
			Contract.Requires<ArgumentException>(ioSamplePayload.Length >= 5, "IO sample payload must be longer than 4.");

			this.ioSamplePayload = ioSamplePayload;
			if (ioSamplePayload.Length % 2 != 0)
				ParseRawIOSample();
			else
				ParseIOSample();
		}

		/// <summary>
		/// Parses the information contained in the IO sample bytes reading the value of each configured DIO and ADC.
		/// </summary>
		private void ParseRawIOSample()
		{
			int dataIndex = 3;

			// Obtain the digital mask.                 // Available digital IOs in 802.15.4
			DigitalHSBMask = ioSamplePayload[1] & 0x01;	// 0 0 0 0 0 0 0 1
			DigitalLSBMask = ioSamplePayload[2] & 0xFF;	// 1 1 1 1 1 1 1 1
			// Combine the masks.
			DigitalMask = (DigitalHSBMask << 8) + DigitalLSBMask;
			// Obtain the analog mask.                                                          // Available analog IOs in 802.15.4
			AnalogMask = ((ioSamplePayload[1] << 8) + (ioSamplePayload[2] & 0xFF)) & 0x7E00;	// 0 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0

			// Read the digital values (if any). There are 9 possible digital lines in 
			// 802.15.4 protocol. The digital mask indicates if there is any digital 
			// line enabled to read its value. If 0, no digital values are received.
			if (DigitalMask > 0)
			{
				// Obtain the digital values.
				digitalHSBValues = ioSamplePayload[3] & 0x7F;
				digitalLSBValues = ioSamplePayload[4] & 0xFF;
				// Combine the values.
				digitalValues = (digitalHSBValues << 8) + digitalLSBValues;

				for (int i = 0; i < 16; i++)
				{
					if (!ByteUtils.IsBitEnabled(DigitalMask, i))
						continue;
					if (ByteUtils.IsBitEnabled(digitalValues, i))
						digitalValuesMap.Add(IOLine.UNKNOWN.GetDIO(i), IOValue.HIGH);
					else
						digitalValuesMap.Add(IOLine.UNKNOWN.GetDIO(i), IOValue.LOW);
				}
				// Increase the data index to read the analog values.
				dataIndex += 2;
			}

			// Read the analog values (if any). There are 6 possible analog lines.
			// The analog mask indicates if there is any analog line enabled to read 
			// its value. If 0, no analog values are received.
			int adcIndex = 9;
			while ((ioSamplePayload.Length - dataIndex) > 1 && adcIndex < 16)
			{
				if (!ByteUtils.IsBitEnabled(AnalogMask, adcIndex))
				{
					adcIndex += 1;
					continue;
				}
				// 802.15.4 protocol does not provide power supply value, so get just the ADC data.
				analogValuesMap.Add(IOLine.UNKNOWN.GetDIO(adcIndex - 9), ((ioSamplePayload[dataIndex] & 0xFF) << 8) + (ioSamplePayload[dataIndex + 1] & 0xFF));
				// Increase the data index to read the next analog values.
				dataIndex += 2;
				adcIndex += 1;
			}
		}

		/// <summary>
		/// Parses the information contained in the IO sample bytes reading the value of each configured DIO and ADC.
		/// </summary>
		private void ParseIOSample()
		{
			int dataIndex = 4;

			// Obtain the digital masks.                // Available digital IOs
			DigitalHSBMask = ioSamplePayload[1] & 0x7F;	// 0 1 1 1 1 1 1 1
			DigitalLSBMask = ioSamplePayload[2] & 0xFF;	// 1 1 1 1 1 1 1 1
			// Combine the masks.
			DigitalMask = (DigitalHSBMask << 8) + DigitalLSBMask;
			// Obtain the analog mask.                  // Available analog IOs
			AnalogMask = ioSamplePayload[3] & 0xBF;		// 1 0 1 1 1 1 1 1

			// Read the digital values (if any). There are 16 possible digital lines.
			// The digital mask indicates if there is any digital line enabled to read 
			// its value. If 0, no digital values are received.
			if (DigitalMask > 0)
			{
				// Obtain the digital values.
				digitalHSBValues = ioSamplePayload[4] & 0x7F;
				digitalLSBValues = ioSamplePayload[5] & 0xFF;
				// Combine the values.
				digitalValues = (digitalHSBValues << 8) + digitalLSBValues;

				for (int i = 0; i < 16; i++)
				{
					if (!ByteUtils.IsBitEnabled(DigitalMask, i))
						continue;
					if (ByteUtils.IsBitEnabled(digitalValues, i))
						digitalValuesMap.Add(IOLine.UNKNOWN.GetDIO(i), IOValue.HIGH);
					else
						digitalValuesMap.Add(IOLine.UNKNOWN.GetDIO(i), IOValue.LOW);
				}
				// Increase the data index to read the analog values.
				dataIndex += 2;
			}

			// Read the analog values (if any). There are 6 possible analog lines.
			// The analog mask indicates if there is any analog line enabled to read 
			// its value. If 0, no analog values are received.
			int adcIndex = 0;
			while ((ioSamplePayload.Length - dataIndex) > 1 && adcIndex < 8)
			{
				if (!ByteUtils.IsBitEnabled(AnalogMask, adcIndex))
				{
					adcIndex += 1;
					continue;
				}
				// When analog index is 7, it means that the analog value corresponds to the power 
				// supply voltage, therefore this value should be stored in a different value.
				if (adcIndex == 7)
					powerSupplyVoltage = ((ioSamplePayload[dataIndex] & 0xFF) << 8) + (ioSamplePayload[dataIndex + 1] & 0xFF);
				else
					analogValuesMap.Add(IOLine.UNKNOWN.GetDIO(adcIndex), ((ioSamplePayload[dataIndex] & 0xFF) << 8) + (ioSamplePayload[dataIndex + 1] & 0xFF));
				// Increase the data index to read the next analog values.
				dataIndex += 2;
				adcIndex += 1;
			}
		}

		/// <summary>
		/// Indicates whether or not the <see cref="IOSample"/> has digital values.
		/// </summary>
		/// <returns>true if there are digital values, false otherwise.</returns>
		public bool HasDigitalValues
		{
			get
			{
				return digitalValuesMap.Count > 0;
			}
		}

		/// <summary>
		/// Indicates whether the IO sample contains a digital value for the specified <paramref name="ioLine"/>
		/// </summary>
		/// <param name="ioLine">The IO line to check if has a digital value.</param>
		/// <returns>true if the specified <paramref name="ioLine"/> has a digital value, false otherwises.</returns>
		public bool HasDigitalValue(IOLine ioLine)
		{
			return digitalValuesMap.ContainsKey(ioLine);
		}

		/// <summary>
		/// Gets the digital values dictionary.
		/// </summary>
		/// <example>To verify if this sample contains a valid digital values, use the method <see cref="HasDigitalValues"/>.
		/// <code>if (ioSample.hasDigitalValues()) {
		///  var values = ioSample.getDigitalValues();
		///  ...
		///  } else {
		///  ...
		///  }
		///  }</code></example>
		///  <seealso cref="GetDigitalValue"/>
		///  <seealso cref="HasDigitalValues"/>
		///  <seealso cref="IOLine"/>
		///  <seealso cref="IOValue"/>
		public IDictionary<IOLine, IOValue> DigitalValues
		{
			get
			{
				return digitalValuesMap;
			}
		}

		/// <summary>
		/// Gets the digital value of the provided IO line.
		/// </summary>
		/// <param name="ioLine">The IO line to get its digital value.</param>
		/// <returns>The IOValue of the specified <paramref name="ioLine"/></returns>
		/// <example>To verify if this sample contains a digital value for the specified <paramref name="ioLine"/>, use the method <see cref="hasDigitalValue"/>.
		/// <code>if (ioSample.hasDigitalValue(IOLine.DIO0_AD0)) {
		/// IOValue value = ioSample.getDigitalValue(IOLine.DIO0_AD0);
		/// ...
		/// } else {
		/// ...
		/// }
		/// }
		/// </code></example>
		/// <seealso cref="GetDigitalValues"/>
		/// <seealso cref="HasDigitalValues"/>
		/// <seealso cref="IOLine"/>
		/// <seealso cref="IOValue"/>
		public IOValue GetDigitalValue(IOLine ioLine)
		{
			if (!digitalValuesMap.ContainsKey(ioLine))
				return IOValue.UNKNOWN;
			return digitalValuesMap[ioLine];
		}

		/// <summary>
		/// Indicates whether the IOSample has analog values.
		/// </summary>
		/// <seealso cref="GetAnalogValue"/>
		/// <seealso cref="AnalogValues"/>
		/// <seealso cref="HasAnalogValue"/>
		/// <seealso cref="IOLine"/>
		public bool HasAnalogValues
		{
			get
			{
				return analogValuesMap.Count > 0;
			}
		}

		/// <summary>
		/// Indicates whether or not the specified <paramref name="ioLine"/> has an analog value.
		/// </summary>
		/// <param name="ioLine">The IO line to check if has an analog value.</param>
		/// <returns>true if the specified <paramref name="ioLine"/> has an analog value, false otherwise.</returns>
		/// <seealso cref="GetAnalogValue"/>
		/// <seealso cref="AnalogValues"/>
		/// <seealso cref="HasAnalogValues"/>
		/// <seealso cref="IOLine"/>
		public bool HasAnalogValue(IOLine ioLine)
		{
			return analogValuesMap.ContainsKey(ioLine);
		}

		/// <summary>
		/// Gets the analog values dictionary.
		/// </summary>
		/// <example>To verify if this sample contains a valid analog values, use the method <paramref name="HasAnalogValues"/>.
		/// <code>if (ioSample.hasAnalogValues()) {
		/// var values = ioSample.AnalogValues();
		/// ...
		/// } else {
		/// ...
		/// }
		/// }</code></example>
		/// <seealso cref="GetAnalogValue"/>
		/// <seealso cref="HasAnalogValue"/>
		/// <seealso cref="HasAnalogValues"/>
		/// <seealso cref="IOLine"/>
		public IDictionary<IOLine, int> AnalogValues
		{
			get
			{
				return analogValuesMap;
			}
		}

		/// <summary>
		/// Gets the analog value of the specified <paramref name="ioLine"/>.
		/// </summary>
		/// <example>To verify if this sample contains an analog value for the specified <paramref name="ioLine"/>, use the method <see cref="HasAnalogValue"/>.
		/// <code>if (ioSample.hasAnalogValue(IOLine.DIO0_AD0)) {
		/// var value = ioSample.GetAnalogValue(IOLine.DIO0_AD0);
		/// ...
		/// } else {
		/// ...
		/// }
		/// }</code></example>
		/// <param name="ioLine">The IO line to get its analog value.</param>
		/// <returns>The analog value of the given IO line.</returns>
		/// <seealso cref="AnalogValues"/>
		/// <seealso cref="HasAnalogValue"/>
		/// <seealso cref="HasAnalogValues"/>
		/// <seealso cref="IOLine"/>
		public int GetAnalogValue(IOLine ioLine)
		{
			if (!analogValuesMap.ContainsKey(ioLine))
				return int.MaxValue;

			return analogValuesMap[ioLine];
		}

		/// <summary>
		/// Indicates whether the IOSample has power supply value.
		/// </summary>
		/// <seealso cref="PowerSupplyValue"/>
		public bool HasPowerSupplyValue
		{
			get
			{
				return ByteUtils.IsBitEnabled(AnalogMask, 7);
			}
		}

		/// <summary>
		/// Gets the value of the power supply voltage.
		/// </summary>
		/// <example>To verify if this sample contains the power supply voltage, use the method <paramref name="HasPowerSupplyValue"/>.
		/// <code>if (ioSample.hasPowerSupplyValue()) {
		/// int value = ioSample.PowerSupplyValue;
		/// ...
		/// } else {
		/// ...
		/// }
		/// }</code></example>
		/// <exception cref="OperationNotSupportedException" if the IOSample does not have power supply value.
		/// <seealso cref="HasPowerSupplyValue"/>
		public int PowerSupplyValue
		{
			get
			{
				if (!ByteUtils.IsBitEnabled(AnalogMask, 7))
					throw new OperationNotSupportedException();
				return powerSupplyVoltage;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("{");
			if (HasDigitalValues)
			{
				foreach (IOLine line in digitalValuesMap.Keys)
				{
					sb.Append("[").Append(line).Append(": ").Append(digitalValuesMap[line]).Append("], ");
				}
			}
			if (HasAnalogValues)
			{
				foreach (IOLine line in analogValuesMap.Keys)
				{
					sb.Append("[").Append(line).Append(": ").Append(analogValuesMap[line]).Append("], ");
				}
			}
			if (HasPowerSupplyValue)
			{
				try
				{
					sb.Append("[").Append("Power supply voltage: ").Append(PowerSupplyValue).Append("], ");
				}
				catch (OperationNotSupportedException ) { }
			}

			String s = sb.ToString();
			if (s.EndsWith(", "))
				s = s.Substring(0, s.Length - 2);
			return s + "}";
		}
	}
}
