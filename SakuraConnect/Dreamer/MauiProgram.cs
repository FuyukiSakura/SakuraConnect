
using BlazorBootstrap;
using Blazorise;
using Blazorise.Bootstrap;
using Sakura.Live.Cognitive.Translation.Core;
using Sakura.Live.Connect.Dreamer.Services;
using Sakura.Live.Connect.Dreamer.Services.Ai;
using Sakura.Live.Obs.Core;
using Sakura.Live.OpenAi.Core;
using Sakura.Live.OpenAi.Core.Services;
using Sakura.Live.Osc.Core;
using Sakura.Live.Speech.Core;
using Sakura.Live.Speech.Core.Services;
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
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
            builder.Services.AddScoped<ISettingsService, SettingsService>();
            builder.Services.AddScoped<IAiCharacterService, AiCharacterService>();
            builder.Services.AddScoped<AzureConversationService>();
            builder.Services.AddScoped<TwitchChatResponseService>();

            var app = builder.Build();
            var monitor = app.Services.GetService<IThePandaMonitor>();
            monitor!.StartAsync();
            return app;
        }
    }
}