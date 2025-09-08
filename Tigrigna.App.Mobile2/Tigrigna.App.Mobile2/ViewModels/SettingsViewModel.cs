using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tigrigna.App.Mobile2.Services;


namespace Tigrigna.App.Mobile2.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IProgressStore _progress;

    [ObservableProperty] private string theme = Application.Current?.UserAppTheme.ToString() ?? "Unspecified";

    public SettingsViewModel(IProgressStore progress)
    {
        _progress = progress;
    }

    [RelayCommand]
    public void SetLight() => Application.Current!.UserAppTheme = AppTheme.Light;

    [RelayCommand]
    public void SetDark() => Application.Current!.UserAppTheme = AppTheme.Dark;

    [RelayCommand]
    public async Task ResetProgressAsync() => await _progress.ResetAsync();
}
