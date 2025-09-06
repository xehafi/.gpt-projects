namespace Tigrinya.App.Mobile.Services;

public class SrsService
{
    private readonly DbService _db;
    private readonly Dictionary<string, SrsItem> _items = new();

    public SrsService(DbService db)
    {
        _db = db;
    }

    public async Task InitAsync()
    {
        await _db.InitAsync();
        var rows = await _db.GetSrsAsync();
        foreach (var r in rows)
        {
            _items[r.Id] = new SrsItem(r.Id)
            {
                Reps = r.Reps,
                IntervalDays = r.IntervalDays,
                Ef = r.Ef,
                DueUtc = r.DueUtc,
                LastGrade = r.LastGrade
            };
        }
        // Seed a couple of glyphs if DB empty
        if (_items.Count == 0)
        {
            await EnsureItemAsync("glyph:ሀ");
            await EnsureItemAsync("glyph:ሁ");
        }
    }

    private async Task EnsureItemAsync(string id)
    {
        if (_items.ContainsKey(id)) return;
        var item = new SrsItem(id);
        _items[id] = item;
        await PersistAsync(item);
    }

    public int DueCount()
    {
        var now = DateTime.UtcNow;
        return _items.Values.Count(i => i.DueUtc <= now);
    }

    public SrsItem? GetById(string id) => _items.TryGetValue(id, out var v) ? v : null;

    public async Task GradeAsync(SrsItem item, int grade)
    {
        if (grade < 3)
        {
            item.Reps = 0;
            item.IntervalDays = 1;
        }
        else
        {
            item.Reps += 1;
            if (item.Reps == 1) item.IntervalDays = 1;
            else if (item.Reps == 2) item.IntervalDays = 6;
            else item.IntervalDays = (int)Math.Round(item.IntervalDays * item.Ef);
            item.Ef = Math.Max(1.3, item.Ef + 0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02));
        }
        item.DueUtc = DateTime.UtcNow.AddDays(item.IntervalDays);
        item.LastGrade = grade;
        await PersistAsync(item);
    }

    private Task PersistAsync(SrsItem item)
    {
        return _db.UpsertSrsAsync(new SrsRecord
        {
            Id = item.Id,
            Reps = item.Reps,
            IntervalDays = item.IntervalDays,
            Ef = item.Ef,
            DueUtc = item.DueUtc,
            LastGrade = item.LastGrade
        });
    }
}

public class SrsItem
{
    public string Id { get; }
    public int Reps { get; set; }
    public int IntervalDays { get; set; } = 0;
    public double Ef { get; set; } = 2.5;
    public DateTime DueUtc { get; set; } = DateTime.UtcNow;
    public int LastGrade { get; set; } = 0;

    public SrsItem(string id) => Id = id;
}
