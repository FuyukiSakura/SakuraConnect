
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Connect.Dreamer.Services.Twitch
{
    /// <summary>
    /// Response to the twitch chat with open ai
    /// </summary>
    public class ChatResponseService : BasicAutoStartable
    {
        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly ChatHistoryService _chatHistoryService;

        /// <summary>
        /// Creates a new instance of <see cref="ChatResponseService" />
        /// </summary>
        public ChatResponseService(
            IThePandaMonitor monitor,
            ChatHistoryService chatHistoryService
        )
        {
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

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            await base.StartAsync();
            _monitor.Register<BigBrainService>(this);
            _monitor.Register<AudienceAgent>(this);
            _monitor.Register<ChatMonitorService>(this);
            _monitor.Register<SpeechQueueService>(this);
        }

        /// <summary>
        /// Stops responding to chat log
        /// </summary>
        public override Task StopAsync()
        {
	        _monitor.UnregisterAll(this);
	        return base.StopAsync();
        }
    }
}
