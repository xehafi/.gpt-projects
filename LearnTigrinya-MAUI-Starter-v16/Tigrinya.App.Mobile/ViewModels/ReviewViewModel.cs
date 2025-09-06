using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tigrinya.App.Mobile.Services;

namespace Tigrinya.App.Mobile.ViewModels;

public partial class ReviewViewModel : ObservableObject
{
    private readonly SrsService _srs;

    [ObservableProperty]
    private string dueSummary = "0 due items";

    public IRelayCommand StartReviewCommand { get; }

    public ReviewViewModel(SrsService srs)
    {
        _srs = srs;
        StartReviewCommand = new RelayCommand(StartReview);
        Refresh();
    }

    public void Refresh()
    {
        var due = _srs.DueCount();
        DueSummary = $"{due} due items";
    }

    private async void StartReview()
    {
        await Shell.Current.DisplayAlert("Review", "SRS session starting (stub).", "OK");
    }
}
