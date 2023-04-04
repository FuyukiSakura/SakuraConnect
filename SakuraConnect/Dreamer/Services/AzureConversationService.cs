
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
        readonly ConversationService _conversationService;
        readonly AzureSpeechService _speechService;

        /// <summary>
        /// Is fired when the conversation service gets a response
        /// </summary>
        public event EventHandler<string> OnResponse;

        /// <summary>
        /// Creates a new instance of <see cref="AzureConversationService" />
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="conversationService"></param>
        /// <param name="speechService"></param>
        public AzureConversationService(
            IThePandaMonitor monitor,
            ConversationService conversationService,
            AzureSpeechService speechService)
        {
            _monitor = monitor;
            _conversationService = conversationService;
            _speechService = speechService;
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
            var response = await _conversationService.TalkToAsync(e.Result.Text);
            OnResponse?.Invoke(this, response);
        }

        /// <summary>
        /// Starts listening to user input and generate outputs
        /// </summary>
        public void Start()
        {
            _speechService.Recognized += OnSpeechRecognized;
            _monitor.Register(this, _conversationService);
            _monitor.Register(this, _speechService);
        }

        /// <summary>
        /// Stops listening to user input
        /// </summary>
        public void Stop()
        {
            _monitor.Unregister(this);
            _speechService.Recognized -= OnSpeechRecognized;
        }
    }
}
