﻿using Sakura.Live.Osc.Core.Events;
using System.Net.Sockets;
using System.Text.Json;
using Sakura.Live.Osc.Core.Models;
using Sakura.Live.Osc.Core.Settings;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Osc.Core.Services
{
	/// <summary>
    /// Duplicates Osc broadcasts
    /// </summary>
    public class OscDuplicateService
    {
        /// <summary>
        /// Gets or sets a list of senders the duplication service
        /// should send to
        /// </summary>
        public List<OscSender> Senders { get; } = new();
        
        readonly Socket _socket = new (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Dependencies
        readonly ISettingsService _settingsSvc;
        readonly IThePandaMonitor _monitorSvc;
        readonly IPandaMessenger _messenger;

        /// <summary>
        /// Creates a new instance of <see cref="OscDuplicateService" />
        /// </summary>
        public OscDuplicateService(
            ISettingsService settingsSvc,
	        IThePandaMonitor monitorSvc,
            IPandaMessenger messenger)
        {
            _settingsSvc = settingsSvc;
            _monitorSvc = monitorSvc;
            _messenger = messenger;
            LoadSettings();
        }

        /// <summary>
        /// Saves settings to the system
        /// </summary>
        void SaveSettings()
        {
            var jsonSettingString = JsonSerializer.Serialize(Senders);
            _settingsSvc.Set(VmcPreferenceKeys.Duplicators, jsonSettingString);
        }

        /// <summary>
        /// Loads settings from the system
        /// </summary>
        void LoadSettings()
        {
            var jsonSettingString = _settingsSvc.Get(VmcPreferenceKeys.Duplicators,
                OscSender.Default);
            var settings = JsonSerializer
                .Deserialize<OscSender[]>(jsonSettingString);
            if (settings == null)
            {
                return;
            }
            Senders.AddRange(settings);
        }

        /// <summary>
        /// Starts listening and duplicating OSC requests
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync() 
        {
            SaveSettings();
            _messenger.Register<OscEventArgs>(this, ReceiverService_OnOscReceived);
            _monitorSvc.Register<OscReceiverService>(this);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles Osc received event
        /// </summary>
        /// <param name="e"></param>
        void ReceiverService_OnOscReceived(OscEventArgs e)
        {
            foreach (var oscSender in Senders)
            {
                _ = Task.Run(() => _socket.SendTo(e.OscData, oscSender.EndPoint));
            }
        }

        /// <summary>
        /// Stops listening to VMC port
        /// </summary>
        public void Stop()
        {
            _messenger.UnregisterAll(this);
            _monitorSvc.UnregisterAll(this);
        }
    }
}
