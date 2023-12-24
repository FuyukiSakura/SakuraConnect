using System.Text;
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;
using SakuraConnect.Shared.Models.Messaging.Ai;

namespace Sakura.Live.Connect.Dreamer.Services.Twitch
{
    /// <summary>
    /// Monitors the twitch chat and log the chat history
    /// </summary>
    public class ChatMonitorService : BasicAutoStartable
    {
        readonly List<CommentData> _chatHistory = new();
        
        /// <summary>
        /// The maximum length of the chat history
        /// </summary>
        public int MaxHistoryLength { get; set; } = 30;

        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly IPandaMessenger _messenger;

        /// <summary>
        /// Creates a new instance of <see cref="ChatMonitorService" />
        /// </summary>
        public ChatMonitorService(
            IThePandaMonitor monitor,
            IPandaMessenger messenger)
        {
            _monitor = monitor;
            _messenger = messenger;
        }

        /// <summary>
        /// Adds a chat message to the history when twitch chat message received
        /// </summary>
        /// <param name="e"></param>
        void OnMessageReceived(CommentReceivedEventArg e)
        {
            foreach (var chat in e.Comments.Where(comment => !comment.Comment.StartsWith("!")))
            {
                _ = ChatLogger.LogAsync($"[{chat.ReceivedAt:T}] | {chat.Username,-10} |{chat.Comment}", LogPurpose.Chat);
                AddComment(chat);
            }
        }

        /// <summary>
        /// Adds a chat message to the history
        /// removes the first element if the length exceeds max
        /// </summary>
        /// <param name="message"></param>
        void AddComment(CommentData message)
        {
            if (_chatHistory.Count != 0
                && _chatHistory.Last().Comment == message.Comment)
            {
                _ = ChatLogger.LogAsync($"[{DateTime.Now:T}] Duplicate message: {message.Comment}", LogPurpose.Debug);
                return; // Ignore duplicate messages
            }

            if (_chatHistory.Count > MaxHistoryLength)
            {
                _chatHistory.RemoveAt(0); // Remove first element if length exceeds max
            }
            _chatHistory.Add(message);
        }

        /// <summary>
        /// Generates a chat log for OpenAI request
        /// </summary>
        /// <returns></returns>
        public List<ChatMessage> CreateForRequest()
        {
            var sb = new StringBuilder();
            var chatMessages = new List<ChatMessage>();
            foreach (var msg in _chatHistory.OrderBy(chat => chat.ReceivedAt))
            {
                if (msg.Role == SpeechQueueRole.Self)
                {
                    // Treat the assistant message as a separation of old user messages
                    // So the AI only respond to the latest user messages
                    chatMessages.Add(ChatMessage.FromUser(sb.ToString()));
                    chatMessages.Add(ChatMessage.FromAssistant(msg.Comment));
                    sb.Clear();
                    continue;
                }

                sb.AppendLine($"{msg.Username}: {msg.Comment}");
            }

            if (sb.Length > 0)
            {
                // Add the last user message if there is any
                chatMessages.Add(ChatMessage.FromUser(sb.ToString()));
            }
            return chatMessages;
        }

        /// <summary>
        /// Creates chat log as a multi-line string
        /// </summary>
        /// <returns></returns>
        public string CreateChatLog()
        {
            var sb = new StringBuilder();
            foreach (var msg in _chatHistory.OrderBy(chat => chat.ReceivedAt))
            {
                switch (msg.Role)
                {
                    case SpeechQueueRole.Guidance:
                        // Ignore guidance messages to prevent the guidance
                        // from bias towards its own messages
                        continue;
                    case SpeechQueueRole.Self:
                        sb.AppendLine($"{SystemNames.AI}: {msg.Comment}");
                        continue;
                    default:
                        sb.AppendLine($"{msg.Username}: {msg.Comment}");
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the last comment
        /// </summary>
        /// <returns></returns>
        public CommentData GetLastComment()
        {
            return _chatHistory.OrderBy(chat => chat.ReceivedAt)
                .LastOrDefault(data => data.Role != SpeechQueueRole.Guidance);
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
