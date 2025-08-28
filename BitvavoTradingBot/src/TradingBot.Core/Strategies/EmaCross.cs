using TradingBot.Core.Models;
using TradingBot.Core.Indicators;

namespace TradingBot.Core.Strategies;

public sealed class EmaCross
{
    private readonly int _fast;
    private readonly int _slow;
    private readonly Ema _emaFast;
    private readonly Ema _emaSlow;
    private readonly RollingAtr _atr;
    private readonly decimal _minGap; // relative gap threshold

    public EmaCross(int fast=12, int slow=26, int atr=14, decimal minGap=0.001m)
    {
        if (fast <= 0 || slow <= 0 || slow <= fast) throw new ArgumentException("EMA periods invalid (slow must be > fast).");
        _fast = fast; _slow = slow; _minGap = minGap;
        _emaFast = new Ema(fast);
        _emaSlow = new Ema(slow);
        _atr = new RollingAtr(atr);
    }

    public (Signal? signal, decimal atr) OnCandle(string symbol, Candle c)
    {
        var f = _emaFast.Update(c.Close);
        var s = _emaSlow.Update(c.Close);
        var atr = _atr.Update(c);
        if (!_emaSlow.Ready || !_atr.Ready) return (null, atr);

        var relGap = (f - s) / c.Close;
        if (f > s && relGap > _minGap)
            return (new Signal(symbol, SignalAction.Buy, c.Close, null, 0.55), atr);

        if (f < s && (-relGap) > _minGap)
            return (new Signal(symbol, SignalAction.Sell, c.Close, null, 0.55), atr);

        return (new Signal(symbol, SignalAction.Hold, c.Close, null, 0.0), atr);
    }
}
