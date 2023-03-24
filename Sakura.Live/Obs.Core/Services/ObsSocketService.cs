using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using Sakura.Live.Obs.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Obs.Core.Services
{
    public class ObsSocketService : BasicAutoStartable
    {
        readonly OBSWebsocket _obs = new ();

        ///
        /// <inheritdoc cref="OBSWebsocket.Connected"/>
        ///
        public EventHandler Connected;

        ///
        /// <inheritdoc cref="OBSWebsocket.Disconnected"/>
        ///
        public EventHandler Disconnected;

        /// <summary>
        /// Creates a new instance of <see cref="ObsSocketService" />
        /// </summary>
        public ObsSocketService()
        {
            _obs.Connected += (sender, args) => Connected?.Invoke(sender, args);
            _obs.Disconnected += (sender, args) =>
            {
                Disconnected?.Invoke(sender, EventArgs.Empty);
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
            var settings = ObsWebSocketConnectionInput.Saved;
            ConnectAsync(settings.ConnectionString, settings.Password);
            if (_obs.IsConnected)
            {
                Status = ServiceStatus.Running;
                await base.StartAsync();
                await HeartBeatAsync();
            }
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
        async Task HeartBeatAsync()
        {
            while (_obs.IsConnected)
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default);
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