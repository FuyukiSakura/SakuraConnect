
using Microsoft.Extensions.DependencyInjection;

namespace Sakura.Live.ThePanda.Core
{
	/// <summary>
	/// Extension methods for setting up ThePanda services in an <see cref="IServiceCollection" />.
	/// </summary>
	public static class ThePandaDependencyInjectionExtensions
	{
		/// <summary>
		/// Add ThePanda services to the specified <see cref="IServiceCollection" />
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddThePanda(this IServiceCollection services)
		{
			services.AddSingleton<IThePandaMonitor, ThePandaMonitor>();
			return services;
		}
	}
}
