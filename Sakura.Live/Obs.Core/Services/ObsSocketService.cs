using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using Sakura.Live.Obs.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Obs.Core.Services
{
    /// <summary>
    /// Accesses OBS websocket v5
    /// </summary>
    public class ObsSocketService : BasicAutoStartable
    {
        readonly OBSWebsocket _obs = new ();
        readonly ISettingsService _settingsServiceService;
        bool _isConnected;

        /// <summary>
        /// Gets or sets the OBS websocket settings
        /// </summary>
        public ObsWebSocketConnectionInput ObsWsSettings { get; set; } = new ();

        ///
        /// <inheritdoc cref="OBSWebsocket.Connected"/>
        ///
        public EventHandler? Connected;

        ///
        /// <inheritdoc cref="OBSWebsocket.Disconnected"/>
        ///
        public EventHandler? Disconnected;

        /// <summary>
        /// Creates a new instance of <see cref="ObsSocketService" />
        /// </summary>
        public ObsSocketService(ISettingsService settingService)
        {
            _settingsServiceService = settingService;
            _obs.Connected += (sender, args) => Connected?.Invoke(sender, args);
            _obs.Disconnected += (sender, args) =>
            {
                _isConnected = false;
                Disconnected?.Invoke(sender, EventArgs.Empty);
            };
            LoadSettings();
        }

        /// <summary>
        /// Saves the OBS websocket settings to the system
        /// </summary>
        public void SaveSettings()
        {
            _settingsServiceService.Set(ObsPreferenceKeys.Url, ObsWsSettings.Url);
            _settingsServiceService.Set(ObsPreferenceKeys.Port, ObsWsSettings.Port);
            _settingsServiceService.Set(ObsPreferenceKeys.Password, ObsWsSettings.Password);
        }

        /// <summary>
        /// Loads the OBS websocket settings from the system
        /// </summary>
        public void LoadSettings()
        {
            ObsWsSettings = new ObsWebSocketConnectionInput
            {
                Url = _settingsServiceService.Get(ObsPreferenceKeys.Url, "127.0.0.1"),
                Port = _settingsServiceService.Get(ObsPreferenceKeys.Port, "4455"),
                Password = _settingsServiceService.Get(ObsPreferenceKeys.Password, "")
            };
        }

        /// <summary>
        /// Connects to the OBS server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="password"></param>
        public void ConnectAsync(string url, string password)
        {
            Disconnect();
            _obs.ConnectAsync(url, password);
        }

        /// <summary>
        /// Disconnects from OBS web socket
        /// </summary>
        public void Disconnect()
        {
            _obs.Disconnect();
        }

        /// <summary>
        /// Starts the OBS socket connection with default configured value
        /// </summary>
        /// <returns></returns>
        public override async Task StartAsync()
        {
            SaveSettings();
            ConnectAsync(ObsWsSettings.ConnectionString, ObsWsSettings.Password);
            
            _isConnected = true;
            Status = ServiceStatus.Running;
            await base.StartAsync();
        }

        /// <summary>
        /// Disconnects the OBS socket
        /// </summary>
        /// <returns></returns>
        public override async Task StopAsync()
        {
            Disconnect();
            await base.StopAsync();
        }

        /// <summary>
        /// Updates the heart beat timer when the obs connection is still connected
        /// </summary>
        /// <returns></returns>
        protected override async Task HeartBeatAsync(CancellationToken token)
        {
            while (_isConnected
                   && !token.IsCancellationRequested)
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default, token);
            }
        }

        /// <summary>
        /// Changes text of the Text(GDI) element
        /// and copy all other properties
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="target">The name of the OBS source</param>
        public void SetText(string text, string target)
        {
            if (!_obs.IsConnected)
            {
                // Socket not connected, will throw exception if tries to access
                return;
            }

            try
            {
                var settings = _obs.GetInputSettings(target);
                settings.Settings["text"] = text;
                _obs.SetInputSettings(new InputSettings
                {
                    InputKind = "text",
                    InputName = target,
                    Settings = settings.Settings
                });
            }
            catch (ErrorResponseException)
            {
                // Specified source may not exist
            }
        }
    }
}