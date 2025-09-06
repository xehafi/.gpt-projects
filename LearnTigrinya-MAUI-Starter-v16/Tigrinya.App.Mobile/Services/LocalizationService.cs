namespace Tigrinya.App.Mobile.Services;

public class LocalizationService
{
    private readonly DbService _db;
    private string _lang = "English"; // "English" or "Dutch"

    public LocalizationService(DbService db)
    {
        _db = db;
    }

    public async Task InitAsync()
    {
        try { var p = await _db.GetOrCreateProfileAsync(); _lang = p.BaseLang; } catch { }
    }

    public string L(string key)
    {
        var lang = _lang == "Dutch" ? "nl" : "en";
        return key switch
        {
            // Learn
            "Learn_SkillTree" => lang == "nl" ? "Vaardigheden" : "Skill Tree",
            "Learn_StartHere" => lang == "nl" ? "Begin hier" : "Start here",
            "Learn_Start" => lang == "nl" ? "Start" : "Start",
            "Learn_Recommended" => lang == "nl" ? "Aanbevolen" : "Recommended",
            // Review
            "Review_Title" => lang == "nl" ? "Herhalen (SRS)" : "Review (SRS)",
            "Review_WeakItems" => lang == "nl" ? "Zwakke items" : "Weak Items",
            // Profile
            "Profile_Title" => lang == "nl" ? "Profiel & Avatar" : "Profile & Avatar",
            "Profile_Save" => lang == "nl" ? "Opslaan" : "Save",
            "Profile_BaseLang" => lang == "nl" ? "Basistaal" : "Base language",
            "Profile_AvatarLabel" => lang == "nl" ? "Kies je avatar (9 Eritrese groepen)" : "Choose your avatar (9 Eritrean groups)",
            "Profile_Outfit" => lang == "nl" ? "Kledij" : "Outfit",
            // Placement
            "Placement_Title" => lang == "nl" ? "Plaatsingstest" : "Placement Check",
            "Placement_Desc" => lang == "nl" ? "Beantwoord enkele vragen om je startniveau te bepalen." : "Answer a few quick questions so we can suggest a starting point.",
            "Placement_Play" => lang == "nl" ? "Afspelen" : "Play",
            "Placement_Finish" => lang == "nl" ? "Afronden" : "Finish",
            "Placement_Q1" => lang == "nl" ? "Kies de letter ‘ሀ’." : "Pick the letter 'ሀ'.",
            "Placement_Q2" => lang == "nl" ? "Welk nummer hoor je?" : "Which number do you hear?",
            "Placement_Q3" => lang == "nl" ? "Hoe antwoord je op ‘ሰላም!’ ?" : "How would you respond to 'ሰላም!' ?",
            // QA
            "QA_Title" => lang == "nl" ? "Content-kwaliteitscontrole" : "Content QA — Reviewer",
            "QA_Content" => lang == "nl" ? "Content" : "Content",
            "QA_Open" => lang == "nl" ? "Open kwaliteitscontrole" : "Open QA Review",
            // Lesson Player
            "LP_Next" => lang == "nl" ? "Volgende" : "Next",
            "LP_Skip" => lang == "nl" ? "Overslaan" : "Skip",
            "LP_Check" => lang == "nl" ? "Controleer" : "Check",
            "LP_PlayAudio" => lang == "nl" ? "Audio afspelen" : "Play audio",
            // XP
            "XP_DailyGoal" => lang == "nl" ? "Dagdoel" : "Daily goal",
            "XP_Weekly" => lang == "nl" ? "Week" : "Week",
            "XP_Streak" => lang == "nl" ? "Reeks" : "Streak",
            "Profile_DailyGoal" => lang == "nl" ? "Dagdoel" : "Daily goal",
            "Profile_Today" => lang == "nl" ? "Vandaag" : "Today",
            "Explore_Title" => lang == "nl" ? "Verkennen" : "Explore",
            "Review_Title2" => lang == "nl" ? "Herhalen" : "Review",
            _ => key
        };
    }
}
