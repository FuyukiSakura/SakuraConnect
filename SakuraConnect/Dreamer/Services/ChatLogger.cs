
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace Sakura.Live.Connect.Dreamer.Services
{
    /// <summary>
    /// Writes chat messages to a log file
    /// </summary>
    public static class ChatLogger
    {
        /// <summary>
        /// Logs a message to a file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="suffix">Suffix of the log file</param>
        /// <returns></returns>
        public static async Task LogAsync(string message, string suffix = "")
        {
            var path = Path.Combine(
                FileSystem.Current.AppDataDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + $".{suffix}.txt"
            );
            await File.AppendAllTextAsync(path, message + Environment.NewLine);
        }

        /// <summary>
        /// Logs the request and response of open ai
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <param name="sender">Identifier of which agent sends the request</param>
        /// <returns></returns>
        public static async Task LogOpenAiRequest(ChatCompletionCreateRequest request, string result, string sender)
        {
            var messages = request.Messages.Select(CreateMessage);
            var message = string.Join(Environment.NewLine, messages);
            var report = "-----Start----------------------------------------------------------------------------------" + Environment.NewLine +
                         $"[{DateTime.Now:T}] {sender}{Environment.NewLine}"+
                         $"============={Environment.NewLine}{message}{Environment.NewLine}============={Environment.NewLine}"
                         + result + Environment.NewLine +
                         "-----End-----------------------------------------------------------------------------------" + Environment.NewLine;
            await LogAsync(report, LogPurpose.Debug);
        }

        /// <summary>
        /// Separates assistant messages from user messages
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        static string CreateMessage(ChatMessage msg)
        {
            if (msg.Role == StaticValues.ChatMessageRoles.Assistant)
            {
                return Environment.NewLine + "Assistant" + Environment.NewLine 
                       + "-----" + Environment.NewLine
                       + msg.Content + Environment.NewLine
                       + "-----" + Environment.NewLine;
            }
            return msg.Content;
        }
    }

    /// <summary>
    /// Suffixes for log files
    /// </summary>
    public static class LogPurpose
    {
        public const string Chat = "chat";
        public const string Debug = "debug";
    }
}
