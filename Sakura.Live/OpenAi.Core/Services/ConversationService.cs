﻿
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
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Gets the current Message Queue
        /// </summary>
        public string MessageQueue { get; private set; }= "";

        /// <summary>
        /// Gets the reply language
        /// </summary>
        public string ReplyLanguage { get; private set; } = "";

        /// <summary>
        /// Gets or sets the character of the conversational AI
        /// </summary>
        public string Prompt { get; set; } = "You are a vtuber.";

        /// <summary>
        /// Creates a new instance of <see cref="ConversationService" />
        /// </summary>
        public ConversationService(
            ISettingsService settingsService,
            IThePandaMonitor monitor,
            OpenAiService openAiSvc,
            ChatHistoryService chatHistoryService
        ) {
            _settingsService = settingsService;
            _monitor = monitor;
            _openAiSvc = openAiSvc;
            _chatHistoryService = chatHistoryService;
            LoadSettings();
        }

        /// <summary>
        /// Talks to the AI and get a response
        /// based on the conversation
        /// </summary>
        /// <returns></returns>
        public async Task<string> TalkAsync()
        {
            var prompt = Prompt;
            if (ReplyLanguage == "zh-HK")
            {
                prompt += ". Respond in Cantonese";
            }
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(prompt)
                },
                Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo,
                Temperature = 1,
                MaxTokens = 1024
            };
            var chatMessage = ChatMessage.FromUser(MessageQueue);
            MessageQueue = ""; // Reset

            _chatHistoryService.AddChat(chatMessage);
            _chatHistoryService.GetAllChat().ForEach(request.Messages.Add);
            var completionResult = await _openAiSvc.Get().ChatCompletion.CreateCompletion(request);
            if (!completionResult.Successful) return "Sorry, I didn't get that";

            var response = completionResult.Choices.First().Message.Content;
            _chatHistoryService.AddChat(ChatMessage.FromAssistance(response));
            return response;
        }

        /// <summary>
        /// Queues the sentences
        /// </summary>
        /// <param name="message"></param>
        /// <param name="language">The language in the current message the user said</param>
        public void Queue(string message, string language)
        {
            MessageQueue += message;
            ReplyLanguage = language;
        }

        /// <summary>
        /// Checks if the message queue is empty
        /// </summary>
        public bool IsQueueEmpty => MessageQueue == "";

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
