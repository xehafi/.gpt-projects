using TradingBot.Core.Models;

public sealed class CandleBuilder
{
    private Candle? _cur;
    private static long FloorToMinuteMs(long ms) => ms - (ms % 60000);

    public (Candle? closed, Candle openOrUpdate) OnTrade(long ms, decimal price, decimal size)
    {
        var bucket = FloorToMinuteMs(ms);
        if (_cur is null || _cur.Value.Time.ToUnixTimeMilliseconds() != bucket)
        {
            var newC = new Candle(DateTimeOffset.FromUnixTimeMilliseconds(bucket), price, price, price, price, size);
            var closed = _cur;
            _cur = newC;
            return (closed, newC);
        }
        else
        {
            var c = _cur.Value;
            var high = price > c.High ? price : c.High;
            var low = price < c.Low ? price : c.Low;
            var vol = c.Volume + size;
            _cur = new Candle(c.Time, c.Open, high, low, price, vol);
            return (null, _cur.Value);
        }
    }
}
