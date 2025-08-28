using TradingBot.Core.Models;

namespace TradingBot.Core.Trading;

public sealed class Position
{
    public string Symbol { get; }
    public decimal Quantity { get; private set; }
    public decimal EntryPrice { get; private set; }
    public decimal StopPrice { get; private set; }

    public Position(string symbol, decimal qty, decimal entryPrice, decimal stopPrice)
    {
        Symbol = symbol;
        Quantity = qty;
        EntryPrice = entryPrice;
        StopPrice = stopPrice;
    }

    public decimal UnrealizedPnl(decimal lastPrice) => (lastPrice - EntryPrice) * Quantity;
    public void UpdateStop(decimal newStop) => StopPrice = newStop;
}

public sealed class Portfolio
{
    public decimal Cash { get; private set; }
    public Dictionary<string, Position> Positions { get; } = new(StringComparer.OrdinalIgnoreCase);
    public decimal FeeRate { get; set; } = 0.001m; // 0.10%
    public decimal Equity(decimal markPriceForAllPositions) => Cash + Positions.Values.Sum(p => p.UnrealizedPnl(markPriceForAllPositions)); // simplistic

    public Portfolio(decimal startingCash) => Cash = startingCash;

    public bool HasPosition(string symbol) => Positions.ContainsKey(symbol);

    public void OpenLong(string symbol, decimal qty, decimal price, decimal stopPrice)
    {
        var cost = qty * price;
        var fee = cost * FeeRate;
        Cash -= (cost + fee);
        Positions[symbol] = new Position(symbol, qty, price, stopPrice);
    }

    public decimal CloseLong(string symbol, decimal price)
    {
        if (!Positions.TryGetValue(symbol, out var pos)) return 0m;
        var proceeds = pos.Quantity * price;
        var fee = proceeds * FeeRate;
        Cash += (proceeds - fee);
        var realized = (price - pos.EntryPrice) * pos.Quantity - fee; // fee subtracted once on exit here (entry fee already paid)
        Positions.Remove(symbol);
        return realized;
    }
}
