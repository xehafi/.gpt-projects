using Serilog;
using Trading.Framework.Core.Abstractions;
using Trading.Framework.Infrastructure.Bitvavo;
using Trading.Framework.Infrastructure.Persistence;
using Trading.Framework.Application.Strategies;
using Trading.Framework.Application.Orchestrator;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables();
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IMarketDataSource, BitvavoTickerSource>();
builder.Services.AddSingleton<ITradeRepository, PostgresTradeRepository>();
builder.Services.AddSingleton<ITradingStrategy, EchoStrategy>();
builder.Services.AddSingleton<TickerOrchestrator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }));

// read APIs
app.MapGet("/api/tickers", async (string? market, int take, ITradeRepository repo, CancellationToken ct) =>
{
    take = take <= 0 ? 200 : take;
    var items = await repo.GetRecentTickersAsync(market, take, ct);
    return Results.Ok(items);
});
app.MapGet("/api/last", async (string market, ITradeRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(market)) return Results.BadRequest("market is required");
    var last = await repo.GetLastTickerAsync(market, ct);
    return last is null ? Results.NotFound() : Results.Ok(last);
});
// Step 3 series + summary
app.MapGet("/api/series", async (string market, int minutes, ITradeRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(market)) return Results.BadRequest("market is required");
    minutes = minutes <= 0 ? 60 : minutes;
    var since = DateTimeOffset.UtcNow.AddMinutes(-minutes);
    var series = await repo.GetTickersSinceAsync(market, since, ct);
    return Results.Ok(series);
});
app.MapGet("/api/summary", async (string market, int minutes, ITradeRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(market)) return Results.BadRequest("market is required");
    minutes = minutes <= 0 ? 60 : minutes;
    var since = DateTimeOffset.UtcNow.AddMinutes(-minutes);
    var series = await repo.GetTickersSinceAsync(market, since, ct);
    if (series.Count == 0) return Results.Ok(new { market, minutes, count = 0 });
    var first = series.First();
    var last = series.Last();
    decimal change = last.Price - first.Price;
    decimal pct = first.Price == 0 ? 0 : (change / first.Price) * 100m;
    return Results.Ok(new { market, minutes, count = series.Count, lastPrice = last.Price, firstPrice = first.Price, change, pct });
});

app.Run();
