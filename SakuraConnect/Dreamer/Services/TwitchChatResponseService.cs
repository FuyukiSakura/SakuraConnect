
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

        // Dependencies
        readonly OpenAiService _openAiService;
        readonly TwitchChatService _twitchChatService;
        readonly AzureTextToSpeechService _speechService;
        readonly ChatHistoryService _chatHistoryService;
        readonly IThePandaMonitor _monitor;

        /// <summary>
        /// Creates a new instance of <see cref="TwitchChatResponseService" />
        /// </summary>
        public TwitchChatResponseService(
            IThePandaMonitor monitor,
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

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            await base.StartAsync();
            _ = HeartBeatAsync();
            _ = ResponseAsync();
        }

        /// <summary>
        /// Monitors and responds to chat log
        /// </summary>
        /// <returns></returns>
        public async Task ResponseAsync()
        {
            while (_isRunning)
            {
                await Task.Delay(10_000); // Wait 10 seconds before the next response
                var request = new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem("You are a virtual streamer")
                    },
                    Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo, Temperature = 1, MaxTokens = 1024
                };
                _chatHistoryService.GetAllChat()
                    .ForEach(request.Messages.Add);
                var instruction = ChatMessage.FromUser("Summarize the content you haven't responded to yet and create an interactive response.");
                request.Messages.Add(instruction);

                var response = await _openAiService.CreateCompletionAsync(request);
                _chatHistoryService.AddChat(ChatMessage.FromAssistance(response));
                await _speechService.SpeakAsync(response, "zh-HK");
            }

            await StopAsync();
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
