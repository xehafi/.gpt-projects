using Dapper;
using Npgsql;
using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Trading.Framework.Infrastructure.Persistence;

public sealed class PostgresTradeRepository : ITradeRepository
{
    private readonly string _connStr;
    public PostgresTradeRepository(IConfiguration cfg)
        => _connStr = cfg.GetConnectionString("TradingDatabase") ?? "Host=localhost;Port=5432;Database=bitvavo;Username=postgres;Password=postgres";

    public async Task EnsureDatabaseAsync(CancellationToken ct)
    {
        await using var con = new NpgsqlConnection(_connStr);
        await con.OpenAsync(ct);
        var sql = @"
        create table if not exists public.tickers(
            id bigserial primary key,
            market text not null,
            price numeric not null,
            best_bid numeric not null,
            best_ask numeric not null,
            sequence bigint not null,
            ts timestamptz not null
        );";
        await con.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct));
    }

    public async Task SaveTickerAsync(Ticker t, CancellationToken ct)
    {
        await using var con = new NpgsqlConnection(_connStr);
        await con.OpenAsync(ct);
        var sql = "insert into public.tickers(market, price, best_bid, best_ask, sequence, ts) values (@Market,@Price,@BestBid,@BestAsk,@Sequence,@Timestamp)";
        await con.ExecuteAsync(new CommandDefinition(sql, t, cancellationToken: ct));
    }
}