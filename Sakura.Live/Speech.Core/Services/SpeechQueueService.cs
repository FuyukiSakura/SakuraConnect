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
        readonly List<SpeechQueueItem> _speechQueue = new();

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
        /// <param name="item"></param>
        public void Queue(SpeechQueueItem item)
        {
            if (string.IsNullOrEmpty(item.Text) ||
                item.Text == SpeechQueueItem.TerminationText)
            {
                return;
            }
            _speechQueue.Add(item);
        }

        /// <summary>
        /// Speaks an item in the queue
        /// </summary>
        /// <returns></returns>
        public async Task SpeakAsync()
        {
            while (_isRunning)
            {
                // Prioritize user messages
                var item = _speechQueue.FirstOrDefault(item => item.Role == SpeechQueueRole.User) 
                           ?? _speechQueue.FirstOrDefault(); // Or get the first item in the queue
                if (item == null)
                {
                    // No input, wait 5 seconds
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    continue;
                }
                
                _speechQueue.Remove(item);
                await _azureTtsSvc.SpeakAsync(item.Text, item.Language);
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
                var oldMessages = _speechQueue.Where(item => DateTime.Now - item.TimeStamp > TimeSpan.FromMinutes(3));
                _speechQueue.RemoveAll(item => oldMessages.Contains(item));
            }
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            _isRunning = true;
            _ = SpeakAsync();
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
