using Microsoft.JSInterop;
using Sakura.Live.Cognitive.Translation.Core.Services;
using Sakura.Live.Obs.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services
{
    public class ObsSubtitleService
    {
        readonly SemaphoreSlim _startStopLock = new(1, 1);

        public bool IsRunning { get; set; } = true;

        // Dependency injection
        readonly IThePandaMonitor _monitor;
        readonly IPandaMessenger _messenger;
        readonly ObsSocketService _obs;
        readonly TranslationService _translationService;

        /// <summary>
        /// Creates a new instance of <see cref="ObsSubtitleService" />
        /// </summary>
        public ObsSubtitleService(
            IThePandaMonitor monitor,
            IPandaMessenger messenger,
            ObsSocketService obs,
            TranslationService translationService)
        {
            _monitor = monitor;
            _obs = obs;
            _messenger = messenger;
            _translationService = translationService;
        }

        public async Task StartStopAsync()
        {
            await _startStopLock.WaitAsync();
            if (IsRunning)
            {
                Stop();
                IsRunning = false;
            }
            else
            {
                Start();
                IsRunning = true;
            }
            _startStopLock.Release();
        }

        void Start()
        {
            _translationService.SaveSettings();
            _monitor.Register<ObsSocketService>(this);

        }

        void Stop()
        {
            _monitor.UnregisterAll(this);
        }

        /// <summary>
        /// Handles the result from speech recognition service
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [JSInvokable("VoiceRecognized")]
        public void Recognize_OnResult(string text)
        {
            
        }
    }
}
