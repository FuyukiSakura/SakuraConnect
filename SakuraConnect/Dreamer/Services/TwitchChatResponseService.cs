
using OpenAI.GPT3.ObjectModels.RequestModels;
using Sakura.Live.OpenAi.Core.Services;
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
        readonly AzureTextToSpeechService _speechService;
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Creates a new instance of <see cref="TwitchChatResponseService" />
        /// </summary>
        public TwitchChatResponseService(
            IThePandaMonitor monitor,
            IAiCharacterService characterService,
            OpenAiService openAiService,
            TwitchChatService twitchChatService,
            AzureTextToSpeechService speechService,
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
                _lastSpoke = DateTime.Now;
                _lastRespondedMessage = _chatHistoryService.GetLastUserMessage();
                var response = await ThinkAsync(
                    "Summarize the user input above and create an interactive response."
                );
                await _speechService.SpeakAsync(response, "zh-TW");
                _lastSpoke = DateTime.Now; // Avoid talking too much
            }

            await StopAsync();
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
                var response = await ThinkAsync("可以講講你的生活嗎？");
                await _speechService.SpeakAsync(response, "zh-TW");
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

            try
            {
                var response = await _openAiService.CreateCompletionAsync(request);
                await ChatLogger.LogAsync($"{prompt}: {response}");
                _chatHistoryService.AddChat(ChatMessage.FromAssistance(response));
                return response;
            }
            catch (Exception e)
            {
                return "Error.";
            }
        }

        /// <summary>
        /// Checks if the last responded message is the same as the last user message
        /// wait for new user message if it is
        /// </summary>
        /// <returns></returns>
        async Task WaitUserInput()
        {
            while (true)
            {
                if (_lastRespondedMessage != null // Ignores when the app started
                    && _chatHistoryService.GetLastUserMessage() == _lastRespondedMessage)
                {
                    await Task.Delay(10_000);
                    continue;
                }
                break;
            }
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
