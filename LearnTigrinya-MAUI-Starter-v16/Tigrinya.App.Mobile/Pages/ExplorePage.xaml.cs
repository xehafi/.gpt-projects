namespace Tigrinya.App.Mobile.Pages;

public partial class ExplorePage : ContentPage
{
    public ExplorePage()
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
                TitleLabel.Text = loc.L("Explore_Title");
            }
        }
        catch {}
    }

    private async void OnOpenQa(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("qa");
    }

    private async void OnPlayGlyph(object sender, EventArgs e)
    {
        if (sender is Button b && b.Text is string s)
        {
            // Placeholder: play audio for glyph 's' if available.
            await DisplayAlert("Audio", $"Play sound for: {s}", "OK");
        }
    }
}
