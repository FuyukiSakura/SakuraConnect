
namespace Sakura.Live.Speech.Core.Models
{
    public static class Languages
    {
        /// <summary>
        /// Gets the locale code of English language
        /// </summary>
        public const string English = "en-AU";

        /// <summary>
        /// Gets the locale code of Mandarin language
        /// </summary>
        public const string Mandarin = "zh-TW";

        /// <summary>
        /// Gets the locale code of Cantonese language
        /// </summary>
        public const string Cantonese = "zh-HK";

        /// <summary>
        /// Gets the locale code of Japanese language
        /// </summary>
        public const string Japanese = "ja-JP";

        /// <summary>
        /// Converts language code to language name
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetLanguage(string code) => code switch
        {
            "en" => English,
            "zh_cht" => Mandarin,
            "zh_chs" => Mandarin,
            "ja" => Japanese,
            _ => Cantonese
        };
    }
}
