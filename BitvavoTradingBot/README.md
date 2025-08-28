# BitvavoTradingBot (Skeleton)

A clean .NET 8 solution skeleton for a Windows-based trading bot that:
- Loads historical OHLCV candles from CSV
- Runs a **statistical strategy** (EMA crossover w/ ATR filter) via a backtester
- Applies a simple **risk manager** and **execution simulator** (with stop-loss approximation)
- Produces a basic performance summary

This is a starting point you can extend with Bitvavo REST/WebSocket, database storage, richer strategies, and live/paper trading.

## Requirements
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- Windows 10 or later (Linux/Mac also fine)
- (Optional) Docker Desktop, if you plan to use Postgres (compose file is included)

## Quick start (backtest on sample data)
```bash
cd BitvavoTradingBot
dotnet build
dotnet run --project src/TradingBot.App -- --mode backtest --symbol BTC-EUR --file ./data/BTC-EUR-1m-sample.csv
```
If you omit args, defaults are used:
```bash
dotnet run --project src/TradingBot.App
```

## Project layout
```
BitvavoTradingBot/
  data/                          # sample CSV
  infra/
    docker-compose.postgres.yml  # optional Postgres for future persistence
    schema.sql                   # example schema
  src/
    TradingBot.Core/             # domain, indicators, strategy, risk, portfolio
    TradingBot.Backtest/         # CSV loader, simulator, backtest runner
    TradingBot.Exchange/         # Paper broker (stub), Bitvavo client stub
    TradingBot.App/              # console entrypoint
```

## Next steps
- Implement real Bitvavo client in `TradingBot.Exchange` (REST + WebSocket).
- Replace CSV loader with a DB loader (Postgres) or exchange pull.
- Add metrics, logging, parameter sweeps, and walk-forward validation.
- Wrap into a Windows Service for 24/7 live/paper modes.

## Notes
- This skeleton intentionally avoids external NuGet dependencies for a smooth first build. 
- Numbers are decimals; risk sizing is simplified; intrabar behavior is approximated using candle OHLC.
- Fees default to 0.10% per trade (configurable).

Good luck and have fun!
