
namespace Sakura.Live.Obs.Core.Models
{
    /// <summary>
    /// Contains metadata of OBS Text (GDI+) source of subtitle
    /// and it's corresponding locale
    /// </summary>
    public class ObsSubtitleConfig
    {
        /// <summary>
        /// Gets or sets the locale of the current subtitle source
        /// </summary>
        public string Locale { get; set; } = "";

        /// <summary>
        /// Gets or sets the source name this config targets in
        /// OBS source list
        /// </summary>
        public string SourceName { get; set; } = "";
    }
}
