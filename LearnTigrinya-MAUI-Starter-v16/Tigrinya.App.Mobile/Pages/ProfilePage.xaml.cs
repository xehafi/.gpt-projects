namespace Tigrinya.App.Mobile.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var loc = this.Handler?.MauiContext?.Services.GetService(typeof(Tigrinya.App.Mobile.Services.LocalizationService)) as Tigrinya.App.Mobile.Services.LocalizationService;
            if (loc != null)
            {
                await loc.InitAsync();
                // If you added x:Name to labels, set their text here; otherwise skip for brevity.
            }
        }
        catch {}
    }
}
