using CommunityToolkit.Mvvm.ComponentModel;
using Tigrigna.App.Mobile2.Services;

namespace Tigrigna.App.Mobile2.ViewModels;

public partial class LessonViewModel : ObservableObject
{
    private readonly ContentService _content;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string subtitle = string.Empty;
    [ObservableProperty] private string icon = string.Empty;
    [ObservableProperty] private int xp;

    public LessonViewModel(ContentService content)
    {
        _content = content;
    }

    public async Task LoadByIdAsync(string skillId)
    {
        var skill = await _content.GetSkillByIdAsync(skillId);
        if (skill is null) return;

        Title = skill.Title;
        Subtitle = skill.Subtitle;
        Icon = skill.Icon;
        Xp = skill.Xp;
    }
}
