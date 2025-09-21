namespace Trading.Framework.Core.Abstractions;

public interface IMarketDataSource : IAsyncDisposable
{
    Task ConnectAsync(CancellationToken ct);
    Task SubscribeTickersAsync(IEnumerable<string> markets, CancellationToken ct);
    IAsyncEnumerable<Ticker> ReadTickersAsync(CancellationToken ct);
    IAsyncEnumerable<Ticker> StreamTickersAsync(string market, CancellationToken ct);
}
