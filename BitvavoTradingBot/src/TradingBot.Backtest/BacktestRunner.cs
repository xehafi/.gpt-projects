using TradingBot.Core.Strategies;
using TradingBot.Core.Trading;

namespace TradingBot.Backtest;

public static class BacktestRunner
{
    public static void Run(string symbol, string csvPath, decimal startingCash = 10_000m)
    {
        Console.WriteLine($"Backtest: symbol={symbol}, file={csvPath}");
        var candles = CsvLoader.Load(csvPath).ToList();
        var strategy = new EmaCross(fast:12, slow:26, atr:14, minGap:0.001m);
        var risk = new FixedFractionalRiskManager(riskFraction:0.005m, atrMult:2m);
        var sim = new Simulator(startingCash, risk);
        var stats = sim.Run(symbol, candles, strategy);

        Console.WriteLine("\n===== RESULTS =====");
        Console.WriteLine($"Trades        : {stats.Trades}");
        Console.WriteLine($"Realized PnL  : {stats.RealizedPnl:F2} (EUR)");
        Console.WriteLine($"Max Drawdown  : {stats.MaxDrawdown:F2} (EUR)");
        Console.WriteLine($"Peak Equity   : {stats.PeakEquity:F2} (EUR)");
        Console.WriteLine($"Trough Equity : {stats.TroughEquity:F2} (EUR)");
    }
}
