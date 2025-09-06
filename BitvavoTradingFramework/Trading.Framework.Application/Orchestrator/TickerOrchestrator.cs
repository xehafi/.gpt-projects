using Trading.Framework.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Trading.Framework.Application.Orchestrator;
public sealed class TickerOrchestrator
{
    private readonly IMarketDataSource _source;
    private readonly ITradingStrategy _strategy;
    private readonly ITradeRepository _repo;
    private readonly ILogger<TickerOrchestrator> _log;

    public TickerOrchestrator(IMarketDataSource source, ITradingStrategy strategy, ITradeRepository repo, ILogger<TickerOrchestrator> log)
    {
        _source = source;
        _strategy = strategy;
        _repo = repo;
        _log = log;
    }

    public async Task RunAsync(string market, CancellationToken ct)
    {
        await _repo.EnsureDatabaseAsync(ct);
        await foreach (var t in _source.StreamTickersAsync(market, ct))
        {
            await _repo.SaveTickerAsync(t, ct);
            await _strategy.OnTickerAsync(t, ct);
        }
    }
}