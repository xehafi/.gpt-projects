namespace Tigrinya.App.Mobile.Services;

public class XpService
{
    private readonly DbService _db;
    public XpService(DbService db) { _db = db; }

    public async Task<(int today, int goal, int streak, int weekly)> GetStatusAsync()
    {
        var p = await _db.GetOrCreateProfileAsync();
        await NormalizeDates(p);
        return (p.TodayXp, p.DailyGoal, p.StreakDays, p.WeeklyXp);
    }

    public async Task<(int today, int goal, int streak, int weekly, bool metGoal, bool firstXpToday)> AddXpAsync(int xp)
    {
        var p = await _db.GetOrCreateProfileAsync();
        // normalize also sets TodayXp=0 if new day and adjusts streak
        var beforeToday = p.TodayXp;
        await NormalizeDates(p);
        var firstXpToday = beforeToday == 0 && p.TodayXp == 0;
        p.TotalXp += xp;
        p.TodayXp += xp;
        p.WeeklyXp += xp;
        await _db.UpsertProfileAsync(p);
        var met = p.TodayXp >= p.DailyGoal;
        return (p.TodayXp, p.DailyGoal, p.StreakDays, p.WeeklyXp, met, firstXpToday);
    }

    private async Task NormalizeDates(UserProfile p)
    {
        var today = DateTime.UtcNow.Date;
        // Streak update
        if (p.LastActiveDateUtc.Date < today)
        {
            var diff = (today - p.LastActiveDateUtc.Date).Days;
            if (diff == 1) p.StreakDays += 1;
            else if (diff > 1) p.StreakDays = 1; // new streak
            p.TodayXp = 0;
            p.LastActiveDateUtc = today;
        }
        // Week rollover (ISO-ish: start Monday)
        var dow = (int)today.DayOfWeek; // Sunday=0
        var monday = today.AddDays(dow == 0 ? -6 : 1 - dow);
        if (p.WeekStartUtc.Date != monday)
        {
            p.WeekStartUtc = monday;
            p.WeeklyXp = 0;
        }
        await _db.UpsertProfileAsync(p);
    }
}
