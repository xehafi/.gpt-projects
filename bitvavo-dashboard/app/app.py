import os, time
from typing import List, Dict, Optional
from datetime import datetime, timezone, timedelta

import pandas as pd
from fastapi import FastAPI, HTTPException, Body
from fastapi.responses import HTMLResponse, PlainTextResponse, FileResponse
from fastapi.staticfiles import StaticFiles
from pydantic import BaseModel
from dotenv import set_key, dotenv_values

from app.db import fetch_favorites, upsert_price_rows
from app.bot_core import BOT, Config, compute_signals, fetch_ohlcv_df, make_exchange, normalize_symbol

app = FastAPI(title="Bitvavo Bot Dashboard")
app.mount("/static", StaticFiles(directory="app/static"), name="static")

@app.get("/", response_class=HTMLResponse)
def index():
    return FileResponse("app/static/index.html")

# ----------- Bot controls -----------
@app.get("/api/status")
def status():
    return BOT.get_status()

@app.get("/api/logs", response_class=PlainTextResponse)
def logs():
    return BOT.get_logs()

@app.post("/api/start")
def start():
    BOT.start()
    return {"ok": True}

@app.post("/api/stop")
def stop():
    BOT.stop()
    return {"ok": True}

# ----------- Config -----------
class ConfIn(BaseModel):
    BOT_MODE: Optional[str] = None
    BOT_SYMBOL: Optional[str] = None
    BOT_TIMEFRAME: Optional[str] = None
    BOT_RISK_FRACTION: Optional[float] = None
    TAKE_PROFIT_PCT: Optional[float] = None
    STOP_LOSS_PCT: Optional[float] = None
    LIVE_TRADING: Optional[str] = None
    LIVE_CONFIRM: Optional[str] = None

def _save_env(updates: dict):
    env_path = ".env"
    cur = dotenv_values(env_path)
    cur.update({k:v for k,v in updates.items() if v is not None})
    for k,v in updates.items():
        if v is not None:
            set_key(env_path, k, str(v))
    BOT.reload_env()
    return cur

@app.get("/api/config")
def get_conf():
    cfg = Config()
    return cfg.__dict__

@app.post("/api/config")
def set_conf(data: ConfIn):
    updates = {k:v for k,v in data.dict().items() if v is not None}
    _save_env(updates)
    return {"ok": True, "applied": updates}

# ----------- HOME: account balances & prices -----------
@app.get("/api/home/holdings")
def home_holdings():
    cfg = Config()
    ex = make_exchange(cfg)
    bal = ex.fetch_balance()
    items = []
    totals_eur = 0.0

    # We consider assets with total > 0, skip EUR itself in per-asset unless you want it.
    for base, amt in (bal.get("total") or {}).items():
        try:
            total = float(amt or 0)
        except Exception:
            continue
        if total <= 0: 
            continue
        # Try price in EUR
        price = None
        if base.upper() == "EUR":
            price = 1.0
        else:
            symbol = f"{base.upper()}/EUR"
            try:
                t = ex.fetch_ticker(symbol)
                price = float(t["last"]) if t and t.get("last") is not None else None
            except Exception:
                price = None
        value = total * price if (price is not None) else None
        free = float((bal.get("free") or {}).get(base, 0) or 0)
        used = float((bal.get("used") or {}).get(base, 0) or 0)
        items.append({
            "asset": base,
            "free": round(free, 8),
            "used": round(used, 8),
            "total": round(total, 8),
            "price_eur": round(price, 6) if price is not None else None,
            "value_eur": round(value, 2) if value is not None else None
        })
        if value is not None:
            totals_eur += value

    # Sort by value desc (unknown prices last)
    items.sort(key=lambda x: (-1 if x["value_eur"] is None else -x["value_eur"]))
    return {"items": items, "total_value_eur": round(totals_eur, 2)}

# ----------- FAVORITES: status via DB list -----------
@app.get("/api/favorites")
def favorites_status():
    try:
        cfg = Config()
        ex = make_exchange(cfg)
        favs = fetch_favorites()
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"init_failed: {e}")

    out: List[Dict] = []
    for f in favs:
        asset_db = f["asset"]                 # e.g., "XRP-EUR"
        symbol = normalize_symbol(asset_db)   # -> "XRP/EUR"
        try:
            df = fetch_ohlcv_df(ex, symbol, cfg.timeframe, limit=max(cfg.sma_long*3, 200))
            df = compute_signals(df, cfg)
            last = df.iloc[-1]
            price = float(last["close"])
            sma_s = float(last["sma_s"]) if pd.notna(last["sma_s"]) else None
            sma_l = float(last["sma_l"]) if pd.notna(last["sma_l"]) else None
            rsi   = float(last["rsi"])   if pd.notna(last["rsi"])   else None
            signal = "entry" if bool(last["entry"]) else ("exit" if bool(last["exit"]) else "neutral")
            out.append({
                "id": f["id"],
                "asset": asset_db,  # keep original display
                "name": f.get("name") or "",
                "last_price": price,
                "sma_short": sma_s,
                "sma_long": sma_l,
                "rsi": rsi,
                "signal": signal
            })
        except Exception as e:
            out.append({
                "id": f["id"],
                "asset": asset_db,
                "name": f.get("name") or "",
                "error": str(e)
            })
    return {"items": out}

# ----------- SYNC endpoints (write to Postgres) -----------
class SyncIn(BaseModel):
    asset: str                    # e.g., "XRP-EUR" or "ALL"
    kind: str                     # "hourly" or "latest"
    lookback: Optional[int] = None  # candles to fetch; default sensible per kind

@app.post("/api/sync")
def sync_prices(data: SyncIn = Body(...)):
    cfg = Config()
    ex = make_exchange(cfg)

    kind = data.kind.lower().strip()
    if kind not in ("hourly", "latest"):
        raise HTTPException(400, "kind must be 'hourly' or 'latest'")

    # Choose timeframe and default limits
    if kind == "hourly":
        timeframe = "1h"
        limit = data.lookback or 500       # up to ~500 hours
        table = "pricehistory"
    else:
        timeframe = "5m"
        limit = data.lookback or 288       # ~24h
        table = "latestpricehistory"

    assets: List[str] = []
    if data.asset.upper() == "ALL":
        favs = fetch_favorites()
        assets = [f["asset"] for f in favs]
        if not assets:
            raise HTTPException(400, "No favorites to sync when asset=ALL")
    else:
        assets = [data.asset]

    synced = []
    errors = []

    for asset_db in assets:
        symbol = normalize_symbol(asset_db)
        try:
            ohlcv = ex.fetch_ohlcv(symbol, timeframe=timeframe, limit=limit)
            # map to rows for DB
            rows = []
            for ts_ms, o,h,l,c,v in ohlcv:
                ts = datetime.fromtimestamp(ts_ms/1000, tz=timezone.utc).replace(tzinfo=None)  # naive UTC
                rows.append((ts, float(o), float(h), float(l), float(c), float(v)))
            upsert_price_rows(table, asset_db, rows)  # store using DB's asset format (e.g., XRP-EUR)
            synced.append({"asset": asset_db, "count": len(rows), "table": table})
        except Exception as e:
            errors.append({"asset": asset_db, "error": str(e)})

    return {"ok": True, "synced": synced, "errors": errors}
