
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Accesses Azure Speech Service
    /// </summary>
    public class AzureSpeechService : BasicAutoStartable
    {
        // Dependencies
        readonly ISettingsService _settingsService;

        SpeechRecognizer? _recognizer;
        public EventHandler<SpeechRecognitionEventArgs>? Recognizing;
        public EventHandler<SpeechRecognitionEventArgs>? Recognized;
        public EventHandler<SpeechRecognitionCanceledEventArgs>? Canceled;

        /// <summary>
        /// Gets or sets the subscription key for Azure Speech Service
        /// </summary>
        public string SubscriptionKey { get; set; } = "";

        /// <summary>
        /// Gets or sets the region for Azure Speech Service
        /// </summary>
        public string Region { get; set; } = "";

        /// <summary>
        /// Gets the languages the user is going to speak
        /// </summary>
        public List<string> SpeechLanguages { get; set; } = new ()
        {
            Languages.English,
            Languages.Mandarin,
            Languages.Japanese,
            Languages.Cantonese
        };

        /// <summary>
        /// Creates a new instance of <see cref="AzureSpeechService" />
        /// </summary>
        /// <param name="settingsService"></param>
        public AzureSpeechService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();
        }

        /// <summary>
        /// Starts azure-stt session
        /// </summary>
        /// <returns></returns>
        public override async Task StartAsync()
        {
            SaveSettings();
            if (_recognizer != null)
            {
                await StopAsync();
            }

            // Currently the v2 endpoint is required. In a future SDK release you won't need to set it.
            var endpointString = $"wss://{Region}.stt.speech.microsoft.com/speech/universal/v2";
            var endpointUrl = new Uri(endpointString);

            var config = SpeechConfig.FromEndpoint(endpointUrl, SubscriptionKey);
            var autoDetectSourceLanguageConfig =
                AutoDetectSourceLanguageConfig.FromLanguages(SpeechLanguages.ToArray());

            using var audioInput = AudioConfig.FromDefaultMicrophoneInput();
            _recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, audioInput);

            _recognizer.Recognizing += Recognizing;
            _recognizer.Recognized += Recognized;
            _recognizer.Canceled += RecognizerOnCanceled;
            _recognizer.Canceled += Canceled;
            _recognizer.SessionStopped += RecognizerOnSessionStopped;

            // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
            await _recognizer.StartContinuousRecognitionAsync();
            await HeartBeatAsync();
        }

        /// <summary>
        /// Stops azure-stt session if the SDK fires a stopped event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void RecognizerOnSessionStopped(object? sender, SessionEventArgs e)
        {
            await StopAsync();
        }

        /// <summary>
        /// Stops azure-stt recognition when server returns a cancelled event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void RecognizerOnCanceled(object? sender, SpeechRecognitionCanceledEventArgs e)
        {
            Status = e.Reason == CancellationReason.Error ? ServiceStatus.Error : ServiceStatus.Stopped;
            await StopAsync();
        }

        /// <summary>
        /// Stops the recognition service
        /// </summary>
        public override async Task StopAsync()
        {
            if (_recognizer != null)
            {
                await _recognizer.StopContinuousRecognitionAsync();
            }
            await base.StopAsync();
        }

        /// <summary>
        /// Saves Azure Speech settings to the system
        /// </summary>
        void SaveSettings()
        {
            _settingsService.Set(AzureSpeechPreferenceKeys.SubscriptionKey, SubscriptionKey);
            _settingsService.Set(AzureSpeechPreferenceKeys.Region, Region);
        }

        /// <summary>
        /// Loads Azure Speech settings from the system
        /// </summary>
        void LoadSettings()
        {
            SubscriptionKey = _settingsService.Get(AzureSpeechPreferenceKeys.SubscriptionKey, "");
            Region = _settingsService.Get(AzureSpeechPreferenceKeys.Region, "");
        }

        /// <summary>
        /// Updates the heart beat timer when
        /// session stop is not detected
        /// </summary>
        /// <returns></returns>
        async Task HeartBeatAsync()
        {
            Status = ServiceStatus.Running;
            while (Status == ServiceStatus.Running)
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default);
            }
        }
    }
}
