using Sakura.Live.Connect.Dreamer.Data;
using Sakura.Live.Osc.Core;
using Sakura.Live.ThePanda.Core;

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
#if DEBUG
		    builder.Services.AddBlazorWebViewDeveloperTools();
#endif
	        builder.Services.AddThePanda();
	        builder.Services.AddOscCore();
            builder.Services.AddSingleton<WeatherForecastService>();
            
            var app = builder.Build();
            var monitor = app.Services.GetService<IThePandaMonitor>();
            monitor!.StartAsync();
            return app;
        }
    }
}