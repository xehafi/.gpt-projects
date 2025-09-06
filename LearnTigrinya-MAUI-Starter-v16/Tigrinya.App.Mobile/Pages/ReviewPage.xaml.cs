namespace Tigrinya.App.Mobile.Pages;

public partial class ReviewPage : ContentPage
{
    public ReviewPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is Tigrinya.App.Mobile.ViewModels.ReviewViewModel vm)
        {
            vm.Refresh();
        }
        try
        {
            var loc = this.Handler?.MauiContext?.Services.GetService(typeof(Tigrinya.App.Mobile.Services.LocalizationService)) as Tigrinya.App.Mobile.Services.LocalizationService;
            if (loc != null)
            {
                await loc.InitAsync();
                TitleLabel.Text = loc.L("Review_Title");
            }
        }
        catch {}
    }
}
