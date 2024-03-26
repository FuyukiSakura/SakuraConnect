
using System.Collections.Immutable;

namespace Sakura.Live.Cognitive.Translation.Core.Models
{
    /// <summary>
    /// The OBS translation settings
    /// </summary>
    public class TranslationOption
    {
        /// <summary>
        /// Gets or sets the language of the translation
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Gets or sets the target OBS source
        /// the text should display
        /// </summary>
        public string ObsSource { get; set; } = "English Subtitle";

        /// <summary>
        /// Gets the default list of translation options
        /// </summary>
        public static readonly List<TranslationOption> DefaultList = new()
        {
            new TranslationOption
            {
                Language = "en",
                ObsSource = "English Subtitle"
            },
            new TranslationOption
            {
                Language = "ja",
                ObsSource = "Japanese Subtitle"
            }
        };
    }
}
