namespace Tigrinya.App.Mobile.Pages;

public partial class QaReviewPage : ContentPage
{
    private readonly Services.QaService _qa;
    private readonly Services.DbService _db;
    private readonly Services.LocalizationService _loc;
    private string _contentId = "animals-1";
    private List<Services.QaItem> _items = new();
    private Dictionary<string, string?> _reviewers = new();

    public QaReviewPage(Services.QaService qa, Services.DbService db, Services.LocalizationService loc)
    {
        InitializeComponent();
        _qa = qa;
        _db = db;
        _loc = loc;
        LoadAsync(_contentId);
    }

    private async void LoadAsync(string contentId)
    {
        await _loc.InitAsync();
        _contentId = contentId;
        _items = await _qa.LoadChecklistAsync(contentId);
        ChecklistView.ItemsSource = _items;
        TitleLabel.Text = _loc.L("QA_Title");
        ContentPicker.Title = _loc.L("QA_Content");
        StatusLabel.Text = $"Loaded {_items.Count} checks for {contentId}";
    }

    private void OnContentChanged(object sender, EventArgs e)
    {
        if (sender is Picker p && p.SelectedItem is string id)
        {
            LoadAsync(id);
        }
    }

    private async void OnToggle(object sender, ToggledEventArgs e)
    {
        if (sender is Switch sw && sw.BindingContext is Services.QaItem it)
        {
            var rec = new Services.QaRecord
            {
                Key = $"{_contentId}:{it.Id}",
                ContentId = _contentId,
                ItemId = it.Id,
                Approved = e.Value,
                Reviewer = _reviewers.ContainsKey(it.Id) ? _reviewers[it.Id] : null,
                TimestampUtc = DateTime.UtcNow
            };
            await _db.UpsertQaRecordAsync(rec);
        }
    }

    private void OnReviewerChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is Services.QaItem it)
        {
            _reviewers[it.Id] = entry.Text;
        }
    }
}
