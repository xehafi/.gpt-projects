namespace TradingBot.Core.Models;

public readonly record struct Candle(
    DateTimeOffset Time,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume);
