
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core;

namespace Sakura.Live.Connect.Dreamer.Services.Twitch
{
    /// <summary>
    /// Response to the twitch chat with open ai
    /// </summary>
    /// <remarks>
    /// Creates a new instance of <see cref="AiChatServices" />
    /// </remarks>
    public class AiChatServices(IThePandaMonitor monitor)
    {
        readonly object _monitorLock = new();
        readonly SemaphoreSlim _startStopLock = new(1, 1);
        public bool IsChatResponseRunning { get; set; }
        public bool IsChatMonitorRunning { get; set; }

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
                    monitor.UnregisterAll(this);
                    IsChatResponseRunning = false;
                }
                else
                {
                    monitor.Register<BigBrainService>(this);
                    monitor.Register<AudienceAgent>(this);
                    monitor.Register<ChatMonitorService>(this);
                    monitor.Register<SpeechQueueService>(this);
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
                    monitor.Unregister<ChatMonitorService>(_monitorLock);
                    IsChatMonitorRunning = false;
                }
                else
                {
                    monitor.Register<ChatMonitorService>(_monitorLock);
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
