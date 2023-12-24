
namespace SakuraConnect.Shared.Models.Messaging.Ai
{
    /// <summary>
    /// Is sent when the AI has finished speaking
    /// </summary>
    public class EndedSpeakingEventArg
    {
        public string Text { get; set; } = "";
        public string Reason { get; set; } = "";
    }
}
