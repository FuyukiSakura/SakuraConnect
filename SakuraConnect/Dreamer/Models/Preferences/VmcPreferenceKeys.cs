
namespace Sakura.Live.Connect.Dreamer.Models.Preferences
{
    /// <summary>
    /// The keys for storing VMC settings in <see cref="Microsoft.Maui.Storage.Preferences"/>
    /// </summary>
    public static class VmcPreferenceKeys
    {
        /// <summary>
        /// Gets the container key for VMC settings
        /// </summary>
        public const string ContainerKey = "Vmc";

        /// <summary>
        /// Gets the system preference key for OSC duplicators
        /// </summary>
        public const string Duplicators = "Duplicators";

        /// <summary>
        /// the system preference key for OSC listener port
        /// </summary>
        public const string Port = "Port";
    }
}
