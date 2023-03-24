namespace Sakura.Live.Obs.Core.Models
{
    /// <summary>
    /// Presents all data required to start an <see cref="OBSWebsocketDotNet"/> connection
    /// </summary>
    public class ObsWebSocketConnectionInput
    {
        /// <summary>
        /// Gets or sets the url of the connection
        /// </summary>
        public string Url { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets the port of the connection
        /// </summary>
        public string Port { get; set; } = "4455";

        /// <summary>
        /// Gets or sets the password of the connection
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// Gets the connection string of the web socket connection
        /// </summary>
        public string ConnectionString => $"ws://{Url}:{Port}";

        /// <summary>
        /// Gets the saved OBS connection from settings
        /// </summary>
        public static ObsWebSocketConnectionInput Saved => new()
        {
            Url = "127.0.0.1",
            Port = "4455",
            Password = "Slimness-Studio-Winter-Jittery6-Finlike"
        };
    }
}
