
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.Speech.Core.Services;
using System.Text;
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
        /// <param name="prompt"></param>
        /// <param name="forRole">Which role is the speaker</param>
        /// <returns></returns>
        public async Task<string> ThinkAsync(string prompt, SpeechQueueRole forRole)
        {
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(_characterService.GetPersonalityPrompt())
                },
                Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo,
                Temperature = 1,
                MaxTokens = 1024
            };
            _chatHistoryService.GetAllChat()
                .ForEach(request.Messages.Add);
            var instruction = ChatMessage.FromUser(prompt);
            request.Messages.Add(instruction);
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
                _chatHistoryService.AddChat(ChatMessage.FromAssistant(response));
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
