
using System.Diagnostics;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    /// <summary>
    /// Guides the audience to interact with the streamer
    /// </summary>
    public class AudienceAgent : BasicAutoStartable
    {
        DateTime _lastcommentTime = DateTime.Now;

        // Dependencies
        readonly IPandaMessenger _messenger;
        readonly OpenAiService _openAiService;
        readonly IAiCharacterService _characterService;
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Creates a new instance of <see cref="AudienceAgent" />
        /// </summary>
        public AudienceAgent(IPandaMessenger messenger,
            OpenAiService openAiService,
            IAiCharacterService characterService,
            ChatHistoryService chatHistoryService)
        {
            _messenger = messenger;
            _openAiService = openAiService;
            _characterService = characterService;
            _chatHistoryService = chatHistoryService;
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
                if (DateTime.Now - _lastcommentTime <= TimeSpan.FromSeconds(30))
                {
                    // On do it when there is no chat for 30 seconds
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
                    ChatMessage.FromUser(_chatHistoryService.GenerateChatLog())
                },
                Model = OpenAI.ObjectModels.Models.Gpt_4_1106_preview,
                MaxTokens = 128,
                Temperature = 1,
                Stop = "\n"
            };

            var response = await _openAiService.CreateCompletionAndResponseAsync(request);
            _messenger.Send(new CommentReceivedEventArg
            {
                Comments = new ()
                {
                    new ()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Comment = response,
                        Username = "AWildCat",
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
            _lastcommentTime = DateTime.Now;
        }
    }
}
