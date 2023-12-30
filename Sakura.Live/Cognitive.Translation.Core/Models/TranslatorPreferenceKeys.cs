
namespace Sakura.Live.Cognitive.Translation.Core.Models
{
    /// <summary>
    /// Settings related to Azure Translator
    /// </summary>
    public static class TranslatorPreferenceKeys
    {
        /// <summary>
        /// Gets the system preference key for Azure Translator API Key
        /// </summary>
        public const string ApiKey = "SCONNECT_AZURE_TRANSLATOR_API_KEY";

        /// <summary>
        /// Gets the system preference key for Azure Translator Region
        /// </summary>
        public const string Location = "SCONNECT_AZURE_TRANSLATOR_LOCATION";

        /// <summary>
        /// Gets the system preference key for Azure Translator Translations targets
        /// </summary>
        public const string Translations = "SCONNECT_AZURE_TRANSLATOR_TRANSLATIONS";
    }
}
