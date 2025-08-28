using TradingBot.Core.Models;

namespace TradingBot.Core.Indicators;

public sealed class RollingAtr
{
    private readonly int _n;
    private readonly Queue<decimal> _trs = new();
    private decimal? _prevClose;

    public RollingAtr(int n = 14)
    {
        if (n < 2) throw new ArgumentOutOfRangeException(nameof(n));
        _n = n;
    }

    public bool Ready => _trs.Count >= _n;
    public decimal Value => _trs.Count == 0 ? 0m : _trs.Average();

    public decimal Update(Candle c)
    {
        if (_prevClose is null)
        {
            _prevClose = c.Close;
            return 0m;
        }
        var tr = Math.Max(c.High - c.Low, Math.Max(Math.Abs(c.High - _prevClose.Value), Math.Abs(c.Low - _prevClose.Value)));
        _trs.Enqueue(tr);
        while (_trs.Count > _n) _trs.Dequeue();
        _prevClose = c.Close;
        return Value;
    }
}
