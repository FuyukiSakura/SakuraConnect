
namespace Sakura.Live.ThePanda.Core.Interfaces
{
	/// <summary>
	/// Represents a service that can be auto started
	/// </summary>
	public interface IAutoStartable
	{
		/// <summary>
		/// Is triggered when the service starts
		/// </summary>
		event EventHandler Started;

		/// <summary>
		/// Is triggered when the service stops
		/// </summary>
		event EventHandler Stopped;
        
		/// <summary>
		/// Is triggered when the service status changes
		/// </summary>
		event EventHandler<ServiceStatus> StatusChanged;

        /// <summary>
        /// Is used for cancelling the unmanaged dependency services
        /// </summary>
        CancellationTokenSource CancellationTokenSource { get; }

		/// <summary>
		/// Starts the service
		/// </summary>
		/// <returns></returns>
		Task StartOnceAsync();

		/// <summary>
		/// Stops the service
		/// </summary>
		/// <returns></returns>
		Task StopAsync();

		/// <summary>
		/// Gets if the running status of the service
		/// </summary>
		ServiceStatus Status { get; set; }

		/// <summary>
		/// Gets the last update time of the service
		/// </summary>
		/// <remarks>
		/// This can be used to determine if a service is not running properly
		/// and the service will be restarted when timeout
		/// </remarks>
		DateTime LastUpdate { get; }
	}
}
