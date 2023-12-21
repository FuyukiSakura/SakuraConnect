
using Sakura.Live.Connect.Dreamer.Models.Games.DND;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    /// <summary>
    /// Handles the logic of the DND game
    /// </summary>
    public interface IDndGameService : IAutoStartable
    {
        /// <summary>
        /// Sets up the game with initial prompts
        /// </summary>
        /// <returns></returns>
        Task<GameData> StartGameAsync();

        /// <summary>
        /// Calculates the next scene data by analyzing the chat history
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        Task<GameData> NextSceneAsync(GameData game);
    }
}
