namespace Tigrinya.App.Mobile.Pages;

public partial class PlacementPage : ContentPage
{
    private readonly Services.AudioService _audio;
    private readonly Services.DbService _db;
    private readonly Services.LocalizationService _loc;
    private int _score = 0;
    private bool _q1Done = false, _q2Done = false, _q3Done = false;

    public PlacementPage(Services.AudioService audio, Services.DbService db, Services.LocalizationService loc)
    {
        InitializeComponent();
        _audio = audio;
        _db = db;
        _loc = loc;
    }

    private void OnQ1(object sender, EventArgs e)
    {
        if (_q1Done) return;
        if (sender is Button b && b.Text == "ሀ") { _score += 1; }
        _q1Done = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _loc.InitAsync();
        TitleLabel.Text = _loc.L("Placement_Title");
        DescLabel.Text = _loc.L("Placement_Desc");
        Q1Label.Text = _loc.L("Placement_Q1");
        Q2Label.Text = _loc.L("Placement_Q2");
        Q3Label.Text = _loc.L("Placement_Q3");
        PlayBtn.Text = $"▶ {_loc.L("Placement_Play")}";
        FinishBtn.Text = _loc.L("Placement_Finish");
    }

    private async void OnPlayQ2(object sender, EventArgs e)
    {
        await _audio.PlayAsync(null, ttsFallback: "7");
    }

    private void OnQ2(object sender, EventArgs e)
    {
        if (_q2Done) return;
        if (sender is Button b && b.Text == "7") { _score += 1; }
        _q2Done = true;
    }

    private void OnQ3(object sender, EventArgs e)
    {
        if (_q3Done) return;
        if (sender is Button b && b.Text == "ሰላም!") { _score += 1; }
        _q3Done = true;
    }

    private async void OnFinish(object sender, EventArgs e)
    {
        // Simple rubric: 0-1 -> A0, 2 -> A1, 3 -> A2
        string level = _score <= 1 ? "A0" : (_score == 2 ? "A1" : "A2");
        // Suggestions
        string s1 = "alphabet-1";
        string s2 = "numbers-0-20";
        if (level == "A2") { s1 = "greetings-1"; s2 = "animals-1"; }

        var p = await _db.GetOrCreateProfileAsync();
        p.HasCompletedPlacement = true;
        p.PlacementLevel = level;
        p.Suggested1 = s1;
        p.Suggested2 = s2;
        await _db.UpsertProfileAsync(p);

        StatusLabel.Text = $"Placement: {level}. Suggested: {s1}, {s2}.";
        await DisplayAlert("Placement", $"Level: {level}\nSuggested: {s1}, {s2}", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
