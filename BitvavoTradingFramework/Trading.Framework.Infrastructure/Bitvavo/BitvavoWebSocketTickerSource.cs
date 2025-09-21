using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices; // for [EnumeratorCancellation]
using System.Text.Json;
using System.Threading.Channels;
using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;

namespace Trading.Framework.Infrastructure.Bitvavo;

public sealed class BitvavoWebSocketTickerSource : BackgroundService, IMarketDataSource, IAsyncDisposable
{
    private readonly IBitvavoWsClient _ws;
    private readonly ILogger<BitvavoWebSocketTickerSource> _log;
    private readonly BitvavoOptions _opt;
    private readonly Channel<Ticker> _channel;

    public BitvavoWebSocketTickerSource(
        IBitvavoWsClient ws,
        IOptions<BitvavoOptions> opt,
        ILogger<BitvavoWebSocketTickerSource> log)
    {
        _ws = ws;
        _log = log;
        _opt = opt.Value;
        _channel = Channel.CreateUnbounded<Ticker>(new UnboundedChannelOptions { SingleWriter = true, SingleReader = false });
    }

    public IAsyncEnumerable<Ticker> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _ws.StartAsync(OnMessageAsync, stoppingToken);
        _log.LogInformation("Bitvavo ticker source started for {markets}", string.Join(",", _opt.Markets));
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        await _ws.StopAsync(ct);
        await base.StopAsync(ct);
    }

    private async Task OnMessageAsync(JsonElement json)
    {
        try
        {
            if (json.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in json.EnumerateArray())
                    await TryPublishAsync(item);
            }
            else
            {
                await TryPublishAsync(json);
            }
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "OnMessage parsing failed");
        }
    }

    private async Task TryPublishAsync(JsonElement element)
    {
        BitvavoTickerMsg? msg = null;
        try
        {
            msg = element.Deserialize<BitvavoTickerMsg>();
        }
        catch
        {
            // ignore non-ticker frames
        }

        if (msg is null) return;
        if (!string.Equals(msg.Event, "ticker", StringComparison.OrdinalIgnoreCase)) return;

        if (BitvavoMapping.TryToTicker(msg, out var ticker))
        {
            await _channel.Writer.WriteAsync(ticker);
        }
    }

    // -------- Implemented members --------

    // Starts the websocket if you want to use this source without relying on HostedService start.
    public Task ConnectAsync(CancellationToken ct)
        => _ws.StartAsync(OnMessageAsync, ct);

    // Updates the markets list; next reconnect will resubscribe using these.
    // If you need immediate effect, call StopAsync + ConnectAsync after this call.
    public Task SubscribeTickersAsync(IEnumerable<string> markets, CancellationToken ct)
    {
        if (markets is null) throw new ArgumentNullException(nameof(markets));
        var arr = markets as string[] ?? markets.ToArray();
        _opt.Markets = arr;
        _log.LogInformation("Updated subscription markets to {markets}. Will take effect on next (re)connect.", string.Join(",", _opt.Markets));
        return Task.CompletedTask;
    }

    // Alias over the existing reader.
    public IAsyncEnumerable<Ticker> ReadTickersAsync(CancellationToken ct)
        => ReadAllAsync(ct);

    // Dispose underlying WS client.
    public async ValueTask DisposeAsync()
        => await _ws.DisposeAsync();

    // Convenience: stream only a single market.
    public async IAsyncEnumerable<Ticker> StreamTickersAsync(string market, [EnumeratorCancellation] CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(market)) yield break;

        await foreach (var t in _channel.Reader.ReadAllAsync(ct))
        {
            if (string.Equals(t.Market, market, StringComparison.OrdinalIgnoreCase))
                yield return t;
        }
    }
}
