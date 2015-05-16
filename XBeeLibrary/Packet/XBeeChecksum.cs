namespace Kveer.XBeeApi.Packet
{
	/**
	 * This class stores, computes and verifies the checksum of the API packets.
	 * 
	 * <p>To test data integrity, a checksum is calculated and verified on 
	 * non-escaped API data.</p>
	 * 
	 * <p><b>To calculate</b></p>
	 * 
	 * <p>Not including frame delimiters and Length, add all bytes keeping only the 
	 * lowest 8 bits of the result and subtract the result from {@code 0xFF}.</p>
	 * 
	 * <p><b>To verify</b></p>
	 * 
	 * <p>Add all bytes (include checksum, but not the delimiter and Length). If the 
	 * checksum is correct, the sum will equal {@code 0xFF}.</p>
	 */
	public class XBeeChecksum
	{
		// Variables.
		private int value = 0;

		/// <summary>
		/// Adds the given byte to the checksum.
		/// </summary>
		/// <param name="value">Byte to add.</param>
		public void Add(int value)
		{
			this.value += value;
		}
		/// <summary>
		/// Adds the given data to the checksum.
		/// </summary>
		/// <param name="data">Byte array to add.</param>
		public void Add(byte[] data)
		{
			if (data == null)
				return;
			for (int i = 0; i < data.Length; i++)
				Add(data[i]);
		}

		/// <summary>
		/// Resets the checksum.
		/// </summary>
		public void Reset()
		{
			value = 0;
		}

		/// <summary>
		/// Generates the checksum byte for the API packet.
		/// </summary>
		/// <returns>Checksum byte.</returns>
		public byte Generate()
		{
			value = value & 0xFF;
			return (byte)(0xFF - value);
		}

		/// <summary>
		/// Validates the checksum.
		/// </summary>
		/// <returns>true if checksum is valid, false otherwise.</returns>
		public bool Validate()
		{
			value = value & 0xFF;
			return value == 0xFF;
		}
	}
}