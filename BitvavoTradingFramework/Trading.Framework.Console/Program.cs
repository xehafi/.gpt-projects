using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Trading.Framework.Core.Abstractions;
using Trading.Framework.Infrastructure.Bitvavo;
using Trading.Framework.Infrastructure.Persistence;
using Trading.Framework.Application.Orchestrator;
using Trading.Framework.Application.Strategies;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/trading-console.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddLogging(l => l.AddSerilog());
builder.Services.AddSingleton<IMarketDataSource, BitvavoTickerSource>();
builder.Services.AddSingleton<ITradeRepository, PostgresTradeRepository>();
builder.Services.AddSingleton<ITradingStrategy, EchoStrategy>();
builder.Services.AddSingleton<TickerOrchestrator>();

var app = builder.Build();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var market = app.Services.GetRequiredService<IConfiguration>()["Market"] ?? "BTC-EUR";
var orchestrator = app.Services.GetRequiredService<TickerOrchestrator>();
await orchestrator.RunAsync(market, cts.Token);
