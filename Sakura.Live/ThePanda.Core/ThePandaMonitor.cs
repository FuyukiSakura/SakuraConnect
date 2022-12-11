using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.ThePanda.Core
{
	/// <summary>
    /// Monitors backend services
    /// </summary>
    internal class ThePandaMonitor : IThePandaMonitor
    {
        /// <summary>
        /// Sets how many seconds before a service is considered dead without response
        /// </summary>
        const int TimeoutIn = 10;

        /// <summary>
        /// Sets the time between each monitor interval in milliseconds
        /// </summary>
        const int MonitorInterval = 2000;

        readonly SemaphoreSlim _serviceLock = new (1,1);
        readonly Dictionary<IAutoStartable, HashSet<object>> _services = new ();

        ///
        /// <inheritdoc />
        ///
        public bool IsRunning { get; private set; }

        ///
        /// <inheritdoc />
        ///
        public void Register(object sender, IAutoStartable service)
        {
            if (service.Status == ServiceStatus.Stopped)
            {
                // Only starts a service that is not running
                // let the monitor handle if it's in Error
                _ = service.StartAsync();
            }

            if (_services.ContainsKey(service))
            {
                _services[service].Add(sender);
                return;
            }

            _services.Add(service, new HashSet<object>(new []
            {
                sender
            }));
        }

        ///
        /// <inheritdoc />
        ///
        public void Unregister(object sender)
        {
            var parentServiceSets = _services
                .Where(pair => pair.Value.Contains(sender))
                .Select(pair => pair.Value)
                .ToArray();
            foreach (var parents in parentServiceSets)
            {
                parents.Remove(sender);
            }

            StopServicesNoLongerReferenced();
        }

        /// <summary>
        /// Stops all services that are no longer referenced by any other services
        /// </summary>
        void StopServicesNoLongerReferenced()
        {
            var noLongerRequiredServices = _services
                .Where(pair => !MonitorRequired(pair) 
                               && pair.Key.Status != ServiceStatus.Stopped) // Checks if the service is already stopped
                .Select(pair => pair.Key)
                .ToArray();
            foreach (var service in noLongerRequiredServices)
            {
                service.Status = ServiceStatus.Stopped;
                service.StopAsync();
            }
        }

        ///
        /// <inheritdoc />
        ///
        public async Task StartAsync()
        {
            await _serviceLock.WaitAsync();

            IsRunning = true;
            while (IsRunning)
            {
                StopServicesNoLongerReferenced();

                var now = DateTime.Now;
                var monitoredServices = _services.Where(MonitorRequired)
                    .Select(pairs => pairs.Key)
                    .ToArray();
                foreach (var autoStartable in monitoredServices)
                {
                    if (!((now - autoStartable.LastUpdate).TotalSeconds > TimeoutIn)) continue;

                    autoStartable.Status = ServiceStatus.Error;
                    _ = autoStartable.StartAsync();
                }

                await Task.Delay(MonitorInterval);
            }
            _serviceLock.Release();
        }

        /// <summary>
        /// Checks if a service given is dependent by another service
        /// </summary>
        /// <param name="servicePair"></param>
        /// <returns></returns>
        static bool MonitorRequired(KeyValuePair<IAutoStartable, HashSet<object>> servicePair) 
            => servicePair.Value.Count >= 1;

        ///
        /// <inheritdoc />
        ///
        public void Stop()
        {
            IsRunning = false;
        }
    }
}