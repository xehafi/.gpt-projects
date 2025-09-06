namespace Trading.Framework.Core.Abstractions;
public interface IMarketDataSource
{
    IAsyncEnumerable<Ticker> StreamTickersAsync(string market, CancellationToken ct = default);
}