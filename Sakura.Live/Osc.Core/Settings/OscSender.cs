using System.Net;
using System.Text.Json.Serialization;

namespace Sakura.Live.Osc.Core.Settings
{
	/// <summary>
	/// Contains the data of where the OSC requests should send
	/// </summary>
	public class OscSender
	{
		/// <summary>
		/// Gets or sets the unique id of the Osc sender
		/// mainly for use for identifying connections from ConnectU server
		/// </summary>
		public string Id { get; set; } = "";

		/// <summary>
		/// Gets or sets the name of the Osc sender server
		/// </summary>
		public string Name { get; set; } = "";

		/// <summary>
		/// Gets or sets the ip address of the Osc target
		/// </summary>
		public string IpAddress { get; set; } = "";

		/// <summary>
		/// Gets or sets the port of the Osc target
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// Gets the <see cref="IPEndPoint" /> of the osc sender
		/// </summary>
		[JsonIgnore]
		public IPEndPoint EndPoint => new (IPAddress.Parse(IpAddress), Port);

		/// <summary>
		/// Gets my own Id for sender settings
		/// </summary>
		public static string MyId = "me";

		/// <summary>
		/// Gets the default settings string as json
		/// </summary>
		public static string Default =
			"[{\"Name\":\"Beat Saber\",\"IpAddress\":\"127.0.0.1\",\"Port\":39539},{\"Name\":\"VTuber Plus\",\"IpAddress\":\"127.0.0.1\",\"Port\":39551}]";
	}
}
