using System.Diagnostics;
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;
using SakuraConnect.Shared.Models.Messaging.Ai;

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
        readonly IPandaMessenger _messenger;
        readonly AzureTextToSpeechService _azureTtsSvc;
        readonly AzureTextAnalyticsService _textAnalyticsService;

        /// <summary>
        /// Creates a new instance of <see cref="SpeechQueueService" />
        /// </summary>
        public SpeechQueueService(
            IThePandaMonitor monitor,
            IPandaMessenger messenger,
            AzureTextToSpeechService azureTtsSvc,
            AzureTextAnalyticsService textAnalyticsService
        ) {
            _monitor = monitor;
	        _messenger = messenger;
            _azureTtsSvc = azureTtsSvc;
            _textAnalyticsService = textAnalyticsService;
        }

        /// <summary>
        /// Adds a speech item to the queue
        /// </summary>
        /// <param name="speechId"></param>
        /// <param name="text"></param>
        /// <param name="role"></param>
        public void Queue(Guid speechId, string text, SpeechQueueRole role)
        {
            _speechQueue.Add(speechId, new SpeechQueueItem
            {
                Role = role,
                Text = text,
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
                // await WaitForText(speechPair.Value);
                await SpeakAsync(speechPair.Value);
                _speechQueue.Remove(speechPair.Key);

                if (_speechQueue.Count == 0)
                {
                    // No more messages, send finished speaking event
                    _messenger.Send(new EndedSpeakingEventArg());
                }
                // Take short brake before next speech
                await Task.Delay(500);
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
        /// Speaks out the queued item text
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        async Task SpeakAsync(SpeechQueueItem item)
        {
            IsSpeaking = true;

            if (!string.IsNullOrWhiteSpace(item.Text)) // Do not speak empty text
            {
                Debug.WriteLine("Synthesized: " + item.Text);
                await _azureTtsSvc.SpeakAsync(item.Text);
            }
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

        /// <summary>
        /// Queues the result from the brain service
        /// </summary>
        /// <param name="obj"></param>
        void OnResultReceived(ThinkResultEventArgs obj)
        {
	        Queue(Guid.NewGuid(), obj.PlainText, SpeechQueueRole.User);
        }

        ///
        /// <inheritdoc />
        ///
        public override async Task StartAsync()
        {
            await base.StartAsync();
            _messenger.Register<ThinkResultEventArgs>(this, OnResultReceived);
            _ = MonitorAsync(CancellationTokenSource.Token);
            _ = CleanupAsync(CancellationTokenSource.Token);
            _monitor.Register<AzureTextAnalyticsService>(this);
            _monitor.Register<AzureTextToSpeechService>(this);
        }

        /// <summary>
        /// Stops speaking and cleaning up the queue
        /// </summary>
        public override async Task StopAsync()
        {
            await base.StopAsync();
            _monitor.UnregisterAll(this);
            _messenger.UnregisterAll(this);
            _speechQueue.Clear();
        }
    }
}
