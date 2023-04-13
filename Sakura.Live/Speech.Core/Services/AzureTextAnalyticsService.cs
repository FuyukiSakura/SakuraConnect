using Azure;
using Azure.AI.TextAnalytics;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Accesses Azure Language Detection service
    /// </summary>
    public class AzureTextAnalyticsService : BasicAutoStartable
    {
        TextAnalyticsClient? _client;

        // Dependencies
        readonly AzureTextAnalyticsSettingsService _settingsService;

        /// <summary>
        /// Creates a new instance of <see cref="AzureTextAnalyticsService" />
        /// </summary>
        public AzureTextAnalyticsService(AzureTextAnalyticsSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// Detects the language of a text
        /// </summary>
        /// <returns></returns>
        public string DetectLanguage(string input)
        {
            var detectedLanguage = _client?.DetectLanguage(input);
            return detectedLanguage?.Value.Iso6391Name ?? "";
        }

        /// <summary>
        /// Creates a new endpoint when started
        /// effectively updates the setting if they are changed
        /// </summary>
        /// <returns></returns>
        public override Task StartAsync()
        {
            var credential = new AzureKeyCredential(_settingsService.SubscriptionKey);
            var endpoint = new Uri(_settingsService.Endpoint);
            _client = new TextAnalyticsClient(endpoint, credential);
            return base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _client = null;
            return base.StopAsync();
        }
    }
}
