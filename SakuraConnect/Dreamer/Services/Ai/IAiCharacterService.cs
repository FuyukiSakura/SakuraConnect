
namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
    public interface IAiCharacterService
    {
        /// <summary>
        /// The AI's name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// How the stream should be started
        /// </summary>
        string Topic { get; set; }

        /// <summary>
        /// Defines the character of the ai
        /// </summary>
        string Character { get; set; }

        /// <summary>
        /// Defines what the audience character do
        /// to assist the ai
        /// </summary>
        string AudienceCharacter { get; set; }

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
        /// Gets the topic prompt for today's stream
        /// </summary>
        /// <returns></returns>
        string GetTopicPrompt();

        /// <summary>
        /// Gets the personality prompt for open AI according to the character
        /// </summary>
        /// <returns>A prompt to instruct Open AI on how to talk to the user.</returns>
        string GetPersonalityPrompt();

        /// <summary>
        /// Gets the audience guiding prompt for open AI according to the audience character
        /// </summary>
        string GetAudiencePrompt();
    }
}
