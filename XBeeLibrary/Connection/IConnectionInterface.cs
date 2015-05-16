using System.IO;
using System.IO.Ports;

namespace Kveer.XBeeApi.Connection
{
	/// <summary>
	/// This interface represents a protocol independent connection with an XBee device.
	/// </summary>
	/// <remarks>As an important point, the class implementing this interface must call <code>this.Notify()</code> whenever new data is available to read. Not doing this will make the <code>DataReader</code> class to wait forever for new data.</remarks>
	public interface IConnectionInterface
	{
		SerialPort SerialPort { get; }
	}
}