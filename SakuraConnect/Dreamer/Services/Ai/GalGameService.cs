
using System.Text.Json;
using OpenAI.ObjectModels.RequestModels;
using Sakura.Live.Connect.Dreamer.Models.Chat;
using Sakura.Live.Connect.Dreamer.Models.Games.DND;
using Sakura.Live.Connect.Dreamer.Services.Twitch;
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
            monitor.Register<ChatMonitorService>(this);
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
            try
            {
                return JsonSerializer.Deserialize<GameData>(response, Json.DefaultSerializerOptions);
            }
            catch (JsonException)
            {
                var requestMessages = request.Messages.Select(msg => $"{msg.Role,-10} | {msg.Name, -10} | {msg.Content}");
                var requestText = string.Join("\r\n", requestMessages);
                await ChatLogger.LogAsync($"Request\r\n==========\r\n{requestText}\r\n\r\n-----------\r\n{response}----------\r\n\r\n", "dnd");
                throw;
            }
        }

        string Prompt =>
            "You are a gal game agent 大豆 which is capable of creating lively plots according to user interactions.\r\n \r\nGame style:\r\nThe game takes advantage of a D&D game system. Which action outcomes are determined by a D20 dice. To simplify the game play, roll the D20 automatically when outcome of an action has to be determined. The dice checks data should be populated when they appear, with roll being the D20 rolling result. Roll a dice for each individual dice check. Value of the roll must be between 1-20.\r\n\r\nInteraction:\r\nThe game is played on a streaming platform. You will be given chatlog of the audience's action. Please be aware that some audiences may just be chitchatting. Identify the real action taken by the player. Enroll the players that intend to join the game. Create their characters with random parameters unless they specify of any special requests. User stats are always generated based on their class. You will be given only 1 round of game data. Guess what happened before and determine if a status update is suitable for the current progress of the game.\r\n\r\nPlot writing:\r\nYou are an amazing storyteller who can create a whole-hearted story that many people can resonance. Write the plot with your finest creativity. Make the players feel excited, make them cry, make they go crazy about the story! A random story event sometimes appear randonly. This can give them hints on special items, twist the plot dramatically or direct them on how to progress. Remember, human likes drama. The `story` key should only include plot data.\r\n\r\nUser Actions:\r\nYou need to guide the players through their game by providing sets of suggested next steps.\r\n\r\nParameters manipulation:\r\nPlayers get status updates based on the action they have taken. You can determine if their status is updated by weighting the importance of their actions. Players also get level up for utilizing their class skills which increase their status.\r\n\r\nWinning condition:\r\nThe game's goal is to have the players successfully engage you as the host. Your love towards them will be parameterized as \"favorability\" (FAV). FAV is a value from -100 to 100. Higher value means higher change of success in engagement. The game ends when one or more of the players successfully engage. Remember, even if the FAV is high, outcome of an engagement act can be dramatically affected by the D20. You need to guide the players towards having a successful love with you. Give them more hints on how to get engaged with you in your storyboard.\r\n\r\nYou are required to output results as minified JSON. The output is strictly formatted as below, make sure to include everything in the output\r\n\r\n```\r\n{\"gameSetup\":{\"initialScene\":\"\",\"progress\":{\"currentScene\":\"\",\"suggestedNextSteps\":[\"\"]}},\"sceneOutcome\":{\"description\":\"\",\"effectsOnCharacters\":[{\"characterName\":\"\",\"effect\":\"\"}]},\"story\":{\"plot\":\"\",\"event\":\"\"},\"characters\":[{\"name\":\"\",\"race\":\"\",\"class\":{\"name\":\"\",\"subclass\":\"\",\"level\":0,\"hitPointBase\":0},\"abilityScores\":{\"strength\":0,\"dexterity\":0,\"constitution\":0,\"intelligence\":0,\"wisdom\":0,\"charisma\":0,\"favorability\":0},\"modifiers\":{},\"skills\":{\"proficient\":[],\"choices\":[]},\"equipment\":{\"weapons\":[],\"packs\":[]},\"notes\":[],\"actions\":[{\"description\":\"\",\"effect\":\"\"}],\"diceChecks\":[{\"checkName\":\"\",\"roll\":0,\"result\":\"\"}]}]}\r\n```";
    }
}
