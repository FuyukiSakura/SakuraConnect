using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.Extensions.Logging;
using Sakura.Live.OpenAi.Core;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Interfaces;
using Sakura.Live.Twitch.Core;
using SakuraConnect.Streamer.Services;
using SakuraConnect.Streamer.Services.Ai;

namespace SakuraConnect.Streamer
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
            builder.Services
                .AddBlazorise( options =>
                {
                    options.Immediate = true;
                } )
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddThePanda()
                .AddTwitchCore()
                .AddOpenAiCore();
            builder.Services.AddTransient<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<IAiCharacterService, AiCharacterService>();
            var app = builder.Build();
            var monitor = app.Services.GetService<IThePandaMonitor>();
            monitor!.StartAsync();
            return app;
        }
    }
}
