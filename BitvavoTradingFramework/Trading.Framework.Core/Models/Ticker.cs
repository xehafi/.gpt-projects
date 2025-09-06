namespace Trading.Framework.Core;
public sealed record Ticker(string Market, decimal Price, decimal BestBid, decimal BestAsk, long Sequence, DateTimeOffset Timestamp);