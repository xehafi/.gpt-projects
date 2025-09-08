using CommunityToolkit.Mvvm.DependencyInjection;
using Tigrigna.App.Mobile2.ViewModels;

namespace Tigrigna.App.Mobile2.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public SettingsPage() : this(Ioc.Default.GetRequiredService<SettingsViewModel>()) { }
}
