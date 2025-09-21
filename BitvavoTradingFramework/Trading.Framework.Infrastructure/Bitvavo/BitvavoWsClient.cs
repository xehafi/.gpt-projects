using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Trading.Framework.Infrastructure.Bitvavo;

public interface IBitvavoWsClient : IAsyncDisposable
{
    Task StartAsync(Func<JsonElement, Task> onMessage, CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}

public sealed class BitvavoWsClient : IBitvavoWsClient
{
    private readonly ILogger<BitvavoWsClient> _log;
    private readonly BitvavoOptions _opt;
    private ClientWebSocket? _ws;
    private Task? _receiveLoop;
    private Task? _pingLoop;
    private volatile bool _running;
    private readonly object _sync = new();

    public BitvavoWsClient(ILogger<BitvavoWsClient> log, IOptions<BitvavoOptions> opt)
    {
        _log = log;
        _opt = opt.Value;
    }

    public async Task StartAsync(Func<JsonElement, Task> onMessage, CancellationToken ct)
    {
        if (_running) return;
        _running = true;

        _receiveLoop = Task.Run(() => RunWithReconnectAsync(onMessage, ct), ct);
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _running = false;
        try
        {
            if (_ws is { State: WebSocketState.Open })
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "stop", ct);
        }
        catch { /* ignore */ }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);
        _ws?.Dispose();
    }

    private async Task RunWithReconnectAsync(Func<JsonElement, Task> onMessage, CancellationToken outerCt)
    {
        var delay = _opt.Reconnect.InitialDelayMs;

        while (_running && !outerCt.IsCancellationRequested)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(outerCt);
            try
            {
                await ConnectAsync(cts.Token);
                await SubscribeAsync(cts.Token);
                _pingLoop = Task.Run(() => PingLoopAsync(cts.Token), cts.Token);

                await ReceiveLoopAsync(onMessage, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "WS error; will reconnect");
            }
            finally
            {
                cts.Cancel();
                try { if (_pingLoop is { }) await _pingLoop; } catch { }
                await CloseSocketQuietlyAsync();
            }

            if (!_running) break;

            // backoff + jitter
            var jitter = Random.Shared.Next(0, _opt.Reconnect.JitterMs + 1);
            var wait = Math.Min(delay + jitter, _opt.Reconnect.MaxDelayMs);
            _log.LogInformation("Reconnecting in {ms} ms ...", wait);
            await Task.Delay(wait, outerCt);
            delay = Math.Min(delay * 2, _opt.Reconnect.MaxDelayMs);
        }
    }

    private async Task ConnectAsync(CancellationToken ct)
    {
        var ws = new ClientWebSocket();
        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(_opt.PingIntervalSeconds);
        await ws.ConnectAsync(new Uri(_opt.WsUrl), ct);
        lock (_sync) _ws = ws;
        _log.LogInformation("Connected to {url}", _opt.WsUrl);
    }

    private async Task SubscribeAsync(CancellationToken ct)
    {
        // Bitvavo public: subscribe to ticker channel for configured markets.
        var payload = new
        {
            action = "subscribe",
            channels = new[]
            {
                new { name = "ticker", markets = _opt.Markets }
            }
        };

        await SendAsync(payload, ct);
        _log.LogInformation("Subscribed to ticker for {markets}", string.Join(",", _opt.Markets));
    }

    private async Task PingLoopAsync(CancellationToken ct)
    {
        var interval = TimeSpan.FromSeconds(_opt.PingIntervalSeconds);
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(interval, ct);
            try
            {
                await SendAsync(new { action = "ping" }, ct);
            }
            catch (Exception ex)
            {
                _log.LogDebug(ex, "Ping failed");
                break; // let outer reconnect
            }
        }
    }

    private async Task ReceiveLoopAsync(Func<JsonElement, Task> onMessage, CancellationToken ct)
    {
        var buffer = new byte[64 * 1024];
        var ms = new MemoryStream();

        while (!ct.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            lock (_sync)
            {
                if (_ws is null || _ws.State != WebSocketState.Open) break;
                result = _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct).GetAwaiter().GetResult();
            }

            if (result.MessageType == WebSocketMessageType.Close) break;

            ms.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
            {
                var json = Encoding.UTF8.GetString(ms.ToArray());
                ms.SetLength(0);

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // ignore pongs/acks quietly; deliver others
                    var action = root.TryGetProperty("action", out var a) ? a.GetString() : null;
                    if (string.Equals(action, "pong", StringComparison.OrdinalIgnoreCase)) continue;

                    await onMessage(root);
                }
                catch (Exception ex)
                {
                    _log.LogDebug(ex, "Failed to parse WS message: {json}", json);
                }
            }
        }
    }

    private async Task SendAsync<T>(T payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        lock (_sync)
        {
            if (_ws is null || _ws.State != WebSocketState.Open)
                throw new InvalidOperationException("Socket not open");
        }

        await _ws!.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    private async Task CloseSocketQuietlyAsync()
    {
        try
        {
            lock (_sync)
            {
                if (_ws is { State: WebSocketState.Open })
                    _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "reconnect", CancellationToken.None).GetAwaiter().GetResult();
            }
        }
        catch { /* ignore */ }
        finally
        {
            lock (_sync)
            {
                _ws?.Dispose();
                _ws = null;
            }
        }
    }
}
