using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.Connect.Dreamer.Models.OneComme;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services
{
    /// <summary>
    /// Reads chat from OneComme
    /// </summary>
    public class OneCommeService(IPandaMessenger messenger) : BasicAutoStartable
    {
        const string Url = "ws://127.0.0.1:11180/sub";
        bool _isReconnecting;
        readonly WebSocket _socket = new (Url, false);
        readonly SemaphoreSlim _socketLock = new (1, 1);

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            await _socketLock.WaitAsync();
            try
            {
                _socket.Closed += Reconnect_OnClosed;
                _socket.MessageReceived += Socket_OnMessageReceived;
                await _socket.ConnectAsync();
            }
            finally
            {
            
                _socketLock.Release();
            }
            await base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StopAsync()
        {
            await _socketLock.WaitAsync();
            try
            {
                _socket.Closed -= Reconnect_OnClosed;
                _socket.MessageReceived -= Socket_OnMessageReceived;
                _socket.Close();
            }
            finally
            {
                _socketLock.Release();
            }
            await base.StopAsync();
        }

        ///
        /// <inheritdoc />
        ///
        protected override async Task HeartBeatAsync(CancellationToken token)
        {
            while (_socket.IsConnected
                   && !token.IsCancellationRequested) // Checks if the client is connected
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default);
            }

            if (!token.IsCancellationRequested)
            {
                Status = ServiceStatus.Error;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Socket_OnMessageReceived(object? sender, string e)
        {
            var msg = OneCommeSubscriptionMessage.Serialize(e);
            if (msg == null) return; // Cannot parse result, listen for next event

            if (msg.Type != OneCommeSubscriptionMessage.Comments)
            {
                // Ignore other message types
                return;
            }

            var comments = msg.Data.Comments
                .Select(comment => new CommentData
                {
                    Id = comment.Id,
                    Comment = comment.Data.Comment,
                    Username = comment.Data.Name
                }).ToList();
            messenger.Send(new CommentReceivedEventArg
            {
                Comments = comments
            });
        }

        /// <summary>
        /// Attempts to reconnect the socket when closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void Reconnect_OnClosed(object sender, string e)
        {
            // If the socket is already connected or a reconnection attempt is in progress, do nothing
            if (_socket.IsConnected || _isReconnecting)
            {
                return;
            }

            _isReconnecting = true;
            Status = ServiceStatus.Error;
            await _socket.ReconnectAsync();
            _isReconnecting = false;
        }
    }
}
