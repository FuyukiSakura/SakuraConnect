using Microsoft.CognitiveServices.Speech;
using Sakura.Live.Speech.Core.Models;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Accesses Azure text-to-speech services
    /// </summary>
    public class AzureTextToSpeechService
    {
        // Dependencies
        readonly AzureSpeechSettingsService _settings;

        /// <summary>
        /// Creates a new instance of <see cref="AzureTextToSpeechService" />
        /// </summary>
        /// <param name="settings"></param>
        public AzureTextToSpeechService(AzureSpeechSettingsService settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Speaks the specified text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language">The language to speak in</param>
        /// <returns></returns>
        public async Task SpeakAsync(string text, string language)
        {
            var speechConfig = SpeechConfig.FromSubscription(_settings.SubscriptionKey, _settings.Region);

            // The language of the voice that speaks.
            using var speechSynthesizer = new SpeechSynthesizer(speechConfig);
            var speechSynthesisResult = await speechSynthesizer.SpeakSsmlAsync(CreateSsml(text, language));
#if DEBUG 
            OutputSpeechSynthesisResult(speechSynthesisResult, text);
#endif
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
        /// <param name="language"></param>
        /// <returns></returns>
        public static string CreateSsml(string text, string language)
        {
            var voice = language switch
            {
                Languages.Mandarin => "zh-TW-HsiaoChenNeural",
                Languages.Japanese => "ja-JP-NanamiNeural",
                Languages.Cantonese => "zh-HK-HiuGaaiNeural",
                _ => "zh-HK-HiuGaaiNeural"
            };

            var rate = language switch
            {
                Languages.Cantonese => "+50%",
                Languages.English => "+20%",
                _ => "+0%"
            };
            return $"<speak version=\"1.0\" xmlns=\"https://www.w3.org/2001/10/synthesis\" xml:lang=\"{language}\"><voice name=\"{voice}\"><prosody pitch=\"+700%\" rate=\"{rate}\">{text}</prosody></voice></speak>";
        }
    }
}
