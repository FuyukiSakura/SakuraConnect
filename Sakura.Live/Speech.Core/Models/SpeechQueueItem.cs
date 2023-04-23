
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
        /// The role of the person requesting the speech
        /// </summary>
        public SpeechQueueRole Role { get; set; } = SpeechQueueRole.User;

        /// <summary>
        /// The text to speak
        /// </summary>
        public string Text { get; set; } = "";

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
