
using System.Diagnostics;
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.Connect.Dreamer.Services.Twitch;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;
using SakuraConnect.Shared.Models.Messaging.Ai;

namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    /// <summary>
    /// Guides the audience to interact with the streamer
    /// </summary>
    public class AudienceAgent : BasicAutoStartable
    {
        DateTime _lastCommentTime = DateTime.Now;

        // Dependencies
        readonly IPandaMessenger _messenger;
        readonly OpenAiService _openAiService;
        readonly IAiCharacterService _characterService;
        readonly ChatMonitorService _chatMonitorService;

        /// <summary>
        /// Creates a new instance of <see cref="AudienceAgent" />
        /// </summary>
        public AudienceAgent(IPandaMessenger messenger,
            OpenAiService openAiService,
            IAiCharacterService characterService,
            ChatMonitorService chatMonitorService)
        {
            _messenger = messenger;
            _openAiService = openAiService;
            _characterService = characterService;
            _chatMonitorService = chatMonitorService;
        }

        /// <summary>
        /// Monitors the chat and generate response when there is no chat
        /// </summary>
        /// <returns></returns>
        async Task MonitorComment()
        {
            while (Status == ServiceStatus.Running 
                   && !CancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(10_000);
                if (_chatMonitorService.GetLastComment().Role != SpeechQueueRole.Self)
                {
                    // Only generate response when the last message is from the AI
                    _lastCommentTime = DateTime.Now;
                    continue;
                }

                if (DateTime.Now - _lastCommentTime <= TimeSpan.FromSeconds(60))
                {
                    // On do it when there is no chat for 60 seconds
                    // The AI may need to think for a while too Orz
                    continue;
                }

                try
                {
                    await CreateResponseAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Audience error: " + e);
                }
            }
        }

        /// <summary>
        /// Generates a comment as a user, leaving no trace of the AI
        /// </summary>
        /// <returns></returns>
        async Task CreateResponseAsync()
        {
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(_characterService.GetAudiencePrompt()),
                    ChatMessage.FromUser(_chatMonitorService.CreateChatLog())
                },
                Model = OpenAI.ObjectModels.Models.Gpt_4_1106_preview,
                MaxTokens = 128,
                Temperature = 1,
                Stop = "\n"
            };

            var response = await _openAiService.CreateCompletionAndResponseAsync(request);
            _ = ChatLogger.LogOpenAiRequest(request, response, SystemNames.Audience);
            _messenger.Send(new CommentReceivedEventArg
            {
                Comments =
                {
                    new CommentData
                    {
                        Id = Guid.NewGuid().ToString(),
                        Comment = response,
                        Username = SystemNames.Audience,
                        Role = SpeechQueueRole.Guidance
                    }
                }
            });
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StartAsync()
        {
            _messenger.Register<CommentReceivedEventArg>(this, OnCommentReceived);
            _ = MonitorComment();
            return base.StartAsync();
        }

        /// <summary>
        /// Logs the last comment time
        /// </summary>
        /// <param name="obj"></param>
        void OnCommentReceived(CommentReceivedEventArg obj)
        {
            _lastCommentTime = DateTime.Now;
        }
    }
}
