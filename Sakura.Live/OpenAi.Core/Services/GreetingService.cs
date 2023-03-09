using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace Sakura.Live.OpenAi.Core.Services
{
    /// <summary>
    /// Greets the user with a message
    /// </summary>
    public class GreetingService
    {
        /// <summary>
        /// Gets or sets the API key of Open AI
        /// </summary>
        readonly OpenAiService _service;

        /// <summary>
        /// Creates a new instance of <see cref="GreetingService" />
        /// </summary>
        /// <param name="service"></param>
        public GreetingService(OpenAiService service)
        {
            _service = service;
        }

        /// <summary>
        /// Greets the audience according to the selected tone of the user
        /// </summary>
        /// <param name="prompt">The prompt for generating the customized greet</param>
        /// <param name="username">The name of the user to be greeted</param>
        /// <returns></returns>
        public async Task<string> GreetsAsync(string prompt, string username)
        {
            var completionResult = await _service.Get().ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromUser($"{prompt}:{username}")
                },
                Model = Models.ChatGpt3_5Turbo,
                Temperature = 2,
                MaxTokens = 50
            });

            if (completionResult.Successful)
            {
                return completionResult.Choices.First().Message.Content;
            }

            // TODO: Adds fallback message
            return "";
        }
    }
}
