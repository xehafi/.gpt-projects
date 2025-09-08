using CommunityToolkit.Mvvm.DependencyInjection;
using Tigrigna.App.Mobile2.ViewModels;

namespace Tigrigna.App.Mobile2.Views;

public partial class ProgressPage : ContentPage
{
    private readonly ProgressViewModel _vm;

    public ProgressPage(ProgressViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public ProgressPage() : this(Ioc.Default.GetRequiredService<ProgressViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
