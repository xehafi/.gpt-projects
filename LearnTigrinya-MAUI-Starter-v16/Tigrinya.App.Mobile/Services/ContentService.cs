using System.Text.Json;

namespace Tigrinya.App.Mobile.Services;

public class ContentService
{
    public async Task<Lesson?> LoadLessonAsync(string id)
    {
        try
        {
            string? name = id switch
            {
                "alphabet-1" => "content/alphabet1.json",
                "greetings-1" => "content/greetings1.json",
                _ => $"content/{id}.json"
            };
            if (name == null) return null;
            using var stream = await FileSystem.OpenAppPackageFileAsync(name);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<Lesson>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }
}

public class Lesson
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<TaskItem> Tasks { get; set; } = new();
}

public class DialogStep
{
    public string? PromptEn { get; set; }
    public string? PromptNl { get; set; }
    public string? Audio { get; set; }
    public List<string> Choices { get; set; } = new();
    public int AnswerIndex { get; set; } = 0;
}

public class LabelOption
{
    public string? En { get; set; }
    public string? Nl { get; set; }
    public string? Tig { get; set; } // TODO: fill with reviewed Tigrinya labels
}

public class GrammarExample
{
    public string Tig { get; set; } = string.Empty;
    public string? GlossEn { get; set; }
    public string? GlossNl { get; set; }
    public string? TranslationEn { get; set; }
    public string? TranslationNl { get; set; }
}

public class TaskItem
{
    public string Type { get; set; } = string.Empty; // trace, listen_select, read_tile, picture_word, type_mini_keyboard, dialog_goal, grammar_tip
    public string? Glyph { get; set; }
    public List<string>? Tiles { get; set; }
    public List<string>? Choices { get; set; }
    public string? Answer { get; set; }
    public List<string>? Images { get; set; }
    public string? AnswerImage { get; set; }
    public string? Audio { get; set; }
    public string? PromptEn { get; set; }
    public string? PromptNl { get; set; }
    // Dialog
    public string? Goal { get; set; }
    public List<DialogStep>? Dialog { get; set; }
    // Grammar tip
    public string? Title { get; set; }
    public string? TipEn { get; set; }
    public string? TipNl { get; set; }
    public List<GrammarExample>? Examples { get; set; }
    public List<string>? QuizChoices { get; set; }
    public int? AnswerIndex { get; set; }
    // Numbers
    public int? TargetNumber { get; set; }
    public string? TargetText { get; set; }
    public string? PriceText { get; set; }
    // Image labeling
    public List<LabelOption>? LabelChoices { get; set; }
    public LabelOption? CorrectLabel { get; set; }
    // Flashcard / Audio
    public string? FrontText { get; set; }
    public string? BackText { get; set; }
    public string? TigText { get; set; }
    public string? EnText { get; set; }
    public string? NlText { get; set; }
    public string? AudioTig { get; set; }
    public string? AudioEn { get; set; }
    public string? AudioNl { get; set; }
    // Translation choices (EN/NL)
    public List<string>? QuizChoicesEn { get; set; }
    public List<string>? QuizChoicesNl { get; set; }
    // Word ordering
    public List<string>? Tokens { get; set; }
}
