using OpenAI.GPT3;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using TwitchLib.Communication.Interfaces;

namespace Sakura.Live.OpenAi.Core.Services
{
    /// <summary>
    /// Accesses the Open AI service
    /// </summary>
    public class OpenAiService : BasicAutoStartable
    {
        OpenAIService? _openAiService;

        /// <summary>
        /// Gets or sets the API key of Open AI
        /// </summary>
        public string ApiKey { get; set; } = "";

        /// <summary>
        /// Gets the instance of Open AI service
        /// </summary>
        /// <returns></returns>
        public IOpenAIService? Get()
        {
            return _openAiService;
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
            Start(ApiKey);
            await base.StartAsync();
        }
    }
}
