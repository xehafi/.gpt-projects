using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Tigrigna.App.Mobile2.Models;
using Tigrigna.App.Mobile2.ViewModels;

namespace Tigrigna.App.Mobile2.Views;

public partial class LearnPage : ContentPage
{
    private readonly LearnViewModel _vm;

    // DI-first constructor
    public LearnPage(LearnViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        _vm.LessonRequested += OnLessonRequested;
        BindingContext = _vm;
    }

    // Keeps Shell/XAML happy if it tries to new() without DI
    public LearnPage() : this(Ioc.Default.GetRequiredService<LearnViewModel>()) { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private async void OnLessonRequested(object? sender, SkillCard e)
    {
        await DisplayAlert("Lesson", $"Open lesson: {e.Title}", "OK");
    }
}
