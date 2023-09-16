﻿
using Sakura.Live.OpenAi.Core.Models;
using Sakura.Live.OpenAi.Core.Services;
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
        public string Character { get; set; } = "You are a vtuber";

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
        public string GetPersonalityPrompt()
        {
            SaveSettings();
            return $"{Character}. {WordLimitPrompt}";
        }

        ///
        /// <inheritdoc />
        ///
        public void SaveSettings()
        {
            _settingsService.Set(OpenAiPreferenceKeys.AiName, Name);
            _settingsService.Set(OpenAiPreferenceKeys.CharacterPrompt, Character);
            _settingsService.Set(OpenAiPreferenceKeys.GreetingPrompt, GreetingStyle);
        }

        ///
        /// <inheritdoc />
        ///
        public void LoadSettings()
        {
            Name = _settingsService.Get(OpenAiPreferenceKeys.AiName, "");
            Character = _settingsService.Get(OpenAiPreferenceKeys.CharacterPrompt, "You are a vtuber");
            GreetingStyle = _settingsService.Get(OpenAiPreferenceKeys.GreetingPrompt, "Act cute");
        }

        /// <summary>
        /// Generates the prompt for the word limit
        /// </summary>
        string WordLimitPrompt => $"The maximum number of words that can be generated by the ai is {WordLimit}.";
    }
}
