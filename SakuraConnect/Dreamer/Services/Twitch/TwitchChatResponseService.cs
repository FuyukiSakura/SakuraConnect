﻿
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.Twitch.Core.Services;

namespace Sakura.Live.Connect.Dreamer.Services.Twitch
{
    /// <summary>
    /// Response to the twitch chat with open ai
    /// </summary>
    public class TwitchChatResponseService : BasicAutoStartable
    {
        bool _isRunning;
        ChatMessage _lastRespondedMessage;
        DateTime _entryReceived = DateTime.MinValue;

        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly BigBrainService _brainService;
        readonly OpenAiService _openAiService;
        readonly TwitchChatService _twitchChatService;
        readonly TwitchChatMonitorService _twitchChatMonitorService;
        readonly SpeechQueueService _speechService;
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Creates a new instance of <see cref="TwitchChatResponseService" />
        /// </summary>
        public TwitchChatResponseService(
            IThePandaMonitor monitor,
            BigBrainService brainService,
            OpenAiService openAiService,
            TwitchChatService twitchChatService,
            TwitchChatMonitorService twitchChatMonitorService,
            SpeechQueueService speechService,
            ChatHistoryService chatHistoryService
        )
        {
            _openAiService = openAiService;
            _twitchChatService = twitchChatService;
            _twitchChatMonitorService = twitchChatMonitorService;
            _speechService = speechService;
            _chatHistoryService = chatHistoryService;
            _monitor = monitor;
            _brainService = brainService;
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
        /// Monitors and responds to chat log
        /// </summary>
        /// <returns></returns>
        public async Task ResponseAsync()
        {
            while (_isRunning)
            {
                await WaitUserInput();
                _entryReceived = DateTime.Now;
                _lastRespondedMessage = _chatHistoryService.GetLastUserMessage();
                await ChatLogger.LogAsync("Started responding to: " + _lastRespondedMessage?.Content);
                await GenerateResponseAsync(SpeechQueueRole.User, "Responded");
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
                if (_speechService.IsSpeaking)
                {
                    // Don't queue if the bot is speaking
                    await Task.Delay(10_000);
                    continue;
                }

                if (DateTime.Now - _entryReceived < TimeSpan.FromSeconds(20))
                {
                    await Task.Delay(10_000);
                    continue;
                }

                _entryReceived = DateTime.Now;
                await GenerateResponseAsync(SpeechQueueRole.Self, "Soliloquize");
            }
        }

        /// <summary>
        /// Generates response and add to the queue
        /// </summary>
        /// <param name="role">The role of the requester</param>
        /// <param name="logPrefix">The prefix of the log item</param>
        /// <returns></returns>
        async Task GenerateResponseAsync(SpeechQueueRole role, string logPrefix)
        {
            var response = await _brainService.ThinkAsync(role);
            await ChatLogger.LogAsync($"{logPrefix}: {response}");
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
            _entryReceived = DateTime.Now;
            _ = Task.Run(ResponseAsync);
            _ = Task.Run(SoliloquizeAsync);
        }

        /// <summary>
        /// Starts responding to chat log
        /// </summary>
        public void Start()
        {
            _isRunning = true;
            _monitor.Register<TwitchChatService>(this);
            _monitor.Register<TwitchChatMonitorService>(this);
            _monitor.Register<OpenAiService>(this);
            _monitor.Register<SpeechQueueService>(this);
            _monitor.Register<TwitchChatResponseService>(this);
        }

        /// <summary>
        /// Stops responding to chat log
        /// </summary>
        public void Stop()
        {
            _monitor.Unregister(this);
            _isRunning = false;
        }
    }
}