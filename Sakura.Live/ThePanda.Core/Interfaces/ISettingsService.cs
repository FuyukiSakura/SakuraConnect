
namespace Sakura.Live.ThePanda.Core.Interfaces
{
    /// <summary>
    /// Accesses the system settings
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets the value of the settings of the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue">The value to return when setting is not found</param>
        /// <returns></returns>
        public string Get(string key, string defaultValue);

        /// <summary>
        /// Sets the value of the settings of the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value);
    }
}
