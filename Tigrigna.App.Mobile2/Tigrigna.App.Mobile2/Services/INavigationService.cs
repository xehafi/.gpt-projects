namespace Tigrigna.App.Mobile2.Services;

public interface INavigationService
{
    Task GoToLessonAsync(string skillId);
    Task GoBackAsync();
}
