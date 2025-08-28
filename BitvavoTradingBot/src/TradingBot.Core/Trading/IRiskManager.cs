using TradingBot.Core.Models;

namespace TradingBot.Core.Trading;

public interface IRiskManager
{
    /// <summary>
    /// Decide quantity and stop given signal + current ATR and portfolio.
    /// Return (qty, stopPrice); if qty <= 0, skip.
    /// </summary>
    (decimal qty, decimal stopPrice) SizeLong(string symbol, decimal price, decimal atr, Portfolio portfolio);
}

public sealed class FixedFractionalRiskManager : IRiskManager
{
    private readonly decimal _riskFraction;
    private readonly decimal _atrMult;
    private readonly decimal _minQty;

    public FixedFractionalRiskManager(decimal riskFraction = 0.005m, decimal atrMult = 2m, decimal minQty = 0.0001m)
    {
        _riskFraction = riskFraction;
        _atrMult = atrMult;
        _minQty = minQty;
    }

    public (decimal qty, decimal stopPrice) SizeLong(string symbol, decimal price, decimal atr, Portfolio portfolio)
    {
        if (atr <= 0) return (0m, 0m);
        var stopDistance = atr * _atrMult;
        var riskAmount = portfolio.Cash * _riskFraction;
        if (stopDistance <= 0 || riskAmount <= 0) return (0m, 0m);
        var qty = riskAmount / stopDistance;
        if (qty < _minQty) return (0m, 0m);
        var stopPrice = price - stopDistance;
        return (decimal.Round(qty, 6), stopPrice);
    }
}
