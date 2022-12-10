using Sakura.Live.ThePanda.Core.Interfaces;

namespace Sakura.Live.ThePanda.Core.Helpers
{
	/// <summary>
	/// Basic implementations of <see cref="IAutoStartable"/>
	/// calls <see cref="Started"/>, <see cref="Stopped"/> and <see cref="StatusChanged"/> automatically
	/// </summary>
	public abstract class BasicAutoStartable : IAutoStartable
	{
		ServiceStatus _status = ServiceStatus.Stopped;

		///
		/// <inheritdoc />
		///
		public event EventHandler? Started;
        
		///
		/// <inheritdoc />
		///
		public event EventHandler? Stopped;

		///
		/// <inheritdoc />
		///
		public event EventHandler<ServiceStatus>? StatusChanged;

		///
		/// <inheritdoc />
		///
		public virtual async Task StartAsync()
		{
			Started?.Invoke(this, EventArgs.Empty);
			await Task.CompletedTask;
		}

		///
		/// <inheritdoc />
		///
		public virtual async Task StopAsync()
		{
			Stopped?.Invoke(this, EventArgs.Empty);
			await Task.CompletedTask;
		}

		///
		/// <inheritdoc />
		///
		public ServiceStatus Status
		{
			get => _status;
			set
			{
				_status = value;
				StatusChanged?.Invoke(this, _status);
			}
		}

		///
		/// <inheritdoc />
		///
		public DateTime LastUpdate { get; protected set; }
	}
}
