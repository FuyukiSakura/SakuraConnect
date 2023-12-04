
using System.Diagnostics;
using System.Text.Json;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.Speech.Core.Services;
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;
using SakuraConnect.Shared.Models.Messaging;
using SakuraConnect.Shared.Models.Messaging.Ai;

namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    /// <summary>
    /// The big brain of the AI
    /// The class handles the logic of how the AI prioritize and process messages
    /// </summary>
    public class BigBrainService : BasicAutoStartable
    {
        SemaphoreSlim _thinkLock = new(1, 1);
        bool _isWaitingForResponse;

        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly IPandaMessenger _messenger;
        readonly IAiCharacterService _characterService;
        readonly OpenAiService _openAiService;
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Creates a new instance of <see cref="BigBrainService" />
        /// </summary>
        public BigBrainService(
            IThePandaMonitor monitor,
            IPandaMessenger messenger,
            IAiCharacterService characterService,
            OpenAiService openAiService,
            ChatHistoryService chatHistoryService
        )
        {
            _monitor = monitor;
            _messenger = messenger;
            _characterService = characterService;
            _openAiService = openAiService;
            _chatHistoryService = chatHistoryService;
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
                    ChatMessage.FromSystem(_characterService.GetPersonalityPrompt() + " "
                        + SystemPrompts.SeparateLanguageForTts + " "
	                    + SystemPrompts.OutputJson + " "
                        + SystemPrompts.EmotionAndLanguage)
                },
                Model = OpenAI.ObjectModels.Models.Gpt_4_1106_preview,
                Temperature = 1,
                ResponseFormat = new ResponseFormat { Type="json_object" },
                MaxTokens = 512
            };
            var chatlog = _chatHistoryService.GenerateChatLog();
            chatlog.ForEach(request.Messages.Add);
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
                var response = await _openAiService.CreateCompletionAndResponseAsync(request);
                var jsonObj = JsonSerializer.Deserialize<OpenAiJsonObject<List<Comment>>>(response, Json.DefaultSerializerOptions);
                var plainComment = string.Join("\n", jsonObj.Data.Select(x => x.Text));
                _messenger.Send(new ThinkResultEventArgs
                {
                    Comments = jsonObj.Data
                });
                _chatHistoryService.AddChat(ChatMessage.FromAssistant(plainComment));
                return plainComment;
            }
            catch (Exception e)
            {
                await ChatLogger.LogAsync(e.Message);
                return "Sorry, my brain stops working.";
            }
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StartAsync()
        {
            _messenger.Register<CommentReceivedEventArg>(this, ThinkOnCommentReceived);
            _monitor.Register<SpeechQueueService>(this);
            return base.StartAsync();
        }

        /// <summary>
        /// Triggers the think process when a comment is received
        /// </summary>
        /// <param name="obj"></param>
        async void ThinkOnCommentReceived(CommentReceivedEventArg obj)
        {
            await Task.Delay(100); // Wait for the comment to be processed
	        if (_isWaitingForResponse)
	        {
                // Do not start new threads if the AI is still thinking
                // and a new comment is already queued
		        return;
	        }

	        if (_thinkLock.CurrentCount == 0)
	        {
                // Thinking is already in progress
                _isWaitingForResponse = true;
	        }
            await _thinkLock.WaitAsync();
	        await ThinkAsync(SpeechQueueRole.User);
            _thinkLock.Release();
            _isWaitingForResponse = false;
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _monitor.UnregisterAll(this);
            return base.StopAsync();
        }
    }
}
