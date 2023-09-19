
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.Speech.Core.Services;
using System.Text;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    /// <summary>
    /// The big brain of the AI
    /// The class handles the logic of how the AI prioritize and process messages
    /// </summary>
    public class BigBrainService : BasicAutoStartable
    {
        readonly IThePandaMonitor _monitor;
        readonly IAiCharacterService _characterService;
        readonly OpenAiService _openAiService;
        readonly ChatHistoryService _chatHistoryService;
        readonly SpeechQueueService _speechQueueService;

        /// <summary>
        /// Creates a new instance of <see cref="BigBrainService" />
        /// </summary>
        public BigBrainService(
            IThePandaMonitor monitor,
            IAiCharacterService characterService,
            OpenAiService openAiService,
            ChatHistoryService chatHistoryService,
            SpeechQueueService speechQueueService
        )
        {
            _monitor = monitor;
            _characterService = characterService;
            _openAiService = openAiService;
            _chatHistoryService = chatHistoryService;
            _speechQueueService = speechQueueService;
        }

        /// <summary>
        /// Instructs open ai to think about the chat history
        /// </summary>
        /// <param name="forRole">Which role is the speaker</param>
        /// <returns></returns>
        public async Task<string> ThinkAsync(SpeechQueueRole forRole)
        {
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(_characterService.GetPersonalityPrompt())
                },
                Model = OpenAI.ObjectModels.Models.Gpt_3_5_Turbo_0613,
                Temperature = 1,
                MaxTokens = 512,
                FrequencyPenalty = 1f,
            };
            var chatlog = _chatHistoryService.GenerateChatLog();
            request.Messages.Add(ChatMessage.FromUser(chatlog));
            return await QueueResponse(request, forRole);
        }

        /// <summary>
        /// Queue the response and return the first chunk of result ASAP
        /// </summary>
        /// <returns></returns>
        async Task<string> QueueResponse(ChatCompletionCreateRequest request, SpeechQueueRole forRole)
        {
            try
            {
                var responses = _openAiService.CreateCompletionAsync(request);
                var speechId = Guid.NewGuid();
                _speechQueueService.Queue(speechId, forRole);
                var response = await QueueAndCombineResponseAsync(responses, speechId);
                if (response.StartsWith("大豆醬")
                    || response.StartsWith("大豆酱")) // Simplified chinese
                {
                    response = response.Remove(0,5);
                }
                _chatHistoryService.AddChat(ChatMessage.FromAssistant($"{_characterService.Name}: {response}"));
                return response;
            }
            catch (Exception e)
            {
                await ChatLogger.LogAsync(e.Message);
                return "Sorry, my brain stops working.";
            }
        }

        /// <summary>
        /// Combines the response from OpenAI
        /// and return the first chunk of result ASAP
        /// </summary>
        /// <param name="completionResult"></param>
        /// <param name="speechId">The id of the chat result this response is related to</param>
        /// <returns></returns>
        async Task<string> QueueAndCombineResponseAsync(
            IAsyncEnumerable<ChatCompletionCreateResponse> completionResult,
            Guid speechId
        ) {
            var responseBuilder = new StringBuilder();
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

                _speechQueueService.Append(speechId, choice.Message.Content);
                responseBuilder.Append(choice.Message.Content);
            }
            return responseBuilder.ToString();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StartAsync()
        {
            _monitor.Register(this, _openAiService);
            _monitor.Register(this, _speechQueueService);
            return base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _monitor.Unregister(this);
            return base.StopAsync();
        }
    }
}
