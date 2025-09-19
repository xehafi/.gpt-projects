# Bitvavo Trading Framework (Starter + Step 3)

- .NET 8, VS2022, Razor Pages
- Local PostgreSQL 17 (no Docker)
- Console ingestor + Web dashboard
- Step 3 adds: `/api/series`, `/api/summary`, and `/market` page with a live line chart and stats.

## Run
1. Open `BitvavoTradingFramework.sln`.
2. Update connection strings in `Trading.Framework.Console/appsettings.json` and `Trading.Framework.Web/appsettings.json`.
3. Run **Trading.Framework.Console** to insert ticks.
4. Run **Trading.Framework.Web** and open `/market?market=BTC-EUR&minutes=60`.
