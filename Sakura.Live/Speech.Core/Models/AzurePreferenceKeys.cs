namespace Sakura.Live.Speech.Core.Models
{
    /// <summary>
    /// Saves azure services preferences
    /// </summary>
    public static class AzureSpeechPreferenceKeys
    {
        /// <summary>
        /// Gets the system preference key of service region for Azure Speech Service
        /// </summary>
        public const string Region = "SCONNECT_AZURE_SPEECH_REGION";

        /// <summary>
        /// Gets the system preference key of subscription key for Azure Speech Service
        /// </summary>
        public const string SubscriptionKey = "SCONNECT_AZURE_SPEECH_SUBSCRIPTION_KEY";
    }
}
