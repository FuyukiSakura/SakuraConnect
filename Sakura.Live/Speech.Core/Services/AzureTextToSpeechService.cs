using System.Diagnostics;
using Microsoft.CognitiveServices.Speech;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Accesses Azure text-to-speech services
    /// </summary>
    public class AzureTextToSpeechService : BasicAutoStartable
    {
        SpeechSynthesizer? _speechSynthesizer;

        // Dependencies
        readonly AzureSpeechSettingsService _settings;

        /// <summary>
        /// Creates a new instance of <see cref="AzureTextToSpeechService" />
        /// </summary>
        public AzureTextToSpeechService(AzureSpeechSettingsService settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Speaks the specified text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task SpeakAsync(string text)
        {
            if (_speechSynthesizer == null)
            {
                // Service not running
                return;
            }

            // The language of the voice that speaks.
            var speechSynthesisResult = await _speechSynthesizer.SpeakSsmlAsync(CreateSsml(text));
#if DEBUG 
            OutputSpeechSynthesisResult(speechSynthesisResult, text);
#endif
        }

        /// <summary>
        /// Interrupts the speech
        /// </summary>
        /// <returns></returns>
        public async Task InterruptAsync()
        {
            if (_speechSynthesizer == null)
            {
                // Service not running
                return;
            }

            await _speechSynthesizer.StopSpeakingAsync();
        }

#if DEBUG 
        /// <summary>
        /// Debug use
        /// </summary>
        /// <param name="speechSynthesisResult"></param>
        /// <param name="text"></param>
        static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
        {
            switch (speechSynthesisResult.Reason)
            {
                case ResultReason.SynthesizingAudioCompleted:
                    Console.WriteLine($"Speech synthesized for text: [{text}]");
                    break;
                case ResultReason.Canceled:
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
                default:
                    break;
            }
        }
#endif

        /// <summary>
        /// Creates the SSML script for the specified text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CreateSsml(string text)
        {
            return $"<speak version=\"1.0\" xmlns=\"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\"><voice name=\"en-US-JennyMultilingualV2Neural\"><prosody pitch=\"+150%\">{text}</prosody></voice></speak>";
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            var speechConfig = SpeechConfig.FromSubscription(_settings.SubscriptionKey, _settings.Region);
            _speechSynthesizer = new SpeechSynthesizer(speechConfig);
            await base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StopAsync()
        {
            await base.StopAsync();
            await InterruptAsync();
            _speechSynthesizer = null;
        }
    }
}
