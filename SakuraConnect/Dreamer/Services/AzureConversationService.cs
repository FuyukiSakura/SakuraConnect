
using System.Text;
using Microsoft.CognitiveServices.Speech;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.Connect.Dreamer.Services.Twitch;
using Sakura.Live.Speech.Core.Models;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services
{
    /// <summary>
    /// Uses Azure speech services to get input
    /// and open ai to generate responses to user input
    /// </summary>
    public class AzureConversationService
    {
        /// <summary>
        /// Records the user spoken input
        /// </summary>
        readonly StringBuilder _messageQueue = new();

        /// <summary>
        /// Records the language of the user input
        /// </summary>
        string _replyLanguage = "";

        /// <summary>
        /// Checks if the message queue is empty
        /// </summary>
        bool IsQueueEmpty => _messageQueue.Length == 0;

        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly IPandaMessenger _messenger;
        readonly AzureSpeechService _speechService;
        readonly AzureTextToSpeechService _ttsService;

        DateTime _lastInputTime = DateTime.Now;
        bool _isRunning;

        /// <summary>
        /// Is fired when the conversation service gets a response
        /// </summary>
        public event EventHandler<string> OnResponse;

        /// <summary>
        /// Creates a new instance of <see cref="AzureConversationService" />
        /// </summary>
        public AzureConversationService(
            IThePandaMonitor monitor,
            IPandaMessenger messenger,
            AzureSpeechService speechService,
            AzureTextToSpeechService ttsService)
        {
            _monitor = monitor;
            _messenger = messenger;
            _speechService = speechService;
            _ttsService = ttsService;
        }

        /// <summary>
        /// Asks open ai for a response when receiving a speech input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void OnSpeechRecognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Result.Text)
                || e.Result.Text.Length < 5) // Ignore short inputs
            {
                // Do not fire for empty inputs
                return;
            }
            
            var languageResult = AutoDetectSourceLanguageResult.FromResult(e.Result);
            _messageQueue.Append(e.Result.Text);
            _replyLanguage = languageResult.Language;
            _lastInputTime = DateTime.Now;
            await ChatLogger.LogAsync($"Recognized({languageResult.Language}): {e.Result.Text}");
        }

        /// <summary>
        /// Process the user input
        /// </summary>
        /// <returns></returns>
        async Task ListenAsync()
        {
            while (_isRunning)
            {
                while (IsQueueEmpty
                       || (DateTime.Now - _lastInputTime).TotalSeconds < 3) // Wait for 3 seconds of silence
                {
                    await Task.Delay(500);
                }

                _ = ChatLogger.LogAsync("Input: " + _messageQueue);
                var speech = _messageQueue.ToString();
                _messageQueue.Clear();
                if (_replyLanguage == "zh-HK")
                {
                    speech += "(In Cantonese)";
                }
                _messenger.Send(new CommentReceivedEventArg
                {
                    Comments = new List<CommentData>
                    {
	                    new ()
	                    {
		                    Comment = speech,
		                    Username = "冬雪桜"
	                    }
                    }
                });
            }
        }

        /// <summary>
        /// Interrupts the AI speech when user has spoken for 5 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void OnSpeechRecognizing(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason != ResultReason.RecognizingSpeech
                || e.Result.Duration < TimeSpan.FromSeconds(2))
            {
                return;
            }
            
            await _ttsService.InterruptAsync();
        }

        /// <summary>
        /// Starts listening to user input and generate outputs
        /// </summary>
        public void Start()
        {
            _speechService.Recognized += OnSpeechRecognized;
            _speechService.Recognizing += OnSpeechRecognizing;
            _monitor.Register<ChatMonitorService>(this);
            _monitor.Register<BigBrainService>(this);
            _monitor.Register<AzureSpeechService>(this);
            _isRunning = true;
            _ = ListenAsync();
        }

        /// <summary>
        /// Stops listening to user input
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _monitor.Unregister(this);
            _speechService.Recognized -= OnSpeechRecognized;
        }
    }
}
