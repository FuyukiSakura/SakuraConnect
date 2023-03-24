using Microsoft.Extensions.DependencyInjection;
using Sakura.Live.Obs.Core.Services;

namespace Sakura.Live.Obs.Core
{
	public static class ObsCoreDependencyInjectionExtensions
	{
		/// <summary>
		/// Add ThePanda services to the specified <see cref="IServiceCollection" />
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddObsCore(this IServiceCollection services)
		{
			services.AddSingleton<ObsSocketService>();
			return services;
		}
	}
}
