from __future__ import annotations
import os, time, threading
from dataclasses import dataclass
from typing import Optional, List
import numpy as np
import pandas as pd
import ccxt
from dotenv import load_dotenv

load_dotenv()

def normalize_symbol(sym: str) -> str:
    """Convert 'xrp-eur' or 'XRP-EUR' to 'XRP/EUR' for ccxt."""
    if not sym: return sym
    return sym.strip().upper().replace("-", "/")

@dataclass
class Config:
    mode: str = os.getenv("BOT_MODE", "paper")
    symbol: str = os.getenv("BOT_SYMBOL", "BTC/EUR")
    timeframe: str = os.getenv("BOT_TIMEFRAME", "1m")
    risk_fraction: float = float(os.getenv("BOT_RISK_FRACTION", "0.95"))
    fee_pct: float = float(os.getenv("BOT_FEE_PCT", "0.0015"))
    slippage_pct: float = float(os.getenv("BOT_SLIPPAGE_PCT", "0.0005"))
    sma_short: int = int(os.getenv("SMA_SHORT", "20"))
    sma_long: int = int(os.getenv("SMA_LONG", "50"))
    rsi_len: int = int(os.getenv("RSI_LENGTH", "14"))
    rsi_min: float = float(os.getenv("RSI_MIN", "45"))
    rsi_max: float = float(os.getenv("RSI_MAX", "60"))
    take_profit_pct: float = float(os.getenv("TAKE_PROFIT_PCT", "0.04"))
    stop_loss_pct: float = float(os.getenv("STOP_LOSS_PCT", "0.02"))
    live_trading: str = os.getenv("LIVE_TRADING", "NO")
    live_confirm: str = os.getenv("LIVE_CONFIRM", "")
    api_key: Optional[str] = os.getenv("BITVAVO_API_KEY") or None
    api_secret: Optional[str] = os.getenv("BITVAVO_API_SECRET") or None
    def live_enabled(self) -> bool:
        return (self.mode == "live"
                and self.live_trading.upper() == "YES"
                and self.live_confirm == "I_UNDERSTAND")

def sma(series: pd.Series, length: int) -> pd.Series:
    return series.rolling(length, min_periods=length).mean()

def rsi(series: pd.Series, length: int = 14) -> pd.Series:
    d = series.diff()
    up = np.where(d > 0, d, 0.0)
    dn = np.where(d < 0, -d, 0.0)
    roll_up = pd.Series(up, index=series.index).ewm(alpha=1/length, adjust=False).mean()
    roll_dn = pd.Series(dn, index=series.index).ewm(alpha=1/length, adjust=False).mean()
    rs = roll_up / (roll_dn + 1e-12)
    return 100 - (100 / (1 + rs))

def compute_signals(df: pd.DataFrame, cfg: Config) -> pd.DataFrame:
    df = df.copy()
    df["sma_s"] = sma(df["close"], cfg.sma_short)
    df["sma_l"] = sma(df["close"], cfg.sma_long)
    df["rsi"] = rsi(df["close"], cfg.rsi_len)
    df["cross_up"] = (df["sma_s"] > df["sma_l"]) & (df["sma_s"].shift(1) <= df["sma_l"].shift(1))
    df["cross_dn"] = (df["sma_s"] < df["sma_l"]) & (df["sma_s"].shift(1) >= df["sma_l"].shift(1))
    df["rsi_ok"] = (df["rsi"] >= cfg.rsi_min) & (df["rsi"] <= cfg.rsi_max)
    df["entry"] = df["cross_up"] & df["rsi_ok"]
    df["exit"]  = df["cross_dn"]
    return df

class Position:
    def __init__(self): self.qty=0.0; self.entry=0.0

class PaperBroker:
    def __init__(self, cfg: Config, cash: float = 10_000.0):
        self.cfg=cfg; self.cash=cash; self.pos=Position()
        self.history: List[dict]=[]
    def _px(self, p: float, side: str) -> float:
        slip=p*self.cfg.slippage_pct; fee=p*self.cfg.fee_pct
        return p+slip+fee if side=="buy" else p-slip-fee
    def buy(self, p: float):
        if self.pos.qty>0: return
        px=self._px(p,"buy"); spend=self.cash*self.cfg.risk_fraction
        if spend<=0: return
        qty=spend/px; self.cash-=qty*px; self.pos.qty=qty; self.pos.entry=px
        self.history.append({"t":"buy","p":px,"q":qty})
    def sell(self, p: float):
        if self.pos.qty<=0: return
        px=self._px(p,"sell"); proceeds=self.pos.qty*px; self.cash+=proceeds
        self.history.append({"t":"sell","p":px,"q":self.pos.qty}); self.pos=Position()
    def equity(self, last: float)->float:
        val=self.pos.qty*(last*(1-self.cfg.slippage_pct-self.cfg.fee_pct)) if self.pos.qty>0 else 0.0
        return self.cash+val

class LiveBroker:
    def __init__(self, cfg: Config, ex):
        if not cfg.live_enabled():
            raise RuntimeError("Live trading blocked. Set BOT_MODE=live, LIVE_TRADING=YES, LIVE_CONFIRM=I_UNDERSTAND.")
        self.cfg=cfg; self.ex=ex
    def buy(self, price_hint: float):
        px=round(price_hint*(1-0.001),2)
        bal=self.ex.fetch_balance()
        quote=self.cfg.symbol.split("/")[1]; free=bal["free"].get(quote,0)
        spend=free*self.cfg.risk_fraction
        if spend<=0: return
        qty=spend/px; qty=float(self.ex.amount_to_precision(self.cfg.symbol, qty))
        px=float(self.ex.price_to_precision(self.cfg.symbol, px))
        self.ex.create_limit_buy_order(self.cfg.symbol, qty, px)
    def sell(self, price_hint: float):
        px=round(price_hint*(1+0.001),2)
        bal=self.ex.fetch_balance()
        base=self.cfg.symbol.split("/")[0]; qty=bal["free"].get(base,0) or 0.0
        if qty<=0: return
        qty=float(self.ex.amount_to_precision(self.cfg.symbol, qty))
        px=float(self.ex.price_to_precision(self.cfg.symbol, px))
        self.ex.create_limit_sell_order(self.cfg.symbol, qty, px)

def make_exchange(cfg: Config):
    ex = ccxt.bitvavo({"apiKey": cfg.api_key or "", "secret": cfg.api_secret or "", "enableRateLimit": True})
    ex.options = { **getattr(ex,"options",{}), "defaultType":"spot" }
    return ex

def fetch_ohlcv_df(ex, symbol: str, timeframe: str, limit: int=600) -> pd.DataFrame:
    o = ex.fetch_ohlcv(symbol, timeframe=timeframe, limit=limit)
    df = pd.DataFrame(o, columns=["ts","open","high","low","close","volume"])
    df["ts"] = pd.to_datetime(df["ts"], unit="ms", utc=True)
    return df

class BotRunner:
    def __init__(self):
        self.cfg = Config()
        self.ex = make_exchange(self.cfg)
        self.thread: Optional[threading.Thread]=None
        self.stop_evt = threading.Event()
        self.paper = PaperBroker(self.cfg)
        self.status = {"running": False, "mode": self.cfg.mode, "symbol": self.cfg.symbol,
                       "timeframe": self.cfg.timeframe, "equity": self.paper.cash, "position_qty": 0.0,
                       "last_price": None, "last_action": None}
        self.log_lines: List[str]=[]
    def log(self, msg: str):
        print(msg); self.log_lines.append(msg); self.log_lines=self.log_lines[-300:]
    def reload_env(self):
        load_dotenv(override=True); self.cfg = Config()
        self.ex = make_exchange(self.cfg)
        self.log("Config reloaded.")
    def _loop(self):
        self.log("Bot loop started.")
        self.status["running"]=True
        while not self.stop_evt.is_set():
            norm_symbol = normalize_symbol(self.cfg.symbol)
            df = fetch_ohlcv_df(self.ex, norm_symbol, self.cfg.timeframe, limit=max(self.cfg.sma_long*3, 200))
            df = compute_signals(df, self.cfg)
            last = df.iloc[-1]; px=float(last["close"])
            self.status["last_price"]=px
            if self.cfg.mode=="paper":
                if self.paper.pos.qty==0 and bool(last["entry"]):
                    self.paper.buy(px); self.status["last_action"]="BUY"; self.log(f"[PAPER] BUY @ {px}")
                elif self.paper.pos.qty>0:
                    entry=self.paper.pos.entry
                    if px>=entry*(1+self.cfg.take_profit_pct) or px<=entry*(1-self.cfg.stop_loss_pct) or bool(last["exit"]):
                        self.paper.sell(px); self.status["last_action"]="SELL"; self.log(f"[PAPER] SELL @ {px}")
                self.status["equity"]=round(self.paper.equity(px),2)
                self.status["position_qty"]=self.paper.pos.qty
            elif self.cfg.mode=="live":
                try:
                    broker = LiveBroker(self.cfg, self.ex)
                    if bool(last["entry"]): broker.buy(px); self.status["last_action"]="LIVE_BUY"; self.log(f"[LIVE] BUY signal @ {px}")
                    elif bool(last["exit"]): broker.sell(px); self.status["last_action"]="LIVE_SELL"; self.log(f"[LIVE] SELL signal @ {px}")
                except Exception as e:
                    self.log(f"[LIVE BLOCKED] {e}")
            time.sleep(60)
        self.status["running"]=False
        self.log("Bot loop stopped.")
    def start(self):
        if self.thread and self.thread.is_alive(): return
        self.stop_evt.clear()
        self.thread = threading.Thread(target=self._loop, daemon=True)
        self.thread.start()
    def stop(self):
        if self.thread and self.thread.is_alive():
            self.stop_evt.set(); self.thread.join(timeout=5)
    def get_status(self): return self.status
    def get_logs(self): return "\n".join(self.log_lines[-200:])
BOT = BotRunner()
