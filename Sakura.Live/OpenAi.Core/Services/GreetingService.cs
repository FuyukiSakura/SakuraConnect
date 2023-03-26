using OpenAI.GPT3.ObjectModels.RequestModels;
using Sakura.Live.OpenAi.Core.Models;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;
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

        /// <summary>
        /// Gets or sets the prompt for generating the customized greet
        /// </summary>
        public string Prompt { get; set; } =
            "You are a vtuber.";

        /// <summary>
        /// Gets or sets the number of characters to be generated
        /// </summary>
        public int Characters { get; set; } = 30;

        // Dependencies
        readonly ISettingsService _settingsService;
        readonly IThePandaMonitor _monitor;
        readonly OpenAiService _service;
        readonly TwitchChatService _twitchChat;

        /// <summary>
        /// Creates a new instance of <see cref="GreetingService" />
        /// </summary>
        /// <param name="settingsService"></param>
        /// <param name="monitor"></param>
        /// <param name="service"></param>
        /// <param name="twitchChat"></param>
        public GreetingService(ISettingsService settingsService,
            IThePandaMonitor monitor,
            OpenAiService service,
            TwitchChatService twitchChat)
        {
            _settingsService = settingsService;
            _service = service;
            _twitchChat = twitchChat;
            _monitor = monitor;
            LoadSettings();
        }

        /// <summary>
        /// Saves OpenAI greeting settings to the system
        /// </summary>
        void SaveSettings()
        {
            _settingsService.Set(OpenAiPreferenceKeys.GreetingPrompt, Prompt);
        }

        /// <summary>
        /// Loads OpenAI greeting settings from the system
        /// </summary>
        void LoadSettings()
        {
            Prompt = _settingsService.Get(OpenAiPreferenceKeys.GreetingPrompt, Prompt);
        }

        /// <summary>
        /// Greets the audience according to the selected tone of the user
        /// </summary>
        /// <param name="prompt">The prompt for generating the customized greet</param>
        /// <param name="message">The audience's input</param>
        /// <param name="username">The name of the user to be greeted</param>
        /// <returns></returns>
        public async Task<string> GreetsAsync(
            string prompt,
            string message,
            string username)
        {
            var completionResult = await _service.Get().ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(
                        prompt 
                        + $"You can only response within {Characters} words."), // Adds character limits
                    ChatMessage.FromUser($"{username}: {message}"),
                },
                Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo,
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
            SaveSettings();
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
            _ = _twitchChat.SendMessage(message); // Fire and forget
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
