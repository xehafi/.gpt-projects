-- Example schema for later DB usage
CREATE TABLE IF NOT EXISTS candles(
  symbol TEXT NOT NULL,
  tf TEXT NOT NULL,
  open_time TIMESTAMPTZ NOT NULL,
  open NUMERIC NOT NULL,
  high NUMERIC NOT NULL,
  low NUMERIC NOT NULL,
  close NUMERIC NOT NULL,
  volume NUMERIC NOT NULL,
  PRIMARY KEY(symbol, tf, open_time)
);

CREATE TABLE IF NOT EXISTS runs(
  id UUID PRIMARY KEY,
  mode TEXT NOT NULL,
  symbol TEXT NOT NULL,
  started_at TIMESTAMPTZ NOT NULL,
  ended_at TIMESTAMPTZ NOT NULL,
  pnl NUMERIC NOT NULL,
  max_drawdown NUMERIC NOT NULL,
  sharpe NUMERIC,
  notes TEXT
);
