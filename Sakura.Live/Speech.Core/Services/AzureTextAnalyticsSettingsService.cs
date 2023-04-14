
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Manages Azure Text Analytics service settings
    /// </summary>
    public class AzureTextAnalyticsSettingsService
    {
        readonly ISettingsService _settingsService;

        /// <summary>
        /// Gets or sets the endpoint for Azure Text Analytics Service
        /// </summary>
        public string Endpoint { get; set; } = "";

        /// <summary>
        /// Gets or sets the subscription key for Azure Text Analytics Service
        /// </summary>
        public string SubscriptionKey { get; set; } = "";

        /// <summary>
        /// Creates a new instance of <see cref="AzureTextAnalyticsSettingsService" />
        /// </summary>
        public AzureTextAnalyticsSettingsService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            Load();
        }

        /// <summary>
        /// Saves Azure Text Analytics settings to the system
        /// </summary>
        public void Save()
        {
            _settingsService.Set(AzureTextAnalyticsPreferenceKeys.Endpoint, Endpoint);
            _settingsService.Set(AzureTextAnalyticsPreferenceKeys.Key, SubscriptionKey);
        }

        /// <summary>
        /// Loads Azure Text Analytics settings from the system
        /// </summary>
        public void Load()
        {
            Endpoint = _settingsService.Get(AzureTextAnalyticsPreferenceKeys.Endpoint, "");
            SubscriptionKey = _settingsService.Get(AzureTextAnalyticsPreferenceKeys.Key, "");
        }
    }
}
