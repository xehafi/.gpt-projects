using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Trading.Framework.Core.Abstractions;

namespace Trading.Framework.Infrastructure.Bitvavo;

public static class BitvavoServiceCollectionExtensions
{
    public static IServiceCollection AddBitvavoTickerIngestion(this IServiceCollection services)
    {
        services.TryAddSingleton<IBitvavoWsClient, BitvavoWsClient>();
        services.TryAddSingleton<IMarketDataSource, BitvavoWebSocketTickerSource>();
        services.AddHostedService(sp => (BitvavoWebSocketTickerSource)sp.GetRequiredService<IMarketDataSource>());
        services.AddHostedService<TickerPersistenceWorker>();
        return services;
    }
}
