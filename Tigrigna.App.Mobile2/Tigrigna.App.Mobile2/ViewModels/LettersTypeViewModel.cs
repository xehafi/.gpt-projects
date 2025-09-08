using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tigrigna.App.Mobile2.Models;
using Tigrigna.App.Mobile2.Services;

namespace Tigrigna.App.Mobile2.ViewModels;

public partial class LettersTypeViewModel : ObservableObject
{
    private readonly ContentService _content;
    private readonly IProgressStore _progress;
    private readonly IAudioService _audio;

    [ObservableProperty] private LetterItem? current;
    [ObservableProperty] private ObservableCollection<LetterItem> options = new();
    [ObservableProperty] private string feedback = string.Empty;
    [ObservableProperty] private int questionIndex;
    [ObservableProperty] private int totalQuestions = 5;
    [ObservableProperty] private int score;

    private List<LetterItem> _pool = new();

    public LettersTypeViewModel(ContentService content, IProgressStore progress, IAudioService audio)
    {
        _content = content;
        _progress = progress;
        _audio = audio;
    }

    public async Task InitializeAsync()
    {
        _pool = (await _content.GetRandomLettersAsync(TotalQuestions)).ToList();
        QuestionIndex = 0;
        Score = 0;
        await LoadQuestionAsync();
    }

    [RelayCommand]
    public async Task PlayAsync()
    {
        if (Current is null) return;
        await _audio.PlayAsync($"{Current.Id}.mp3", Current.Transliteration);
    }

    [RelayCommand]
    public async Task ChooseAsync(LetterItem chosen)
    {
        if (Current is null) return;

        if (chosen.Id == Current.Id)
        {
            Score++;
            Feedback = "✅ Correct!";
        }
        else
        {
            Feedback = $"❌ Nope — correct was: {Current.Glyph}";
        }

        await Task.Delay(600);
        if (QuestionIndex + 1 >= TotalQuestions)
        {
            var gained = Math.Max(1, Score);
            await _progress.AddXpAsync(gained);
            Completed?.Invoke(this, gained);
            return;
        }

        QuestionIndex++;
        await LoadQuestionAsync();
    }

    private async Task LoadQuestionAsync()
    {
        Feedback = string.Empty;

        Current = _pool[QuestionIndex];

        var all = (await _content.GetBeginnerLettersAsync()).ToList();
        var rng = new Random();

        var distractors = all.Where(x => x.Id != Current!.Id)
                             .OrderBy(_ => rng.Next())
                             .Take(2)
                             .ToList();

        var opts = new List<LetterItem> { Current! };
        opts.AddRange(distractors);
        Options = new ObservableCollection<LetterItem>(opts.OrderBy(_ => rng.Next()));
    }

    public event Action<LettersTypeViewModel, int>? Completed;
}
