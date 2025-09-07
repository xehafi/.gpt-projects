namespace Tigrigna.App.Mobile2.Services;

public class ShellNavigationService : INavigationService
{
    public Task GoToLessonAsync(string skillId)
        => Shell.Current.GoToAsync($"lesson?skillId={Uri.EscapeDataString(skillId)}");

    public Task GoBackAsync()
        => Shell.Current.GoToAsync("..");
}
