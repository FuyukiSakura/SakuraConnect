﻿using BlazorBootstrap;
﻿using Blazorise;
using Blazorise.Bootstrap;
using Sakura.Live.Connect.Dreamer.Services;
using Sakura.Live.Obs.Core;
using Sakura.Live.Osc.Core;
using Sakura.Live.ThePanda.Core;
using Sakura.Live.ThePanda.Core.Interfaces;

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
            builder.Services.AddScoped<ISettingsService, SettingsService>();
	        builder.Services.AddOscCore();
            builder.Services.AddObsCore();

            var app = builder.Build();
            var monitor = app.Services.GetService<IThePandaMonitor>();
            monitor!.StartAsync();
            return app;
        }
    }
}