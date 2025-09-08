using Microsoft.Maui.Storage;

namespace Tigrigna.App.Mobile2.Services;

public class PreferencesProgressStore : IProgressStore
{
    private const string XpKey = "progress.total_xp";
    private const string CompletedKey = "progress.completed_ids"; // CSV

    public Task<int> GetTotalXpAsync()
        => Task.FromResult(Preferences.Get(XpKey, 0));

    public Task AddXpAsync(int delta)
    {
        var current = Preferences.Get(XpKey, 0);
        Preferences.Set(XpKey, Math.Max(0, current + delta));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> GetCompletedSkillIdsAsync()
    {
        var csv = Preferences.Get(CompletedKey, string.Empty);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(csv))
            foreach (var id in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
                set.Add(id.Trim());
        return Task.FromResult((IReadOnlyCollection<string>)set);
    }

    public async Task MarkSkillCompletedAsync(string skillId)
    {
        var set = new HashSet<string>(await GetCompletedSkillIdsAsync(), StringComparer.OrdinalIgnoreCase) { skillId };
        Preferences.Set(CompletedKey, string.Join(",", set));
    }

    public Task ResetAsync()
    {
        Preferences.Remove(XpKey);
        Preferences.Remove(CompletedKey);
        return Task.CompletedTask;
    }
}
