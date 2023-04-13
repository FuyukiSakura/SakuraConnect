
namespace Sakura.Live.Speech.Core.Models
{
    /// <summary>
    /// Keys to access Azure Text Analytics Service preferences
    /// </summary>
    public static class AzureTextAnalyticsPreferenceKeys
    {
        /// <summary>
        /// Gets the system preference key of service region for Azure Text Analytics Service
        /// </summary>
        public const string Endpoint = "SCONNECT_AZURE_TEXT_ANALYTICS_ENDPOINT";

        /// <summary>
        /// Gets the system preference key of subscription key for Azure Text Analytics Service
        /// </summary>
        public const string Key = "SCONNECT_AZURE_TEXT_ANALYTICS_KEY";
    }
}
