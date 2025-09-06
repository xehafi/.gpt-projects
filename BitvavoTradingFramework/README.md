# Bitvavo Trading Framework (Starter)

**Targets**: Windows 10, Visual Studio 2022, .NET 8, Razor Pages, PostgreSQL 17 (local). No Docker.

This starter creates a clean layered structure:
- **Trading.Framework.Core**: Models & abstractions.
- **Trading.Framework.Application**: Orchestrator and sample strategy.
- **Trading.Framework.Infrastructure**: Postgres repository + placeholder Bitvavo ticker source.
- **Trading.Framework.Console**: Runs the orchestrator (streams a fake ticker to DB).
- **Trading.Framework.Web**: Minimal Razor dashboard + /health endpoint.
- **Trading.Framework.Tests**: xUnit smoke test.

## Quick Start
1. Open `BitvavoTradingFramework.sln` in Visual Studio 2022.
2. Ensure .NET SDK 8.x and PostgreSQL 17 are installed and `postgres` user can connect.
3. Update the connection string in `Trading.Framework.Console/appsettings.json` and `Trading.Framework.Web/appsettings.json` if needed.
4. Start **Trading.Framework.Console** (F5). It will create `public.tickers` and append one row per second.
5. Start **Trading.Framework.Web** to view the dashboard and hit `/health`.

> Replace `BitvavoTickerSource` with a real client when you're ready to integrate Bitvavo WebSockets/REST.
