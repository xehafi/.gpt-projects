using Tigrigna.App.Mobile2.Models;

namespace Tigrigna.App.Mobile2.Services;

public class ContentService
{
    public Task<IReadOnlyList<SkillCard>> GetStarterSkillsAsync()
    {
        IReadOnlyList<SkillCard> skills =
        [
            new() { Title = "Letters",  Subtitle = "Ge'ez basics", Xp = 10, Icon = "🔤" },
            new() { Title = "Numbers",  Subtitle = "1–10",        Xp = 8,  Icon = "🔢" },
            new() { Title = "Family",   Subtitle = "ኣቦ / ኣደ …",  Xp = 12, Icon = "👨‍👩‍👧" },
        ];
        return Task.FromResult(skills);
    }
    public async Task<SkillCard?> GetSkillByIdAsync(string id)
    {
        var list = await GetStarterSkillsAsync();
        return list.FirstOrDefault(s => s.Id == id);
    }
}

