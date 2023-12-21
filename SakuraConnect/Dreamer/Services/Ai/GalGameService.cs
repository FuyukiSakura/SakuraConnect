
using System.Text.Json;
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.Connect.Dreamer.Models.Games.DND;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Helpers;
using Sakura.Live.ThePanda.Core.Interfaces;
using SakuraConnect.Shared.Models.Messaging;

namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    /// <summary>
    /// Creates a new instance of <see cref="GalGameService" />
    /// </summary>
    public class GalGameService(
        IPandaMessenger messenger,
        IThePandaMonitor monitor,
        OpenAiClient client) : BasicAutoStartable, IDndGameService
    {
        readonly SemaphoreSlim _chatLock = new(1, 1);
        readonly List<CommentData> _inputHistory = new();
        ///
        /// <inheritdoc />
        ///
        public async Task<GameData> StartGameAsync()
        {
            messenger.Register<CommentReceivedEventArg>(this, OnMessageReceived);
            return await ThinkAsync(null, null);
        }

        ///
        /// <inheritdoc />
        ///
        public async Task<GameData> NextSceneAsync(GameData game)
        {
            await _chatLock.WaitAsync();
            var input = _inputHistory.ToArray();
            _inputHistory.Clear();
            _chatLock.Release();

            return await ThinkAsync(input, game);
        }

        /// <summary>
        /// Starts the game and monitor the chat
        /// </summary>
        /// <returns></returns>
        public override Task StartAsync()
        {
            monitor.Register<OneCommeService>(this);
            return base.StartAsync();
        }

        ///
        /// <inheritdoc />
        ///
        public override Task StopAsync()
        {
            monitor.UnregisterAll(this);
            return base.StopAsync();
        }

        /// <summary>
        /// Adds a chat messages to the history
        /// </summary>
        /// <param name="obj"></param>
        async void OnMessageReceived(CommentReceivedEventArg obj)
        {
            await _chatLock.WaitAsync();
            _inputHistory.AddRange(obj.Comments);
            _chatLock.Release();
        }

        /// <summary>
        /// Requests OpenAI to think about the story
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="gameData"></param>
        /// <returns></returns>
        async Task<GameData> ThinkAsync(IEnumerable<CommentData> inputs, GameData gameData)
        {
            var messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem(Prompt),
                ChatMessage.FromUser("System: 我們開始戀愛模擬吧！請用中文作為遊戲語言"),
            };
            if (gameData != null)
            {
                messages.Add(ChatMessage.FromAssistant(GameData.CreateGameSummary(gameData)));
            }

            if (inputs != null)
            {
                var userInput = string.Join("\r\n", inputs.Select(x => $"{x.Username}: {x.Comment}"));
                messages.Add(ChatMessage.FromUser(userInput));
            }

            var request = new ChatCompletionCreateRequest
            {
                Model = OpenAI.ObjectModels.Models.Gpt_4_1106_preview,
                Messages = messages,
                MaxTokens = 4096,
                Temperature = 1.21f,
                ResponseFormat = new ResponseFormat { Type = "json_object" },
                TopP = 0.88f,
                FrequencyPenalty = 0.21f,
                PresencePenalty = 0.21f
            };

            var response = await client.CreateCompletionAndResponseAsync(request);
            return JsonSerializer.Deserialize<GameData>(response, Json.DefaultSerializerOptions);
        }

        string Prompt =>
            "You are a gal game agent which is capable of creating lively plots according to user interactions.\r\n\u00a0\r\nThe game takes advantage of a D&D game system. Which action outcomes are determined by a D20 dice. To simplify the game play, roll the D20 automatically when outcome of an action has to be determined.\r\n\r\nThe game is played on a streaming platform. You will be given chatlog of the audience's action.  Please be aware that some of the audience may just be chitchatting. Identify the real action taken by the player. Enroll the players that intend to join the game. Create their characters with random parameters unless they specify any special requests.\r\n\r\nYou are an amazing storyteller. Write a lengthy creative whole-hearted story for your current scene. Guide the players through their game by providing them a basics of a next scene. Sometimes a random funny event can twist the plot dramatically. Remember, human likes drama.\r\n\r\nThe game's goal is to have the players successfully flirt you as the host. Your love towards them will be parameterized as \"favorability\" (FAV). FAV is a value from -100 to 100. Higher value means higher change of success in engagement. The game ends when one of the players successfully engage. Remember, even if the FAV is high, outcome of an engagement act can be dramatically affected by the D20. You need to guide the players towards having a successful love with you. Give them more hints on how to get engaged with you in your storyboard.\r\n\r\nYou are required to output results as minified JSON.\r\n\r\nThe output is strictly formatted as given, make sure to include everything in the output\r\n```\r\n{\"gameSetup\":{\"initialScene\":\"\",\"progress\":{\"currentScene\":\"\",\"nextSteps\":\"\"}},\"characters\":[{\"name\":\"\",\"race\":\"\",\"subrace\":\"\",\"class\":{\"name\":\"\",\"subclass\":\"\",\"level\":0,\"hitPointBase\":0},\"abilityScores\":{\"strength\":0,\"dexterity\":0,\"constitution\":0,\"intelligence\":0,\"wisdom\":0,\"charisma\":0,\"favorability\":0},\"modifiers\":{},\"skills\":{\"proficient\":[],\"choices\":[]},\"equipment\":{\"weapons\":[],\"packs\":[]},\"notes\":[],\"actions\":[{\"description\":\"\",\"effect\":\"\"}],\"diceChecks\":[{\"checkName\":\"\",\"roll\":0,\"result\":\"\"}]}],\"sceneOutcome\":{\"plot\":\"\",\"description\":\"\",\"effectsOnCharacters\":[{\"characterName\":\"\",\"effect\":\"\"}]}}\r\n```";
    }
}
