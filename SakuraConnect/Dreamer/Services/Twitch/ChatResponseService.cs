
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services.Twitch
{
    /// <summary>
    /// Response to the twitch chat with open ai
    /// </summary>
    public class ChatResponseService : BasicAutoStartable
    {
        // Dependencies
        readonly IThePandaMonitor _monitor;

        /// <summary>
        /// Creates a new instance of <see cref="ChatResponseService" />
        /// </summary>
        public ChatResponseService(
            IThePandaMonitor monitor,
            IPandaMessenger messenger)
        {
            _monitor = monitor;
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
