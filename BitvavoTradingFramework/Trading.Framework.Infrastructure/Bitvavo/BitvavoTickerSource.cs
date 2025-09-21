using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;
using Microsoft.Extensions.Logging;
namespace Trading.Framework.Infrastructure.Bitvavo;
public sealed class BitvavoTickerSource : IMarketDataSource
{
    private readonly ILogger<BitvavoTickerSource> _log;
    public BitvavoTickerSource(ILogger<BitvavoTickerSource> log) => _log = log;

    public Task ConnectAsync(CancellationToken ct)
    {
        // No-op for mock source
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // Nothing to dispose in mock
        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<Ticker> ReadTickersAsync(CancellationToken ct)
    {
        // Forward to a default market stream if needed
        await foreach (var t in StreamTickersAsync("BTC-EUR", ct))
            yield return t;
    }

    public async IAsyncEnumerable<Ticker> StreamTickersAsync(string market, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var rand = new Random(); long seq = 0; decimal price = 100m;
        while (!ct.IsCancellationRequested)
        {
            var delta = (decimal)(rand.NextDouble() - 0.5) * 0.5m;
            price = Math.Max(0.0001m, price + delta);
            yield return new Ticker(market, price, price-0.01m, price+0.01m, ++seq, DateTimeOffset.UtcNow);
            try { await Task.Delay(TimeSpan.FromSeconds(1), ct); } catch { yield break; }
        }
    }

    public Task SubscribeTickersAsync(IEnumerable<string> markets, CancellationToken ct)
    {
        // Mock source ignores market list; real WS source handles it
        _log.LogInformation("[MOCK] Subscribe called for {Count} market(s)", markets?.Count() ?? 0);
        return Task.CompletedTask;
    }
}