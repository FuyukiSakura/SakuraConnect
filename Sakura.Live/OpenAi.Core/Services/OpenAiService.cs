using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using Sakura.Live.OpenAi.Core.Models;
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
        public async Task<string> CreateCompletionAndResponseAsync(ChatCompletionCreateRequest request)
        {
            if (_openAiService == null)
            {
                return "Sorry, my brain is not installed.";
            }

            var completionResult = _openAiService.ChatCompletion.CreateCompletionAsStream(request);
            return await CombineResponseAsync(completionResult);
        }

        ///
        /// <inheritdoc cref="IChatCompletionService.CreateCompletionAsStream"/>
        ///
        public IAsyncEnumerable<ChatCompletionCreateResponse>? CreateCompletionAsync(ChatCompletionCreateRequest request)
        {
            return _openAiService?.ChatCompletion.CreateCompletionAsStream(request);
        }

        /// <summary>
        /// Combines the response from OpenAI
        /// and return the first chunk of result ASAP
        /// </summary>
        /// <param name="completionResult"></param>
        /// <returns></returns>
        static async Task<string> CombineResponseAsync(IAsyncEnumerable<ChatCompletionCreateResponse> completionResult)
        {
            var response = "";
            await foreach (var result in completionResult)
            {
                if (!result.Successful)
                {
                    // Unsuccessful
                    continue;
                }

                var choice = result.Choices.FirstOrDefault();
                if (choice == null)
                {
                    // No choices available
                    continue;
                }

                response += choice.Message.Content;
            }
            return response;
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
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            SaveSettings();
            await base.StartAsync();
            Start(ApiKey);
        }
    }
}
