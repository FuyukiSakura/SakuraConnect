using System.Diagnostics;
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
		readonly SemaphoreSlim _statusLock = new (1,1);
		
        ///
        /// <inheritdoc />
        ///
		public CancellationTokenSource CancellationTokenSource { get; protected set; } = new ();

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

        /// <summary>
        /// Checks if the thread is still running
        /// </summary>
        /// <returns></returns>
        protected virtual async Task HeartBeatAsync(CancellationToken token)
        {
            while (Status == ServiceStatus.Running
                   && !token.IsCancellationRequested)
            {
                LastUpdate = DateTime.Now;
                await Task.Delay(HeartBeat.Default, token);
            }
        }

		/// <summary>
		/// The actual start of the service
		/// </summary>
		/// <returns></returns>
		public virtual Task StartAsync()
		{
            _ = HeartBeatAsync(CancellationTokenSource.Token);
            return Task.CompletedTask;
		}

		///
		/// <inheritdoc />
		///
        public async Task StartOnceAsync()
        {
			await _statusLock.WaitAsync();
			CancellationTokenSource.Cancel(); // Cancel the previous thread
			CancellationTokenSource = new CancellationTokenSource();

            if (Status == ServiceStatus.Running)
            {
                _statusLock.Release();
                return;
            }

            try
            {
                Status = ServiceStatus.Running;
				LastUpdate = DateTime.Now;
                await StartAsync();
                Started?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
				Debug.WriteLine(e.Message);
                Status = ServiceStatus.Error;
            }
            finally
            {
                _statusLock.Release();
            }
        }

		///
		/// <inheritdoc />
		///
		public virtual async Task StopAsync()
		{
			Stopped?.Invoke(this, EventArgs.Empty);
			Status = ServiceStatus.Stopped;
			CancellationTokenSource.Cancel();
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
