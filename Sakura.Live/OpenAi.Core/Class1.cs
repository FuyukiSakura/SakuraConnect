using Microsoft.Extensions.DependencyInjection;
using Sakura.Live.OpenAi.Core.Services;

namespace Sakura.Live.OpenAi.Core
{
    public static class OpenAiCoreDependencyInjectionExtensions
    {
        /// <summary>
        /// Add open ai services to the specified <see cref="IServiceCollection" />
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenAiCore(this IServiceCollection services)
        {
            services.AddSingleton<OpenAiService>();
            services.AddSingleton<GreetingService>();
            return services;
        }
    }
}