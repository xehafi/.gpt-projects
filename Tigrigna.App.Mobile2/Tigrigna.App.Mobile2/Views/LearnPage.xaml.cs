using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Tigrigna.App.Mobile2.Models;
using Tigrigna.App.Mobile2.Services;
using Tigrigna.App.Mobile2.ViewModels;

namespace Tigrigna.App.Mobile2.Views;

public partial class LearnPage : ContentPage
{
    private readonly LearnViewModel _vm;
    private readonly INavigationService _nav;

    public LearnPage(LearnViewModel vm, INavigationService nav)
    {
        InitializeComponent();
        _vm = vm;
        _nav = nav;
        _vm.LessonRequested += OnLessonRequested;
        BindingContext = _vm;
    }

    // keep the parameterless fallback for Shell/XAML if needed
    public LearnPage()
        : this(Ioc.Default.GetRequiredService<LearnViewModel>(),
               Ioc.Default.GetRequiredService<INavigationService>())
    { }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private async void OnLessonRequested(object? sender, SkillCard skill)
    {
        await _nav.GoToLessonAsync(skill.Id);
    }
}
