using TradingBot.Core.Models;
using TradingBot.Core.Strategies;
using TradingBot.Core.Trading;

namespace TradingBot.Backtest;

public sealed class Trade
{
    public DateTimeOffset EntryTime { get; init; }
    public DateTimeOffset? ExitTime { get; set; }
    public decimal EntryPrice { get; init; }
    public decimal ExitPrice { get; set; }
    public decimal Qty { get; init; }
    public decimal PnL { get; set; }
}

public sealed class Stats
{
    public int Trades { get; set; }
    public decimal RealizedPnl { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal PeakEquity { get; set; }
    public decimal TroughEquity { get; set; }
}

public sealed class Simulator
{
    private readonly Portfolio _portfolio;
    private readonly IRiskManager _risk;
    private readonly List<Trade> _trades = new();
    private decimal _equity;
    private decimal _peak;

    public Simulator(decimal startingCash, IRiskManager risk)
    {
        _portfolio = new Portfolio(startingCash);
        _risk = risk;
        _equity = startingCash;
        _peak = startingCash;
    }

    public Stats Run(string symbol, IEnumerable<Candle> candles, EmaCross strategy)
    {
        Trade? openTrade = null;
        decimal maxDD = 0m;
        foreach (var c in candles)
        {
            var (sig, atr) = strategy.OnCandle(symbol, c);
            // Stop-loss check for open position using intrabar low
            if (_portfolio.HasPosition(symbol))
            {
                var pos = _portfolio.Positions[symbol];
                if (c.Low <= pos.StopPrice)
                {
                    // exit at stop price
                    var realized = _portfolio.CloseLong(symbol, pos.StopPrice);
                    if (openTrade is not null)
                    {
                        openTrade.ExitTime = c.Time;
                        openTrade.ExitPrice = pos.StopPrice;
                        openTrade.PnL = realized;
                        _trades.Add(openTrade);
                        openTrade = null;
                    }
                }
            }

            if (sig is null) { UpdateEquity(c.Close); continue; }

            if (sig.Action == SignalAction.Buy && !_portfolio.HasPosition(symbol))
            {
                var (qty, stop) = _risk.SizeLong(symbol, c.Close, atr, _portfolio);
                if (qty > 0)
                {
                    _portfolio.OpenLong(symbol, qty, c.Close, stop);
                    openTrade = new Trade { EntryTime = c.Time, EntryPrice = c.Close, Qty = qty };
                }
            }
            else if (sig.Action == SignalAction.Sell && _portfolio.HasPosition(symbol))
            {
                var realized = _portfolio.CloseLong(symbol, c.Close);
                if (openTrade is not null)
                {
                    openTrade.ExitTime = c.Time;
                    openTrade.ExitPrice = c.Close;
                    openTrade.PnL = realized;
                    _trades.Add(openTrade);
                    openTrade = null;
                }
            }

            UpdateEquity(c.Close);
            maxDD = Math.Max(maxDD, _peak - _equity);
        }

        // Close at last price if still open (optional)
        if (_portfolio.HasPosition(symbol))
        {
            var last = candles.Last();
            var realized = _portfolio.CloseLong(symbol, last.Close);
            if (openTrade is not null)
            {
                openTrade.ExitTime = last.Time;
                openTrade.ExitPrice = last.Close;
                openTrade.PnL = realized;
                _trades.Add(openTrade);
            }
        }

        return new Stats
        {
            Trades = _trades.Count,
            RealizedPnl = _trades.Sum(t => t.PnL),
            MaxDrawdown = maxDD,
            PeakEquity = _peak,
            TroughEquity = _peak - maxDD
        };
    }

    private void UpdateEquity(decimal mark)
    {
        var posValue = _portfolio.Positions.Values.Sum(p => p.UnrealizedPnl(mark));
        _equity = _portfolio.Cash + posValue;
        _peak = Math.Max(_peak, _equity);
    }
}
