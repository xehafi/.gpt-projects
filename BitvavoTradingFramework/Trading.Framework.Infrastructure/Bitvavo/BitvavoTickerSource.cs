using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;
using Microsoft.Extensions.Logging;
namespace Trading.Framework.Infrastructure.Bitvavo;
public sealed class BitvavoTickerSource : IMarketDataSource
{
    private readonly ILogger<BitvavoTickerSource> _log;
    public BitvavoTickerSource(ILogger<BitvavoTickerSource> log) => _log = log;
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
}