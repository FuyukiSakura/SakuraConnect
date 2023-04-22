
using System.Diagnostics;
using System.Text;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.Twitch.Core.Services;
using TwitchLib.Client.Events;

namespace Sakura.Live.Connect.Dreamer.Services
{
    /// <summary>
    /// Response to the twitch chat with open ai
    /// </summary>
    public class TwitchChatResponseService : BasicAutoStartable
    {
        bool _isRunning;
        ChatMessage? _lastRespondedMessage;
        DateTime _lastSpoke = DateTime.MinValue;

        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly IAiCharacterService _characterService;
        readonly OpenAiService _openAiService;
        readonly TwitchChatService _twitchChatService;
        readonly SpeechQueueService _speechService;
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Creates a new instance of <see cref="TwitchChatResponseService" />
        /// </summary>
        public TwitchChatResponseService(
            IThePandaMonitor monitor,
            IAiCharacterService characterService,
            OpenAiService openAiService,
            TwitchChatService twitchChatService,
            SpeechQueueService speechService,
            ChatHistoryService chatHistoryService
        ) {
            _openAiService = openAiService;
            _twitchChatService = twitchChatService;
            _speechService = speechService;
            _chatHistoryService = chatHistoryService;
            _monitor = monitor;
            _characterService = characterService;
            InitializeChat();
        }

        /// <summary>
        /// Starts the chat with a greeting message
        /// </summary>
        void InitializeChat()
        {
            _chatHistoryService.MaxHistoryLength = 30;
            _chatHistoryService.AddChat(ChatMessage.FromUser("You just started your stream, greet the users if any. Try to keep the conversation by sharing your experiences."));
        }

        /// <summary>
        /// Adds a chat message to the history when twitch chat message received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (!e.ChatMessage.Message.Contains("following") // Allow moderator for following
                && (e.ChatMessage.Message.StartsWith("!") // Ignore commands
                || e.ChatMessage.IsModerator))
            {
                return;
            }
            var msg = ChatMessage.FromUser($"{e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
            _chatHistoryService.AddChat(msg);
        }

        /// <summary>
        /// Monitors and responds to chat log
        /// </summary>
        /// <returns></returns>
        public async Task ResponseAsync()
        {
            while (_isRunning)
            {
                await WaitUserInput();
                _lastRespondedMessage = _chatHistoryService.GetLastUserMessage();
                await ChatLogger.LogAsync("Started responding to: " + _lastRespondedMessage?.Content);
                _ = Task.Run(GenerateResponseAsync); // Fire and forget
                _lastSpoke = DateTime.Now;
            }

            await StopAsync();
        }

        /// <summary>
        /// Generates response and add to the queue
        /// </summary>
        /// <returns></returns>
        async Task GenerateResponseAsync()
        {
            var response = await ThinkAsync(
                "Answer within 50 words. Try to match the language above."
            );
            await ChatLogger.LogAsync($"Responded: {response}");
        }

        /// <summary>
        /// Talks to itself if no user input for a while
        /// </summary>
        /// <returns></returns>
        async Task SoliloquizeAsync()
        {
            while (_isRunning)
            {
                if (DateTime.Now - _lastSpoke < TimeSpan.FromMinutes(1))
                {
                    await Task.Delay(30_000);
                    continue;
                }

                _lastSpoke = DateTime.Now;
                var response = await ThinkAsync("Carry on.");

                await ChatLogger.LogAsync($"Soliloquize: {response}");
                _lastSpoke = DateTime.Now; // Avoid talking too much
            }
        }

        /// <summary>
        /// Instructs open ai to think about the chat history
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        async Task<string> ThinkAsync(string prompt)
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
            var instruction =
                ChatMessage.FromUser(prompt);
            request.Messages.Add(instruction);
            return await QueueResponse(request);
        }

        /// <summary>
        /// Queue the response and return the first chunk of result ASAP
        /// </summary>
        /// <returns></returns>
        async Task<string> QueueResponse(ChatCompletionCreateRequest request)
        {
            try
            {
                var responses = _openAiService.CreateCompletionAsync(request);
                var speechId = Guid.NewGuid();
                _speechService.Queue(speechId, SpeechQueueRole.User);
                var response = await CombineResponseAsync(responses, speechId);
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
        async Task<string> CombineResponseAsync(
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

                _speechService.Append(speechId, choice.Message.Content);
                responseBuilder.Append(choice.Message.Content);
            }
            return responseBuilder.ToString();
        }

        /// <summary>
        /// Checks if the last responded message is the same as the last user message
        /// wait for new user message if it is
        /// </summary>
        /// <returns></returns>
        async Task WaitUserInput()
        {
            var started = DateTime.Now;
            while (IsUserInputting(started)) 
            {
                if (_lastRespondedMessage == null)
                {
                    // Ignores when the app started
                    break;
                }

                await Task.Delay(1_000);
            }
        }

        /// <summary>
        /// Checks if the last responded message is the same as the last user message
        /// </summary>
        /// <param name="started"></param>
        /// <returns></returns>
        bool IsUserInputting(DateTime started)
        {
            var waitForSeconds = 10; // Wait for 10 seconds of input before responding
            if (_speechService.IsSpeaking)
            {
                // Wait for 30 seconds of input if the bot is speaking
                waitForSeconds = 30;
            }
            
            var noNewUserInput = _chatHistoryService.GetLastUserMessage() == _lastRespondedMessage;
            var notWaitedFor = (DateTime.Now - started).TotalSeconds < waitForSeconds;
            return noNewUserInput
                   || notWaitedFor;
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            await base.StartAsync();
            _lastSpoke = DateTime.Now;
            _ = HeartBeatAsync();
            _ = ResponseAsync();
            _ = SoliloquizeAsync();
        }

        /// <summary>
        /// Starts responding to chat log
        /// </summary>
        public void Start()
        {
            _twitchChatService.OnMessageReceived += OnMessageReceived;
            _isRunning = true;
            _monitor.Register(this, _twitchChatService);
            _monitor.Register(this, _openAiService);
            _monitor.Register(this, _speechService);
            _monitor.Register(this, this);
        }

        /// <summary>
        /// Stops responding to chat log
        /// </summary>
        public void Stop()
        {
            _monitor.Unregister(this);
            _twitchChatService.OnMessageReceived -= OnMessageReceived;
            _isRunning = false;
        }
    }
}
