using Trading.Framework.Core.Abstractions;
namespace Trading.Framework.Application.Orchestrator;
public sealed class TickerOrchestrator
{
    private readonly IMarketDataSource _source;
    private readonly ITradingStrategy _strategy;
    private readonly ITradeRepository _repo;
    public TickerOrchestrator(IMarketDataSource source, ITradingStrategy strategy, ITradeRepository repo)
    { _source = source; _strategy = strategy; _repo = repo; }
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