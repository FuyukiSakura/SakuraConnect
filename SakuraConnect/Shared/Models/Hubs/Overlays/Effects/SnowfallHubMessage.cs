namespace SakuraConnect.Shared.Models.Hubs.Overlays.Effects
{
    public static class SnowfallHubMessage
    {
        /// <summary>
        /// Gets the method name of subscribe
        /// </summary>
        public const string Subscribe = "Subscribe";

        /// <summary>
        /// Gets the event name for triggering the snowing
        /// </summary>
        public const string StartSnow = "StartSnow";

        /// <summary>
        /// Gets the event name for changing icon of the snow
        /// </summary>
        public const string ChangeIcon = "ChangeIcon";
    }
}
