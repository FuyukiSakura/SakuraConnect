using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.Connect.Dreamer.Services
{
    ///
    /// <inheritdoc />
    ///
    public class SettingsService : ISettingsService
    {
        ///
        /// <inheritdoc />
        ///
        public string Get(string key, string defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        ///
        /// <inheritdoc />
        ///
        public void Set(string key, string value)
        {
            Preferences.Set(key, value);
        }
    }
}
