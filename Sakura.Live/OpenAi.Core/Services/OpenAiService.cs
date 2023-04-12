using OpenAI.GPT3;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using Sakura.Live.OpenAi.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.OpenAi.Core.Services
{
    /// <summary>
    /// Accesses the Open AI service
    /// </summary>
    public class OpenAiService : BasicAutoStartable
    {
        readonly ISettingsService _settingsService;
        OpenAIService? _openAiService;

        /// <summary>
        /// Gets or sets the API key of Open AI
        /// </summary>
        public string ApiKey { get; set; } = "";

        /// <summary>
        /// Creates a new instance of <see cref="OpenAiService" />
        /// </summary>
        /// <param name="settingsService"></param>
        public OpenAiService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();
        }

        /// <summary>
        /// Saves OpenAI settings to the system
        /// </summary>
        void SaveSettings()
        {
            _settingsService.Set(OpenAiPreferenceKeys.ApiKey, ApiKey);
        }

        /// <summary>
        /// Loads OpenAI settings from the system
        /// </summary>
        void LoadSettings()
        {
            ApiKey = _settingsService.Get(OpenAiPreferenceKeys.ApiKey, "");
        }

        /// <summary>
        /// Gets the instance of Open AI service
        /// </summary>
        /// <returns></returns>
        public IOpenAIService? Get()
        {
            return _openAiService;
        }

        ///
        /// <inheritdoc cref="IChatCompletionService.CreateCompletionAsStream"/>
        ///
        public IAsyncEnumerable<ChatCompletionCreateResponse>? CreateCompletionAsStream(ChatCompletionCreateRequest request)
        {
            return _openAiService?.ChatCompletion.CreateCompletionAsStream(request);
        }

        ///
        /// <inheritdoc cref="IChatCompletionService.CreateCompletionAsStream"/>
        ///
        public async Task<string> CreateCompletionAsync(ChatCompletionCreateRequest request)
        {
            if (_openAiService == null)
            {
                return "Sorry, I didn't get that.";
            }

            var completionResult = await _openAiService.ChatCompletion.CreateCompletion(request);
            return completionResult.Successful ?
                completionResult.Choices.First().Message.Content :
                "RESPONDED_WITH_ERROR";
        }

        /// <summary>
        /// Creates an instance of OpenAI service of the given API Key
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        void Start(string apiKey)
        {
            _openAiService = new OpenAIService(new OpenAiOptions
            {
                ApiKey =  apiKey
            });
            _ = HeartBeatAsync();
        }

        /// <summary>
        /// Checks if the thread is still running
        /// </summary>
        /// <returns></returns>
        async Task HeartBeatAsync()
        {
            Status = ServiceStatus.Running;
            while (Status == ServiceStatus.Running) // Checks if the client is connected
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default);
            }
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            SaveSettings();
            Start(ApiKey);
            await base.StartAsync();
        }
    }
}
