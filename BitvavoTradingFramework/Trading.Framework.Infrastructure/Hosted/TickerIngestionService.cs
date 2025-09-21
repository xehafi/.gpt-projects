using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;
using Trading.Framework.Infrastructure.Persistence;

namespace Trading.Framework.Infrastructure.Hosted;

public sealed class TickerIngestionService : BackgroundService
{
    private readonly IMarketDataSource _source;
    private readonly ITradeRepository _repo;
    private readonly IConfiguration _cfg;
    private readonly ILogger<TickerIngestionService> _log;

    public TickerIngestionService(
        IMarketDataSource source,
        ITradeRepository repo,
        IConfiguration cfg,
        ILogger<TickerIngestionService> log)
    {
        _source = source;
        _repo = repo;
        _cfg = cfg;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var markets = _cfg.GetSection("Bitvavo:Markets").Get<string[]>() ?? Array.Empty<string>();
        if (markets.Length == 0)
        {
            _log.LogWarning("No markets configured; ingestion idle.");
            return;
        }

        // Exponential backoff reconnect loop
        var initial = _cfg.GetValue("Bitvavo:Reconnect:InitialMs", 500);
        var max = _cfg.GetValue("Bitvavo:Reconnect:MaxMs", 15000);
        var jitter = _cfg.GetValue("Bitvavo:Reconnect:Jitter", true);
        var rng = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _source.ConnectAsync(stoppingToken);
                await _source.SubscribeTickersAsync(markets, stoppingToken);

                await foreach (var t in _source.ReadTickersAsync(stoppingToken))
                {
                    try
                    {
                        await _repo.SaveTickerAsync(t, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Failed to persist ticker {Market} seq {Seq}", t.Market, t.Sequence);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Market data connection dropped. Will retry.");
            }

            // backoff
            initial = Math.Min(initial * 2, max);
            var wait = jitter ? initial + rng.Next(0, initial / 2) : initial;
            await Task.Delay(wait, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _source.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
