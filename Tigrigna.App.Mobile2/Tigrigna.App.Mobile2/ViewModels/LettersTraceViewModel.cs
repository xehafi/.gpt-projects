using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tigrigna.App.Mobile2.Services;

namespace Tigrigna.App.Mobile2.ViewModels;

public partial class LettersTraceViewModel : ObservableObject
{
    private readonly IProgressStore _progress;

    [ObservableProperty] private string glyph = "ኣ"; // default; we can pass a glyph later

    public LettersTraceViewModel(IProgressStore progress)
    {
        _progress = progress;
    }

    [RelayCommand]
    public async Task CompleteAsync()
    {
        await _progress.AddXpAsync(2);
        Completed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Completed;
}
