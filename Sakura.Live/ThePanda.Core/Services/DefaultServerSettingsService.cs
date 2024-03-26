using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.ThePanda.Core.Services
{
    /// <summary>
    /// Gets the settings from the server environment variables.
    /// </summary>
    public class DefaultServerSettingsService : ISettingsService
    {
        ///
        /// <inheritdoc />
        ///
        public string Get(string key, string defaultValue)
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }

        ///
        /// <inheritdoc />
        ///
        public void Set(string key, string value)
        {
            // You cannot change settings on a server
            throw new NotImplementedException();
        }
    }
}
