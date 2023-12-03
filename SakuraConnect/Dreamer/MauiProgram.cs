
using BlazorBootstrap;
using Blazorise;
using Blazorise.Bootstrap;
using CommunityToolkit.Maui;
using Sakura.Live.Cognitive.Translation.Core;
using Sakura.Live.Connect.Dreamer.Services;
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.Connect.Dreamer.Services.Twitch;
using Sakura.Live.Obs.Core;
using Sakura.Live.OpenAi.Core;
using Sakura.Live.Osc.Core;
using Sakura.Live.Speech.Core;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Interfaces;
using Sakura.Live.Twitch.Core;

namespace Sakura.Live.Connect.Dreamer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddBlazorise()
                .AddBootstrapProviders();
#if DEBUG
		    builder.Services.AddBlazorWebViewDeveloperTools();
#endif
	        builder.Services.AddThePanda();
	        builder.Services.AddOscCore();
            builder.Services.AddObsCore();
            builder.Services.AddSpeechCore();
            builder.Services.AddTranslationCore();

            builder.Services.AddOpenAiCore();
            builder.Services.AddTwitchCore();
            AddDreamerCore(builder);

            var app = builder.Build();
            var monitor = app.Services.GetService<IThePandaMonitor>();
            monitor!.StartAsync();
            return app;
        }

        /// <summary>
        /// Add the services of the dreamer app
        /// </summary>
        /// <param name="builder"></param>
        static void AddDreamerCore(MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<BigBrainService>();
            builder.Services.AddSingleton<GreetingService>();
            builder.Services.AddScoped<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<IAiCharacterService, AiCharacterService>();
            builder.Services.AddSingleton<IPandaMessenger, SimpleMessenger>();
            builder.Services.AddSingleton<OneCommeService>();
            builder.Services.AddScoped<AzureConversationService>();
            builder.Services.AddSingleton<ChatMonitorService>();
            builder.Services.AddScoped<ChatResponseService>();
        }
    }
}