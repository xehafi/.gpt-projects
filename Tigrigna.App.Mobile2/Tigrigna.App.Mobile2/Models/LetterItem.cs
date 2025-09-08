namespace Tigrigna.App.Mobile2.Models;

public class LetterItem
{
    public string Id { get; set; } = string.Empty;
    public string Glyph { get; set; } = string.Empty;
    public string Transliteration { get; set; } = string.Empty;
    public int Xp { get; set; } = 1;
}

public class LettersRoot
{
    public List<LetterItem> Letters { get; set; } = new();
}
