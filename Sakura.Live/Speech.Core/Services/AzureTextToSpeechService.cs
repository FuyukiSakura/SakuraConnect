using Microsoft.CognitiveServices.Speech;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Accesses Azure text-to-speech services
    /// </summary>
    public class AzureTextToSpeechService
    {
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
        /// <returns></returns>
        public async Task SpeakAsync(string text)
        {
            var speechConfig = SpeechConfig.FromSubscription(_settings.SubscriptionKey, _settings.Region);

            // The language of the voice that speaks.
            using var speechSynthesizer = new SpeechSynthesizer(speechConfig);
            var speechSynthesisResult = await speechSynthesizer.SpeakSsmlAsync(CreateSsml(text));
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
        /// <returns></returns>
        public static string CreateSsml(string text) => 
            $"<speak version=\"1.0\" xmlns=\"https://www.w3.org/2001/10/synthesis\" xml:lang=\"zh-HK\"><voice name=\"zh-HK-HiuGaaiNeural\"><prosody pitch=\"+700%\" rate=\"+30.00%\">{text}</prosody></voice></speak>";
    }
}
