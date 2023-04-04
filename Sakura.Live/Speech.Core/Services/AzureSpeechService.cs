
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Accesses Azure Speech Service
    /// </summary>
    public class AzureSpeechService : BasicAutoStartable
    {
        // Dependencies
        readonly AzureSpeechSettingsService _settings;

        SpeechRecognizer? _recognizer;
        public event EventHandler<SpeechRecognitionEventArgs>? Recognizing;
        public event EventHandler<SpeechRecognitionEventArgs>? Recognized;
        public event EventHandler<SpeechRecognitionCanceledEventArgs>? Canceled;

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
        /// <param name="settings"></param>
        public AzureSpeechService(AzureSpeechSettingsService settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Starts azure-stt session
        /// </summary>
        /// <returns></returns>
        public override async Task StartAsync()
        {
            _settings.Save();
            if (_recognizer != null)
            {
                await StopAsync();
            }

            // Currently the v2 endpoint is required. In a future SDK release you won't need to set it.
            var endpointString = $"wss://{_settings.Region}.stt.speech.microsoft.com/speech/universal/v2";
            var endpointUrl = new Uri(endpointString);

            var config = SpeechConfig.FromEndpoint(endpointUrl, _settings.SubscriptionKey);
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
