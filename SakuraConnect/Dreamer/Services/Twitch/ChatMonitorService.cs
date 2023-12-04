using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services.Twitch
{
    /// <summary>
    /// Monitors the twitch chat and log the chat history
    /// </summary>
    public class ChatMonitorService : BasicAutoStartable
    {
        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly IPandaMessenger _messenger;
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Creates a new instance of <see cref="ChatMonitorService" />
        /// </summary>
        public ChatMonitorService(
            IThePandaMonitor monitor,
            IPandaMessenger messenger,
            ChatHistoryService chatHistoryService)
        {
            _monitor = monitor;
            _messenger = messenger;
            _chatHistoryService = chatHistoryService;
        }

        /// <summary>
        /// Adds a chat message to the history when twitch chat message received
        /// </summary>
        /// <param name="e"></param>
        void OnMessageReceived(CommentReceivedEventArg e)
        {
            foreach (var chat in from data in e.Comments 
                     where !data.Comment.StartsWith("!") 
                     select ChatMessage.FromUser($"{data.Username}: {data.Comment}"))
            {
                _ = ChatLogger.LogAsync(chat.Content, "chat");
                _chatHistoryService.AddChat(chat);
            }
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StartAsync()
        {
            _messenger.Register<CommentReceivedEventArg>(this, OnMessageReceived);
            _monitor.Register<OneCommeService>(this);
            return base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _monitor.UnregisterAll(this);
            _messenger.UnregisterAll(this);
            return base.StopAsync();
        }
    }
}
