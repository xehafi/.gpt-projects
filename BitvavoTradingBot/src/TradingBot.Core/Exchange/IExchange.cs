using TradingBot.Core.Models;

namespace TradingBot.Core.Exchange;

public interface IExchange
{
    Task<DateTimeOffset> GetServerTime(CancellationToken ct = default);
    IAsyncEnumerable<Candle> GetCandles(string symbol, TimeSpan timeframe, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<string> PlaceOrder(string symbol, string side, decimal qty, decimal? price = null, CancellationToken ct = default);
    Task<bool> CancelOrder(string clientOrderId, CancellationToken ct = default);
}
