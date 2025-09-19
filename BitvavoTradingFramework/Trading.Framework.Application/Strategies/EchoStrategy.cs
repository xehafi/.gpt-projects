using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Trading.Framework.Application.Strategies;
public sealed class EchoStrategy : ITradingStrategy
{
    private readonly ILogger<EchoStrategy> _log;
    public EchoStrategy(ILogger<EchoStrategy> log) => _log = log;
    public Task OnTickerAsync(Ticker t, CancellationToken ct)
    {
        _log.LogInformation("[{Market}] Price={Price} Bid={Bid} Ask={Ask} Seq={Seq}", t.Market, t.Price, t.BestBid, t.BestAsk, t.Sequence);
        return Task.CompletedTask;
    }
}