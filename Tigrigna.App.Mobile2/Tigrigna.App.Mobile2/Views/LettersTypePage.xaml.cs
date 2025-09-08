using CommunityToolkit.Mvvm.DependencyInjection;
using Tigrigna.App.Mobile2.ViewModels;

namespace Tigrigna.App.Mobile2.Views;

public partial class LettersTypePage : ContentPage
{
    private readonly LettersTypeViewModel _vm;

    public LettersTypePage(LettersTypeViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        _vm.Completed += async (_, gained) =>
        {
            await DisplayAlert("Nice!", $"You earned +{gained} XP", "OK");
            await Shell.Current.GoToAsync("..");
        };
        BindingContext = _vm;
    }

    public LettersTypePage() : this(Ioc.Default.GetRequiredService<LettersTypeViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }
}
