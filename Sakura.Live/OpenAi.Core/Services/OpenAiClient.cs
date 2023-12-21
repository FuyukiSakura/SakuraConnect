using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using Sakura.Live.OpenAi.Core.Models;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.OpenAi.Core.Services
{
    /// <summary>
    /// Accesses the Open AI service
    /// </summary>
    public class OpenAiClient
    {
        readonly ISettingsService _settingsService;

        /// <summary>
        /// Gets or sets the API key of Open AI
        /// </summary>
        public string ApiKey { get; set; } = "";

        /// <summary>
        /// Creates a new instance of <see cref="OpenAiClient" />
        /// </summary>
        /// <param name="settingsService"></param>
        public OpenAiClient(ISettingsService settingsService)
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

        ///
        /// <inheritdoc cref="IChatCompletionService.CreateCompletionAsStream"/>
        ///
        public IAsyncEnumerable<ChatCompletionCreateResponse>? CreateCompletionAsStream(ChatCompletionCreateRequest request)
        {
            var client = CreateClient(ApiKey);
            return client?.ChatCompletion.CreateCompletionAsStream(request);
        }

        ///
        /// <inheritdoc cref="IChatCompletionService.CreateCompletionAsStream"/>
        ///
        public async Task<string> CreateCompletionAndResponseAsync(ChatCompletionCreateRequest request)
        {
            var result = CreateCompletionAsync(request);
            return await CombineResponseAsync(result);
        }

        ///
        /// <inheritdoc cref="IChatCompletionService.CreateCompletionAsStream"/>
        ///
        public IAsyncEnumerable<ChatCompletionCreateResponse> CreateCompletionAsync(ChatCompletionCreateRequest request)
        {
            var client = CreateClient(ApiKey);
            return client.ChatCompletion.CreateCompletionAsStream(request);
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
                await Task.Delay(1);
            }
            return response;
        }

        /// <summary>
        /// Creates an instance of OpenAI service of the given API Key
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        OpenAIService CreateClient(string apiKey)
        {
            SaveSettings();
            return new OpenAIService(new OpenAiOptions
            {
                ApiKey =  apiKey
            });
        }
    }
}
