using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tigrigna.App.Mobile2.Services;


namespace Tigrigna.App.Mobile2.ViewModels;

public partial class ProgressViewModel : ObservableObject
{
    private readonly IProgressStore _progress;

    [ObservableProperty] private int totalXp;
    [ObservableProperty] private int completedCount;

    public ProgressViewModel(IProgressStore progress)
    {
        _progress = progress;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        TotalXp = await _progress.GetTotalXpAsync();
        var ids = await _progress.GetCompletedSkillIdsAsync();
        CompletedCount = ids.Count;
    }

    [RelayCommand]
    public async Task ResetAsync()
    {
        await _progress.ResetAsync();
        await LoadAsync();
    }
}
