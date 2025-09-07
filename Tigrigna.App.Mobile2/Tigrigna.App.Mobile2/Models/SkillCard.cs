namespace Tigrigna.App.Mobile2.Models;

public class SkillCard
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "📘";
    public int Xp { get; set; }
}
