namespace Tigrinya.App.Mobile.Services;

public class QaService
{
    public async Task<List<QaItem>> LoadChecklistAsync(string contentId)
    {
        try
        {
            var name = $"content/qa/{contentId}.qa.json";
            using var stream = await FileSystem.OpenAppPackageFileAsync(name);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var items = System.Text.Json.JsonSerializer.Deserialize<List<QaItem>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return items ?? new List<QaItem>();
        }
        catch
        {
            return new List<QaItem>();
        }
    }
}

public class QaItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string[]? Notes { get; set; }
    public string? SuggestedRole { get; set; }
}
