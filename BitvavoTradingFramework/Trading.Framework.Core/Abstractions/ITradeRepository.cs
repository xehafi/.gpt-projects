namespace Trading.Framework.Core.Abstractions;
public interface ITradeRepository
{
    Task EnsureDatabaseAsync(CancellationToken ct);
    Task SaveTickerAsync(Ticker t, CancellationToken ct);
}