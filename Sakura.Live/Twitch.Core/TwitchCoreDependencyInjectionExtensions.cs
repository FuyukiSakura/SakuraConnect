using Microsoft.Extensions.DependencyInjection;
using Sakura.Live.Twitch.Core.Services;

namespace Sakura.Live.Twitch.Core
{
    public static class TwitchCoreDependencyInjectionExtensions
    {
        /// <summary>
        /// Add ThePanda services to the specified <see cref="IServiceCollection" />
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTwitchCore(this IServiceCollection services)
        {
            services.AddSingleton<TwitchChatService>();
            return services;
        }
    }
}