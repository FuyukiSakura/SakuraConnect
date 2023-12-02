using System.Net.Sockets;
using Sakura.Live.Osc.Core.Events;
using Sakura.Live.Osc.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Osc.Core.Services
{
    /// <summary>
    /// Listens to OSC UDP events
    /// </summary>
    public class OscReceiverService : BasicAutoStartable
    {
        // Dependencies
        readonly IPandaMessenger _messenger;
        readonly ISettingsService _settingsSvc;

        /// <summary>
        /// Sets the port number the service will listen to
        /// </summary>
        // TODO: Restarts service if port changed
	    public int Port { get; set; } = 39550;

        /// <summary>
        /// Creates a new instance of <see cref="OscReceiverService" />
        /// </summary>
        public OscReceiverService(ISettingsService settingsSvc, IPandaMessenger messenger)
        {
            _settingsSvc = settingsSvc;
            LoadSettings();
            _messenger = messenger;
        }

        CancellationTokenSource? _stopListeningToken;

        /// <summary>
        /// Starts listening and duplicating OSC requests
        /// </summary>
        /// <param name="listenPort"></param>
        /// <returns></returns>
        public async Task StartAsync(int listenPort)
        {
            var listener = new UdpClient(listenPort);
            _stopListeningToken = new CancellationTokenSource();
            _ = HeartBeatAsync();

            try
            {
                while (!_stopListeningToken.IsCancellationRequested)
                {
                    var result = await listener.ReceiveAsync(_stopListeningToken.Token);
                    _messenger.Send(new OscEventArgs
                    {
                        OscData = result.Buffer,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            finally
            {
                listener.Close();
                Status = ServiceStatus.Stopped;
            }
        }

        /// <summary>
        /// Checks if the Udp client is connected
        /// </summary>
        /// <returns></returns>
        async Task HeartBeatAsync()
        {
            Status = ServiceStatus.Running;
            while (Status == ServiceStatus.Running)
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default);
            }
        }

        ///
        /// <inheritdoc cref="StartAsync(int)"/>
        ///
        public override async Task StartAsync()
        {
            SaveSettings();
            _ = StartAsync(Port);
            await base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StopAsync()
        {
            _stopListeningToken?.Cancel();
            await base.StopAsync();
        }

        /// <summary>
        /// Saves settings to the system
        /// </summary>
        void SaveSettings()
        {
            _settingsSvc.Set(VmcPreferenceKeys.Port, Port.ToString());
        }

        /// <summary>
        /// Loads settings from the system
        /// </summary>
        void LoadSettings()
        {
            Port = int.Parse(_settingsSvc.Get(VmcPreferenceKeys.Port, "39550"));
        }
    }
}