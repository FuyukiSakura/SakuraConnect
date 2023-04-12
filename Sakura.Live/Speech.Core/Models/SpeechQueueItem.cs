
namespace Sakura.Live.Speech.Core.Models
{
    /// <summary>
    /// Defines an item to be spoken
    /// </summary>
    public class SpeechQueueItem
    {
        /// <summary>
        /// Item is skipped if the text is this
        /// </summary>
        public const string TerminationText = "RESPONDED_WITH_ERROR";

        /// <summary>
        /// Creates a new instance of <see cref="SpeechQueueItem" />
        /// </summary>
        /// <param name="text">The text to speak</param>
        /// <param name="role">The role of the respondent</param>
        public SpeechQueueItem(string text, SpeechQueueRole role)
        {
            Role = role;
            Text = text;
        }
        
        /// <summary>
        /// The speaker's role
        /// </summary>
        public SpeechQueueRole Role { get; set; }

        /// <summary>
        /// The text to speak
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The language to speak in
        /// </summary>
        public string Language { get; set; } = "zh-TW";

        /// <summary>
        /// The time the item was added to the queue
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }
}
