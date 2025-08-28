using TradingBot.App;
using TradingBot.Backtest;
using TradingBot.Exchange;

var argsDict = ParseArgs(args);
var mode = argsDict.GetValueOrDefault("mode", "backtest");
var symbol = argsDict.GetValueOrDefault("symbol", "BTC-EUR");
var file = argsDict.GetValueOrDefault("file", "D:/.gpt-projects/BitvavoTradingBot/data/BTC-EUR-1m-sample.csv");
var starting = decimal.TryParse(argsDict.GetValueOrDefault("cash", "10000"), out var c) ? c : 10_000m;
var interval = argsDict.GetValueOrDefault("interval", "1m");

Console.WriteLine($"Mode: {mode}");

switch (mode.ToLowerInvariant())
{
    case "backtest":
        BacktestRunner.Run(symbol, file, starting);
        break;
    case "candles":
        var apiKey = Environment.GetEnvironmentVariable("BITVAVO_KEY") ?? "";
        var apiSecret = Environment.GetEnvironmentVariable("BITVAVO_SECRET") ?? "";
        var client = new BitvavoRestClient(apiKey, apiSecret);
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow;
        await foreach (var cc in client.GetCandles(symbol, TimeSpan.FromMinutes(1), from, to))
            Console.WriteLine($"{cc.Time:u} O:{cc.Open} C:{cc.Close}");
        break;
    case "paper":
        using (var cts = new CancellationTokenSource())
        {
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };
            var runner = new LivePaperRunner(symbol, interval, starting);
            var task = runner.RunAsync(cts.Token);
            // no extra await here; RunAsync keeps the task alive
            await task;
        }
        break;
    default:
        Console.WriteLine("Unknown mode. Supported: backtest");
        break;
}

static Dictionary<string,string> ParseArgs(string[] args)
{
    var dict = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    for (int i=0; i<args.Length; i++)
    {
        var a = args[i];
        if (a.StartsWith("--")) 
        {
            var key = a[2..];
            string? val = (i+1 < args.Length && !args[i+1].StartsWith("--")) ? args[++i] : "true";
            dict[key] = val;
        }
    }
    return dict;
}
