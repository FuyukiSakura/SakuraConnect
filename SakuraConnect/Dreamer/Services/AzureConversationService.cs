
using Microsoft.CognitiveServices.Speech;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Speech.Core.Services;
using Sakura.Live.ThePanda.Core;

namespace Sakura.Live.Connect.Dreamer.Services
{
    /// <summary>
    /// Uses Azure speech services to get input
    /// and open ai to generate responses to user input
    /// </summary>
    public class AzureConversationService
    {
        // Dependencies
        readonly IThePandaMonitor _monitor;
        readonly ChatLoggingService _chatLoggingService;
        readonly ConversationService _conversationService;
        readonly AzureSpeechService _speechService;
        readonly AzureTextToSpeechService _textToSpeechService;

        DateTime _lastResponse = DateTime.Now;
        bool _isRunning;

        /// <summary>
        /// Is fired when the conversation service gets a response
        /// </summary>
        public event EventHandler<string> OnResponse;

        /// <summary>
        /// Creates a new instance of <see cref="AzureConversationService" />
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="chatLoggingService"></param>
        /// <param name="conversationService"></param>
        /// <param name="speechService"></param>
        /// <param name="textToSpeechService"></param>
        public AzureConversationService(
            IThePandaMonitor monitor,
            ChatLoggingService chatLoggingService,
            ConversationService conversationService,
            AzureSpeechService speechService,
            AzureTextToSpeechService textToSpeechService)
        {
            _monitor = monitor;
            _chatLoggingService = chatLoggingService;
            _conversationService = conversationService;
            _speechService = speechService;
            _textToSpeechService = textToSpeechService;
        }

        /// <summary>
        /// Asks open ai for a response when receiving a speech input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void OnSpeechRecognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Result.Text))
            {
                // Do not fire for empty inputs
                return;
            }
            _conversationService.Queue(e.Result.Text);
            _lastResponse = DateTime.Now;
            await _chatLoggingService.LogAsync("Recognized: " + e.Result.Text);
        }

        /// <summary>
        /// Process the user input
        /// </summary>
        /// <returns></returns>
        async Task TalkAsync()
        {
            while (_isRunning)
            {
                while (_conversationService.IsQueueEmpty
                       || (DateTime.Now - _lastResponse).TotalSeconds < 3) // 3 Seconds interval
                {
                    await Task.Delay(500);
                }

                _ = _chatLoggingService.LogAsync("Input: " + _conversationService.MessageQueue);
                var response = await _conversationService.TalkAsync();
                OnResponse?.Invoke(this, response);
                _ = _chatLoggingService.LogAsync("AI: " + response + Environment.NewLine);
                await _textToSpeechService.SpeakAsync(response);
            }
        }

        /// <summary>
        /// Starts listening to user input and generate outputs
        /// </summary>
        public void Start()
        {
            _speechService.Recognized += OnSpeechRecognized;
            _monitor.Register(this, _conversationService);
            _monitor.Register(this, _speechService);
            _isRunning = true;
            _ = TalkAsync();
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
