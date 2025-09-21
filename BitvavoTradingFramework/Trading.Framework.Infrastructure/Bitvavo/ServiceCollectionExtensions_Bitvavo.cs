using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting; // Add this import
using Trading.Framework.Core.Abstractions;

namespace Trading.Framework.Infrastructure.Bitvavo;

public static class BitvavoServiceCollectionExtensions
{
    public static IServiceCollection AddBitvavoTickerIngestion(this IServiceCollection services, bool useMock = false)
    {
        if (useMock)
        {
            services.TryAddSingleton<IMarketDataSource, BitvavoTickerSource>();
            // FIX: Register BitvavoTickerSource as IHostedService using a wrapper
            services.AddHostedService<BitvavoTickerSourceHostedService>();
        }
        else
        {
            services.TryAddSingleton<IBitvavoWsClient, BitvavoWsClient>();
            services.TryAddSingleton<IMarketDataSource, BitvavoWebSocketTickerSource>();
            services.AddHostedService(sp => (BitvavoWebSocketTickerSource)sp.GetRequiredService<IMarketDataSource>());
        }
        services.AddHostedService<TickerPersistenceWorker>();
        return services;
    }
}

// Add this wrapper class to adapt BitvavoTickerSource to IHostedService
public class BitvavoTickerSourceHostedService : IHostedService
{
    private readonly BitvavoTickerSource _source;

    public BitvavoTickerSourceHostedService(IMarketDataSource source)
    {
        _source = source as BitvavoTickerSource ?? throw new ArgumentException("IMarketDataSource is not BitvavoTickerSource");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _source.ConnectAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _source.DisposeAsync();
    }
}
