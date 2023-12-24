
namespace SakuraConnect.Shared.Models.Messaging.Ai
{
	/// <summary>
	/// Wrapper for json_mode response
	/// as it only returns as an object instead of array
	/// </summary>
	public class OpenAiJsonObject<T>
	{
		public T Data { get; set; } = default!;
	}
}
