using System.Globalization;
using System.Text.Json;
using TradingBot.Core.Models;
using TradingBot.Core.Strategies;
using TradingBot.Core.Trading;
using TradingBot.Exchange;

namespace TradingBot.App;

public sealed class LivePaperRunner
{
    private readonly string _symbol;
    private readonly string _interval; // "1m"
    private readonly EmaCross _strategy;
    private readonly Portfolio _portfolio;
    private readonly IRiskManager _risk;

    private Candle? _current;

    public LivePaperRunner(string symbol, string interval = "1m", decimal startingCash = 10_000m)
    {
        _symbol = symbol;
        _interval = interval;
        _strategy = new EmaCross();
        _portfolio = new Portfolio(startingCash);
        _risk = new FixedFractionalRiskManager(0.005m, 2m);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await using var ws = new BitvavoWsClient();
        
        ws.OnOpen += () => Console.WriteLine("[WS] Connected");
        ws.OnClose += () => Console.WriteLine("[WS] Closed");
        ws.OnError += ex => Console.WriteLine("[WS] Error: " + ex.Message);
        ws.OnMessage += OnMessage;

        await ws.ConnectAsync(ct);
        await ws.StartHeartbeatAsync(ct);
        await ws.SubscribeCandlesAsync(_symbol, _interval, ct); // try native candles
        await ws.SubscribeTradesAsync(_symbol, ct);             // fallback/augment

        // Wait up to 5s for the "subscribed" ack so the user sees progress:
        _ = await Task.WhenAny(ws.Subscribed, Task.Delay(TimeSpan.FromSeconds(5), ct));

        Console.WriteLine($"[Paper] Subscribed candles for {_symbol} {_interval}. Waiting for updates…");

        // keep process alive
        try { await Task.Delay(Timeout.Infinite, ct); }
        catch (TaskCanceledException) { /* ignore */ }
    }

    private static decimal D(JsonElement e) =>
        e.ValueKind == JsonValueKind.Number
            ? e.GetDecimal()
            : decimal.Parse(e.GetString() ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);

    private CandleBuilder _builder = new();

    private void OnMessage(JsonElement msg)
    {
        if (msg.TryGetProperty("event", out var evEl))
        {
            var ev = evEl.GetString();
            if (ev == "trades")
            {
                foreach (var tr in msg.GetProperty("trades").EnumerateArray())
                {
                    long ms = long.Parse(tr.GetProperty("timestamp").GetString()!);
                    var price = decimal.Parse(tr.GetProperty("price").GetString()!, NumberStyles.Any, CultureInfo.InvariantCulture);
                    var size = decimal.Parse(tr.GetProperty("amount").GetString()!, NumberStyles.Any, CultureInfo.InvariantCulture);
                    var (closed, openOrUpdate) = _builder.OnTrade(ms, price, size);
                    if (closed is Candle done) Step(done);
                    _current = openOrUpdate; // live forming candle
                }
                return;
            }
            if (ev == "candle") { /* your existing candle handling */ return; }
        }
        // log other messages...
    }

    //private void OnMessage(JsonElement msg)
    //{
    //    if (!msg.TryGetProperty("event", out var evEl))
    //    {
    //        Console.WriteLine("[WS] (no event): " + msg.ToString());
    //        return;
    //    }

    //    var ev = evEl.GetString();
    //    if (ev == "subscribed")
    //    {
    //        Console.WriteLine("[WS] Subscribed: " + msg.ToString());
    //        return;
    //    }
    //    if (ev == "error")
    //    {
    //        Console.WriteLine("[WS] ERROR: " + msg.ToString());
    //        return;
    //    }
    //    if (ev != "candle")
    //    {
    //        Console.WriteLine("[WS] " + ev + ": " + msg.ToString());
    //        return;
    //    }

    //    // candle event shape:
    //    // { "event":"candle","market":"BTC-EUR","interval":"1m","candle":[ [ timeMs,"o","h","l","c","v" ] ] }
    //    var arr = msg.GetProperty("candle")[0];
    //    long ms = arr[0].ValueKind == JsonValueKind.String ? long.Parse(arr[0].GetString()!) : arr[0].GetInt64();
    //    decimal D(JsonElement e) =>
    //        e.ValueKind == JsonValueKind.Number ? e.GetDecimal() :
    //        decimal.Parse(e.GetString() ?? "0", System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

    //    var t = DateTimeOffset.FromUnixTimeMilliseconds(ms);
    //    var cndl = new Candle(t, D(arr[1]), D(arr[2]), D(arr[3]), D(arr[4]), D(arr[5]));

    //    if (_current is Candle prev && prev.Time != cndl.Time)
    //        Step(prev); // close previous candle

    //    _current = cndl;
    //}

    private void Step(Candle c)
    {
        // on each Step()
        File.AppendAllText("paper_trades.csv", $"{c.Time:u},{c.Open},{c.High},{c.Low},{c.Close},{_portfolio.Cash}\n");

        if (!RiskOk(c.Close)) return;

        // Stop check
        if (_portfolio.HasPosition(_symbol))
        {
            var pos = _portfolio.Positions[_symbol];
            if (c.Low <= pos.StopPrice)
            {
                var realized = _portfolio.CloseLong(_symbol, pos.StopPrice);
                Console.WriteLine($"{c.Time:u} STOP EXIT {realized:+0.00;-0.00} at {pos.StopPrice}");
            }
        }

        // Strategy
        var (sig, atr) = _strategy.OnCandle(_symbol, c);
        if (sig is null) return;

        if (sig.Action == SignalAction.Buy && !_portfolio.HasPosition(_symbol))
        {
            var (qty, stop) = _risk.SizeLong(_symbol, c.Close, atr, _portfolio);
            if (qty > 0)
            {
                _portfolio.OpenLong(_symbol, qty, c.Close, stop);
                Console.WriteLine($"{c.Time:u} BUY {qty} at {c.Close}  stop {stop}");
            }
        }
        else if (sig.Action == SignalAction.Sell && _portfolio.HasPosition(_symbol))
        {
            var realized = _portfolio.CloseLong(_symbol, c.Close);
            Console.WriteLine($"{c.Time:u} SELL EXIT {realized:+0.00;-0.00} at {c.Close}");
        }
    }

    private decimal _dayStartEquity = 10000m;
    private decimal _maxDailyLoss = 0.03m; // 3%
    private int _maxOpenPositions = 1;

    private bool RiskOk(decimal mark)
    {
        // very rough equity calc: cash + unrealized
        var eq = _portfolio.Cash + _portfolio.Positions.Values.Sum(p => (mark - p.EntryPrice) * p.Quantity);
        if (eq <= _dayStartEquity * (1 - _maxDailyLoss)) return false;
        if (_portfolio.Positions.Count >= _maxOpenPositions) return false;
        return true;
    }

}
