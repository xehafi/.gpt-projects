using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trading.Framework.Core.Abstractions;
using Trading.Framework.Core;

namespace Trading.Framework.Infrastructure.Bitvavo;

public sealed class TickerPersistenceWorker : BackgroundService
{
    private readonly IMarketDataSource _source;
    private readonly ITradeRepository _repo;
    private readonly ILogger<TickerPersistenceWorker> _log;

    public TickerPersistenceWorker(IMarketDataSource source, ITradeRepository repo, ILogger<TickerPersistenceWorker> log)
    {
        _source = source;
        _repo = repo;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var tick in _source.ReadTickersAsync(stoppingToken))
        {
            try
            {
                await _repo.AddTickerAsync(tick, stoppingToken); // you already have table + insert
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to persist ticker {market} @ {ts}", tick.Market, tick.Timestamp);
            }
        }
    }
}
