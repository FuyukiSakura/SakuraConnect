using Sakura.Live.Osc.Core.Events;
using System.Net.Sockets;
using Sakura.Live.Osc.Core.Settings;
using Sakura.Live.ThePanda.Core;

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
        public List<OscSender> Senders { get; set; } = new();
        
        readonly Socket _socket = new (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        readonly IThePandaMonitor _monitorSvc;
        readonly OscReceiverService _receiverService;

        /// <summary>
        /// Creates a new instance of <see cref="OscDuplicateService" />
        /// </summary>
        /// <param name="monitorSvc"></param>
        /// <param name="receiverSvc"></param>
        public OscDuplicateService(
	        IThePandaMonitor monitorSvc,
            OscReceiverService receiverSvc)
        {
            _monitorSvc = monitorSvc;
            _receiverService = receiverSvc;
        }

        /// <summary>
        /// Starts listening and duplicating OSC requests
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync() 
        {
            _receiverService.OscReceived += ReceiverService_OnOscReceived;
            _monitorSvc.Register(this, _receiverService);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles Osc received event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ReceiverService_OnOscReceived(object? sender, OscEventArgs e)
        {
            var duplicateEndpoints = Senders.Select(s => s.EndPoint)
                .ToArray();
            foreach (var ipEndPoint in duplicateEndpoints)
            {
                _ = Task.Run(() => _socket.SendTo(e.OscData, ipEndPoint));
            }
        }

        /// <summary>
        /// Stops listening to VMC port
        /// </summary>
        public void Stop()
        {
            _receiverService.OscReceived -= ReceiverService_OnOscReceived;
            _monitorSvc.Unregister(this);
        }
    }
}
