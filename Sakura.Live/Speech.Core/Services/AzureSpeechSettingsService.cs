using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Manages Azure Speech service settings
    /// </summary>
    public class AzureSpeechSettingsService
    {
        readonly ISettingsService _settingsService;

        /// <summary>
        /// Gets or sets the subscription key for Azure Speech Service
        /// </summary>
        public string SubscriptionKey { get; set; } = "";

        /// <summary>
        /// Gets or sets the region for Azure Speech Service
        /// </summary>
        public string Region { get; set; } = "";

        /// <summary>
        /// Creates a new instance of <see cref="AzureSpeechSettingsService" />
        /// </summary>
        /// <param name="settings"></param>
        public AzureSpeechSettingsService(ISettingsService settings)
        {
            _settingsService = settings;
            Load();
        }

        /// <summary>
        /// Saves Azure Speech settings to the system
        /// </summary>
        public void Save()
        {
            _settingsService.Set(AzureSpeechPreferenceKeys.SubscriptionKey, SubscriptionKey);
            _settingsService.Set(AzureSpeechPreferenceKeys.Region, Region);
        }

        /// <summary>
        /// Loads Azure Speech settings from the system
        /// </summary>
        public void Load()
        {
            SubscriptionKey = _settingsService.Get(AzureSpeechPreferenceKeys.SubscriptionKey, "");
            Region = _settingsService.Get(AzureSpeechPreferenceKeys.Region, "");
        }
    }
}
