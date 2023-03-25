using Microsoft.Extensions.DependencyInjection;
using Sakura.Live.Cognitive.Translation.Core.Services;

namespace Sakura.Live.Cognitive.Translation.Core
{
	public static class TranslationCoreDependencyInjectionExtensions
	{
		/// <summary>
		/// Add ThePanda services to the specified <see cref="IServiceCollection" />
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddTranslationCore(this IServiceCollection services)
		{
			services.AddSingleton<TranslationService>();
			return services;
		}
	}
}
