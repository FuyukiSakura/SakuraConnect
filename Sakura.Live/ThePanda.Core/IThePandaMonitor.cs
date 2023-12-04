using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.ThePanda.Core
{
	/// <summary>
	/// Monitors backend services
	/// </summary>
	public interface IThePandaMonitor
	{
		/// <summary>
		/// Gets the running status of the monitor
		/// </summary>
		public bool IsRunning { get; }

		/// <summary>
		/// Adds a service to the monitor list
		/// </summary>
		/// <param name="sender">the service that depends on the registered service</param>
		public void Register<T>(object sender) where T:IAutoStartable;

		/// <summary>
		/// Unregisters a single service from the monitor list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sender"></param>
		public void Unregister<T>(object sender) where T:IAutoStartable;

		/// <summary>
		/// Unregisters a parent service and releases all of it's child service 
		/// </summary>
		/// <param name="sender"></param>
		/// <remarks>
		/// A child service can be relied on multiple parents.
		/// The child service will only stop until all parents are unregistered
		/// </remarks>
		public void UnregisterAll(object sender);

		/// <summary>
		/// Starts the panda monitor
		/// </summary>
		/// <returns></returns>
		public Task StartAsync();

		/// <summary>
		/// Stops the panda monitor
		/// </summary>
		public void Stop();
	}
}
