using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.Twitch.Core.Services;
using TwitchLib.Client.Events;

namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    /// <summary>
    /// Greets the user with a message
    /// </summary>
    public class GreetingService : BasicAutoStartable
    {
        readonly List<string> _greetedUsers = new ();

        /// <summary>
        /// Gets or sets the number of characters to be generated
        /// </summary>
        public int Characters { get; set; } = 30;

        // Dependencies
        readonly IAiCharacterService _characterService;
        readonly IThePandaMonitor _monitor;
        readonly OpenAiService _service;
        readonly TwitchChatService _twitchChat;

        /// <summary>
        /// Creates a new instance of <see cref="GreetingService" />
        /// </summary>
        public GreetingService(IAiCharacterService characterService,
            IThePandaMonitor monitor,
            OpenAiService service,
            TwitchChatService twitchChat)
        {
            _characterService = characterService;
            _service = service;
            _twitchChat = twitchChat;
            _monitor = monitor;
        }

        /// <summary>
        /// Greets the audience according to the selected tone of the user
        /// </summary>
        /// <param name="prompt">The prompt for generating the customized greet</param>
        /// <param name="message">The audience's input</param>
        /// <param name="username">The name of the user to be greeted</param>
        /// <returns></returns>
        public async Task GreetsAsync(
            string prompt,
            string message,
            string username)
        {
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(
                        prompt
                        + $"You can only response within {Characters} words."), // Adds character limits
                    ChatMessage.FromUser($"{username}: {message}"),
                },
                Model = OpenAI.ObjectModels.Models.Gpt_3_5_Turbo_16k_0613,
                Temperature = 1,
                MaxTokens = 256
            };
            var response = await _service.CreateCompletionAndResponseAsync(request);
            await _twitchChat.SendMessage(response); // Fire and forget
        }

        /// <summary>
        /// Starts greeting Twitch users
        /// </summary>
        /// <returns></returns>
        public override Task StartAsync()
        {
            // _twitchChat.OnMessageReceived += TwitchChat_OnMessageReceived;
            _monitor.Register<TwitchChatService>(this);
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
            _ = Task.Run(() => GreetsAsync(_characterService.GetGreetingPrompt(),
                args.ChatMessage.Message,
                args.ChatMessage.DisplayName));
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            _monitor.UnregisterAll(this);
            return base.StopAsync();
        }
    }
}
