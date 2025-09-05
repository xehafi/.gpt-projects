using SQLite;

namespace Tigrinya.App.Mobile.Services;

public class UserProfile
{
    [PrimaryKey]
    public string Id { get; set; } = "default";

    public int TotalXp { get; set; }
    public int TodayXp { get; set; }
    public int WeeklyXp { get; set; }
    public int DailyGoal { get; set; } = 10;
    public int StreakDays { get; set; } = 0;
    public DateTime LastActiveDateUtc { get; set; } = DateTime.UtcNow.Date;
    public DateTime WeekStartUtc { get; set; } = DateTime.UtcNow.Date;
    public string BaseLang { get; internal set; }
    public object? AvatarGroup { get; internal set; }
    public object? OutfitId { get; internal set; }
    public bool HasCompletedPlacement { get; internal set; }
    public string PlacementLevel { get; internal set; }
    public string Suggested1 { get; internal set; }
    public string Suggested2 { get; internal set; }
}

public class DbService
{
    private readonly SQLiteAsyncConnection _db;

    public DbService()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "learn_tigrinya.db3");
        _db = new SQLiteAsyncConnection(path);
        _db.CreateTableAsync<UserProfile>().Wait();
        _db.CreateTableAsync<SrsRecord>().Wait();
    }

    public async Task<UserProfile> GetOrCreateProfileAsync()
    {
        var existing = await _db.Table<UserProfile>().FirstOrDefaultAsync();
        if (existing != null) return existing;

        var profile = new UserProfile();
        await _db.InsertAsync(profile);
        return profile;
    }

    public Task<int> UpsertProfileAsync(UserProfile profile)
    {
        return _db.InsertOrReplaceAsync(profile);
    }

    public Task<List<SrsRecord>> GetSrsAsync() =>
        _db.Table<SrsRecord>().ToListAsync();
    public Task<int> UpsertSrsAsync(SrsRecord rec) =>
        _db.InsertOrReplaceAsync(rec);

    internal async Task InitAsync()
    {
        throw new NotImplementedException();
    }

    internal async Task UpsertQaRecordAsync(Services.QaRecord rec)
    {
        throw new NotImplementedException();
    }
}

public class SrsRecord
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public int Reps { get; set; }
    public int IntervalDays { get; set; }
    public double Ef { get; set; } = 2.5;
    public DateTime DueUtc { get; set; } = DateTime.UtcNow;
    public int LastGrade { get; set; }
}
