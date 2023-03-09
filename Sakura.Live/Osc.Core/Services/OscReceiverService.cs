using System.Net.Sockets;
using Sakura.Live.Osc.Core.Events;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Osc.Core.Services
{
    /// <summary>
    /// Listens to OSC UDP events
    /// </summary>
    public class OscReceiverService : BasicAutoStartable
    {
        /// <summary>
        /// Sets the port number the service will listen to
        /// </summary>
        // TODO: Restarts service if port changed
	    public int Port { get; set; } = 39550;

        CancellationTokenSource? _stopListeningToken;

        /// <summary>
        /// Is triggered when a OSC buffer is received
        /// </summary>
        public event EventHandler<OscEventArgs>? OscReceived;

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
                    OscReceived?.Invoke(this, new OscEventArgs
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
    }
}