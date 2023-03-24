using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace Sakura.Live.Twitch.Core.Services
{
    /// <summary>
    /// Accesses the Twitch client
    /// </summary>
    public class TwitchChatService : BasicAutoStartable
    {
        readonly TwitchClient _client;

        /// <summary>
        /// Gets or sets the Twitch username for login
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// Gets or sets the Twitch access token for login
        /// </summary>
        public string AccessToken { get; set; } = "";

        /// <summary>
        /// Gets or sets the Twitch channel
        /// </summary>
        public string Channel { get; set; } = "";

        /// <summary>
        /// Is triggered when a message is received
        /// </summary>
        public event EventHandler<OnMessageReceivedArgs> OnMessageReceived
        {
            add => _client.OnMessageReceived += value;
            remove => _client.OnMessageReceived -= value;
        }

        /// <summary>
        /// Is triggered when a whisper is received
        /// </summary>
        public event EventHandler<OnWhisperReceivedArgs> OnWhisperReceived
        {
            add => _client.OnWhisperReceived += value;
            remove => _client.OnWhisperReceived -= value;
        }

        /// <summary>
        /// Creates a new instance of <see cref="TwitchChatService" />
        /// </summary>
        public TwitchChatService()
        {
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.OnDisconnected += Twitch_OnDisconnected;
        }

        /// <summary>
        /// Sends a message to the channel
        /// </summary>
        /// <param name="message"></param>
        public async Task SendMessage(string message)
        {
            var retries = 0;
            while ((!_client.IsConnected 
                   || Status != ServiceStatus.Running)
                   && retries < 5) // Prevents flooding of message after disconnection event
            {
                // Waits until the client is connected to prevent app crash
                await Task.Delay(HeartBeat.Default);
                ++retries;
            }
            _client.SendMessage(Channel, message);
        }

        /// <summary>
        /// Checks if the Twitch client is still connected
        /// </summary>
        /// <returns></returns>
        async Task HeartBeatAsync()
        {
            Status = ServiceStatus.Running;
            while (Status == ServiceStatus.Running 
                   && _client.IsConnected) // Checks if the client is connected
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default);
            }
        }

        /// <summary>
        /// Starts the service
        /// </summary>
        /// <param name="username">Twitch Username</param>
        /// <param name="accessToken">Twitch Access token</param>
        /// <param name="channel">Name of the channel to monitor</param>
        void Start(string username, string accessToken, string channel)
        {
            var credentials = new ConnectionCredentials(username, accessToken);
            _client.Initialize(credentials, channel);
            _client.Connect();
            Status = ServiceStatus.Running;
            _ = HeartBeatAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StartAsync()
        {
            Start(Username, AccessToken, Channel);
            return base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _client.Disconnect();
            return base.StopAsync();
        }

        /// <summary>
        /// Change status when client is disconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Twitch_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            Status = ServiceStatus.Error;
        }
    }
}
