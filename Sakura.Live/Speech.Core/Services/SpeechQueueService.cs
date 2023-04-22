using System.Diagnostics;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core.Helpers;

namespace Sakura.Live.Speech.Core.Services
{
    /// <summary>
    /// Manages to speech queue to provide responsive speech
    /// </summary>
    public class SpeechQueueService : BasicAutoStartable
    {
        bool _isRunning;
        readonly Dictionary<Guid, SpeechQueueItem> _speechQueue = new();

        // Dependencies
        readonly AzureTextToSpeechService _azureTtsSvc;

        /// <summary>
        /// Creates a new instance of <see cref="SpeechQueueService" />
        /// </summary>
        public SpeechQueueService(AzureTextToSpeechService azureTtsSvc)
        {
            _azureTtsSvc = azureTtsSvc;
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
        public async Task MonitorAsync()
        {
            while (_isRunning)
            {
                // Prioritize user messages
                var speechPair = _speechQueue.FirstOrDefault(item => item.Value.Role == SpeechQueueRole.User);
                if (speechPair.Key == Guid.Empty)
                {
                    // No input, wait 2 seconds
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    continue;
                }

                // Simply wait for more results before speaking
                await WaitForText(speechPair.Value);
                await SpeakAsync(speechPair.Value);
                _speechQueue.Remove(speechPair.Key);
            }
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
                await Task.Delay(TimeSpan.FromSeconds(1));
                retries++;
            }
        }

        /// <summary>
        /// Speaks out the queued item text
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        async Task SpeakAsync(SpeechQueueItem item)
        {
            var speakIndex = 0;
            while (speakIndex < item.Text.Length)
            {
                var speakText = item.Text[speakIndex..];
                Debug.WriteLine("Synthesized: " + speakText);
                await _azureTtsSvc.SpeakAsync(speakText, item.Language);
                speakIndex += speakText.Length;
            }
        }

        /// <summary>
        /// Constantly remove old messages from the queue
        /// </summary>
        /// <returns></returns>
        public async Task CleanupAsync()
        {
            while (_isRunning)
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
            _isRunning = true;
            _ = MonitorAsync();
            _ = CleanupAsync();
            await base.StartAsync();
        }

        /// <summary>
        /// Stops speaking and cleaning up the queue
        /// </summary>
        public override async Task StopAsync()
        {
            await base.StopAsync();
            _isRunning = false;
            _speechQueue.Clear();
        }
    }
}
