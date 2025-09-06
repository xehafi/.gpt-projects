namespace Trading.Framework.Core.Abstractions;
public interface ITradingStrategy
{
    Task OnTickerAsync(Ticker t, CancellationToken ct);
}