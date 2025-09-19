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
        => _connStr = cfg.GetConnectionString("TradingDatabase") ?? "Host=localhost;Port=5432;Database=bitvavo;Username=postgres;Password=1deszebil2";

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
    private sealed record TickerRow(string Market, decimal Price, decimal BestBid, decimal BestAsk, long Sequence, DateTime Timestamp);
    public async Task<IReadOnlyList<Ticker>> GetRecentTickersAsync(string? market, int take, CancellationToken ct)
    {
        take = Math.Clamp(take, 1, 1000);
        await using var con = new NpgsqlConnection(_connStr);
        await con.OpenAsync(ct);

        var sql = string.IsNullOrWhiteSpace(market)
    ? """
      select market as Market, price as Price, best_bid as BestBid, best_ask as BestAsk,
             sequence as Sequence, ts as Timestamp
      from public.tickers
      order by ts desc
      limit @take
      """
    : """
      select market as Market, price as Price, best_bid as BestBid, best_ask as BestAsk,
             sequence as Sequence, ts as Timestamp
      from public.tickers
      where market = @market
      order by ts desc
      limit @take
      """;

        var tmp = await con.QueryAsync<TickerRow>(new CommandDefinition(sql, new { market, take }, cancellationToken: ct));

        var rows = tmp.Select(r =>
            new Trading.Framework.Core.Ticker(
                r.Market, r.Price, r.BestBid, r.BestAsk, r.Sequence,
                new DateTimeOffset(DateTime.SpecifyKind(r.Timestamp, DateTimeKind.Utc)))
        ).ToList();
        // rows = await con.QueryAsync<Ticker>(new CommandDefinition(sql, new { market, take }, cancellationToken: ct));

        return rows.AsList();
    }
   
    public async Task<Ticker?> GetLastTickerAsync(string market, CancellationToken ct)
    {
        await using var con = new NpgsqlConnection(_connStr);
        await con.OpenAsync(ct);
        const string sql = """
        select  market       as Market,
                price        as Price,
                best_bid     as BestBid,
                best_ask     as BestAsk,
                sequence     as Sequence,
                (ts at time zone 'utc') as TimestampUtc
        from public.tickers
        where market = @market
        order by ts desc, id desc   -- tie-breaker for same timestamp
        limit 1
        """;

        var row = await con.QueryFirstOrDefaultAsync<TickerRow>(
            new CommandDefinition(sql, new { market }, cancellationToken: ct));

        if (row is null) return null;

        // Wrap the UTC DateTime into a DateTimeOffset(UTC)
        var utc = DateTime.SpecifyKind(row.Timestamp, DateTimeKind.Utc);
        return new Ticker(row.Market, row.Price, row.BestBid, row.BestAsk, row.Sequence, new DateTimeOffset(utc));

    }
}