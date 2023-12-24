using Microsoft.Extensions.DependencyInjection;
using Sakura.Live.Speech.Core.Services;

namespace Sakura.Live.Speech.Core
{
	public static class SpeechCoreDependencyInjectionExtensions
	{
		/// <summary>
		/// Add ThePanda services to the specified <see cref="IServiceCollection" />
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddSpeechCore(this IServiceCollection services)
        {
            services.AddSingleton<SpeechQueueService>();
            services.AddSingleton<AzureSpeechSettingsService>();
			services.AddSingleton<AzureSpeechService>();
            services.AddSingleton<AzureTextToSpeechService>();
			services.AddSingleton<AzureTextAnalyticsSettingsService>();
			services.AddSingleton<AzureTextAnalyticsService>();
            return services;
		}
	}
}
