using System.Text;
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
        public string GenerateChatLog()
        {
            var sb = new StringBuilder();
            foreach (var msg in _chatHistory)
            {
                sb.AppendLine(msg.Content);
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
