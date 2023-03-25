
namespace Sakura.Live.Cognitive.Translation.Core.Models
{
    /// <summary>
    /// Layouts the translation result returned by Azure Cognitive Services
    /// </summary>
    public class TranslationResult
    {
        /// <summary>
        /// Gets or sets the detected language
        /// </summary>
        public DetectedLanguage DetectedLanguage { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of translations
        /// </summary>
        public List<Translation> Translations { get; set; } = new ();
    }

    /// <summary>
    /// Layouts the detected language
    /// </summary>
    public class DetectedLanguage
    {
        /// <summary>
        /// Gets or sets the language code of the detected language
        /// </summary>
        public string Language { get; set; } = "";

        /// <summary>
        /// Gets or sets the confidence of the detected language
        /// </summary>
        public float Score { get; set; }
    }

    /// <summary>
    /// Layouts the translation result
    /// </summary>
    public class Translation {
        /// <summary>
        /// Gets or sets the text translated
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Gets or sets the language code of the translated text
        /// </summary>
        public string To { get; set; } = "";
    }
}
