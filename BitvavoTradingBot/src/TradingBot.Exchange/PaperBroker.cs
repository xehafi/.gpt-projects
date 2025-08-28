using TradingBot.Core.Exchange;
using TradingBot.Core.Models;
namespace TradingBot.Exchange;

/// <summary>
/// Minimal stub that satisfies IExchange for later paper/live wiring.
/// Not used by the backtester (which runs offline on CSV).
/// </summary>
public sealed class PaperBroker : IExchange
{
    public Task<bool> CancelOrder(string clientOrderId, CancellationToken ct = default)
        => Task.FromResult(true);

    public async IAsyncEnumerable<Candle> GetCandles(
    string symbol,
    TimeSpan timeframe,
    DateTimeOffset from,
    DateTimeOffset to,
    [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        // No candles in the paper stub yet.
        yield break;
    }


    public Task<string> PlaceOrder(string symbol, string side, decimal qty, decimal? price = null, CancellationToken ct = default)
        => Task.FromResult(Guid.NewGuid().ToString("N"));

    public Task<DateTimeOffset> GetServerTime(CancellationToken ct = default)
        => Task.FromResult(DateTimeOffset.UtcNow);
}
