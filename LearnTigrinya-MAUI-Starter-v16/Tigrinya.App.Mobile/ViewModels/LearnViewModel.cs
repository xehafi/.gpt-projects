using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Tigrinya.App.Mobile.Services;

namespace Tigrinya.App.Mobile.ViewModels;

public partial class LearnViewModel : ObservableObject
{
    private readonly ContentService _content;

    [ObservableProperty]
    private ObservableCollection<SkillCard> skills = new();

    // Titles / labels shown on the page (placeholders; wire your loc later)
    [ObservableProperty] private string learnTitle = string.Empty;
    [ObservableProperty] private string startHereTitle = string.Empty;
    [ObservableProperty] private string xpBanner = string.Empty;
    [ObservableProperty] private string startLabel = string.Empty;

    public IRelayCommand<string> OpenLessonCommand { get; }

    public LearnViewModel(ContentService content)
    {
        _content = content;
        OpenLessonCommand = new RelayCommand<string>(OpenLesson);
        LoadSkills();

        // Optional: set initial labels/XPs until you hook real services
        _ = RefreshXpAsync();
        RefreshLabels();
    }

    private void LoadSkills()
    {
        // Seed with sample skills
        Skills.Add(new SkillCard { Id = "alphabet-1", Title = "Alphabet I", Description = "ሀ-series (ha)" });
        Skills.Add(new SkillCard { Id = "numbers-0-20", Title = "Numbers 0–20", Description = "Count and say prices" });
    }

    private async void OpenLesson(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;

        var lesson = await _content.LoadLessonAsync(id);
        if (lesson is null)
        {
            await Shell.Current.DisplayAlert("Lesson", "Not implemented yet.", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"lesson?lessonId={id}");
    }

    // ---- Moved INSIDE the class ----

    private async Task RefreshXpAsync()
    {
        // Placeholder values until you inject a real XP service
        var today = 0;
        var goal = 10;
        var streak = 0;
        var weekly = 0;

        LearnTitle = "Skill Tree";
        StartHereTitle = "Start here";
        XpBanner = $"Daily: {today}/{goal} • Streak: {streak} 🔥 • Weekly: {weekly} XP";

        await Task.CompletedTask;
    }

    private void RefreshLabels()
    {
        // Placeholder until you inject localization
        StartLabel = "Start";
    }
}

// Keep this simple model outside the ViewModel class
public class SkillCard
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
