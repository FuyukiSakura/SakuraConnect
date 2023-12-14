
using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.ThePanda.Core
{
    ///
    /// <inheritdoc />
    ///
    public class SimpleMessenger : IPandaMessenger
    {
        readonly Dictionary<Type, Dictionary<object, List<Action<object>>>> _handlers = new();
        
        ///
        /// <inheritdoc />
        ///
        public void Register<T>(object subscriber, Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = new Dictionary<object, List<Action<object>>>();
            }

            if (!_handlers[type].ContainsKey(subscriber))
            {
                _handlers[type][subscriber] = new List<Action<object>>();
            }

            _handlers[type][subscriber].Add(message => handler((T)message));
        }

        ///
        /// <inheritdoc />
        ///
        public void Send<T>(T message)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
            {
                return;
            }

            foreach (var handler in _handlers[type].Keys
                         .SelectMany(subscriber => _handlers[type][subscriber]))
            {
                _ = Task.Run(() => handler(message));
            }
        }

        ///
        /// <inheritdoc />
        ///
        public void Unregister<T>(object subscriber)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
            {
                return;
            }

            if (!_handlers[type].ContainsKey(subscriber))
            {
                return;
            }

            _handlers[type].Remove(subscriber);
        }

        ///
        /// <inheritdoc />
        ///
        public void UnregisterAll(object subscriber)
        {
            foreach (var type in _handlers.Keys
                         .Where(type => _handlers[type].ContainsKey(subscriber)))
            {
                _handlers[type].Remove(subscriber);
            }
        }
    }
}
