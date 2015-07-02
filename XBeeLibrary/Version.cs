namespace Kveer.XBeeApi
{
	/// <summary>
	/// Class used to retrieve the current version of the XBee Java Library.
	/// </summary>
	public class Version
	{
		// Constants.
		public static string CURRENT_VERSION = typeof(Version).Assembly.GetName().Version.ToString();

		/// <summary>
		/// Gets the current version of the XBee C# Library
		/// </summary>
		public static string CurrentVersion
		{
			get
			{
				return CURRENT_VERSION;
			}
		}
	}
}