﻿
using Sakura.Live.OpenAi.Core.Models;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    /// <summary>
    /// Defines the character of the ai
    /// </summary>
    public class AiCharacterService : IAiCharacterService
    {
        // Dependencies
        readonly ISettingsService _settingsService;

        ///
        /// <inheritdoc />
        ///
        public string Name { get; set; } = "";

        ///
        /// <inheritdoc />
        ///
        public string Topic { get; set; } = "";

        ///
        /// <inheritdoc />
        ///
        public string Character { get; set; } = "";

        ///
        /// <inheritdoc />
        ///
        public string AudienceCharacter { get; set; } = "";

        ///
        /// <inheritdoc />
        ///
        public string GreetingStyle { get; set; } = "Act cute";

        /// <summary>
        /// Limits the number of words that can be generated by the ai
        /// </summary>
        public int WordLimit { get; set; } = 50;

        /// <summary>
        /// Creates a new instance of <see cref="AiCharacterService" />
        /// </summary>
        /// <param name="settingsService"></param>
        public AiCharacterService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();
        }

        ///
        /// <inheritdoc />
        ///
        public string GetGreetingPrompt() {
            SaveSettings();
            return $"{Character}. {GreetingStyle}";
        }

        ///
        /// <inheritdoc />
        ///
        public string GetTopicPrompt()
        {
            SaveSettings();
            return $"{Topic}";
        }

        ///
        /// <inheritdoc />
        ///
        public string GetPersonalityPrompt()
        {
            SaveSettings();
            return $"{Character}";
        }

        ///
        /// <inheritdoc />
        ///
        public string GetAudiencePrompt()
        {
            SaveSettings();
            return $"{AudienceCharacter}";
        }

        /// <summary>
        /// Saves OpenAI settings to the system
        /// </summary>
        void SaveSettings()
        {
            _settingsService.Set(OpenAiPreferenceKeys.AiName, Name);
            _settingsService.Set(OpenAiPreferenceKeys.AiTopic, Topic);
            _settingsService.Set(OpenAiPreferenceKeys.CharacterPrompt, Character);
            _settingsService.Set(OpenAiPreferenceKeys.AudienceAgentPrompt, AudienceCharacter);
            _settingsService.Set(OpenAiPreferenceKeys.GreetingPrompt, GreetingStyle);
        }

        /// <summary>
        /// Loads OpenAI settings from the system
        /// </summary>
        void LoadSettings()
        {
            Name = _settingsService.Get(OpenAiPreferenceKeys.AiName, "");
            Topic = _settingsService.Get(OpenAiPreferenceKeys.AiTopic, "We are just chitchatting.");
            Character = _settingsService.Get(OpenAiPreferenceKeys.CharacterPrompt, "You are a vtuber");
            AudienceCharacter = _settingsService.Get(OpenAiPreferenceKeys.AudienceAgentPrompt, "You are a cool and engaging audience");
            GreetingStyle = _settingsService.Get(OpenAiPreferenceKeys.GreetingPrompt, "Act cute");
        }
    }
}
