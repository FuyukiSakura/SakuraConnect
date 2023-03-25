using System.Text;
using System.Text.Json;
using Sakura.Live.Cognitive.Translation.Core.Models;
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Cognitive.Translation.Core.Services
{
    /// <summary>
    /// Accesses the Azure translation service
    /// </summary>
    public class TranslationService
    {
        const string Endpoint = "https://api.cognitive.microsofttranslator.com";

        // Dependencies
        readonly ISettingsService _settingsService;

        /// <summary>
        /// Gets or sets the API key of the Azure Translator service
        /// </summary>
        public string ApiKey { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the region of the Azure Translator service
        /// </summary>
        public string Location { get; set; } = "";

        /// <summary>
        /// Creates a new instance of <see cref="TranslationService" />
        /// </summary>
        /// <param name="settingsService"></param>
        public TranslationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();
        }

        /// <summary>
        /// Saves the Azure Translator settings to the system
        /// </summary>
        public void SaveSettings()
        {
            _settingsService.Set(TranslatorPreferenceKeys.ApiKey, ApiKey);
            _settingsService.Set(TranslatorPreferenceKeys.Location, Location);
            _settingsService.Set(TranslatorPreferenceKeys.Translations, JsonSerializer.Serialize(Translations));
        }

        /// <summary>
        /// Loads the Azure Translator settings from the system
        /// </summary>
        public void LoadSettings()
        {
            ApiKey = _settingsService.Get(TranslatorPreferenceKeys.ApiKey, "");
            Location = _settingsService.Get(TranslatorPreferenceKeys.Location, "asia");
            Translations = JsonSerializer.Deserialize<List<TranslationOption>>(
                _settingsService.Get(TranslatorPreferenceKeys.Translations,
                    JsonSerializer.Serialize(TranslationOption.DefaultList)))!;
        }

        /// <summary>
        /// Gets or sets the target list of translation languages.
        /// </summary>
        public List<TranslationOption> Translations { get; set; } = new ();

        /// <summary>
        /// Translates the given text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<TranslationResult> TranslateAsync(string text)
        {
            // Input and output languages are defined as parameters.
            var route = "/translate?api-version=3.0" + GetToLanguages();
            var body = new object[] { new { Text = text } };
            var requestBody = JsonSerializer.Serialize(body);

            using var client = new HttpClient();
            using var request = new HttpRequestMessage();

            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(Endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", ApiKey);
            // location required if you're using a multi-service or regional (not global) resource.
            request.Headers.Add("Ocp-Apim-Subscription-Region", Location);

            // Send the request and get response.
            var response = await client.SendAsync(request).ConfigureAwait(false);
            // Read response as a string.
            var result = await response.Content.ReadAsStringAsync();
            result = result.Substring(1, result.Length - 2);

            var options = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            return JsonSerializer.Deserialize<TranslationResult>(result, options)
                   ?? new TranslationResult();
        }

        /// <summary>
        /// Combines the language to translate to into a string
        /// </summary>
        /// <returns></returns>
        string GetToLanguages()
        {
            return "&to="
                   + string.Join("&to=", Translations.Select(t => t.Language));
        }
    }
}
