
namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    public interface IAiCharacterService
    {
        /// <summary>
        /// The AI's name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Defines the character of the ai
        /// </summary>
        string Character { get; set; }

        /// <summary>
        /// Defines the greeting style of the ai
        /// </summary>
        string GreetingStyle { get; set; }

        /// <summary>
        /// Gets the greeting prompt for open AI according to the character and greeting style
        /// </summary>
        /// <returns>A prompt to instruct Open AI on how to greet the user.</returns>
        string GetGreetingPrompt();

        /// <summary>
        /// Gets the personality prompt for open AI according to the character
        /// </summary>
        /// <returns>A prompt to instruct Open AI on how to talk to the user.</returns>
        string GetPersonalityPrompt();

        /// <summary>
        /// Saves OpenAI settings to the system
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Loads OpenAI settings from the system
        /// </summary>
        void LoadSettings();
    }
}
