using OpenAI.GPT3.ObjectModels.RequestModels;

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
        /// Gets all chat messages
        /// </summary>
        /// <returns></returns>
        public List<ChatMessage> GetAllChat()
        {
            return _chatHistory;
        }
    }
}
