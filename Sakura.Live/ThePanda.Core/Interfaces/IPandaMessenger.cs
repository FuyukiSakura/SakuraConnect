
namespace Sakura.Live.ThePanda.Core.Interfaces
{
    /// <summary>
    /// Sends messages between objects
    /// </summary>
    public interface IPandaMessenger
    {
        /// <summary>
        /// Registers a handler for a message type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void Register<T>(object subscriber, Action<T> handler);
        
        /// <summary>
        /// Sends a message to all registered handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Send<T>(T message);

        /// <summary>
        /// Unregisters a handler for a message type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriber"></param>
        public void Unregister<T>(object subscriber);

        /// <summary>
        /// Unregisters all handlers for a subscriber
        /// </summary>
        /// <param name="subscriber"></param>
        public void UnregisterAll(object subscriber);
    }
}
