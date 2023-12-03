using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics;
using SakuraConnect.Shared.Models.Messaging;

namespace Sakura.Live.Connect.Dreamer.Models.OneComme
{
    /// <summary>
    /// Serializes and deserializes OneComme subscription messages
    /// </summary>
    public static class OneCommeSubscriptionMessage
    {
        /// <summary>
        /// Comment events
        /// </summary>
        public const string Comments = "comments";

        /// <summary>
        /// Serialize json string returned OneComme
        /// returns null when serialize error occurs
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static OneCommeEvent? Serialize(string jsonString)
        {
            try
            {
                return JsonSerializer.Deserialize<OneCommeEvent>(jsonString, Json.DefaultSerializerOptions);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
