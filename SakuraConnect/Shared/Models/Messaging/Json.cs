using System.Text.Json;
using System.Text.Json.Serialization;

namespace SakuraConnect.Shared.Models.Messaging
{
	/// <summary>
	/// Helper class for json
	/// </summary>
	public class Json
	{
		/// <summary>
		/// Gets the default SerializerOptions
		/// </summary>
		public static readonly JsonSerializerOptions DefaultSerializerOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			Converters ={
				new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
			},
		};
	}
}
