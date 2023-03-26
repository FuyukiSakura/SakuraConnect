namespace Sakura.Live.Twitch.Core.Models
{
    /// <summary>
    /// Represents all preference keys of Twitch settings in the app
    /// </summary>
    public static class TwitchPreferenceKeys
    {
        /// <summary>
        /// Gets the system preference key for Twitch username
        /// </summary>
        public const string Username = "SCONNECT_TWITCH_USERNAME";

        /// <summary>
        /// Gets the system preference key for Twitch access token
        /// </summary>
        public const string AccessToken = "SCONNECT_TWITCH_ACCESS_TOKEN";

        /// <summary>
        /// Gets the system preference key for Twitch channel
        /// </summary>
        public const string Channel = "SCONNECT_TWITCH_CHANNEL";
    }
}
