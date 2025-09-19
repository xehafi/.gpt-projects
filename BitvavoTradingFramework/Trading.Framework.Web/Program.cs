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

app.MapGet("/api/tickers", async (string? market, int take, ITradeRepository repo, CancellationToken ct) =>
{
    take = take <= 0 ? 200 : take; // default
    var items = await repo.GetRecentTickersAsync(market, take, ct);
    return Results.Ok(items);
});

app.MapGet("/api/last", async (string market, ITradeRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(market)) return Results.BadRequest("market is required");
    var last = await repo.GetLastTickerAsync(market, ct);
    return last is null ? Results.NotFound() : Results.Ok(last);
});

app.Run();
