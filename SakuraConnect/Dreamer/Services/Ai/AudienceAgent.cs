
using System.Diagnostics;
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.Connect.Dreamer.Services.Twitch;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Models;
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
        /// Generates a comment as a user, leaving no trace of the AI
        /// </summary>
        /// <returns></returns>
        async Task<string> CreateResponseAsync()
        {
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(_characterService.GetAudiencePrompt()),
                    ChatMessage.FromUser(await _chatMonitorService.CreateChatLogAsync())
                },
                Model = OpenAI.ObjectModels.Models.Gpt_4_1106_preview,
                MaxTokens = 128,
                Temperature = 1,
                Stop = "\n"
            };

            var response = await _openAiService.CreateCompletionAndResponseAsync(request);
            _ = ChatLogger.LogOpenAiRequest(request, response, SystemNames.Audience);
            return response;
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StartAsync()
        {
            _messenger.Register<EndedSpeakingEventArg>(this, OnFinishedSpeaking);
            return base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _messenger.UnregisterAll(this);
            return base.StopAsync();
        }

        /// <summary>
        /// Generates a new response when the AI finishes speaking
        /// </summary>
        /// <param name="obj"></param>
        async void OnFinishedSpeaking(EndedSpeakingEventArg obj)
        {
            if (_chatMonitorService.GetLastComment()?.Role != SpeechQueueRole.Self)
            {
                // New comment is received, no need to assist
                return;
            }

            // Wait for 10 seconds to see if there is any new comment
            await Task.Delay(10_000);
            if (_chatMonitorService.GetLastComment()?.Role != SpeechQueueRole.Self)
            {
                // Check again after 10 seconds
                return;
            }

            var speechLine = "Continue";
            try
            {
                speechLine = await CreateResponseAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Audience error: " + e);
            }

            _messenger.Send(new CommentReceivedEventArg
            {
                Comments =
                {
                    new CommentData
                    {
                        Id = Guid.NewGuid().ToString(),
                        Comment = speechLine,
                        Username = SystemNames.Audience,
                        Role = SpeechQueueRole.Guidance
                    }
                }
            });
        }
    }
}
