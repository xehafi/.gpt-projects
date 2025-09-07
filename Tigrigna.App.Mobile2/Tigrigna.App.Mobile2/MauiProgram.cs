using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tigrigna.App.Mobile2.Services;
using Tigrigna.App.Mobile2.ViewModels;
using Tigrigna.App.Mobile2.Views;

namespace Tigrigna.App.Mobile2;  // ← IMPORTANT

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()   // resolves because we're in the same namespace
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // DI registrations
        builder.Services.AddSingleton<ContentService>();
        builder.Services.AddTransient<LearnViewModel>();
        builder.Services.AddTransient<LearnPage>();

        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

        builder.Services.AddTransient<LessonViewModel>();
        builder.Services.AddTransient<LessonPage>();

        var app = builder.Build();
        Ioc.Default.ConfigureServices(app.Services);
        return app;
    }
}
