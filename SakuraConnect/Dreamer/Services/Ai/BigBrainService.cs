
using System.Text.Json;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.Speech.Core.Services;
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.Connect.Dreamer.Services.Twitch;
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
        readonly SemaphoreSlim _thinkLock = new(1, 1);
        bool _isWaitingForResponse;

        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly IPandaMessenger _messenger;
        readonly IAiCharacterService _characterService;
        readonly ChatMonitorService _chatMonitorService;
        readonly OpenAiService _openAiService;

        /// <summary>
        /// Creates a new instance of <see cref="BigBrainService" />
        /// </summary>
        public BigBrainService(
            IThePandaMonitor monitor,
            IPandaMessenger messenger,
            IAiCharacterService characterService,
            ChatMonitorService chatMonitorService,
            OpenAiService openAiService
        )
        {
            _monitor = monitor;
            _messenger = messenger;
            _characterService = characterService;
            _chatMonitorService = chatMonitorService;
            _openAiService = openAiService;
        }

        /// <summary>
        /// Instructs open ai to think about the chat history
        /// </summary>
        /// <returns></returns>
        public async Task<string> ThinkAsync()
        {
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(_characterService.GetPersonalityPrompt() + "\r\n\r\nOutput requirements\r\n"
	                    + SystemPrompts.OutputJson + " "
                        + SystemPrompts.EmotionAndLanguage),
                    ChatMessage.FromUser(_characterService.GetTopicPrompt())
                },
                Model = OpenAI.ObjectModels.Models.Gpt_4_1106_preview,
                Temperature = 1.21f,
                ResponseFormat = new ResponseFormat { Type = "json_object" },
                MaxTokens = 321,
                TopP = 0.89f,
                FrequencyPenalty = 0.2f,
                PresencePenalty = 0.2f
            };
            var chatlog = _chatMonitorService.CreateForRequest();
            chatlog.ForEach(request.Messages.Add);
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
                var response = await _openAiService.CreateCompletionAndResponseAsync(request);
                _ = ChatLogger.LogOpenAiRequest(request, response, SystemNames.AI);
                var jsonObj = JsonSerializer.Deserialize<OpenAiJsonObject<List<Comment>>>(response, Json.DefaultSerializerOptions);
                var plainComment = string.Join("\n", jsonObj.Data.Select(x => x.Text));
                _messenger.Send(new ThinkResultEventArgs
                {
                    Comments = jsonObj.Data
                });
                _ = ChatLogger.LogAsync("Responded: " + plainComment);
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
        public override async Task StartAsync()
        {
            _messenger.Register<CommentReceivedEventArg>(this, ThinkOnCommentReceived);
            _monitor.Register<SpeechQueueService>(this);

            var result = await ThinkAsync();
            NotifyCommentFinished(result);
            await base.StartAsync();
        }

        /// <summary>
        /// Triggers the think process when a comment is received
        /// </summary>
        /// <param name="obj"></param>
        async void ThinkOnCommentReceived(CommentReceivedEventArg obj)
        {
            if (obj.Comments.All(comment => string.IsNullOrWhiteSpace(comment.Comment) 
                                            || comment.Comment.StartsWith('!')) // Ignore commands
                || obj.Comments.All(comment => comment.Role == SpeechQueueRole.Self)) // Ignore self
            {
                return;
            }

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

            var comment = new CommentData
            {
                Role = SpeechQueueRole.Self,
                Username = SystemNames.AI,
                ReceivedAt = DateTime.Now
            };
	        var result = await ThinkAsync();
            comment.Comment = result;
            _messenger.Send(new CommentReceivedEventArg
            {
                Comments = { comment }
            });
            _thinkLock.Release();
            _isWaitingForResponse = false;
        }

        /// <summary>
        /// Notify the system that the comment has finished loading
        /// </summary>
        /// <param name="text"></param>
        void NotifyCommentFinished(string text)
        {
            var comment = new CommentData
            {
                Role = SpeechQueueRole.Self,
                Username = SystemNames.AI,
                Comment = text,
                ReceivedAt = DateTime.Now
            };
            _messenger.Send(new CommentReceivedEventArg
            {
                Comments = { comment }
            });
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _monitor.UnregisterAll(this);
            _messenger.UnregisterAll(this);
            return base.StopAsync();
        }
    }
}
