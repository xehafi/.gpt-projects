using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tigrigna.App.Mobile2.Models;
using Tigrigna.App.Mobile2.Services;

namespace Tigrigna.App.Mobile2.ViewModels;

public partial class LearnViewModel : ObservableObject
{
    private readonly ContentService _content;

    [ObservableProperty] private ObservableCollection<SkillCard> skills = new();
    [ObservableProperty] private string learnTitle = "Let’s start learning";
    [ObservableProperty] private string startHereTitle = "Start here";
    [ObservableProperty] private string xpBanner = "0 XP";
    [ObservableProperty] private string startLabel = "Start";

    public LearnViewModel(ContentService content)
    {
        _content = content;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var items = await _content.GetStarterSkillsAsync();
        Skills = new ObservableCollection<SkillCard>(items);
    }

    [RelayCommand]
    private void OpenLesson(SkillCard? skill)
    {
        if (skill is null) return;
        LessonRequested?.Invoke(this, skill);
    }

    public event EventHandler<SkillCard>? LessonRequested;
}
