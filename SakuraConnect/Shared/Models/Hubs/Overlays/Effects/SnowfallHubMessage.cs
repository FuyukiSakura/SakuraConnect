namespace SakuraConnect.Shared.Models.Hubs.Overlays.Effects
{
    public static class SnowfallHubMessage
    {
        /// <summary>
        /// Gets the method name of subscribe
        /// </summary>
        public const string Subscribe = nameof(Subscribe);

        /// <summary>
        /// Gets the event name for triggering the snowing
        /// </summary>
        public const string StartSnow = nameof(StartSnow);

        /// <summary>
        /// Gets the event name for stopping the snowing
        /// </summary>
        public const string StopSnow = nameof(StopSnow);

        /// <summary>
        /// Gets the event name for changing icon of the snow
        /// </summary>
        public const string UpdateIcon = nameof(UpdateIcon);

        /// <summary>
        /// Gets the event name for changing the number of
        /// snowflakes rendered on the screen
        /// </summary>
        public const string UpdateNumber = nameof(UpdateNumber);

        /// <summary>
        /// Gets the event name for changing the number of
        /// snowflakes rendered on the screen
        /// </summary>
        public const string UpdateZoom = nameof(UpdateZoom);

        /// <summary>
        /// Gets the event name for adding number of
        /// snowflakes rendered on the screen
        /// </summary>
        public const string AddSnow = nameof(AddSnow);

        /// <summary>
        /// Gets the event name for changing the size of snow
        /// </summary>
        public const string Zoom = nameof(Zoom);
    }
}
