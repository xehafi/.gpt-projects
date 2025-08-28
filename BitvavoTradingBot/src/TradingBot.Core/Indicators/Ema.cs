namespace TradingBot.Core.Indicators;

public sealed class Ema
{
    private readonly int _n;
    private readonly decimal _alpha;
    public decimal? Value { get; private set; }

    public bool Ready => Value.HasValue;

    public Ema(int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
        _n = n;
        _alpha = 2m / (n + 1);
    }

    public decimal Update(decimal price)
    {
        Value = Value is null ? price : (_alpha * price) + (1 - _alpha) * Value.Value;
        return Value.Value;
    }
}
