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
    public class OneCommeService : BasicAutoStartable
    {
        // Dependencies
        readonly IPandaMessenger _messenger;

        const string Url = "ws://127.0.0.1:11180/sub";
        WebSocket _socket = new (Url, false);

        /// <summary>
        /// Creates a new instance of <see cref="OneCommeService" />
        /// </summary>
        public OneCommeService(IPandaMessenger messenger)
        {
            _messenger = messenger;
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            _socket.Closed += Reconnect_OnClosed;
            _socket.MessageReceived += Socket_OnMessageReceived;
            await _socket.ConnectAsync();
            await base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _socket.Closed -= Reconnect_OnClosed;
            _socket.MessageReceived -= Socket_OnMessageReceived;
            _socket.Close();
            return base.StopAsync();
        }

        ///
        /// <inheritdoc />
        ///
        protected override async Task HeartBeatAsync()
        {
            Status = ServiceStatus.Running;
            while (_socket.IsConnected) // Checks if the client is connected
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default);
            }
            Status = ServiceStatus.Error;
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

            if (msg.Type == OneCommeSubscriptionMessage.Comments)
            {
                var comments = msg.Data.Comments
                    .Select(comment => new CommentData
                    {
                        Id = comment.Id,
                        Comment = comment.Data.Comment,
                        Username = comment.Data.Name
                    }).ToList();
                _messenger.Send(new CommentReceivedEventArg
                {
                    Comments = comments
                });
            }
        }

        /// <summary>
        /// Attempts to reconnect the socket when closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void Reconnect_OnClosed(object sender, string e)
        {
            Status = ServiceStatus.Error;
            await _socket.ReconnectAsync();
        }
    }
}
