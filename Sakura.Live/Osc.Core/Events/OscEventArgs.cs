namespace Sakura.Live.Osc.Core.Events
{
	/// <summary>
	/// Represents an OSC event data
	/// </summary>
	public class OscEventArgs
	{
		/// <summary>
		/// The unique id of the connection
		/// </summary>
		public string Id { get; set; } = "";

		/// <summary>
		/// The osc data
		/// </summary>
		public byte[] OscData { get; set; } = new byte[4];

		/// <summary>
		/// When the Osc data was created
		/// </summary>
		public DateTime CreatedAt { get; set; } = DateTime.MinValue;
	}
}
