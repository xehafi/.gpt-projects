using Microsoft.Maui.Hosting;
using Microsoft.Maui;
using Microsoft.Extensions.Logging;
using Tigrinya.App.Mobile.Services;
using Plugin.Maui.Audio;
using Tigrinya.App.Mobile.ViewModels;

namespace Tigrinya.App.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Drop Ethiopic-capable font (e.g., Noto Sans Ethiopic) in Resources/Fonts and register here.
                // fonts.AddFont("NotoSansEthiopic.ttf", "Ethiopic");
            });

#if DEBUG
        //builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<ContentService>();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<DbService>();
        builder.Services.AddSingleton<IAudioManager>(AudioManager.Current);
        builder.Services.AddSingleton<SrsService>();
        builder.Services.AddSingleton<TemplateService>();
        builder.Services.AddSingleton<QaService>();
        builder.Services.AddSingleton<LocalizationService>();
        builder.Services.AddSingleton<XpService>();

        // ViewModels
        builder.Services.AddSingleton<LearnViewModel>();
        builder.Services.AddSingleton<ReviewViewModel>();
        builder.Services.AddSingleton<ExploreViewModel>();
        builder.Services.AddSingleton<ProfileViewModel>();

        return builder.Build();
    }
}
