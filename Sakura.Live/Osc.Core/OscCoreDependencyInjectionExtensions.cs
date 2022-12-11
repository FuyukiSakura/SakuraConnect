using Microsoft.Extensions.DependencyInjection;
using Sakura.Live.Osc.Core.Services;

namespace Sakura.Live.Osc.Core
{
	public static class OscCoreDependencyInjectionExtensions
	{
		/// <summary>
		/// Add ThePanda services to the specified <see cref="IServiceCollection" />
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddOscCore(this IServiceCollection services)
		{
			services.AddSingleton<OscReceiverService>();
			services.AddSingleton<OscDuplicateService>();
			return services;
		}
	}
}
