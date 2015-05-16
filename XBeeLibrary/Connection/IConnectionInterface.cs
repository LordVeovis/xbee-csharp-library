using System.IO;

namespace Kveer.XBeeApi.Connection
{
	/// <summary>
	/// This interface represents a protocol independent connection with an XBee device.
	/// </summary>
	/// <remarks>As an important point, the class implementing this interface must call <code>this.Notify()</code> whenever new data is available to read. Not doing this will make the <code>DataReader</code> class to wait forever for new data.</remarks>
	public interface IConnectionInterface
	{
		/// <summary>
		/// Attempts to open the connection interface.
		/// </summary>
		/// <exception cref="InterfaceInUseException">if the interface is in use by other application(s).</exception>
		/// <exception cref="InvalidConfigurationException">if the configuration used to open the interface is invalid.</exception>
		/// <exception cref="InvalidInterfaceException">if the interface is invalid or does not exist.</exception>
		/// <exception cref="PermissionDeniedException">if you do not have permissions to  access the interface.</exception>
		/// <seealso cref="Close"/>
		/// <seealso cref="IsOpen"/>
		void Open();

		/// <summary>
		/// Attempts to close the connection interface.
		/// </summary>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="Open"/>
		void Close();

		/// <summary>
		/// Indicates whether the connection interface is open.
		/// </summary>
		/// <seealso cref="Close"/>
		/// <seealso cref="Open"/>
		bool IsOpen { get; }

		/// <summary>
		/// Gets the connection interface input stream to read data from.
		/// </summary>
		/// <returns>The connection interface input stream to read data from.</returns>
		/// <seealso cref="GetOutputStream"/>
		Stream GetInputStream();

		/// <summary>
		/// Gets the connection interface output stream to write data to.
		/// </summary>
		/// <returns>The connection interface output stream to write data to.</returns>
		/// <seealso cref="GetInputStream"/>
		Stream GetOutputStream();

		/// <summary>
		/// Writes the given data in the connection interface.
		/// </summary>
		/// <param name="data">The data to be written in the connection interface.</param>
		/// <exception cref="IOException">if there is any problem writing to the output stream.</exception>
		/// <exception cref="ArgumentNullException">if <paramref name="data"/> is null.</exception>
		/// <seealso cref="WriteData(byte[],int,int)"/>
		void WriteData(byte[] data);

		/// <summary>
		/// Writes the given data in the connection interface.
		/// </summary>
		/// <param name="data">The data to be written in the connection interface.</param>
		/// <param name="offset">The start offset in the data to write.</param>
		/// <param name="Length">The number of bytes to write.</param>
		/// <exception cref="ArgumentOutOfRangeException">if <paramref name="offset"/> is netative or <paramref name="Length"/> is not strictly positive or <paramref name="offset"/> is outside of <paramref name="data"/>'s length or <paramref name="offset"/> + <paramref name="Length"/> is outside of <paramref name="data"/>'s length.</exception>
		/// <exception cref="IOException">if there is any problem writing to the output stream.</exception>
		/// <exception cref="ArgumentNullException">if <paramref name="data"/> is null.</exception>
		/// <seealso cref="WriteData(byte[],int,int)"/>
		void WriteData(byte[] data, int offset, int Length);

		/// <summary>
		/// Reads data from the connection interface and stores it in the specified <paramref name="data"/> array returning the number of read bytes.
		/// </summary>
		/// <param name="data">The byte array to store the read data.</param>
		/// <returns>The number of bytes read.</returns>
		/// <exception cref="IOException">if there is any problem reading from the input stream.</exception>
		/// <exception cref="ArgumentNullException">if <paramref name="data"/> is null.</exception>
		int ReadData(byte[] data);

		/// <summary>
		/// Reads the specified <paramref name="length"/> number of bytes at the specified <paramref name="offset"/> from the connection interface and stores it in the specified <paramref name="data"/> array returning the number of read bytes.
		/// </summary>
		/// <param name="data">The byte array to store the read data.</param>
		/// <param name="offset">The start offset in data array at which the data is written.</param>
		/// <param name="length">Maximum number of bytes to read.</param>
		/// <returns>The number of bytes read.</returns>
		/// <exception cref="ArgumentOutOfRangeException">if <paramref name="offset"/> is netative or <paramref name="length"/> is not strictly positive or <paramref name="offset"/> is outside of <paramref name="data"/>'s length or <paramref name="offset"/> + <paramref name="length"/> is outside of <paramref name="data"/>'s length.</exception>
		/// <exception cref="IOException">if there is any problem reading from the input stream.</exception>
		/// <exception cref="ArgumentNullException">if <paramref name="data"/> is null.</exception>
		int ReadData(byte[] data, int offset, int length);
	}
}