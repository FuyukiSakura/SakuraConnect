using System.Diagnostics;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Manages to speech queue to provide responsive speech
    /// </summary>
    public class SpeechQueueService : BasicAutoStartable
    {
        /// <summary>
        /// Indicates if the speech queue is speaking
        /// </summary>
        public bool IsSpeaking { get; private set; }

        readonly Dictionary<Guid, SpeechQueueItem> _speechQueue = new();

        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly AzureTextToSpeechService _azureTtsSvc;
        readonly AzureTextAnalyticsService _textAnalyticsService;

        /// <summary>
        /// Creates a new instance of <see cref="SpeechQueueService" />
        /// </summary>
        public SpeechQueueService(
            IThePandaMonitor monitor,
            AzureTextToSpeechService azureTtsSvc,
            AzureTextAnalyticsService textAnalyticsService
        ) {
            _monitor = monitor;
            _azureTtsSvc = azureTtsSvc;
            _textAnalyticsService = textAnalyticsService;
        }

        /// <summary>
        /// Adds a speech item to the queue
        /// </summary>
        /// <param name="speechId"></param>
        /// <param name="role"></param>
        public void Queue(Guid speechId, SpeechQueueRole role)
        {
            _speechQueue.Add(speechId, new SpeechQueueItem
            {
                Role = role
            });
        }

        /// <summary>
        /// Appends the text to the speech item
        /// </summary>
        /// <param name="speechId"></param>
        /// <param name="text"></param>
        public void Append(Guid speechId, string text)
        {
            _speechQueue.TryGetValue(speechId, out var item);
            if (item == null)
            {
                // The speech is already terminated or does not exist
                return;
            }
            item.Text += text;
        }

        /// <summary>
        /// Monitors the speech queue and speaks out the text if there's input
        /// </summary>
        /// <returns></returns>
        public async Task MonitorAsync(CancellationToken cancellationToken)
        {
            while (Status == ServiceStatus.Running
                   || !cancellationToken.IsCancellationRequested)
            {
                // Prioritize user messages
                var speechPair = GetNextSpeech();
                if (speechPair.Key == Guid.Empty)
                {
                    // No input, wait 2 seconds
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    continue;
                }

                // Simply wait for more results before speaking
                speechPair.Value.IsSpeaking = true;
                await WaitForText(speechPair.Value);
                
                if (speechPair.Value.Text != "OUT_OF_CONTEXT")
                {
                    SetLanguage(speechPair.Value);
                    await SpeakAsync(speechPair.Value);
                }
                _speechQueue.Remove(speechPair.Key);
            }
        }

        /// <summary>
        /// Gets the next speech item according to input priority
        /// </summary>
        /// <returns></returns>
        KeyValuePair<Guid, SpeechQueueItem> GetNextSpeech()
        {
            var speechPair = _speechQueue.FirstOrDefault(item => 
                item.Value is { Role: SpeechQueueRole.Master, IsSpeaking: false });
            if (speechPair.Key != Guid.Empty)
            {
                // Master has priority
                return speechPair;
            }

            speechPair = _speechQueue.FirstOrDefault(item => 
                item.Value is { Role: SpeechQueueRole.User, IsSpeaking: false });
            if (speechPair.Key != Guid.Empty)
            {
                // User has second priority
                return speechPair;
            }
            return _speechQueue.FirstOrDefault();
        }

        /// <summary>
        /// Wait for text being input and release after 5 seconds regardless of text existence
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        static async Task WaitForText(SpeechQueueItem item)
        {
            // Simply wait for more results before speaking
            var retries = 0;
            while (item.Text.Length == 0
                   && retries < 5) // abandon the message if no text received in 5 seconds
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                retries++;
            }
        }

        /// <summary>
        /// Sets the language of the speech item
        /// based on the existing text
        /// </summary>
        /// <param name="item"></param>
        void SetLanguage(SpeechQueueItem item)
        {
            var langCode = _textAnalyticsService.DetectLanguage(item.Text);
            var lang = Languages.GetLanguage(langCode);
            item.Language = lang;
        } 

        /// <summary>
        /// Speaks out the queued item text
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        async Task SpeakAsync(SpeechQueueItem item)
        {
            IsSpeaking = true;
            var speakIndex = 0;
            while (speakIndex < item.Text.Length)
            {
                var speakText = item.Text[speakIndex..];
                if (speakText.StartsWith("大豆醬")
                    || speakText.StartsWith("大豆酱")) // Simplified chinese
                {
                    speakText = speakText.Remove(0,3);
                    speakIndex += 3;
                }
                var translated = speakText.Split("Translation:");
                if (translated.Length > 1)
                {
                    speakText = translated[0];
                    speakIndex += "Translation:".Length;
                }

                if (!string.IsNullOrWhiteSpace(speakText)) // Do not speak empty text
                {
                    Debug.WriteLine("Synthesized: " + speakText);
                    await _azureTtsSvc.SpeakAsync(speakText, item.Language);
                }

                if (translated.Length > 1)
                {
                    item.Language = Languages.English;
                }
                speakIndex += speakText.Length;
            }
            IsSpeaking = false;
        }

        /// <summary>
        /// Constantly remove old messages from the queue
        /// </summary>
        /// <returns></returns>
        public async Task CleanupAsync(CancellationToken cancel)
        {
            while (Status == ServiceStatus.Running
                   || !cancel.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                var oldMessages = _speechQueue.Where(item => DateTime.Now - item.Value.TimeStamp > TimeSpan.FromMinutes(1));
                foreach (var message in oldMessages)
                {
                    _speechQueue.Remove(message.Key);   
                }
            }
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            await base.StartAsync();
            _ = MonitorAsync(CancellationTokenSource.Token);
            _ = CleanupAsync(CancellationTokenSource.Token);
            _monitor.Register(this, _textAnalyticsService);
            _monitor.Register(this, _azureTtsSvc);
        }

        /// <summary>
        /// Stops speaking and cleaning up the queue
        /// </summary>
        public override async Task StopAsync()
        {
            await base.StopAsync();
            _monitor.Unregister(this);
            _speechQueue.Clear();
        }
    }
}
