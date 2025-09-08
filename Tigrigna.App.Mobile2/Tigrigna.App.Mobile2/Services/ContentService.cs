using System.Text.Json;
using Tigrigna.App.Mobile2.Models;
using Microsoft.Maui.Storage;

namespace Tigrigna.App.Mobile2.Services;

public class ContentService
{
    // Home page skills (stable IDs so progress works across runs)
    public Task<IReadOnlyList<SkillCard>> GetStarterSkillsAsync()
    {
        IReadOnlyList<SkillCard> skills = new List<SkillCard>
        {
            new() { Id = "letters", Title = "Letters",  Subtitle = "Ge'ez basics", Xp = 10, Icon = "🔤" },
            new() { Id = "numbers", Title = "Numbers",  Subtitle = "1–10",         Xp = 8,  Icon = "🔢" },
            new() { Id = "family",  Title = "Family",   Subtitle = "ኣቦ / ኣደ …",  Xp = 12, Icon = "👨‍👩‍👧" },
        };
        return Task.FromResult(skills);
    }

    // 🔧 This is what LessonViewModel is calling
    public async Task<SkillCard?> GetSkillByIdAsync(string id)
    {
        var list = await GetStarterSkillsAsync();
        return list.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    // Beginner Letters content from Resources/Raw/letters.beginner.json
    public async Task<IReadOnlyList<LetterItem>> GetBeginnerLettersAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("letters.beginner.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var root = JsonSerializer.Deserialize<LettersRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return root?.Letters ?? new List<LetterItem>();
    }

    public async Task<IReadOnlyList<LetterItem>> GetRandomLettersAsync(int count)
    {
        var all = (await GetBeginnerLettersAsync()).ToList();
        var rng = new Random();
        return all.OrderBy(_ => rng.Next()).Take(Math.Min(count, all.Count)).ToList();
    }
}
