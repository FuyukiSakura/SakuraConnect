using System.Text;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace Sakura.Live.OpenAi.Core.Services
{
    /// <summary>
    /// Logs chat messages
    /// </summary>
    public class ChatHistoryService
    {
        readonly List<ChatMessage> _chatHistory = new();

        /// <summary>
        /// The maximum length of the chat history
        /// </summary>
        public int MaxHistoryLength { get; set; } = 10;

        /// <summary>
        /// Adds a chat message to the history
        /// </summary>
        /// <param name="message"></param>
        public void AddChat(ChatMessage message)
        {
            if (_chatHistory.Any() 
                && _chatHistory.Last().Content == message.Content)
            {
                return; // Ignore duplicate messages
            }

            if (_chatHistory.Count > MaxHistoryLength)
            {
                _chatHistory.RemoveAt(0); // Remove first element if length exceeds max
            }

            _chatHistory.Add(message);
        }

        /// <summary>
        /// Generates a chat log as a multi-line string
        /// </summary>
        /// <returns></returns>
        public List<ChatMessage> GenerateChatMessage()
        {
            var sb = new StringBuilder();
            var chatMessages = new List<ChatMessage>();
            foreach (var msg in _chatHistory)
            {
	            if (msg.Role == StaticValues.ChatMessageRoles.User)
	            {
		            sb.AppendLine(msg.Content);
	            }
	            else
	            {
                    // Treat the assistant message as a separation of old user messages
                    // So the AI only respond to the latest user messages
                    chatMessages.Add(ChatMessage.FromUser(sb.ToString()));
                    chatMessages.Add(msg);
                    sb.Clear();
	            }
            }

            if (sb.Length > 0)
            {
                // Add the last user message if there is any
	            chatMessages.Add(ChatMessage.FromUser(sb.ToString()));
            }

            if (chatMessages.Last().Role == StaticValues.ChatMessageRoles.Assistant)
            {
                // Don't include the last assistant message as she won't response
				chatMessages.RemoveAt(chatMessages.Count - 1);
			}
            return chatMessages;
        }

        /// <summary>
        /// Generates a plain text chat log
        /// with the AI's line
        /// </summary>
        /// <returns></returns>
        public string GenerateChatLog()
        {
            var sb = new StringBuilder();
            foreach (var msg in _chatHistory)
            {
                if (msg.Role == StaticValues.ChatMessageRoles.User)
                {
                    sb.AppendLine(msg.Content);
                }
                else
                {
                    // Append username of the AI
                    sb.AppendLine($"大豆: {msg.Content}");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the last user message
        /// </summary>
        /// <returns></returns>
        public ChatMessage? GetLastUserMessage()
        {
           return _chatHistory.LastOrDefault(msg => msg.Role == "user");
        }
    }
}
