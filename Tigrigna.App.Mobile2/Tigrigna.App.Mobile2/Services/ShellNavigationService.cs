namespace Tigrigna.App.Mobile2.Services;

public class ShellNavigationService : INavigationService
{
    public Task GoToLessonAsync(string skillId)
        => Shell.Current.GoToAsync($"lesson?skillId={Uri.EscapeDataString(skillId)}");

    public Task GoBackAsync()
        => Shell.Current.GoToAsync("..");
    public Task GoToLettersTypeAsync()
    => Shell.Current.GoToAsync("lettersType");

    public Task GoToLettersTraceAsync(string glyph)
        => Shell.Current.GoToAsync($"lettersTrace?glyph={Uri.EscapeDataString(glyph)}");

}
