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
}
