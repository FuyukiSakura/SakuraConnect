
namespace Sakura.Live.Connect.Dreamer.Services.Ai
{
	/// <summary>
	/// Prompts required to make the system work
	/// </summary>
	public static class SystemPrompts
	{
		/// <summary>
		/// Instructs the AI to output in a json format that can be read by the system
		/// </summary>
		public const string OutputJson =
			"Output the result as JSON in format `{data:[{\"repondTo\":\"\",\"text\":\"\"}]}`.";
	}
}
