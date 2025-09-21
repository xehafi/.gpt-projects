using System.Text.Json.Serialization;
using Trading.Framework.Core;

namespace Trading.Framework.Infrastructure.Bitvavo;

// A typical "ticker" message from Bitvavo carries these fields.
// We only map what we need. JSON uses lowercase keys.
public sealed class BitvavoTickerMsg
{
    [JsonPropertyName("event")] public string? Event { get; set; } // e.g., "ticker"
    [JsonPropertyName("market")] public string? Market { get; set; }
    [JsonPropertyName("price")] public string? Price { get; set; } // stringified decimal
    [JsonPropertyName("bid")] public string? Bid { get; set; }
    [JsonPropertyName("ask")] public string? Ask { get; set; }
    [JsonPropertyName("timestamp")] public long? TimestampMs { get; set; }
}

public static class BitvavoMapping
{
    public static bool TryToTicker(BitvavoTickerMsg m, out Ticker ticker)
    {
        ticker = default!;
        if (m.Market is null || m.Price is null || m.Bid is null || m.Ask is null) return false;

        // Use decimal.TryParse with invariant culture for safety.
        if (!decimal.TryParse(m.Price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price)) return false;
        if (!decimal.TryParse(m.Bid, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var bid)) return false;
        if (!decimal.TryParse(m.Ask, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var ask)) return false;

        var ts = m.TimestampMs.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(m.TimestampMs.Value).UtcDateTime
            : DateTime.UtcNow;

        // Sequence is unknown from public ticker stream; use 0 or derive if available.
        ticker = new Ticker(m.Market, price, bid, ask, Sequence: 0, Timestamp: ts);
        return true;
    }
}
