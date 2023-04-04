
using OpenAI.GPT3.ObjectModels.RequestModels;
using Sakura.Live.OpenAi.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.OpenAi.Core.Services
{
    /// <summary>
    /// Conversational AI
    /// </summary>
    public class ConversationService : BasicAutoStartable
    {
        readonly ISettingsService _settingsService;
        readonly IThePandaMonitor _monitor;
        readonly OpenAiService _openAiSvc;
        readonly List<ChatMessage> _chatHistory = new();
        string _messageQueue = "";

        /// <summary>
        /// Gets or sets the character of the conversational AI
        /// </summary>
        public string Prompt { get; set; } = "You are a vtuber.";

        /// <summary>
        /// Creates a new instance of <see cref="ConversationService" />
        /// </summary>
        /// <param name="settingsService"></param>
        /// <param name="monitor"></param>
        /// <param name="openAiSvc"></param>
        public ConversationService(
            ISettingsService settingsService,
            IThePandaMonitor monitor,
            OpenAiService openAiSvc
        )
        {
            _settingsService = settingsService;
            _monitor = monitor;
            _openAiSvc = openAiSvc;
            LoadSettings();
        }

        /// <summary>
        /// Talks to the AI and get a response
        /// based on the conversation
        /// </summary>
        /// <returns></returns>
        public async Task<string> TalkAsync()
        {
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(Prompt)
                },
                Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo,
                Temperature = 1,
                MaxTokens = 1024
            };
            var chatMessage = ChatMessage.FromUser(_messageQueue);
            _messageQueue = ""; // Reset
            AddChatHistory(chatMessage);
            _chatHistory.ForEach(request.Messages.Add);
            var completionResult = await _openAiSvc.Get().ChatCompletion.CreateCompletion(request);
            if (!completionResult.Successful) return "Sorry, I didn't get that";

            var response = completionResult.Choices.First().Message.Content;
            AddChatHistory(ChatMessage.FromAssistance(response));
            return response;
        }

        /// <summary>
        /// Queues the sentences
        /// </summary>
        /// <param name="message"></param>
        public void Queue(string message)
        {
            _messageQueue += message;
        }

        /// <summary>
        /// Checks if the message queue is empty
        /// </summary>
        public bool IsQueueEmpty => _messageQueue == "";

        /// <summary>
        /// Adds a chat message to the history
        /// </summary>
        /// <param name="message"></param>
        void AddChatHistory(ChatMessage message)
        {
            if (_chatHistory.Count > 10)
            {
                _chatHistory.RemoveAt(0); // Remove first element if length exceeds max
            }

            _chatHistory.Add(message);
        }

        /// <summary>
        /// Loads OpenAI conversation settings from the system
        /// </summary>
        void LoadSettings()
        {
            Prompt = _settingsService.Get(OpenAiPreferenceKeys.ConversationPrompt, Prompt);
        }

        /// <summary>
        /// Saves OpenAI conversation settings to the system
        /// </summary>
        void SaveSettings()
        {
            _settingsService.Set(OpenAiPreferenceKeys.ConversationPrompt, Prompt);
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            SaveSettings();
            _monitor.Register(this, _openAiSvc);
            await base.StartAsync();
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
