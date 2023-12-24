using System.Diagnostics;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;
using Sakura.Live.Twitch.Core.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Exceptions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;

namespace Sakura.Live.Twitch.Core.Services
{
    /// <summary>
    /// Accesses the Twitch client
    /// </summary>
    public class TwitchChatService : BasicAutoStartable
    {
        readonly ISettingsService _settingsService;
        readonly IPandaMessenger _messenger;
        TwitchClient? _client;

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
        /// Creates a new instance of <see cref="TwitchChatService" />
        /// </summary>
        public TwitchChatService(ISettingsService settings, IPandaMessenger messenger)
        {
            _settingsService = settings;
            _messenger = messenger;
            LoadSettings();
        }

        /// <summary>
        /// Saves the Twitch settings to the system
        /// </summary>
        void SaveSettings()
        {
            _settingsService.Set(TwitchPreferenceKeys.Username, Username);
            _settingsService.Set(TwitchPreferenceKeys.AccessToken, AccessToken);
            _settingsService.Set(TwitchPreferenceKeys.Channel, Channel);
        }

        /// <summary>
        /// Loads the Twitch settings from the system
        /// </summary>
        void LoadSettings()
        {
            Username = _settingsService.Get(TwitchPreferenceKeys.Username, "");
            AccessToken = _settingsService.Get(TwitchPreferenceKeys.AccessToken, "");
            Channel = _settingsService.Get(TwitchPreferenceKeys.Channel, "");
        }

        /// <summary>
        /// Sends a message to the channel
        /// </summary>
        /// <param name="message"></param>
        public async Task SendMessage(string message)
        {
            if (_client == null)
            {
                // Service not started
                return;
            }

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
        async Task ReconnectAsync(CancellationToken cancellationToken)
        {
            while (Status == ServiceStatus.Running 
                   || !cancellationToken.IsCancellationRequested) // Checks if the client is connected
            {
                await Task.Delay(HeartBeat.Default, CancellationToken.None);
                if (_client!.JoinedChannels.Count > 0)
                {
                    continue;
                }

                try
                {
                    await _client.JoinChannelAsync(Channel, true);
                    Debug.WriteLine("Twitch client reconnected");
                }
                catch (ClientNotConnectedException e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Starts the service
        /// </summary>
        /// <param name="username">Twitch Username</param>
        /// <param name="accessToken">Twitch Access token</param>
        /// <param name="channel">Name of the channel to monitor</param>
        async Task Start(string username, string accessToken, string channel)
        {
            var credentials = new ConnectionCredentials(username, accessToken);
            var customClient = new WebSocketClient();
            _client = new TwitchClient(customClient);
            _client.OnMessageReceived += TwitchMessageReceived;
            _client.Initialize(credentials, channel);
            await _client.ConnectAsync();
        }

        /// <summary>
        /// Sends the received message to the subscribers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task TwitchMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            _messenger.Send(e);
            return Task.CompletedTask;
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            await base.StartAsync();
            SaveSettings();
            await Start(Username, AccessToken, Channel);
            _ = ReconnectAsync(CancellationTokenSource.Token);
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StopAsync()
        {
            await base.StopAsync();
            if (_client == null)
            {
                // Service not started
                return;
            }

            await _client.DisconnectAsync();
            _client.OnMessageReceived -= TwitchMessageReceived;
        }
    }
}
