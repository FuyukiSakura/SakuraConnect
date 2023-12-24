
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core;

namespace Sakura.Live.Connect.Dreamer.Services.Twitch
{
    /// <summary>
    /// Response to the twitch chat with open ai
    /// </summary>
    public class AiChatServices
    {
        readonly object _monitorLock = new();
        readonly SemaphoreSlim _startStopLock = new(1, 1);
        public bool IsChatResponseRunning { get; set; }
        public bool IsChatMonitorRunning { get; set; }

        // Dependencies
        readonly IThePandaMonitor _monitor;

        /// <summary>
        /// Creates a new instance of <see cref="AiChatServices" />
        /// </summary>
        public AiChatServices(
            IThePandaMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Starts or stops the service depending on the current state
        /// </summary>
        public async Task ToggleChatResponseAsync()
        {
            await _startStopLock.WaitAsync();
            try
            {
                if (IsChatResponseRunning)
                {
                    _monitor.UnregisterAll(this);
                    IsChatResponseRunning = false;
                }
                else
                {
                    _monitor.Register<BigBrainService>(this);
                    _monitor.Register<AudienceAgent>(this);
                    _monitor.Register<ChatMonitorService>(this);
                    _monitor.Register<SpeechQueueService>(this);
                    IsChatResponseRunning = true;
                }
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        /// <summary>
        /// Toggles the chat monitor as a standalone service
        /// </summary>
        /// <returns></returns>
        public async Task ToggleChatMonitorAsync()
        {
            await _startStopLock.WaitAsync();
            try
            {
                if (IsChatMonitorRunning)
                {
                    _monitor.Unregister<ChatMonitorService>(_monitorLock);
                    IsChatMonitorRunning = false;
                }
                else
                {
                    _monitor.Register<ChatMonitorService>(_monitorLock);
                    IsChatMonitorRunning = true;
                }
            }
            finally
            {
                _startStopLock.Release();
            }
        }
    }
}
