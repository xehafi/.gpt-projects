namespace Tigrinya.App.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
        // Fire and forget DB init
        Task.Run(async () =>
        {
            try
            {
                var services = Current?.Handler?.MauiContext?.Services;
                var srs = services?.GetService(typeof(Tigrinya.App.Mobile.Services.SrsService)) as Tigrinya.App.Mobile.Services.SrsService;
                if (srs != null) await srs.InitAsync();
                var loc = services?.GetService(typeof(Tigrinya.App.Mobile.Services.LocalizationService)) as Tigrinya.App.Mobile.Services.LocalizationService;
                if (loc != null) await loc.InitAsync();
                // placement check
                var db = services?.GetService(typeof(Tigrinya.App.Mobile.Services.DbService)) as Tigrinya.App.Mobile.Services.DbService;
                if (db != null)
                {
                    var p = await db.GetOrCreateProfileAsync();
                    if (!p.HasCompletedPlacement)
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync("placement"));
                    }
                }
            }
            catch { }
        });
    }
}
