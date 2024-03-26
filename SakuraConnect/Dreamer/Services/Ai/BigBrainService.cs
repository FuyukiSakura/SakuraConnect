
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
    /// <remarks>
    /// Creates a new instance of <see cref="BigBrainService" />
    /// </remarks>
    public class BigBrainService(
        IThePandaMonitor monitor,
        IPandaMessenger messenger,
        IAiCharacterService characterService,
        ChatMonitorService chatMonitorService,
        OpenAiService openAiService
        ) : BasicAutoStartable
    {
        readonly SemaphoreSlim _thinkLock = new(1, 1);
        bool _isThinking;

        /// <summary>
        /// Thinks about the chat history and respond to the user
        /// sends the result to the chat monitor
        /// </summary>
        /// <returns></returns>
        async Task ThinkAsync()
        {
            var comment = new CommentData
            {
                Role = SpeechQueueRole.Self,
                Username = SystemNames.AI,
                ReceivedAt = DateTime.Now
            };
            var result = await RequestAsync();
            comment.Comment = result;
            messenger.Send(new CommentReceivedEventArg
            {
                Comments = { comment }
            });
        }


        /// <summary>
        /// Instructs open ai to think about the chat history
        /// </summary>
        /// <returns></returns>
        async Task<string> RequestAsync()
        {
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(characterService.GetPersonalityPrompt() + "\r\n\r\nOutput requirements\r\n"
	                    + SystemPrompts.OutputJson + " "
                        + SystemPrompts.EmotionAndLanguage),
                    ChatMessage.FromUser(characterService.GetTopicPrompt())
                },
                Model = OpenAI.ObjectModels.Models.Gpt_4_turbo_preview,
                Temperature = 1.21f,
                ResponseFormat = new ResponseFormat { Type = "json_object" },
                MaxTokens = 321,
                TopP = 0.89f,
                FrequencyPenalty = 0.2f,
                PresencePenalty = 0.2f
            };
            var chatlog = chatMonitorService.CreateForRequest();
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
                var response = await openAiService.CreateCompletionAndResponseAsync(request);
                _ = ChatLogger.LogOpenAiRequest(request, response, SystemNames.AI);
                var jsonObj = JsonSerializer.Deserialize<OpenAiJsonObject<List<Comment>>>(response, Json.DefaultSerializerOptions);
                var plainComment = string.Join("\n", jsonObj.Data.Select(x => x.Text));
                messenger.Send(new ThinkResultEventArgs
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

        /// <summary>
        /// Responses to the user when the AI has finished speaking
        /// </summary>
        /// <param name="obj"></param>
        async void CheckForNewCommentOnSpeakingEnded(EndedSpeakingEventArg obj)
        {
            if (chatMonitorService.GetLastComment()?.Role == SpeechQueueRole.Self)
            {
                return;
            }
            await _thinkLock.WaitAsync();
            try
            {
                // If a "think" operation is already queued or in progress, do nothing
                if (_isThinking)
                {
                    return;
                }

                _isThinking = true;
            }
            finally
            {
                _thinkLock.Release();
            }

            await Task.Delay(100); // Wait for the chat monitor to process the comment
            await ThinkAsync();

            _isThinking = false;
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

            await _thinkLock.WaitAsync();
            try
            {
                // If a "think" operation is already queued or in progress, do nothing
                if (_isThinking)
                {
                    return;
                }

                _isThinking = true;
            }
            finally
            {
                _thinkLock.Release();
            }

            await Task.Delay(100); // Wait for the chat monitor to process the comment
            await ThinkAsync();

            _isThinking = false;
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            await Task.Yield();
            messenger.Register<CommentReceivedEventArg>(this, ThinkOnCommentReceived);
            messenger.Register<EndedSpeakingEventArg>(this, CheckForNewCommentOnSpeakingEnded);
            monitor.Register<SpeechQueueService>(this);

            await ThinkAsync();
            await base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            monitor.UnregisterAll(this);
            messenger.UnregisterAll(this);
            return base.StopAsync();
        }
    }
}
