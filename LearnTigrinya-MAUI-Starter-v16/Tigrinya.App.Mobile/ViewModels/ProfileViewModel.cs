using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tigrinya.App.Mobile.Services;

namespace Tigrinya.App.Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly DbService _db;
    public ObservableCollection<string> BaseLanguages { get; } = new() { "English", "Dutch" };
    [ObservableProperty] private string selectedBaseLanguage = "English";

    public ObservableCollection<AvatarOption> Avatars { get; } = new()
    {
        new AvatarOption("Tigrinya"),
        new AvatarOption("Tigre"),
        new AvatarOption("Saho"),
        new AvatarOption("Afar"),
        new AvatarOption("Bilen"),
        new AvatarOption("Kunama"),
        new AvatarOption("Nara"),
        new AvatarOption("Rashaida"),
        new AvatarOption("Hedareb"),
    };

    public ObservableCollection<OutfitOption> Outfits { get; } = new()
    {
        new OutfitOption("classic","Classic white"),
        new OutfitOption("festival","Festival set"),
        new OutfitOption("casual","Casual")
    };

    public ObservableCollection<int> GoalOptions { get; } = new() { 10, 20, 30, 50 };

    [ObservableProperty] private AvatarOption? selectedAvatar;
    [ObservableProperty] private OutfitOption? selectedOutfit;
    [ObservableProperty] private int selectedDailyGoal = 20;
    [ObservableProperty] private int todayXp = 0;
    public IRelayCommand SaveCommand { get; }

    public ProfileViewModel(DbService db)
    {
        _db = db;
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        Task.Run(LoadAsync);
    }

    private async Task LoadAsync()
    {
        var p = await _db.GetOrCreateProfileAsync();
        SelectedBaseLanguage = p.BaseLang;
        SelectedAvatar = Avatars.FirstOrDefault(a => a.Name == p.AvatarGroup) ?? Avatars[0];
        SelectedOutfit = Outfits.FirstOrDefault(o => o.Id == p.OutfitId) ?? Outfits[0];
        SelectedDailyGoal = p.DailyGoal;
        TodayXp = p.TodayXp;
        OnPropertyChanged(nameof(SelectedBaseLanguage));
    }

    private async Task SaveAsync()
    {
        var p = await _db.GetOrCreateProfileAsync();
        p.BaseLang = SelectedBaseLanguage;
        p.AvatarGroup = SelectedAvatar?.Name ?? p.AvatarGroup;
        p.OutfitId = SelectedOutfit?.Id ?? p.OutfitId;
        p.DailyGoal = SelectedDailyGoal;
        await _db.UpsertProfileAsync(p);
        await Shell.Current.DisplayAlert("Saved", "Profile updated.", "OK");
    }
}

public class AvatarOption
{
    public string Name { get; }
    public AvatarOption(string name) => Name = name;
    public override string ToString() => Name;
}

public class OutfitOption
{
    public string Id { get; }
    public string Title { get; }
    public OutfitOption(string id, string title) { Id = id; Title = title; }
    public override string ToString() => Title;
}
