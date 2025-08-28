using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;
namespace TradingBot.Exchange;

public sealed class BitvavoWsClient : IAsyncDisposable
{
    private readonly ClientWebSocket _ws = new();
    private readonly Uri _uri = new("wss://ws.bitvavo.com/v2/");
    private readonly CancellationTokenSource _cts = new();
    private Task? _recvLoop;

    public event Action<JsonElement>? OnMessage;
    public event Action<Exception>? OnError;
    public event Action? OnOpen;
    public event Action? OnClose;

    private readonly TimeSpan _pingEvery = TimeSpan.FromSeconds(20);
    private readonly TimeSpan _pongTimeout = TimeSpan.FromSeconds(10);
    private DateTime _lastPong = DateTime.UtcNow;

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _ws.ConnectAsync(_uri, ct);
        OnOpen?.Invoke();
        _recvLoop = Task.Run(ReceiveLoop);
    }

    private readonly TaskCompletionSource<bool> _subscribedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task Subscribed => _subscribedTcs.Task;

    public async Task SubscribeTradesAsync(string market, CancellationToken ct = default)
    {
        var payload = new
        {
            action = "subscribe",
            channels = new object[] { new { name = "trades", markets = new[] { market } } }
        };
        var json = JsonSerializer.Serialize(payload);
        await _ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, ct);
    }

    public async Task StartHeartbeatAsync(CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await _ws.SendAsync(Encoding.UTF8.GetBytes("{\"action\":\"ping\"}"),
                        WebSocketMessageType.Text, true, ct);
                }
                catch { /* will reconnect below */ }

                // reconnect if no pong within window
                if (DateTime.UtcNow - _lastPong > _pingEvery + _pongTimeout)
                {
                    OnError?.Invoke(new Exception("WS heartbeat missed; reconnecting..."));
                    await ReconnectAsync(ct);
                }
                await Task.Delay(_pingEvery, ct);
            }
        }, ct);
    }

    private async Task ReconnectAsync(CancellationToken ct)
    {
        try { _ws.Abort(); } catch { }
        _ws.Dispose();
        // fresh socket
        var newWs = new ClientWebSocket();
        typeof(BitvavoWsClient)
            .GetField("_ws", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(this, newWs);

        await ConnectAsync(ct);
        // caller is responsible to re-subscribe after reconnect
    }
    public async Task SubscribeCandlesAsync(string market, string interval, CancellationToken ct = default)
    {
        // Try 3 payload variants until one succeeds.
        var attempts = new[]
        {
        // A. Docs variant: interval is array
        new { action = "subscribe", channels = new object[]{ new { name = "candles", markets = new[] { market }, interval = new[] { interval } } } },
        // B. SDK variant: interval is string
        new { action = "subscribe", channels = new object[]{ new { name = "candles", markets = new[] { market }, interval = interval } } },
        // C. Legacy variant: key 'intervals' (plural)
        new { action = "subscribe", channels = new object[]{ new { name = "candles", markets = new[] { market }, intervals = new[] { interval } } } }
    };

        foreach (var payload in attempts)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            Console.WriteLine("[WS] -> " + json);
            await _ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, ct);

            // Wait briefly for either a 'subscribed' ack or an immediate error
            var ok = await WaitForSubscribeAckOrError(TimeSpan.FromSeconds(2), ct);
            if (ok) return;
        }

        Console.WriteLine("[WS] Failed to subscribe to candles after all payload variants.");
    }

    // Helper: wait briefly for either 'subscribed' or an errorCode 203/205
    private async Task<bool> WaitForSubscribeAckOrError(TimeSpan timeout, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(System.Text.Json.JsonElement msg)
        {
            if (msg.TryGetProperty("event", out var ev) && ev.GetString() == "subscribed")
            {
                tcs.TrySetResult(true);
                return;
            }
            if (msg.TryGetProperty("errorCode", out var code))
            {
                var codeVal = code.GetInt32();
                if (codeVal == 203 || codeVal == 205) // missing/invalid parameter
                    tcs.TrySetResult(false);
            }
        }

        OnMessage += Handler;
        try
        {
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeout, ct));
            return completed == tcs.Task && tcs.Task.Result;
        }
        finally
        {
            OnMessage -= Handler;
        }
    }




    private async Task ReceiveLoop()
    { 
        var buf = new byte[64 * 1024];
        var sb = new StringBuilder();

        try
        {
            while (!_cts.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                sb.Clear();
                WebSocketReceiveResult? res;
                do
                {
                    res = await _ws.ReceiveAsync(buf, _cts.Token);
                    if (res.MessageType == WebSocketMessageType.Close)
                    {
                        OnClose?.Invoke();
                        return;
                    }
                    sb.Append(Encoding.UTF8.GetString(buf, 0, res.Count));
                } while (!res.EndOfMessage);

                var text = sb.ToString();
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                // after parsing `root`
                if (root.TryGetProperty("event", out var evEl) && evEl.GetString() == "pong")
                {
                    _lastPong = DateTime.UtcNow;
                    return;
                }

                //                if (root.TryGetProperty("event", out var evEl))
                //                {
                //                    var ev = evEl.GetString();
                //                    if (ev == "subscribed")
                //                    {
                //                        OnMessage?.Invoke(root);
                //                        if (!_subscribedTcs.Task.IsCompleted) _subscribedTcs.TrySetResult(true);
                //                    }
                //                                        else
                //{
                //                        OnMessage?.Invoke(root); // forward everything, including 'candle', 'error', etc.
                //                    }
                //                }
                //                else
                //                {
                //                    OnMessage?.Invoke(root); // unexpected payloads
                //                }
                OnMessage?.Invoke(doc.RootElement);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        if (_ws.State == WebSocketState.Open)
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        _ws.Dispose();
        _cts.Dispose();
    }
}
