using Tigrigna.App.Mobile2.Services;
using Tigrigna.App.Mobile2.ViewModels;

namespace Tigrigna.App.Mobile2.Views;

[QueryProperty(nameof(SkillId), "skillId")]
public partial class LessonPage : ContentPage
{
    private readonly LessonViewModel _vm;
    private readonly INavigationService _nav;
    private string? _skillId;

    public LessonPage(LessonViewModel vm, INavigationService nav)
    {
        InitializeComponent();
        _vm = vm;
        _nav = nav;
        _vm.LessonCompleted += async (_, __) =>
        {
            await DisplayAlert("Great!", "XP awarded +5 and marked completed.", "OK");
            await _nav.GoBackAsync();
        };
        BindingContext = _vm;
    }


    // Shell will set this when navigating with ?skillId=...
    public string? SkillId
    {
        get => _skillId;
        set
        {
            _skillId = value;
            if (!string.IsNullOrWhiteSpace(value))
                _ = _vm.LoadByIdAsync(value);
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await _nav.GoBackAsync();
    private async void OnStartTypeClicked(object? sender, EventArgs e)
    => await _nav.GoToLettersTypeAsync();

    private async void OnStartTraceClicked(object? sender, EventArgs e)
        => await _nav.GoToLettersTraceAsync(_vm.Icon); // pass glyph; Icon is our glyph here

}
