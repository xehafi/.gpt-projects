from typing import List, Dict, Iterable, Tuple
import psycopg
import os
from datetime import datetime

def get_dsn() -> str:
    dsn = os.getenv("DB_DSN")
    if not dsn:
        raise RuntimeError("DB_DSN not set in .env")
    return dsn

def fetch_favorites() -> List[Dict]:
    dsn = get_dsn()
    sql = """
      select id, asset, name, addedat
      from public.favorites
      order by name asc, asset asc
    """
    with psycopg.connect(dsn, autocommit=True) as conn:
        with conn.cursor() as cur:
            cur.execute(sql)
            rows = cur.fetchall()
    return [
        {"id": r[0], "asset": r[1], "name": r[2] or "", "addedat": r[3]}
        for r in rows
    ]

def upsert_price_rows(table: str, asset: str, rows: Iterable[Tuple[datetime,float,float,float,float,float]]):
    """
    rows: iterable of (timestamp, open, high, low, close, volume)
    Upserts into given table on (asset, timestamp).
    """
    dsn = get_dsn()
    sql = f"""
      insert into public.{table} (asset, "timestamp", open, high, low, close, volume)
      values (%s, %s, %s, %s, %s, %s, %s)
      on conflict (asset, "timestamp")
      do update set open=excluded.open, high=excluded.high, low=excluded.low,
                    close=excluded.close, volume=excluded.volume
    """
    with psycopg.connect(dsn, autocommit=True) as conn:
        with conn.cursor() as cur:
            data = [(asset, ts, o, h, l, c, v) for (ts,o,h,l,c,v) in rows]
            if data:
                cur.executemany(sql, data)
