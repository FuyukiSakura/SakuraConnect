
namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
	/// <summary>
	/// Prompts required to make the system work
	/// </summary>
	public static class SystemPrompts
	{
		/// <summary>
		/// Instructs the AI to create a separation for each language
		/// </summary>
		public const string SeparateLanguageForTts =
			"Put a full stop in front and after if language is changed even if it's in the middle of a sentence.";

		/// <summary>
		/// Add SSML expressions
		/// </summary>
		public const string EmotionAndLanguage =
			"Add SSML`<lang xml:lang>` tag as `zh-HK` in your comment if it is written in Cantonese. Do not include the `<speak>`and `<voice>` tag.";

		/// <summary>
		/// Instructs the AI to output in a json format that can be read by the system
		/// </summary>
		public const string OutputJson =
			"Output the result as JSON in format `{data:[{\"repondTo\":\"\",\"text\":\"\"}]}`.";
	}
}
