namespace Trading.Framework.Core.Abstractions;
using Trading.Framework.Core;
public interface ITradeRepository
{
    Task EnsureDatabaseAsync(CancellationToken ct);
    Task SaveTickerAsync(Ticker t, CancellationToken ct);
    Task<IReadOnlyList<Ticker>> GetRecentTickersAsync(string? market, int take, CancellationToken ct);
    Task<Ticker?> GetLastTickerAsync(string market, CancellationToken ct);
    Task<IReadOnlyList<Ticker>> GetTickersSinceAsync(string market, DateTimeOffset sinceUtc, CancellationToken ct);
}