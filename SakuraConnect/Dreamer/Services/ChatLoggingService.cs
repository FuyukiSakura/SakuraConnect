
namespace Sakura.Live.Connect.Dreamer.Services
{
    /// <summary>
    /// Writes chat messages to a log file
    /// </summary>
    public class ChatLoggingService
    {
        /// <summary>
        /// Logs a message to a file
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task LogAsync(string message)
        {
            var path = Path.Combine(
                FileSystem.Current.AppDataDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + ".txt"
            );
            await File.AppendAllTextAsync(path, message + Environment.NewLine);
        }
    }
}
