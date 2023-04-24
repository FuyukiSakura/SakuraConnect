
namespace Sakura.Live.Connect.Dreamer.Services
{
    /// <summary>
    /// Writes chat messages to a log file
    /// </summary>
    public class ChatLogger
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
    }
}
