using System.Net;
using System.Text.Json.Serialization;

namespace Sakura.Live.Osc.Core.Settings
{
	/// <summary>
	/// Contains the data of where the OSC requests should send
	/// </summary>
	public class OscSender
    {
        string _ipAddress = IPAddress.Loopback.ToString();
        int _port = IPEndPoint.MaxPort;
		
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
		public string IpAddress
        {
            get => _ipAddress;
            set
            {
                if (_ipAddress == value) return;

                _ipAddress = value;
                EndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
            }
        }

		/// <summary>
		/// Gets or sets the port of the Osc target
		/// </summary>
		public int Port
        {
            get => _port;
            set
            {
                if (_port == value) return;

                _port = value;
				EndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
            }
        }

		/// <summary>
		/// Gets the <see cref="IPEndPoint" /> of the osc sender
		/// </summary>
		[JsonIgnore]
		public IPEndPoint EndPoint { get; private set; } = new (IPAddress.Loopback, IPEndPoint.MaxPort);

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
