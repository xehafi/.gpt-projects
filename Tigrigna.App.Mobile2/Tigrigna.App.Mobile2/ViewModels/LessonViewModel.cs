using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tigrigna.App.Mobile2.Services;

namespace Tigrigna.App.Mobile2.ViewModels;

public partial class LessonViewModel : ObservableObject
{
    private readonly ContentService _content;
    private readonly IProgressStore _progress;

    public string? SkillId { get; private set; }

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string subtitle = string.Empty;
    [ObservableProperty] private string icon = string.Empty;
    [ObservableProperty] private int xp;

    public LessonViewModel(ContentService content,IProgressStore progress)
    {
        _content = content;
        _progress = progress;
    }


    public async Task LoadByIdAsync(string skillId)
    {
        SkillId = skillId;
        var skill = await _content.GetSkillByIdAsync(skillId);
        if (skill is null) return;

        Title = skill.Title;
        Subtitle = skill.Subtitle;
        Icon = skill.Icon;
        Xp = skill.Xp;
    }
    [RelayCommand]
    public async Task CompleteAsync()
    {
        if (string.IsNullOrWhiteSpace(SkillId)) return;
        await _progress.AddXpAsync(5);
        await _progress.MarkSkillCompletedAsync(SkillId);
        LessonCompleted?.Invoke(this, EventArgs.Empty);
    }
    public event EventHandler? LessonCompleted;
}
