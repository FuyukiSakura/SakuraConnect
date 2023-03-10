using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.Twitch.Core.Services;
using TwitchLib.Client.Events;

namespace Sakura.Live.OpenAi.Core.Services
{
    /// <summary>
    /// Greets the user with a message
    /// </summary>
    public class GreetingService : BasicAutoStartable
    {
        readonly List<string> _greetedUsers = new ();

        public string Prompt { get; set; } =
            "You are cheerful and energetic virtual streamer 冬雪桜 aka 小櫻. You are trying to start a conversation with your audience.";

        // Dependencies
        readonly OpenAiService _service;
        readonly TwitchChatService _twitchChat;
        readonly IThePandaMonitor _monitor;

        /// <summary>
        /// Creates a new instance of <see cref="GreetingService" />
        /// </summary>
        /// <param name="service"></param>
        /// <param name="twitchChat"></param>
        /// <param name="monitor"></param>
        public GreetingService(OpenAiService service,
            TwitchChatService twitchChat,
            IThePandaMonitor monitor)
        {
            _service = service;
            _twitchChat = twitchChat;
            _monitor = monitor;
        }

        /// <summary>
        /// Greets the audience according to the selected tone of the user
        /// </summary>
        /// <param name="prompt">The prompt for generating the customized greet</param>
        /// <param name="username">The name of the user to be greeted</param>
        /// <returns></returns>
        public async Task<string> GreetsAsync(string prompt, string message, string username)
        {
            var completionResult = await _service.Get().ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(prompt),
                    ChatMessage.FromUser($"{message}"),
                },
                Model = Models.ChatGpt3_5Turbo,
                Temperature = 1,
                MaxTokens = 256
            });

            if (completionResult.Successful)
            {
                return completionResult.Choices.First().Message.Content;
            }

            // TODO: Adds fallback message
            return "";
        }

        /// <summary>
        /// Starts greeting Twitch users
        /// </summary>
        /// <returns></returns>
        public override Task StartAsync()
        {
            _twitchChat.OnMessageReceived += TwitchChat_OnMessageReceived;
            _monitor.Register(this, _twitchChat);
            _monitor.Register(this, _service);
            return base.StartAsync();
        }

        /// <summary>
        /// Greets user when user sends a message for the first time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void TwitchChat_OnMessageReceived(object? sender, OnMessageReceivedArgs args)
        {
            if (_greetedUsers.Contains(args.ChatMessage.Username))
            {
                return;
            }

            _greetedUsers.Add(args.ChatMessage.Username);
            var message = await GreetsAsync(Prompt, args.ChatMessage.Message, args.ChatMessage.DisplayName);
            _twitchChat.SendMessage(message);
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _monitor.Unregister(this);
            return base.StopAsync();
        }
    }
}
