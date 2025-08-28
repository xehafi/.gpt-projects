namespace TradingBot.Core.Models;

public enum SignalAction { Hold = 0, Buy = 1, Sell = 2, Close = 3 }

public sealed record Signal(
    string Symbol,
    SignalAction Action,
    decimal? Price = null,
    decimal? Quantity = null,
    double Confidence = 0.0);
