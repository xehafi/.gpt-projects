namespace Tigrigna.App.Mobile2.Services;

public interface IProgressStore
{
    Task<int> GetTotalXpAsync();
    Task AddXpAsync(int delta);
    Task<IReadOnlyCollection<string>> GetCompletedSkillIdsAsync();
    Task MarkSkillCompletedAsync(string skillId);
    Task ResetAsync();
}
