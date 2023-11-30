using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.Twitch.Core.Services;
using TwitchLib.Client.Events;

namespace Sakura.Live.Connect.Dreamer.Services.Twitch
{
    /// <summary>
    /// Monitors the twitch chat and log the chat history
    /// </summary>
    public class TwitchChatMonitorService : BasicAutoStartable
    {
        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly TwitchChatService _twitchChatService;
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Creates a new instance of <see cref="TwitchChatMonitorService" />
        /// </summary>
        public TwitchChatMonitorService(
            IThePandaMonitor monitor,
            TwitchChatService twitchChatService,
            ChatHistoryService chatHistoryService)
        {
            _monitor = monitor;
            _twitchChatService = twitchChatService;
            _chatHistoryService = chatHistoryService;
        }

        /// <summary>
        /// Adds a chat message to the history when twitch chat message received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (!e.ChatMessage.Message.Contains("following") // Allow moderator for following
                && (e.ChatMessage.Message.StartsWith("!") // Ignore commands
                    || e.ChatMessage.IsModerator))
            {
                return;
            }
            var msg = ChatMessage.FromUser($"{e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
            _ = ChatLogger.LogAsync(msg.Content, "chat");
            _chatHistoryService.AddChat(msg);
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StartAsync()
        {
            _twitchChatService.OnMessageReceived += OnMessageReceived;
            _monitor.Register<TwitchChatService>(this);
            return base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _monitor.Unregister(this);
            _twitchChatService.OnMessageReceived -= OnMessageReceived;
            return base.StopAsync();
        }
    }
}
