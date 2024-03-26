
namespace SakuraConnect.Shared.Models.Messaging.Speech
{
    /// <summary>
    /// Is raised when speech-to-text returns a recognized text
    /// </summary>
    public class SpeechRecognizedEventArgs
    {
        /// <summary>
        /// The text recognized
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}
