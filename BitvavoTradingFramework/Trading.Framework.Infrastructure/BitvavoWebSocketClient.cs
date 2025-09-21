using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;

namespace Trading.Framework.Infrastructure.MarketData;

public sealed class BitvavoWebSocketClient : IMarketDataSource
{
    private readonly Uri _uri;
    private readonly ILogger<BitvavoWebSocketClient> _log;
    private readonly ClientWebSocket _ws = new();
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private readonly Channel<Ticker> _channel = Channel.CreateUnbounded<Ticker>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public BitvavoWebSocketClient(IConfiguration cfg, ILogger<BitvavoWebSocketClient> log)
    {
        _uri = new Uri(cfg["Bitvavo:WebSocketUrl"] ?? "wss://ws.bitvavo.com/v2/");
        _log = log;
    }

    public async Task ConnectAsync(CancellationToken ct)
    {
        _log.LogInformation("Connecting to {Uri}", _uri);
        await _ws.ConnectAsync(_uri, ct);
        _log.LogInformation("Connected.");
        _ = Task.Run(() => ReceiveLoopAsync(ct), ct);
    }

    public async Task SubscribeTickersAsync(IEnumerable<string> markets, CancellationToken ct)
    {
        // One subscribe message listing markets (Bitvavo accepts batch)
        var req = new
        {
            action = "subscribe",
            channels = new[]
            {
                new { name = "ticker", markets = markets.ToArray() }
            }
        };
        var payload = JsonSerializer.Serialize(req, _json);
        await _ws.SendAsync(Encoding.UTF8.GetBytes(payload), WebSocketMessageType.Text, true, ct);
        _log.LogInformation("Subscribed to ticker: {Markets}", string.Join(",", markets));
    }

    public async IAsyncEnumerable<Ticker> ReadTickersAsync([EnumeratorCancellation] CancellationToken ct)
    {
        while (await _channel.Reader.WaitToReadAsync(ct))
        {
            while (_channel.Reader.TryRead(out var t))
                yield return t;
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[64 * 1024];
        var sb = new StringBuilder();

        while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
        {
            sb.Clear();
            WebSocketReceiveResult res;
            do
            {
                res = await _ws.ReceiveAsync(buffer, ct);
                if (res.MessageType == WebSocketMessageType.Close)
                {
                    _log.LogWarning("WS closed by server: {CloseStatus} {Desc}", res.CloseStatus, res.CloseStatusDescription);
                    return;
                }
                sb.Append(Encoding.UTF8.GetString(buffer.AsSpan(0, res.Count)));
            } while (!res.EndOfMessage);

            var json = sb.ToString();
            try
            {
                // Typical ticker message example shape:
                // { "event":"ticker", "market":"BTC-EUR", "price":"...", "bestBid":"...", "bestAsk":"...", "timestamp": 1710000000000, "sequence": 123456 }
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("event", out var evt) && evt.GetString() == "ticker")
                {
                    var market = root.GetProperty("market").GetString()!;
                    decimal price = root.TryGetProperty("price", out var priceEl) && priceEl.GetString() is string ps ? decimal.Parse(ps, System.Globalization.CultureInfo.InvariantCulture) : 0m;
                    decimal bestBid = root.TryGetProperty("bestBid", out var bbEl) && bbEl.GetString() is string bbs ? decimal.Parse(bbs, System.Globalization.CultureInfo.InvariantCulture) : price;
                    decimal bestAsk = root.TryGetProperty("bestAsk", out var baEl) && baEl.GetString() is string bas ? decimal.Parse(bas, System.Globalization.CultureInfo.InvariantCulture) : price;
                    long sequence = root.TryGetProperty("sequence", out var seqEl) ? seqEl.GetInt64() : 0L;

                    // Timestamp: Bitvavo uses ms since epoch in several feeds; fall back to now
                    DateTimeOffset ts =
                        root.TryGetProperty("timestamp", out var tsEl) && tsEl.ValueKind is JsonValueKind.Number
                            ? DateTimeOffset.FromUnixTimeMilliseconds(tsEl.GetInt64())
                            : DateTimeOffset.UtcNow;

                    var ticker = new Ticker(market, price, bestBid, bestAsk, sequence, ts.UtcDateTime);
                    await _channel.Writer.WriteAsync(ticker, ct);
                }
                // ignore non-ticker events (subscribed, pong, etc.)
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to parse WS message: {Json}", json);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_ws.State == WebSocketState.Open)
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "dispose", CancellationToken.None);
        }
        catch { /* ignore */ }
        _ws.Dispose();
        _channel.Writer.TryComplete();
    }

    public IAsyncEnumerable<Ticker> StreamTickersAsync(string market, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
