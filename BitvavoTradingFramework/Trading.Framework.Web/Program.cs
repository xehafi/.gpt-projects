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

app.Run();
